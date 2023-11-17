using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileMono : MonoBehaviour
{
    public Canvas Canvas;

    public TeleportSurfaceMono TeleportSurface;

    public Button TeleportButton;

    private void Awake()
    {
        Canvas = GetComponentInChildren<Canvas>();
        Canvas.worldCamera = Camera.main;

        TeleportButton = GetComponentInChildren<Button>();

        TeleportSurface = GameObject.FindGameObjectWithTag("TeleportSurface").GetComponent<TeleportSurfaceMono>();
    }

    void OnEnable()
    {
        TeleportButton.onClick.AddListener(TeleportSurface.TeleportToTilePos);
    }

    private void OnDisable()
    {
        TeleportButton.onClick.RemoveAllListeners();
    }
}
