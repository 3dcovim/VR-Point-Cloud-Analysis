using Pcx;
using System;
using TMPro;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject MaxValueSpherePrefab;
    public GameObject MinValueSpherePrefab;

    // Game Objects and Scripts
    public MenuManager MenuManager;
    public MainMenu MainMenu;
    public ConfigurationMenu ConfigurationMenu;
    public Histogram_graph HistogramGraph;
    public Window_graph WindowGraph;
    public TMP_Text PropertyTextCanvas;

    [HideInInspector]
    public GameObject[] PointCloudParentArrayGO;
    [HideInInspector]
    public GameObject[] PointCloudArrayGO;
    [HideInInspector]
    public PointCloudRenderer PointCloudRendererPC;
    [HideInInspector]
    public StateManager StateManager;
    [HideInInspector]
    public PointCloud PointCloud;

    //Parameters
    public int SelectedPCindex = 0;
    public int Property;        //Current property from the cloud
    public int SelectProperty; //Test changing the property using the inspector, without HMD
    public bool UseFrustum = false;
    public bool UsePoints = true;

    //Private Parameters
    private bool updateProperty = false;
    private bool justOnce = false;

    //Voxel Size
    private float _voxelSize = 250f;
    public float MinVoxelSize = 150f;
    public float MaxVoxelSize = 550f;
    public float VoxelSize
    {
        get { return _voxelSize; }
        set
        {
            if (value >= MinVoxelSize && value <= MaxVoxelSize)
            {
                _voxelSize = value;
            }
        }
    }

    // Lerp Factor
    [SerializeField] public float leapfactortemporal = 0;   //Test de leapfactor using the inspector, without using HMD
    private float _lerpFactor = 0.5f;
    private float _lerpFactor_ant = 0.5f;
    private float _lerpFactor_max = 1f;
    private float _lerpFactor_min = 0f;
    public float LerpFactor
    {
        get { return _lerpFactor; }
        set
        {
            if (value >= _lerpFactor_min && value <= _lerpFactor_max)
            {
                _lerpFactor = value;
            }
        }
    }

    //Max Scale Value
    private float _meanscale = 0;
    private float _maxscale = 40;
    private float _maxscale_ant;
    public float MaxScale
    {
        get { return _maxscale; }
        set
        {
            if (value >= _minscale)
            {
                _maxscale = value;
            }
        }
    }

    //Min Scale Value
    private float _minscale = 25;
    private float _minscale_ant;
    public float MinScale
    {
        get { return _minscale; }
        set
        {
            if (value <= _maxscale)
            {
                _minscale = value;
            }
        }
    }

    void Awake()
    {
        // ================== SINGLETON ===================================
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        // ================================================================
    }

    // Start is called before the first frame update
    void Start()
    {
        PointCloudParentArrayGO = GameObject.FindGameObjectsWithTag("StateManager");
        PointCloudArrayGO = GameObject.FindGameObjectsWithTag("Point Cloud");

        PointCloud = PointCloudArrayGO[SelectedPCindex].GetComponent<PointCloud>();

        PointCloudRendererPC = PointCloudArrayGO[SelectedPCindex].GetComponent<PointCloudRenderer>();

        StateManager = PointCloudParentArrayGO[SelectedPCindex].GetComponent<StateManager>();

        Property = PointCloud.Property;
        SelectProperty = Property;
    }

    // Update is called once per frame
    void Update()
    {

        if ((_maxscale_ant != _maxscale) || (_minscale_ant != _minscale) && updateProperty)
        {
            CallSwitchProperty(Property);
            _maxscale_ant = _maxscale;
            _minscale_ant = _minscale;
        }
        if (_lerpFactor_ant != LerpFactor)
        {
            CallSwitchProperty(Property);
            _lerpFactor_ant = LerpFactor;
        }
        if (!updateProperty)
        {
            updateProperty = true;
        }
    }

    void LateUpdate()
    {
        if (UseFrustum && Camera.main.gameObject.transform.hasChanged && StateManager.State == State.HighScale)
        {
            SetPointCloud(false, false, false);
        }
        else if (StateManager.HasChanged)
        {
            SetPointCloud(false, false, false);
            StateManager.HasChanged = false;
        }
    }

    public void SetPointCloud(bool includeNaN, bool reduce, bool hasBeenMoved)
    {
        if (reduce || hasBeenMoved)
        {
            PointCloud.ReducePointCloud(VoxelSize, includeNaN);

            PointCloud.UpdateTransformBackward();

            PointCloud.CreateOctree(false);

            PointCloud.SwitchOriginalProperty(Property);

            ConfigurationMenu.UpdatePointCount();

            justOnce = true;
        }

        if (StateManager.State == State.HighScale && UseFrustum)
        {
            PointCloud.ObtainCullingPointCloudIndex();
            justOnce = true;
        }
        else if (justOnce)
        {
            PointCloud.FillFrustumArray();
            justOnce = false;
        }

        if (PointCloud.FrustumFilled)
        {
            PointCloud.RenderFrustum();
        }
    }

    public void CallUpdateTransformBackward()
    {
        PointCloud.UpdateTransformBackward();
    }
    
    public void CallUpdateTransforForward(bool usePoints)
    {
        PointCloud.UpdateTransformFordward(usePoints);
    }

    public void CallCreateOctree(int PCindex, bool option)
    {
        PointCloud.CreateOctree(option);
    }

    public void CallSwitchOriginalProperty(int PCindex, int property)
    {
        PointCloud.SwitchOriginalProperty(property);
    }

    public void CallSwitchProperty(int property)
    {
        Property = property;

        PointCloud.SwitchProperty(property);

        ConfigurationMenu.UpdateMaxScale(PointCloud.MaxScale);
        ConfigurationMenu.UpdateMinScale(PointCloud.MinScale);

        if (property == PointCloud.PropertiesCount)
        {
            MenuManager.ShowHistogramButton.interactable = false;
            MenuManager.RGBmixedSlider.interactable = false;
            MenuManager.SwitchPrintToggle.interactable = false;
        }
        else
        {
            MenuManager.ShowHistogramButton.interactable = true;
            MenuManager.RGBmixedSlider.interactable = true;
            MenuManager.SwitchPrintToggle.interactable = true;
        }

        updateProperty = false;
    }

    public void CallSwitchPrint()
    {
        PointCloud.SwitchPrint();
    }

    public void CallChangeState()
    {

        int _notSelectedPC = (SelectedPCindex == 0) ? 1 : 0;

        if (PointCloudParentArrayGO[_notSelectedPC].GetComponent<StateManager>().State == State.LowScale)
        {
            PointCloudParentArrayGO[_notSelectedPC].GetComponent<StateManager>().ChangeState();
        }

        StateManager.ChangeState();
    }

    public void UpdateInterfaceAfterPC()
    {
        WindowGraph.GetComponent<Window_graph>().ClearWindowGraph();
        HistogramGraph.GetComponent<Histogram_graph>().ClearWindowGraph();
        PropertyTextCanvas.GetComponentInChildren<TMP_Text>().text = "";
        if (HistogramGraph.gameObject.activeInHierarchy) HistogramGraph.GetComponent<Histogram_graph>().OnEnable();
    }

    public void SwitchPointCloud(int _newIndex)
    {
        SelectedPCindex = _newIndex;

        PointCloud = PointCloudArrayGO[SelectedPCindex].GetComponent<PointCloud>();
        StateManager = PointCloudParentArrayGO[SelectedPCindex].GetComponent<StateManager>();
        PointCloudRendererPC = PointCloudArrayGO[SelectedPCindex].GetComponent<PointCloudRenderer>();

        Property = PointCloud.Property;

        MenuManager.PointCloudRenderer = PointCloudRendererPC;
        MenuManager.UpdateDropdownProperties();
    }

    public void SwitchGlobalRanges(int PCindex)
    {
        bool _globalRangesPC = PointCloud.GlobalRanges;
        PointCloud.GlobalRanges = !_globalRangesPC;
    }

    public void SetVoxelSize(float voxelSize)
    {
        VoxelSize = voxelSize;
        PointCloud.VoxelSize = voxelSize;
    }
}
