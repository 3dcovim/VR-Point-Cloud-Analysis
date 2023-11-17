using Pcx;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Managers")]
    public GameObject PointCloudParentGO;
    public GameObject MenuGO;
    public MainManager MainManager;
    [HideInInspector]
    public PointCloudRenderer PointCloudRenderer;
    [Space(10)]

    [Header("Current Player")]
    public GameObject Player;
    [Space(10)]

    [Header("Components")]
    public TMP_Dropdown PropertyDropdown;
    public TMP_Dropdown PointCloudDropdown;
    public TMP_Text PointSizeText;
    public TMP_Text RGBMixedText;
    public Slider PointSizeSlider, RGBmixedSlider;
    public Button ResetPlayerPosButton;
    public Button ShowHistogramButton;
    public Button ExitButton;
    public Button ChangeModeButton;
    public Toggle SwitchPrintToggle;
    [Space(10)]

    [Header("Exhibitor")]
    public ExhibitorMonoBehaviour ExhibitorMono;

    private void Awake()
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

    void Start()
    {
        //Initialization
        PointCloudRenderer = MainManager.PointCloud.PointCloudRendererPC;

        PointSizeSlider.value = PointCloudRenderer.pointSize;
        RGBmixedSlider.value = MainManager.PointCloud.LerpFactor;

        UpdatePointSizeText(PointCloudRenderer.pointSize);

        UpdateDropdownPointCloud();

        UpdateDropdownProperties();

        if (MenuGO.activeSelf) MenuGO.SetActive(false);
    }

    public void UpdateDropdownProperties()
    {
        List<TMP_Dropdown.OptionData> _newOptions = new List<TMP_Dropdown.OptionData>();
        TMP_Dropdown.OptionData _option;

        for (int i = 0; i < PointCloudRenderer.sourceDataProps; ++i)
        {
            _option = new TMP_Dropdown.OptionData("Time " +  (i + 1));
            _newOptions.Add(_option);
        }

        _option = new TMP_Dropdown.OptionData("RGB");
        _newOptions.Add(_option);

        PropertyDropdown.ClearOptions();
        PropertyDropdown.AddOptions(_newOptions);
        PropertyDropdown.value = MainManager.PointCloud.Property;
    }

    public void UpdateDropdownPointCloud()
    {
        List<TMP_Dropdown.OptionData> _newOptions = new List<TMP_Dropdown.OptionData>();
        TMP_Dropdown.OptionData _option;

        for (int i = 0; i < MainManager.PointCloudArrayGO.Length; ++i)
        {
            string _namePC = MainManager.PointCloudArrayGO[i].name;
            _option = new TMP_Dropdown.OptionData(_namePC);
            _newOptions.Add(_option);
        }

        PointCloudDropdown.ClearOptions();
        PointCloudDropdown.AddOptions(_newOptions);

        UpdateDropdownProperties();

        MainManager.UpdateInterfaceAfterPC();
    }

    public void UpdatePointSizeText(float pointsize)
    {
        PointSizeText.text = "Point Size:\n" + pointsize;
    }

    public void ResetPlayerPos()
    {
        Vector3 newPos = ExhibitorMono.gameObject.transform.position;
        Player.transform.position = new Vector3(newPos.x + 4 * Mathf.Sign(newPos.x), 0, newPos.z);
    }

    public void ExitApp()
    {

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    private void OnEnable()
    {
        PropertyDropdown.onValueChanged.AddListener(MainManager.CallSwitchProperty);

        PointCloudDropdown.onValueChanged.AddListener(MainManager.SwitchPointCloud);

        ChangeModeButton.onClick.AddListener(MainManager.CallChangeState);

        PointSizeSlider.onValueChanged.AddListener((tamaño) => PointCloudRenderer.pointSize = PointSizeSlider.value);
        PointSizeSlider.onValueChanged.AddListener(UpdatePointSizeText);

        RGBmixedSlider.onValueChanged.AddListener((factor) => MainManager.PointCloud.SetLerpFactor(RGBmixedSlider.value));

        ResetPlayerPosButton.onClick.AddListener(ResetPlayerPos);

        ExitButton.onClick.AddListener(ExitApp);

        //The implementation of visualization mode button can be found in onClick event in Inspector

        //The implementation of Open Configuration Menu button can be found in onClick event in Inspector

        //The implementation of switch printing toggle can be found in onClick event in Inspector

        //The implementation of show histogram button can be found in onClick event in Inspector (Histogram GO)
    }

    private void OnDisable()
    {
        PropertyDropdown.onValueChanged.RemoveListener(MainManager.PointCloud.SwitchProperty);

        PropertyDropdown.onValueChanged.RemoveListener(MainManager.PointCloud.SwitchOriginalProperty);

        PointSizeSlider.onValueChanged.RemoveAllListeners();

        RGBmixedSlider.onValueChanged.RemoveListener((factor) => MainManager.PointCloud.SetLerpFactor(RGBmixedSlider.value));

        ResetPlayerPosButton.onClick.RemoveAllListeners();

        ExitButton.onClick.RemoveAllListeners();
    }
}
