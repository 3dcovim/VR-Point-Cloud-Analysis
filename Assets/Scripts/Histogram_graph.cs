using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class Histogram_graph : MonoBehaviour
{
    public MainManager MainManager;

    private RectTransform _graphContainer;
    private RectTransform _labelTemplateX;
    private RectTransform _labelTemplateY;
    private RectTransform _dashTemplateX;
    private RectTransform _dashTemplateY;

    List<GameObject> _gameObjectList = new List<GameObject>();
    List<RectTransform> _childrenRectTransforms = new List<RectTransform>();

    private void Awake()
    {
        _graphContainer = GameObject.Find("graphContainer").GetComponent<RectTransform>();
        _labelTemplateX = _graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        _labelTemplateY = _graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        _dashTemplateX = _graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        _dashTemplateY = _graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();

        if (this.isActiveAndEnabled) this.gameObject.SetActive(false);
    }

    public void ShowGraph(int[] valueList, float[] getAxisLabelX = null, int[] getAxisLabelY = null)
    {
        //stackoverflow.com/questions/3624731/what-is-func-how-and-when-is-it-used
        //if (getAxisLabelX == null)
        //{
        //    getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        //}
        //if (getAxisLabelY == null)
        //{
        //    getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        //}

        ClearWindowGraph();

        float _graphHeight = _graphContainer.sizeDelta.y;
        float _yMaximun = float.MinValue;  //Max value for the graphic
        float _yMinimum = float.MaxValue;  //Min value for the graphic

        foreach (float value in valueList)
        {
            if (value > _yMaximun)
            {
                _yMaximun = value;
            }
            if (value < _yMinimum)
            {
                _yMinimum = value;
            }

        }
        _yMaximun = _yMaximun + ((_yMaximun - _yMinimum) * 0.2f);
        _yMinimum = 0f;

        float xSize = 30f;  //Space between values
        for (int i = 0; i <= MainManager.PointCloud.HistogramSeeds; i++)
        {
            float _xPosition = xSize / 2 + i * xSize;

            if (i < MainManager.PointCloud.HistogramSeeds)
            {
                float _yPosition = ((valueList[i] - _yMinimum) / (_yMaximun - _yMinimum)) * _graphHeight;

                GameObject _barGameObject = CreateBar(new Vector2(_xPosition + xSize / 2, _yPosition), xSize * 0.6f, valueList[i]);
                _gameObjectList.Add(_barGameObject);
            }

            RectTransform _labelX = Instantiate(_labelTemplateX, _graphContainer.transform);
            _labelX.gameObject.SetActive(true);
            _labelX.anchoredPosition = new Vector2(_xPosition, -10f);
            _labelX.GetComponent<Text>().text = getAxisLabelX[i].ToString("N1");
            _gameObjectList.Add(_labelX.gameObject);

            RectTransform _dashX = Instantiate(_dashTemplateX, _graphContainer.transform);
            _dashX.gameObject.SetActive(true);
            _dashX.anchoredPosition = new Vector2(_xPosition, 0);
            _gameObjectList.Add(_dashX.gameObject);
        }

        for (int i = 0; i < MainManager.PointCloud.HistogramSeeds; i++)
        {
            RectTransform _labelY = Instantiate(_labelTemplateY, _graphContainer.transform);
            _labelY.gameObject.SetActive(true);
            float normalizedValue = i * 1f / MainManager.PointCloud.HistogramSeeds;
            _labelY.anchoredPosition = new Vector2(-5f, normalizedValue * _graphHeight);
            _labelY.GetComponent<Text>().text = Mathf.RoundToInt(_yMinimum + (normalizedValue * (_yMaximun - _yMinimum))).ToString();
            _gameObjectList.Add(_labelY.gameObject);

            RectTransform _dashY = Instantiate(_dashTemplateY, _graphContainer.transform);
            _dashY.gameObject.SetActive(true);
            _dashY.anchoredPosition = new Vector2(0, normalizedValue * _graphHeight);
            _gameObjectList.Add(_dashY.gameObject);
        }

        ChangeChildrenOrder();
    }

    private GameObject CreateBar(Vector2 graphPosition, float barWidth, int points)
    {
        GameObject _gameObject = new GameObject("bar", typeof(Image));
        _gameObject.transform.SetParent(_graphContainer, false);

        RectTransform _rectTransform = _gameObject.GetComponent<RectTransform>();
        _rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0f);
        _rectTransform.sizeDelta = new Vector2(barWidth, graphPosition.y);
        _rectTransform.anchorMin = new Vector2(0, 0);
        _rectTransform.anchorMax = new Vector2(0, 0);
        _rectTransform.pivot = new Vector2(0.5f, 0f);

        HistogramBar _barScript = _gameObject.AddComponent<HistogramBar>();
        _barScript.enabled = true;

        _barScript.PointsCount = points;

        return _gameObject;
    }

    private void ChangeChildrenOrder()
    {
        int _index = 1;
        for (int i = 0; i < _childrenRectTransforms.Count; i++)
        {
            if (_childrenRectTransforms[i].name == "dot")
            {
                _childrenRectTransforms[i].SetSiblingIndex(_index);
                _index++;
            }
        }
    }

    public void ClearWindowGraph()
    {
        foreach (GameObject gameObject in _gameObjectList)
        {
            Destroy(gameObject);
        }
        foreach (RectTransform rectTransform in _childrenRectTransforms)
        {
            Destroy(rectTransform);
        }
        _childrenRectTransforms.Clear();
    }

    public void ActivateHistogramGraph()
    {
        this.gameObject.SetActive(!this.gameObject.activeInHierarchy);
    }

    public void OnEnable()
    {
        List<float> _valueList = new List<float>();


        for (int i = 0; i < MainManager.PointCloud.ReducedPointCloud.Length; i++)  //AGO23
        {
            _valueList.Add(MainManager.PointCloud.ReducedPointCloud[i].prop_package[MainManager.Property]);  //AGO23
        }



/*        switch (PointCloudManager.Property)     //AGO23
            {
                case 0:
                    for (int i = 0; i < PointCloudManager.ReducedPointCloud.Length; i++)
                    {
                        _valueList.Add(PointCloudManager.ReducedPointCloud[i].Prop1);
                    }
                    break;
                case 1:
                    for (int i = 0; i < PointCloudManager.ReducedPointCloud.Length; i++)
                    {
                        _valueList.Add(PointCloudManager.ReducedPointCloud[i].Prop2);
                    }
                    break;
                case 2:
                    for (int i = 0; i < PointCloudManager.ReducedPointCloud.Length; i++)
                    {
                        _valueList.Add(PointCloudManager.ReducedPointCloud[i].Prop3);
                    }
                    break;
                case 3:
                    for (int i = 0; i < PointCloudManager.ReducedPointCloud.Length; i++)
                    {
                        _valueList.Add(PointCloudManager.ReducedPointCloud[i].Prop4);
                    }
                    break;
                case 4:
                for (int i = 0; i < PointCloudManager.ReducedPointCloud.Length; i++)
                    {
                        _valueList.Add(PointCloudManager.ReducedPointCloud[i].Prop5);
                    }
                    break;
                case 5:
                    for (int i = 0; i < PointCloudManager.ReducedPointCloud.Length; i++)
                    {
                        _valueList.Add(PointCloudManager.ReducedPointCloud[i].Prop6);
                    }
                    break;
            }*/  //AGO23

        float[] _intervalValues = CalculateHistogramInterval(_valueList, MainManager.PointCloud.HistogramSeeds);
        int[] _pointsPerSeed = CalculeHistogramValues(_valueList, MainManager.PointCloud.HistogramSeeds, _intervalValues);

        ShowGraph(_pointsPerSeed, _intervalValues, _pointsPerSeed);
    }

    int[] CalculeHistogramValues(List<float> propertyValues, int seedCount, float[] intervalValues)
    {
        int[] _values = new int[] { 0, 0, 0, 0, 0, 0, 0, 0};

        for (int i = 0; i < propertyValues.Count; i++)
        {
            if (intervalValues[0] <= propertyValues[i] && propertyValues[i] < intervalValues[1])
                _values[0]++;
            else if (intervalValues[1] <= propertyValues[i] && propertyValues[i] < intervalValues[2])
                _values[1]++;
            else if (intervalValues[2] <= propertyValues[i] && propertyValues[i] < intervalValues[3])
                _values[2]++;
            else if (intervalValues[3] <= propertyValues[i] && propertyValues[i] < intervalValues[4])
                _values[3]++;
            else if (intervalValues[4] <= propertyValues[i] && propertyValues[i] < intervalValues[5])
                _values[4]++;
            else if (intervalValues[5] <= propertyValues[i] && propertyValues[i] < intervalValues[6])
                _values[5]++;
            else if (intervalValues[6] <= propertyValues[i] && propertyValues[i] < intervalValues[7])
                _values[6]++;
            else if (intervalValues[7] <= propertyValues[i] && propertyValues[i] <= intervalValues[8])
                _values[7]++;
        }

        return _values;
    }

    float[] CalculateHistogramInterval(List<float> propertyValues, int seedCount)
    {
        float _minPropertyValue = propertyValues.Min();
        float _maxPropertyValue = propertyValues.Max();

        float _interSpace = (_maxPropertyValue - _minPropertyValue) / seedCount;

        float[] _intervalValues = new float[seedCount + 1];
        for(int i = 0; i <= seedCount; i++)
        {
            _intervalValues[i] = _interSpace * i + _minPropertyValue;
        }

        return _intervalValues;
    }
}
