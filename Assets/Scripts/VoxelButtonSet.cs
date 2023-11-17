using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VoxelButtonSet : MonoBehaviour
{
    public MenuManager MenuManager;
    public MainManager MainManager;

    private Button[] _buttonSet;

    public int ActivatedToggle;

    private float _voxelSizetoSend;

    private void Start()
    {
        _buttonSet = GetComponentsInChildren<Button>();
    }

    public void SendVoxelSize(int numToggle)
    {
        ActivatedToggle = numToggle;

        switch (numToggle)
        {
            case 1:
                _voxelSizetoSend = 150f;
                break;

            case 2:
                _voxelSizetoSend = 250f;
                break;

            case 3:
                _voxelSizetoSend = 350f;
                break;

            case 4:
                _voxelSizetoSend = 450f;
                break;
            case 5:
                _voxelSizetoSend = 550f;
                break;
        }

        //PointCloudManager.SetPointCloud(_voxelSizetoSend, MenuManager.IncludeNaNToggle.isOn, true, true);

        MainManager.SetVoxelSize(_voxelSizetoSend);

        //MenuManager.UpdateVoxelSize(_voxelSizetoSend);
    }
}
