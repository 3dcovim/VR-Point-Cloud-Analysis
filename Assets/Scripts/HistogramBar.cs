using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class HistogramBar : MonoBehaviour
{
    private GameObject _pointsText;
    private GameObject _childTextGO;
    private GameObject _childTextButtonGO;
    private Button _childTextButton;
    private TMP_Text _childTextText;
    private RectTransform _childTagTransform;

    public int PointsCount;

    // Start is called before the first frame update
    void Awake()
    {
        _pointsText = Instantiate(new GameObject("PointTag"), this.gameObject.transform);
        _pointsText.SetActive(true);

        _childTagTransform = _pointsText.AddComponent<RectTransform>();
        _childTagTransform.sizeDelta = this.gameObject.GetComponent<RectTransform>().sizeDelta;

        _childTextButtonGO = Instantiate(Resources.Load<GameObject>("Button"), _childTagTransform);
        _childTextButtonGO.SetActive(true);

        _childTextButton = _childTextButtonGO.GetComponent<Button>();

        _childTextGO = _childTextButtonGO.transform.GetChild(0).gameObject;
        _childTextGO.transform.localPosition = new Vector3(0, _childTagTransform.sizeDelta.y * 0.6f, -3f);

        _childTextText = _childTextGO.GetComponent<TMP_Text>();
        _childTextText.text = PointsCount.ToString();
        _childTextText.enabled = false;
    }

    void ActivateTag()
    {
        _childTextText.enabled = true;
        _childTextText.text = PointsCount.ToString();
        _childTextText.fontSize = 12f;
    }
    
    void DeactivateTag()
    {
        _childTextText.enabled = false;
    }

    void ManageTag()
    {
        if (_childTextText.enabled) DeactivateTag();
        else ActivateTag();
    }

    private void OnEnable()
    {
        _childTextButton.onClick.AddListener(ManageTag);
    }

    private void OnDisable()
    {
        _childTextButton.onClick.RemoveAllListeners();

    }


}
