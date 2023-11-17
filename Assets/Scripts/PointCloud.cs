using Pcx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Point
{
    public Vector3 Position;
    public Color32 Color;
}

public struct PointExt
{
    //Point extended
    public Point Point;
    public Vector3 Normal;
    public float[] prop_package;
}

public class PointCloud : MonoBehaviour
{
    //Spatial info
    [HideInInspector]
    public PointExt[] Points;
    public Vector3 Centroid = new Vector3(0, 0, 0);
    public Vector3 Size = new Vector3(0, 0, 0);
    [HideInInspector]
    public float[,] PCBoundingBox = new float[3, 2];    //Column1: Max, Column2: Min // File1: x, File2: y, File3: z

    // Game Objects and Scripts
    [HideInInspector]
    public MainManager MainManager;
    [HideInInspector]
    public PointCloudRenderer PointCloudRendererPC;
    [HideInInspector]
    public GameObject PointCloudParentGO;
    [HideInInspector]
    public GameObject PointCloudGO;
    [HideInInspector]
    public StateManager StateManager;
    [HideInInspector]
    public GameObject MinValueSphere;
    [HideInInspector]
    public GameObject MaxValueSphere;
    [HideInInspector]
    public BoxCollider BoxCollider;
    public List<PointCloudSliceClass> PointCloudSlices = new List<PointCloudSliceClass>();
    Vector3 VoxelReference = new Vector3(10, 5, 10);

    //Point Cloud sets
    public PointExt[] ReducedPointCloud;
    protected Point[] PointsToRender; //It is necessary since PointCloudRenderer works with Point[], not with PointExt[]
    protected Point[] TotalPointsToRender;
    protected PointExt[] UpdatedPositionPointCloud;
    public PointOctree<PointExt> FinalOctree = null;    //Octree related to the final point cloud to be rendered
    public BoundsOctree<PointExt> BoundOctree = null;

    //Parameters
    public int Property; //Current property from the cloud
    public int PropertiesCount;
    public int MaxIndex;
    public int MinIndex;
    public int HistogramSeeds = 8;
    public float VoxelSize;
    public float LerpFactor;
    public float MeanScale;
    public float MaxScale;
    public float MinScale;
    public bool Print = false;
    public bool GlobalRanges = true;
    public bool FrustumFilled = false;

    public int MixType = 0; // 0: Mixing property and Grayscale (from RGB) - Potentiometer controls the brightness
                            // 1: Mixing property and RGB - Potentiometer controls the mix between property and RGB
                            // Minimum value (left) depicts only property; Maximum value (right) depicts only RGB
    public int ScaleType = 0; // 0: maximum and minimum absolutes for all properties
                              // 1: maximum and minimum relative for each property

    // Private parameters
    private int numfloat, numproperties, countdata, factImportNormal;
    private bool importNormal, justOnce;
    private int[] indexFrustum = null;
    private float[] maxproperties;
    private float[] minproperties;
    private float[] meanproperties;
    private float[] databuffer;
    private Color32 _colorAux;

    private void Start()
    {
        BoxCollider = GetComponentInParent<BoxCollider>();
        PointCloudGO = this.gameObject;
        MainManager = GameObject.FindGameObjectWithTag("MainManager").GetComponent<MainManager>();
        PointCloudRendererPC = GetComponent<PointCloudRenderer>();
        StateManager = GetComponentInParent<StateManager>();

        numproperties = PointCloudRendererPC.sourceDataProps;
        countdata = PointCloudRendererPC.sourceDataExt._pointDataExt.Length;
        importNormal = PointCloudRendererPC.sourceDataExt.importNormal;
        numfloat = importNormal ? (numfloat = 7 + numproperties) : (4 + numproperties);
        factImportNormal = importNormal ? 0 : -3;
        databuffer = (float[])PointCloudRendererPC.sourceDataExt._pointDataExt.Clone();

        Property = numproperties;
        maxproperties = new float[numproperties];
        minproperties = new float[numproperties];
        meanproperties = new float[numproperties];
        PropertiesCount = numproperties;

        VoxelSize = 250f;
        LerpFactor = 0f;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assigning the extended data storaged in databuffer to the struct(PointExt) array OriginalPointCloud
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        Points = new PointExt[countdata / numfloat];
        Points = FillPointArray();

        PCBoundingBox = CalculatePCBoundingBox();
        PCBoundingBox = UpdateTransformPCBoundingBox(PCBoundingBox);

        PointCloudSlices = Slice(true);

        UpdateTransformFordward(true);

        SetThisPointCloud(VoxelSize, false, true, true);

        CalculatePropertiesRanges(out MaxScale, out MinScale, out MeanScale);
    }

    #region Transformations

    public void UpdateTransformFordward(bool usePoints)
    {
        UpdatedPositionPointCloud = Points;

        Matrix4x4 _transMatrix = PointCloudGO.transform.localToWorldMatrix;
        for (int i = 0; i < Points.Length; i++)
        {
            Vector3 _newPoint = _transMatrix.MultiplyPoint3x4(new Vector3(Points[i].Point.Position.x, Points[i].Point.Position.y, Points[i].Point.Position.z));
            UpdatedPositionPointCloud[i].Point.Position = _newPoint;
        }
    }

    public void UpdateTransformBackward()
    {
        int[] _index;
        TotalPointsToRender = new Point[ReducedPointCloud.Length];
        Matrix4x4 matriztrans = PointCloudGO.transform.localToWorldMatrix.inverse;
        _index = FillArray(ReducedPointCloud.Length);
        TotalPointsToRender = UpdateTransform(matriztrans, ReducedPointCloud, _index);
    }

    private Point[] UpdateTransform(Matrix4x4 matriztrans, PointExt[] movedPoints, int[] index)
    {
        Point[] _toRender = new Point[index.Length];
        Vector3 _newPoint;

        for (int i = 0; i < index.Length; i++)
        {
            _newPoint = matriztrans.MultiplyPoint3x4(new Vector3(movedPoints[index[i]].Point.Position.x,
                movedPoints[index[i]].Point.Position.y, movedPoints[index[i]].Point.Position.z));

            _toRender[i].Position = _newPoint;
            _toRender[i].Color = movedPoints[index[i]].Point.Color;
        }

        return _toRender;
    }

    private Point[] UpdateTransform(Point[] movedPoints, int[] index)
    {
        Point[] _toRender = new Point[index.Length];
        for (int i = 0; i < index.Length; i++)
        {
            _toRender[i].Position = movedPoints[index[i]].Position;

            _toRender[i].Color = movedPoints[index[i]].Color;
        }
        return _toRender;
    }

    private Point[] UpdateTransformPointBounds(PointExt[] _puntosMovidos)
    {
        Point[] _paraRenderizar = new Point[_puntosMovidos.Length];
        Matrix4x4 matriztrans = PointCloudParentGO.transform.localToWorldMatrix.inverse;
        for (int i = 0; i < _puntosMovidos.Length; i++)
        {
            Vector3 nuevopunto = matriztrans.MultiplyPoint3x4(new Vector3(_puntosMovidos[i].Point.Position.x,
                _puntosMovidos[i].Point.Position.y, _puntosMovidos[i].Point.Position.z));

            _paraRenderizar[i].Position = nuevopunto;

            _paraRenderizar[i].Color = _puntosMovidos[i].Point.Color;
        }
        return _paraRenderizar;
    }

    #endregion

    #region Point Cloud Managment
    public void SetThisPointCloud(float voxelSize, bool includeNaN, bool reduce, bool hasBeenMoved)
    {
        VoxelSize = voxelSize;

        if (reduce || hasBeenMoved)
        {
            ReducePointCloud(voxelSize, includeNaN);

            UpdateTransformBackward();
            
            CreateOctree(false);

            SwitchOriginalProperty(Property);

            justOnce = true;
        }

        if (StateManager.State == State.HighScale && MainManager.UseFrustum)
        {
            ObtainCullingPointCloudIndex();

            justOnce = true;
        }
        else if (justOnce)
        {
            FillFrustumArray();
            justOnce = false;
        }

        if (indexFrustum.Length > 0)
        {
            RenderFrustum();
        }
    }

    public void ReducePointCloud(float voxelSize, bool includeNaN = true)
    {
        CreateOctree(true);
        List<PointExt> _nubePuntosReducida_list = ObtainPointsFromReducedPC(FinalOctree.RootNode, includeNaN);
        ReducedPointCloud = _nubePuntosReducida_list.ToArray();
    }

    List<PointExt> ObtainPointsFromReducedPC(PointOctreeNode<PointExt> rootNode, bool conNaN = true)
    {
        List<PointExt> reducedPC = new List<PointExt>();
        foreach (var child in rootNode.children)
        {
            if (child.HasChildren)
            {
                reducedPC.AddRange(ObtainPointsFromReducedPC(child, conNaN));
            }
            else if (!child.HasChildren && child.HasAnyObjects())
            {
                float _meanRGB_R = 0.0f;
                float _meanRGB_G = 0.0f;
                float _meanRGB_B = 0.0f;
                float _meanRGB_A = 0.0f;
                float[] prop_packageMean = new float[PropertiesCount];
                float[] _prop_package = new float[PropertiesCount];
                int[] _pointCount_prop_package = new int[PropertiesCount];
                for (int indexal = 0; indexal < PropertiesCount; indexal++)
                {
                    prop_packageMean[indexal] = 0.0f;
                    _prop_package[indexal] = 0.0f;
                }
                foreach (var point in child.objects)
                {
                    for (int indexal = 0; indexal < PropertiesCount; indexal++)
                    {
                        if (!float.IsNaN(point.Obj.prop_package[indexal]))
                        {
                            prop_packageMean[indexal] += point.Obj.prop_package[indexal];
                            _pointCount_prop_package[indexal]++;
                        }
                    }
                    //To do: to optimize
                    _meanRGB_R += (Convert.ToSingle(point.Obj.Point.Color.r));
                    _meanRGB_G += (Convert.ToSingle(point.Obj.Point.Color.g));
                    _meanRGB_B += (Convert.ToSingle(point.Obj.Point.Color.b));
                    _meanRGB_A += (Convert.ToSingle(point.Obj.Point.Color.a));
                }

                bool propisNaN = false;
                for (int indexal = 0; indexal < PropertiesCount; indexal++)
                {
                    _prop_package[indexal] = _pointCount_prop_package[indexal] == 0 ? float.NaN : prop_packageMean[indexal] / _pointCount_prop_package[indexal];
                    propisNaN = propisNaN || ((_pointCount_prop_package[indexal] == 0) ? true : false);
                }

                int _count = child.objects.Count;
                _meanRGB_R = _meanRGB_R / _count;
                _meanRGB_G = _meanRGB_G / _count;
                _meanRGB_B = _meanRGB_B / _count;
                _meanRGB_A = _meanRGB_A / _count;
                if (conNaN || !(propisNaN))
                {
                    PointExt _pointToAdd = new PointExt()
                    {
                        prop_package = (float[])_prop_package.Clone(),
                        Point = new Point()
                        {
                            Position = child.Center,
                            Color = new Color32((byte)_meanRGB_R, (byte)_meanRGB_G, (byte)_meanRGB_B, (byte)_meanRGB_A)
                        }
                    };
                    reducedPC.Add(_pointToAdd);
                }
            }
        }
        return reducedPC;
    }

    public void SwitchOriginalProperty(int property)
    {
        if (!GlobalRanges) GetMaxMinPoint(property, out MaxScale, out MinScale);
        if (Print) PrintMaxMinValuesSpheres(ReducedPointCloud[MaxIndex], ReducedPointCloud[MinIndex]);

        SwitchProperty(property);
    }

    public void SwitchProperty(int property)
    {
        float _scalemax = MaxScale;
        float _scalemin = MinScale;
        float _scalemean = MeanScale;
        Property = property;
        if (property < PropertiesCount)
        {
            for (int i = 0; i < TotalPointsToRender.Length; i++)
            {
                if (ScaleType == 1)
                { _scalemax = maxproperties[property]; _scalemin = minproperties[property]; _scalemean = meanproperties[property]; }
                _colorAux = Intensity2Color(ReducedPointCloud[i].prop_package[property], _scalemax, _scalemin);
                switch (MixType)
                {
                    case 0:
                        var _weight_color = (ReducedPointCloud[i].prop_package[property] > _scalemean) ? ((ReducedPointCloud[i].prop_package[property] - _scalemean) / (_scalemax - _scalemean)) :
                            ((_scalemean - ReducedPointCloud[i].prop_package[property]) / (_scalemean - _scalemin));
                        Color32 _colorgray32 = ReducedPointCloud[i].Point.Color;
                        Color _colorgray = _colorgray32;
                        float _graycolor = 255 * _colorgray.grayscale;
                        float _newred = _weight_color * _colorAux.r + LerpFactor * (1 - _weight_color) * _graycolor;
                        float _newgreen = _weight_color * _colorAux.g + LerpFactor * (1 - _weight_color) * _graycolor;
                        float _newblue = _weight_color * _colorAux.b + LerpFactor * (1 - _weight_color) * _graycolor;
                        Color32 _colorAux_new = new Color32((byte)_newred, (byte)_newgreen, (byte)_newblue, _colorAux.a);
                        TotalPointsToRender[i].Color = _colorAux_new;
                        break;
                    case 1:
                        TotalPointsToRender[i].Color = Color32.Lerp(_colorAux, ReducedPointCloud[i].Point.Color, LerpFactor);
                        break;
                }
            }
        }
        else
        {
            for (int i = 0; i < TotalPointsToRender.Length; i++)
            {
                TotalPointsToRender[i].Color = ReducedPointCloud[i].Point.Color;
            }
        }

        PointCloudRendererPC.sourceBuffer = new ComputeBuffer(TotalPointsToRender.Length, sizeof(float) * 4);
        PointCloudRendererPC.sourceBuffer.SetData(TotalPointsToRender);
    }

    Color32 Intensity2Color(float intensity, float maximum, float minimum)
    {
        float _first_seg = 0.33333f, _second_seg = 0.66666f;
        Color32 _color;
        if (minimum < 0)
        {
            maximum = maximum + Mathf.Abs(minimum);
            minimum = 0;
            intensity = intensity + Mathf.Abs(minimum);
        }
        float _range = maximum - minimum;
        if (intensity <= minimum)
            _color = new Color(0f, 0f, 1f, 1f);
        else if ((intensity > minimum) && (intensity <= (minimum + _first_seg * _range)))
            _color = new Color(0f, (intensity - minimum) / (_first_seg * _range), 0f, 1f);
        else if ((intensity > (minimum + _first_seg * _range)) && (intensity <= (minimum + _second_seg * _range)))
            _color = new Color((intensity - (minimum + _first_seg * _range)) / ((_second_seg - _first_seg) * _range), 1f, 0f, 1f);
        else if ((intensity > (minimum + _second_seg * _range)) && (intensity <= (minimum + _range)))
            _color = new Color(1f, (maximum - intensity) / (maximum - (_first_seg * _range)), 0f, 1f);
        else if (float.IsNaN(intensity))
        {
            _color = new Color(0.7843138f, 0.7843138f, 0.7843138f, 0f);
        }
        else
            _color = new Color(1f, 0f, 0f, 1f);

        _color.a = (byte)(Convert.ToSingle(_color.a) * 16f / 255f);   //It is necessary to avoid white points when variyng the LerpFactor variable since EncodeColor limits alpha to 16.
        return _color;
    }

    void CalculatePropertiesRanges(out float properties_maximum, out float properties_minimum, out float properties_mean)
    {
        List<float> _proplist = new List<float>();
        float[] _propvec1 = new float[ReducedPointCloud.Length];
        float[] _propvec2 = new float[ReducedPointCloud.Length];
        float[] _propvec3 = new float[ReducedPointCloud.Length];
        float[] _propvec4 = new float[ReducedPointCloud.Length];
        float[] _propvec5 = new float[ReducedPointCloud.Length];
        float[] _propvec6 = new float[ReducedPointCloud.Length];

        int prop_number = ReducedPointCloud[0].prop_package.Length;
        List<List<float>> _propslists = new List<List<float>>(prop_number);
        for (int indexal = 0; indexal < prop_number; indexal++)
        {
            _propslists.Add(new List<float>(ReducedPointCloud.Length));
        }
        int _indstep = 0;
        for (int indprop = 0; indprop < ReducedPointCloud.Length; indprop++)
        {
            for (int indexal = 0; indexal < prop_number; indexal++)
            {
                _proplist.Add(ReducedPointCloud[indprop].prop_package[indexal]);
                _propslists[indexal].Add(ReducedPointCloud[indprop].prop_package[indexal]);
            }
            _indstep += 6;
        }

        for (int indexal = 0; indexal < prop_number; indexal++)
        {
            _propslists[indexal] = RemoveOutliers(_propslists[indexal].ToList<float>());
            maxproperties[indexal] = _propslists[indexal].Max();
            minproperties[indexal] = _propslists[indexal].Min();
            meanproperties[indexal] = _propslists[indexal].Average();
        }
        properties_maximum = maxproperties.Max();
        properties_minimum = minproperties.Min();
        properties_mean = meanproperties.Average();
    }

    List<float> RemoveOutliers(List<float> proplist)
    {
        int _itemstotal = proplist.Count;
        int _countitems = proplist.Count;

        float _properties_maximum_init = proplist.Max();
        float _properties_minimum_init = proplist.Min();
        float _properties_mean_init = proplist.Average();

        float _properties_maximum_final = proplist.Max();
        float _properties_minimum_final = proplist.Min();
        float _properties_mean_final = proplist.Average();

        float _median_value = new();

        if (proplist.Count % 2 == 0)
        {
            double _median_pos_odd = (proplist.Count / 2);
            double _median_rounded_pos_odd = Math.Round(_median_pos_odd);
            int _median_int_pos_odd = Convert.ToInt32(_median_rounded_pos_odd);
            _median_value = proplist[_median_int_pos_odd];
        }
        else
        {
            int median_pos_even = (proplist.Count / 2) - 1;
            float median_value_average = (proplist[median_pos_even] + proplist[median_pos_even + 1]) / 2.0f;
            _median_value = median_value_average;
        }
        var upperhalfprop = proplist.Where<float>(o => o.CompareTo(_median_value) > 0).OrderByDescending(o => o).ToList();
        var lowerhalfprop = proplist.Where<float>(o => o.CompareTo(_median_value) < 0).OrderByDescending(o => o).ToList();
        float indq1, indq3;
        if (upperhalfprop.Count != 0)
            indq1 = lowerhalfprop.Average();
        else
            indq1 = _median_value;
        if (lowerhalfprop.Count != 0)
            indq3 = upperhalfprop.Average();
        else
            indq3 = _median_value;

        var indq2 = _median_value;
        var indiqr = indq3 - indq1;
        var outliers = proplist.Where<float>(o => (Math.Abs(o - _properties_mean_init)).CompareTo(1.5f * indiqr) > 0).OrderByDescending(o => o).ToList();

        foreach (float itemoutlier in outliers)
        {
            proplist.Remove(proplist.IndexOf(itemoutlier));
        }

        return proplist;
    }

    public void GetMaxMinPoint(int property, out float return3, out float return4)
    {
        MaxIndex = 0;
        MinIndex = 0;
        float _max = float.MinValue;
        float _min = float.MaxValue;
        int prop_number = ReducedPointCloud[0].prop_package.Length;
        if (property < prop_number)
        {
            for (int i = 0; i < ReducedPointCloud.Length; i++)
            {
                if (ReducedPointCloud[i].prop_package[property] > _max)
                {
                    _max = ReducedPointCloud[i].prop_package[property];
                    MaxIndex = i;
                }
                if (ReducedPointCloud[i].prop_package[property] < _min)
                {
                    _min = ReducedPointCloud[i].prop_package[property];
                    MinIndex = i;
                }
            }
        }
        else
        {
            _max = 0;
            _min = 0;
        }
        return3 = _max;
        return4 = _min;
    }

    public void SwitchPrint()
    {
        Print = !Print;

        if (Print)
        {
            if (MaxValueSphere == null)
            {
                MaxValueSphere = Instantiate(MainManager.MaxValueSpherePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                MaxValueSphere.transform.SetParent(PointCloudParentGO.transform);
                MaxValueSphere.SetActive(true);
            }

            if (MinValueSphere == null)
            {
                MinValueSphere = Instantiate(MainManager.MinValueSpherePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                MinValueSphere.transform.SetParent(PointCloudParentGO.transform);
                MinValueSphere.SetActive(true);
            }

            PrintMaxMinValuesSpheres(ReducedPointCloud[MaxIndex], ReducedPointCloud[MinIndex]);
        }
        else
        {
            MaxValueSphere.SetActive(false);
            MinValueSphere.SetActive(false);
        }
    }

    public void PrintMaxMinValuesSpheres(PointExt maxPoint, PointExt minPoint)
    {
        float _sphereScale = PointCloudRendererPC.pointSize * 10f;

        Matrix4x4 _transMatrix = PointCloudParentGO.transform.localToWorldMatrix.inverse;
        Vector3 _newPoint = _transMatrix.MultiplyPoint3x4(new Vector3(maxPoint.Point.Position.x, maxPoint.Point.Position.y, maxPoint.Point.Position.z));
        MaxValueSphere.transform.localPosition = _newPoint;
        MaxValueSphere.transform.localScale = new Vector3(_sphereScale, _sphereScale, _sphereScale);
        if (!MaxValueSphere.activeSelf) MaxValueSphere.SetActive(true);

        _newPoint = _transMatrix.MultiplyPoint3x4(new Vector3(minPoint.Point.Position.x, minPoint.Point.Position.y, minPoint.Point.Position.z));
        MinValueSphere.transform.localPosition = _newPoint;
        MinValueSphere.transform.localScale = new Vector3(_sphereScale, _sphereScale, _sphereScale);
        if (!MinValueSphere.activeSelf) MinValueSphere.SetActive(true);
    }

    public void SetLerpFactor(float factor)
    {
        LerpFactor = factor;
        MainManager.LerpFactor = LerpFactor;
    }

    #endregion

    #region Octree Managment

    public void CreateOctree(bool option)
    {
        //option: use UpdatedPositionPointCloud or ReducedPointCloud;

        // REVISAR

        PointExt[] _pointsToOctree;

        if (option)
        {
            _pointsToOctree = UpdatedPositionPointCloud;
        }
        else
        {
            _pointsToOctree = ReducedPointCloud;
        }
        float _minNodeSize = VoxelSize / 1000f;

        float _maxx = PCBoundingBox[0, 0];
        float _maxy = PCBoundingBox[1, 0];
        float _maxz = PCBoundingBox[2, 0];
        float _minx = PCBoundingBox[0, 1];
        float _miny = PCBoundingBox[1, 1];
        float _minz = PCBoundingBox[2, 1];

        float _scalar = 0.7f;
        float _xRange = _scalar * Mathf.Abs(_maxx - _minx);
        float _yRange = _scalar * Mathf.Abs(_maxy - _miny);
        float _zRange = _scalar * Mathf.Abs(_maxz - _minz);

        Vector3[] _corners = new Vector3[8];
        _corners[0] = new Vector3(_minx, _miny, _minz);
        _corners[1] = new Vector3(_maxx, _miny, _minz);
        _corners[2] = new Vector3(_maxx, _miny, _maxz);
        _corners[3] = new Vector3(_minx, _miny, _maxz);
        _corners[4] = new Vector3(_minx, _maxy, _minz);
        _corners[5] = new Vector3(_maxx, _maxy, _minz);
        _corners[6] = new Vector3(_maxx, _maxy, _maxz);
        _corners[7] = new Vector3(_minx, _maxy, _maxz);

        Vector3 _meanPoint = Vector3.zero;

        for (int i = 0; i < _corners.Length; i++)
            _meanPoint += _corners[i];
        _meanPoint = _meanPoint / _corners.Length;

        float[] _edges = new float[4];
        _edges[0] = (_corners[1] - _corners[0]).magnitude;
        _edges[1] = (_corners[2] - _corners[1]).magnitude;
        _edges[2] = (_corners[3] - _corners[0]).magnitude;
        _edges[3] = (_corners[4] - _corners[0]).magnitude;

        float _initialWorldSize = _minNodeSize;
        while (_initialWorldSize < _edges.Max())
        {
            _initialWorldSize *= 2;
        }
        Vector3 _initialWorldPos = _meanPoint;

        FinalOctree = new PointOctree<PointExt>(_initialWorldSize, _initialWorldPos, _minNodeSize);

        for (int i = 0; i < _pointsToOctree.Length; i++)
        {
            FinalOctree.Add(_pointsToOctree[i], new Vector3(_pointsToOctree[i].Point.Position.x, _pointsToOctree[i].Point.Position.y, _pointsToOctree[i].Point.Position.z));
        }
    }

    public BoundsOctree<PointExt> CreateBoundOctree(PointExt[] initialPoints, float[,] boundingBox, float loosenessVal, float minNodeSize = 1f)
    {
        minNodeSize /= 1000f;

        float _maxx = boundingBox[0, 0];
        float _maxy = boundingBox[1, 0];
        float _maxz = boundingBox[2, 0];
        float _minx = boundingBox[0, 1];
        float _miny = boundingBox[1, 1];
        float _minz = boundingBox[2, 1];

        Vector3[] _corners = new Vector3[8];
        _corners[0] = new Vector3(_minx, _miny, _minz);
        _corners[1] = new Vector3(_maxx, _miny, _minz);
        _corners[2] = new Vector3(_maxx, _miny, _maxz);
        _corners[3] = new Vector3(_minx, _miny, _maxz);
        _corners[4] = new Vector3(_minx, _maxy, _minz);
        _corners[5] = new Vector3(_maxx, _maxy, _minz);
        _corners[6] = new Vector3(_maxx, _maxy, _maxz);
        _corners[7] = new Vector3(_minx, _maxy, _maxz);

        Vector3 _meanPoint = Vector3.zero;
        for (int i = 0; i < _corners.Length; i++)
            _meanPoint += _corners[i];
        _meanPoint = _meanPoint / _corners.Length;

        float[] edges = new float[4];
        edges[0] = (_corners[1] - _corners[0]).magnitude;
        edges[1] = (_corners[2] - _corners[1]).magnitude;
        edges[2] = (_corners[3] - _corners[0]).magnitude;
        edges[3] = (_corners[4] - _corners[0]).magnitude;

        float initialWorldSize = minNodeSize;
        while (initialWorldSize < edges.Max())
        {
            initialWorldSize *= 2;
        }
        Vector3 initialWorldPos = _meanPoint;

        BoundsOctree<PointExt> boundOctree_int = new BoundsOctree<PointExt>(initialWorldSize, initialWorldPos, minNodeSize, loosenessVal);

        for (int i = 0; i < initialPoints.Length; i++)
        {
            boundOctree_int.Add(initialPoints[i], new Bounds(initialPoints[i].Point.Position, new Vector3(1, 1, 1)));
        }

        Debug.Log("OCTREE BOUNDS FILLED");

        return boundOctree_int;
    }

    public PointExt InterOctree(Ray ray, float maxdist, Transform referenPos)
    {

        PointExt[] _closePoints = FinalOctree.GetNearby(ray, maxdist);
        PointExt _closest = ClosestPoint(referenPos, _closePoints);
        return _closest;
    }

    public PointExt ClosestPoint(Transform referencePosition, PointExt[] pointsGroup)
    {
        PointExt _closest;
        int _index = 0;
        float _inicialDistance = float.MaxValue;
        for (int i = 0; i < pointsGroup.Length; i++)
        {
            Vector3 _point = new Vector3(pointsGroup[i].Point.Position.x, pointsGroup[i].Point.Position.y, pointsGroup[i].Point.Position.z);

            Vector3 referenceRelative = referencePosition.InverseTransformPoint(_point);

            if (referenceRelative.z < 0)
            {
                Debug.Log("CameraRelative.z: " + referenceRelative.z);
                continue;
            }

            float _distance = Vector3.Magnitude(_point - referencePosition.position);
            if (_distance < _inicialDistance)
            {
                _index = i;
                _inicialDistance = _distance;
            }

        }
        _closest = pointsGroup[_index];

        return _closest;
    }

    #endregion

    #region Spatial Info

    public PointExt[] FillPointArray()
    {
        PointExt[] _points = new PointExt[countdata / numfloat];

        for (int i = 0; i < _points.Length; i++)
        {
            _points[i].prop_package = new float[numproperties];
            _points[i].Point.Position.x = databuffer[i * numfloat + 0];
            _points[i].Point.Position.y = databuffer[i * numfloat + 1];
            _points[i].Point.Position.z = databuffer[i * numfloat + 2];
            byte[] floatbytes = BitConverter.GetBytes(databuffer[i * numfloat + 3]);
            Color32 pointcolor = new Color32(floatbytes[0], floatbytes[1], floatbytes[2], floatbytes[3]);
            _points[i].Point.Color = pointcolor;
            if (importNormal)
            {
                _points[i].Normal.x = databuffer[i * numfloat + 4];
                _points[i].Normal.y = databuffer[i * numfloat + 5];
                _points[i].Normal.z = databuffer[i * numfloat + 6];
            }
            for (int indu = 0; indu < numproperties; indu++)
            {
                _points[i].prop_package[indu] = databuffer[i * numfloat + 7 + factImportNormal + indu];
            }
        }

        return _points;
    }

    public List<PointCloudSliceClass> Slice(bool usePoints = true)
    {
        float[] _divs = new float[3];

        if (usePoints)
        {
            Size.x = Mathf.Abs(PCBoundingBox[0, 0] - PCBoundingBox[0, 1]);
            Size.y = Mathf.Abs(PCBoundingBox[1, 0] - PCBoundingBox[1, 1]);
            Size.z = Mathf.Abs(PCBoundingBox[2, 0] - PCBoundingBox[2, 1]);

            Centroid = new Vector3(PCBoundingBox[0, 0] - Size.x / 2, PCBoundingBox[1, 0] - Size.y / 2, PCBoundingBox[2, 0] - Size.z / 2);
        }
        else
        {
            Size = BoxCollider.size;

            Centroid = BoxCollider.center;
        }

        // Ternary condiciontal operator (c = a >= 100 ? b : c / 10;)
        _divs[0] = Mathf.Round(Size.x / VoxelReference.x) >= 2 ? Mathf.Round(Size.x / VoxelReference.x) : 2;
        _divs[1] = Mathf.Round(Size.y / VoxelReference.y) >= 2 ? Mathf.Round(Size.y / VoxelReference.y) : 2;
        _divs[2] = Mathf.Round(Size.z / VoxelReference.z) >= 2 ? Mathf.Round(Size.z / VoxelReference.z) : 2;

        int _voxelCount = 0;

        Vector3 _voxelCentroid;

        for (int xx = 0; xx < _divs[0]; xx++)
        {
            _voxelCentroid.x = (Centroid.x + Size.x / 2) - VoxelReference.x / 2 * (2 * xx + 1);

            for (int zz = 0; zz < _divs[2]; zz++)
            {
                _voxelCentroid.z = (Centroid.z + Size.z / 2) - VoxelReference.z / 2 * (2 * zz + 1);

                for (int yy = 0; yy < _divs[1]; yy++)
                {
                    _voxelCentroid.y = (Centroid.y + Size.y / 2) - VoxelReference.y / 2 * (2 * yy + 1);

                    _voxelCount++;

                    Vector3[] _pcSliceBoundingBox = CalculateCorners(VoxelReference, _voxelCentroid);

                    PointCloudSliceClass _newPointCloudSlice = new PointCloudSliceClass(_voxelCentroid, Size, _pcSliceBoundingBox);

                    PointCloudSlices.Add(_newPointCloudSlice);
                }
            }
        }

        return PointCloudSlices;
    }

    public float[,] UpdateTransformPCBoundingBox(float[,] pcBoundingBoxOld)
    {
        float[,] pcBoundingBoxNew = new float[3, 2];

        Vector3 _maxPointOld = new Vector3(pcBoundingBoxOld[0, 0], pcBoundingBoxOld[1, 0], pcBoundingBoxOld[2, 0]);
        Vector3 _minPointOld = new Vector3(pcBoundingBoxOld[0, 1], pcBoundingBoxOld[1, 1], pcBoundingBoxOld[2, 1]);

        Vector3 _maxPointNew;
        Vector3 _minPointNew;

        Matrix4x4 matriztrans = PointCloudGO.transform.localToWorldMatrix;
        _maxPointNew = matriztrans.MultiplyPoint3x4(_maxPointOld);
        _minPointNew = matriztrans.MultiplyPoint3x4(_minPointOld);

        pcBoundingBoxNew[0, 0] = _maxPointNew.x;
        pcBoundingBoxNew[1, 0] = _maxPointNew.y;
        pcBoundingBoxNew[2, 0] = _maxPointNew.z;
        pcBoundingBoxNew[0, 1] = _minPointNew.x;
        pcBoundingBoxNew[1, 1] = _minPointNew.y;
        pcBoundingBoxNew[2, 1] = _minPointNew.z;

        return pcBoundingBoxNew;
    }

    float[,] CalculatePCBoundingBox(bool usePoints = true)
    {
        float[,] _newBoundingBox = new float[3, 2];

        if (usePoints)
        {
            _newBoundingBox[0, 0] = float.MinValue;
            _newBoundingBox[0, 1] = float.MaxValue;
            _newBoundingBox[1, 0] = float.MinValue;
            _newBoundingBox[1, 1] = float.MaxValue;
            _newBoundingBox[2, 0] = float.MinValue;
            _newBoundingBox[2, 1] = float.MaxValue;

            for (int i = 0; i < Points.Length; i++)
            {
                Vector3 _point = Points[i].Point.Position;

                if (_newBoundingBox[0, 0] < _point.x) _newBoundingBox[0, 0] = _point.x;
                if (_newBoundingBox[0, 1] > _point.x) _newBoundingBox[0, 1] = _point.x;
                if (_newBoundingBox[1, 0] < _point.y) _newBoundingBox[1, 0] = _point.y;
                if (_newBoundingBox[1, 1] > _point.y) _newBoundingBox[1, 1] = _point.y;
                if (_newBoundingBox[2, 0] < _point.z) _newBoundingBox[2, 0] = _point.z;
                if (_newBoundingBox[2, 1] > _point.z) _newBoundingBox[2, 1] = _point.z;

            }
        }
        else
        {
            _newBoundingBox[0, 0] = BoxCollider.center.x + BoxCollider.size.x / 2;
            _newBoundingBox[0, 1] = BoxCollider.center.x - BoxCollider.size.x / 2;
            _newBoundingBox[1, 0] = BoxCollider.center.y + BoxCollider.size.y / 2;
            _newBoundingBox[1, 1] = BoxCollider.center.y - BoxCollider.size.y / 2;
            _newBoundingBox[2, 0] = BoxCollider.center.z + BoxCollider.size.z / 2;
            _newBoundingBox[2, 1] = BoxCollider.center.z - BoxCollider.size.z / 2;
        }

        return _newBoundingBox;
    }

    public float[,] CalculatePCBoundingBox(Vector3 point, float[,] newBoundingBox)
    {
        if (newBoundingBox[0, 0] < point.x) newBoundingBox[0, 0] = point.x;
        if (newBoundingBox[0, 1] > point.x) newBoundingBox[0, 1] = point.x;
        if (newBoundingBox[1, 0] < point.y) newBoundingBox[1, 0] = point.y;
        if (newBoundingBox[1, 1] > point.y) newBoundingBox[1, 1] = point.y;
        if (newBoundingBox[2, 0] < point.z) newBoundingBox[2, 0] = point.z;
        if (newBoundingBox[2, 1] > point.z) newBoundingBox[2, 1] = point.z;

        return newBoundingBox;
    }


    Vector3[] CalculateCorners(Vector3 size, Vector3 centroid)
    {
        Vector3[] _corners = new Vector3[8];

        _corners[0] = new Vector3(centroid.x + size.x / 2, centroid.y + size.y / 2, centroid.z + size.z / 2);
        _corners[1] = new Vector3(centroid.x + size.x / 2, centroid.y + size.y / 2, centroid.z - size.z / 2);
        _corners[2] = new Vector3(centroid.x + size.x / 2, centroid.y - size.y / 2, centroid.z + size.z / 2);
        _corners[3] = new Vector3(centroid.x + size.x / 2, centroid.y - size.y / 2, centroid.z - size.z / 2);
        _corners[4] = new Vector3(centroid.x - size.x / 2, centroid.y + size.y / 2, centroid.z + size.z / 2);
        _corners[5] = new Vector3(centroid.x - size.x / 2, centroid.y + size.y / 2, centroid.z - size.z / 2);
        _corners[6] = new Vector3(centroid.x - size.x / 2, centroid.y - size.y / 2, centroid.z + size.z / 2);
        _corners[7] = new Vector3(centroid.x - size.x / 2, centroid.y - size.y / 2, centroid.z - size.z / 2);

        return _corners;
    }

    public int[] ObtainIndexPerVoxel(Vector3[] corners)
    {
        List<int> _indexPerVoxel = new List<int>();

        Bounds _bounds = GeometryUtility.CalculateBounds(corners, Matrix4x4.identity);

        for (int i = 0; i < Points.Length; i++)
        {
            if (IsIn(_bounds, Points[i])) _indexPerVoxel.Add(i);
        }

        return _indexPerVoxel.ToArray();
    }

    public void FillVoxels()
    {
        int[] _pointsArray;

        for (int i = 0; i < PointCloudSlices.Count; i++)
        {
            _pointsArray = ObtainIndexPerVoxel(PointCloudSlices[i].PCSliceBoundingBox);

            PointCloudSlices[i].PointsPerSlice = _pointsArray;
        }
    }

    public void UpdateVoxelsInfo(PointExt[] newPointList)
    {
        ReceivePointList(newPointList);

        FillVoxels();
    }

    public bool IsIn(Bounds bounds, PointExt punto)
    {
        bool _isIn = bounds.Contains(punto.Point.Position);

        return _isIn;
    }

    #endregion

    #region Frustum Management

    public void ObtainCullingPointCloudIndex()
    {
        List<int> _indexFrustrum_list = new List<int>();
        Plane[] _planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        for (int i = 0; i < ReducedPointCloud.Length; i++)
        {
            if (IsInside(_planes, ReducedPointCloud[i])) _indexFrustrum_list.Add(i);
        }

        indexFrustum = _indexFrustrum_list.ToArray();
    }

    public PointExt[] ObtainCullingPointCloudIndexFromBounds()
    {

        List<PointExt> _indexFrustrum_list = new List<PointExt>();
        _indexFrustrum_list = BoundOctree.GetWithinFrustum(Camera.main);

        return _indexFrustrum_list.ToArray();
    }

    public int[] ObtainCullingPointCloudIndexFromVoxel()
    {
        List<int> _indexFrustrum_list = new List<int>();
        Plane[] _planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        for (int i = 0; i < PointCloudSlices.Count; i++)
        {
            if (IsInside(_planes, PointCloudSlices[i].Centroid))
            {
                PointCloudSlices[i].isBeingSeen = true;

                for (int ii = 0; ii < PointCloudSlices[i].PointsPerSlice.Length; ii++)
                {
                    _indexFrustrum_list.Add(PointCloudSlices[i].PointsPerSlice[ii]);
                }
            }
            else
            {
                PointCloudSlices[i].isBeingSeen = false;
            }
        }

        return _indexFrustrum_list.ToArray();
    }

    public void RenderFrustum()
    {
        PointsToRender = new Point[indexFrustum.Length];
        PointsToRender = UpdateTransform(TotalPointsToRender, indexFrustum);
        PointCloudRendererPC.sourceBuffer = new ComputeBuffer(PointsToRender.Length, sizeof(float) * 4);
        PointCloudRendererPC.sourceBuffer.SetData(PointsToRender);
    }

    private bool IsInside(Plane[] planes, PointExt point)
    {
        bool _isInside = true;
        for (int i = 0; i < planes.Length; ++i)
        {
            if (!planes[i].GetSide(point.Point.Position))
            {
                _isInside = false;
                return _isInside;
            }

            //Checking implemented by using octree bounds, but works slower
            /*Bounds bound = new Bounds(punto.puntoNube.position, new Vector3(0.01f, 0.01f, 0.01f));
            if(!GeometryUtility.TestPlanesAABB(planes, bound))
            {
                sidentro = false;
                return sidentro;
            }*/

        }

        return _isInside;
    }

    private bool IsInside(Plane[] planes, Vector3 punto)
    {
        bool _isInside = true;
        for (int i = 0; i < planes.Length; ++i)
        {
            if (!planes[i].GetSide(punto))
            {
                _isInside = false;
                return _isInside;
            }
        }

        return _isInside;
    }

    #endregion


    #region Utilities

    void OnDrawGizmos()
    {

        if (FinalOctree != null)
        {
            FinalOctree.DrawAllBounds(); // Draw node boundaries

            //BoundOctree.DrawAllBounds();
        }
    }

    int[] FillArray(int size)
    {
        int[] _arr = new int[size];

        for (int i = 0; i < size; ++i)
        {
            _arr[i] = i;
        }

        return _arr;
    }

    public void FillFrustumArray()
    {
        indexFrustum = new int[TotalPointsToRender.Length];

        for (int i = 0; i < TotalPointsToRender.Length; ++i)
        {
            indexFrustum[i] = i;
        }

        FrustumFilled = true;
    }

    public void ReceivePointList(PointExt[] newPointList)
    {
        Points = new PointExt[newPointList.Length];
        Points = newPointList;
    }

    private Vector3 GetMeanVector(Vector3[] positions)
    {
        if (positions.Length == 0)
            return Vector3.zero;
        float x = 0f;
        float y = 0f;
        float z = 0f;
        foreach (Vector3 pos in positions)
        {
            x += pos.x;
            y += pos.y;
            z += pos.z;
        }
        return new Vector3(x / positions.Length, y / positions.Length, z / positions.Length);
    }

    #endregion
}

