using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using TMPro;
using Pcx;

public class ConfigurationMenu : MonoBehaviour
{
    public static ConfigurationMenu Instance { get; private set; }
    [SerializeField] MainManager MainManager;

    [Header("Internal Configuration")]
    public Toggle IncludeNanToggle;
    public Toggle SwitchFrustumToggle;
    public Slider VoxelSizeSlider;
    public Button ApplyChangesButton;
    public TMP_Text MaxScaleText;
    public TMP_Text MinScaleText;
    public TMP_Text MaxRangeText;
    public TMP_Text MinRangeText;
    public TMP_Text VoxelSizeText;
    public TMP_Text StepsText;
    public TMP_Text TotalPointsTextCurrent;
    public TMP_Text TotalPointsTextNew;
    public TMP_Dropdown MixTypeDropdown;
    public TMP_Dropdown ScaleTypeDropdown;
    [Space(10)]

    [Header("Parameters")]
    public int StepsCount = 5;
    public float MinRange;
    public float MaxRange;
    [Space(10)]

    [Header("Outputs")]
    public float NewVoxelSize;
    public float NewPointCount;

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

    private void Start()
    {
        //Initialization
        NewVoxelSize = MainManager.VoxelSize;
        NewPointCount = MainManager.PointCloud.ReducedPointCloud.Length;
        MaxRange = MainManager.MaxVoxelSize;
        MinRange = MainManager.MinVoxelSize;

        UpdateVoxelSize(MainManager.VoxelSize);
        UpdateSteps(StepsCount);
        UpdateMaxScale(MainManager.MaxScale);
        UpdateMinScale(MainManager.MinScale);
        UpdateMixType(MainManager.PointCloud.MixType);
        UpdateScaleType(MainManager.PointCloud.ScaleType);
        ChangeMinRange(MinRange);
        ChangeMaxRange(MaxRange);

        VoxelSizeSlider.value = 1;

        this.gameObject.SetActive(false);
    }

   #region Update Parameters and Text Fields
    public void UpdateMaxScale(float maxscale)
    {
        if (MainManager.Property == 6)
        {
            MaxScaleText.text = "Max. scale: -";
        }
        else
        {
            MainManager.MaxScale = maxscale;

            MaxScaleText.text = "Max. scale: " + maxscale.ToString("f2");
        }

    }

    public void UpdateMinScale(float minscale)
    {
        if (MainManager.Property == 6)
        {
            MinScaleText.text = "Min. scale: -";
        }
        else
        {
            MainManager.MinScale = minscale;

            MinScaleText.text = "Min. scale: " + minscale.ToString("f2");
        }
    }

    public void UpdateVoxelSize(float value)
    {
        float _newVoxelSize = Remap(value, MinRange, MaxRange);

        UpdateNextPointCountText(_newVoxelSize);

        _newVoxelSize /= 1000f;

        NewVoxelSize = _newVoxelSize;

        VoxelSizeText.text = "Voxel Size: " + NewVoxelSize.ToString("f2");
    }

    public void UpdatePointCount()
    {
        TotalPointsTextCurrent.text = "Nº points (current): " + MainManager.PointCloud.ReducedPointCloud.Length;

        TotalPointsTextNew.text = "Nº points (new): -";
    }

    public void UpdateNextPointCountText(float _newVoxelSize)
    {
        NewPointCount = Mathf.Round(MainManager.PointCloud.ReducedPointCloud.Length * MainManager.VoxelSize / _newVoxelSize);

        TotalPointsTextNew.text = "Nº points (new): ~" + NewPointCount;
    }

    public void ChangeMinRange(float newValue)
    {
        if (newValue > 0)
        {
            MinRange = newValue;

            MainManager.MinVoxelSize = newValue;

            MinRangeText.text = "Min. range: " + MinRange;

            UpdateVoxelSize(NewVoxelSize);
        }
        else MinRangeText.text = "Min. range less than 0. Enter new value:";
    }
    
    public void ChangeMaxRange(float newValue)
    {
        MaxRange = newValue;

        MainManager.MaxVoxelSize = newValue;

        MaxRangeText.text = "Max. range: " + MaxRange;

        UpdateVoxelSize(NewVoxelSize);
    }

    public void UpdateSteps(int newValue)
    {
        StepsCount = newValue - 1;

        VoxelSizeSlider.maxValue = StepsCount;

        StepsText.text = "Nº steps: " + (StepsCount + 1);

        UpdateVoxelSize(NewVoxelSize);
    }
    public void UpdateMixType(int option)
    {
        MainManager.PointCloud.MixType = option;

        if(option == 0)
        {
            MainManager.MenuManager.RGBMixedText.text = "Property\t\t\t\t\t\tGray";

        }
        else
        {
            MainManager.MenuManager.RGBMixedText.text = "Property\t\t\t\t\t\tRGB";
        }
    }

    public void UpdateScaleType(int option)
    {
        MainManager.PointCloud.ScaleType = option;
    }

    #endregion

    #region Utilities
    public float Remap(float value, float minRange, float maxRange)
    {
        float _newValue = 250f;

        if (minRange > 0)
        {
            _newValue = math.remap(0, StepsCount, minRange, maxRange, value);
        }
        else
        {
            Debug.Log("New value not within range");
        }

        return _newValue;
    }

    public void ApplyChanges()
    {
        MainManager.SetVoxelSize(NewVoxelSize * 1000f);

        MainManager.SetPointCloud(IncludeNanToggle.isOn, true, true);
    }

    public void ActivateConfigurationMenu()
    {
        this.gameObject.SetActive(!this.gameObject.activeInHierarchy);
    }
    #endregion

    private void OnEnable()
    {
        VoxelSizeSlider.onValueChanged.AddListener(UpdateVoxelSize);
        ApplyChangesButton.onClick.AddListener(ApplyChanges);
        MixTypeDropdown.onValueChanged.AddListener(UpdateMixType);
        ScaleTypeDropdown.onValueChanged.AddListener(UpdateScaleType);

        MainManager.MenuManager.PointCloudDropdown.enabled = false;
    }

    private void OnDisable()
    {
        VoxelSizeSlider.onValueChanged.RemoveAllListeners();
        ApplyChangesButton.onClick.RemoveAllListeners();
        MixTypeDropdown.onValueChanged.RemoveAllListeners();
        ScaleTypeDropdown.onValueChanged.RemoveAllListeners();

        MainManager.MenuManager.PointCloudDropdown.enabled = true;
    }
}
