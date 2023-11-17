using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class ElevatorManager : MonoBehaviour
{
    public static ElevatorManager Instance { get; private set; }

    public XROrigin Player;
    public Scrollbar Scrollbar;
    public GameObject ElevatedPlane;

    private StateManager StateManager;
    private BoxCollider BoxCollider;

    private Transform _floor;

    [SerializeField]
    private float _minValue;
    [SerializeField]
    private float _maxValue;

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
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<XROrigin>();
        Scrollbar = GameObject.FindGameObjectWithTag("ElevatorScrollbar").GetComponent<Scrollbar>();
        ElevatedPlane = GameObject.Find("ElevatedPlane");
        StateManager = GameObject.FindGameObjectWithTag("StateManager").GetComponent<StateManager>();
        _floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<Transform>();

        BoxCollider = StateManager.BoxCollider;

        if (ElevatedPlane.activeInHierarchy) ElevatedPlane.SetActive(false);

        _minValue = _floor.position.y;
        //_maxValue = BoxCollider.center.y + BoxCollider.size.y / 2;
        _maxValue = BoxCollider.size.y;
    }

    public void UpdateHeight()
    {
        float _newHeight = Normalize(Scrollbar.value, 0, 1, _minValue, _maxValue);

        Player.transform.position = new Vector3(Player.transform.position.x, _newHeight, Player.transform.position.z);

        if (Scrollbar.value >= 0.2f) ElevatedPlane.SetActive(true);
        else ElevatedPlane.SetActive(false);
    }

    public float Normalize(float val, float valmin, float valmax, float min, float max)
    {
        float _newValue = (((val - valmin) / (valmax - valmin)) * (max - min)) + min;

        return _newValue;
    }
}
