using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_graph : MonoBehaviour
{
    public MainManager MainManager;

    [SerializeField] private Sprite _circleSprite;
    [SerializeField] private Sprite _circleSpriteCurrent;
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

    private GameObject CreateCircle(Vector2 anchorPosition, bool isCurrent)
    {
        GameObject _gameObject = new GameObject("circle", typeof(Image));
        _gameObject.transform.SetParent(_graphContainer, false);

        if(isCurrent) _gameObject.GetComponent<Image>().sprite = _circleSpriteCurrent;
        else _gameObject.GetComponent<Image>().sprite = _circleSprite;

        RectTransform _rectTransform = _gameObject.GetComponent<RectTransform>();
        _rectTransform.anchoredPosition = anchorPosition;
        _rectTransform.sizeDelta = new Vector2(11, 11);
        _rectTransform.anchorMin = new Vector2(0, 0);
        _rectTransform.anchorMax = new Vector2(0, 0);

        return _gameObject;
    }

    public void ShowGraph(List<float> valueList, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        //stackoverflow.com/questions/3624731/what-is-func-how-and-when-is-it-used
        if (getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }
        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        ClearWindowGraph();

        GameObject _lastCircleGO = null;

        float _graphHeight = _graphContainer.sizeDelta.y;
        float _yMaximun = float.MinValue;  //Max value for the graphic
        float _yMinimum = float.MaxValue;  //Min value for the graphic

        foreach (float value in valueList)
        {
            if(value > _yMaximun)
            {
                _yMaximun = value;
            }
            if (value < _yMinimum)
            {
                _yMinimum = value;
            }

        }
        _yMaximun = _yMaximun + ((_yMaximun - _yMinimum) * 0.2f);
        _yMinimum = _yMinimum - ((_yMaximun - _yMinimum) * 0.2f);

        float xSize = 50f;  //Space between values
        for(int i = 0; i < valueList.Count;  i++)
        {
            float _xPosition = xSize + i * xSize;
            float _yPosition = ((valueList[i] - _yMinimum) / (_yMaximun - _yMinimum)) * _graphHeight;

            GameObject _circleGO;

            if (i == MainManager.Property) _circleGO = CreateCircle(new Vector2(_xPosition, _yPosition), true);
            else _circleGO = CreateCircle(new Vector2(_xPosition, _yPosition), false);

            _gameObjectList.Add(_circleGO);
            _childrenRectTransforms.Add(_circleGO.GetComponent<RectTransform>());

            if (_lastCircleGO != null)
            {
                GameObject _dotConnectionGO = CreateDotConnection(_lastCircleGO.GetComponent<RectTransform>().anchoredPosition,
                    _circleGO.GetComponent<RectTransform>().anchoredPosition);
                _gameObjectList.Add(_dotConnectionGO);
            }
            _lastCircleGO = _circleGO;

            RectTransform _labelX = Instantiate(_labelTemplateX, _graphContainer.transform);
            _labelX.gameObject.SetActive(true);
            _labelX.anchoredPosition = new Vector2(_xPosition, -10f);
            _labelX.GetComponent<Text>().text = getAxisLabelX(i);
            _gameObjectList.Add(_labelX.gameObject);

            RectTransform _dashX = Instantiate(_dashTemplateX, _graphContainer.transform);
            _dashX.gameObject.SetActive(true);
            _dashX.anchoredPosition = new Vector2(_xPosition, 0);
            _gameObjectList.Add(_dashX.gameObject);
        }

        int _separatorCount = 10;
        for(int i = 0; i <= _separatorCount; i++)
        {
            RectTransform _labelY = Instantiate(_labelTemplateY, _graphContainer.transform);
            _labelY.gameObject.SetActive(true);
            float normalizedValue = i * 1f / _separatorCount;
            _labelY.anchoredPosition = new Vector2(-5f, normalizedValue * _graphHeight);
            _labelY.GetComponent<Text>().text = getAxisLabelY(_yMinimum + (normalizedValue * (_yMaximun - _yMinimum)));
            _gameObjectList.Add(_labelY.gameObject);

            RectTransform _dashY = Instantiate(_dashTemplateY, _graphContainer.transform);
            _dashY.gameObject.SetActive(true);
            _dashY.anchoredPosition = new Vector2(0, normalizedValue * _graphHeight);
            _gameObjectList.Add(_dashY.gameObject);
        }

        ChangeChildrenOrder();
    }

    private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject _dotConnectionGO = new GameObject("dotConnection", typeof(Image));
        _dotConnectionGO.transform.SetParent(_graphContainer, false);
        _dotConnectionGO.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        RectTransform _rectTransform = _dotConnectionGO.GetComponent<RectTransform>();
        Vector2 _dir = (dotPositionB - dotPositionA).normalized;
        float _distance = Vector2.Distance(dotPositionA, dotPositionB);
        _rectTransform.sizeDelta = new Vector2(_distance, 3f);
        _rectTransform.anchorMin = new Vector2(0, 0);
        _rectTransform.anchorMax = new Vector2(0, 0);
        _rectTransform.anchoredPosition = dotPositionA + _dir * _distance * 0.5f;
        _rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(_dir));

        _childrenRectTransforms.Add(_dotConnectionGO.GetComponent<RectTransform>());

        return _dotConnectionGO;
    }

    private float GetAngleFromVectorFloat(Vector2 dir)
    {
        dir = dir.normalized;
        float _n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (_n < 0) _n += 360;
        return _n;
    }

    private void ChangeChildrenOrder()
    {
        int _index = 1;
        for (int i = 0; i < _childrenRectTransforms.Count; i++)
        {
            if (_childrenRectTransforms[i].name == "circle")
            {
                _childrenRectTransforms[i].SetSiblingIndex(_index);
                _index++;
            }
        }
    }

    public void UpdateCircles(int propertyCount, int currentProperty)
    {
        for(int i = 0; i < propertyCount; i++)
        {
            if (i == currentProperty) _gameObjectList[i].GetComponent<Image>().sprite = _circleSpriteCurrent;
            else _gameObjectList[i].GetComponent<Image>().sprite = _circleSprite;
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
}
