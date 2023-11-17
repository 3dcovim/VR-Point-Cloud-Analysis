using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class TeleportSurfaceMono : MonoBehaviour
{
    public MainManager MainManager;
    public StateManager StateManager;
    public SoundManager SoundManager;

    [HideInInspector]
    public BoxCollider BoxCollider;
    private float[,] PCBoundingBox;
    private float[] _divisions = new float[3];
    private List<GameObject> _midCells = new List<GameObject>();
    private List<Vector3> _midCellsInitPos = new List<Vector3>();
    private Transform _floor;

    public GameObject Player;
    public XROrigin XROrigin;

    public GameObject PlanePrefab;

    private void Start()
    {
        MainManager = GameObject.FindGameObjectWithTag("MainManager").GetComponent<MainManager>();

        StateManager = MainManager.StateManager;
        SoundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();

        Player = GameObject.FindGameObjectWithTag("Player");
        XROrigin = Player.GetComponent<XROrigin>();

        _floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<Transform>();

        BoxCollider = StateManager.GetComponent<BoxCollider>();
        PCBoundingBox = MainManager.PointCloud.PCBoundingBox;

        float _distX = Mathf.Abs(PCBoundingBox[0, 0] - PCBoundingBox[0, 1]) * 1.1f;
        float _distY = Mathf.Abs(PCBoundingBox[1, 0] - PCBoundingBox[1, 1]) * 1.1f;
        float _distZ = Mathf.Abs(PCBoundingBox[2, 0] - PCBoundingBox[2, 1]) * 1.1f;

        BoxCollider.size = new Vector3(_distX, _distY, _distZ);
        gameObject.transform.localPosition = new Vector3(0, BoxCollider.center.x + BoxCollider.center.y, 0);

        _divisions[0] = Mathf.Round(_distX/10);
        _divisions[1] = Mathf.Round(_distY/10);
        _divisions[2] = Mathf.Round(_distZ/10);

        for (int xx = 0; xx < _divisions[0]; xx++)
        {
            float _midCellsx = (BoxCollider.center.x + BoxCollider.size.x / 2) - 5f * (2*xx + 1);

            for (int zz = 0; zz < _divisions[2]; zz++)
            {
                float _midCellsz = (BoxCollider.center.z + BoxCollider.size.z / 2) - 5f * (2*zz + 1);

                GameObject _planeGO = Instantiate(PlanePrefab);
                _planeGO.transform.SetParent(this.transform);
                _planeGO.transform.localPosition = new Vector3(_midCellsx, 0, _midCellsz);
                _planeGO.transform.localRotation = new Quaternion(0, 0, 0, 1);

                _midCellsInitPos.Add(new Vector3(_midCellsx, _floor.position.y, _midCellsz));

                _midCells.Add(_planeGO);
            }
        }

        gameObject.SetActive(false);
    }

    public void TeleportToTilePos()
    {
        GameObject _tileGO = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInParent<TileMono>().gameObject;

        int _ind = 0;

        foreach (GameObject tile in _midCells)
        {
            if (Object.ReferenceEquals(_tileGO, tile))
            {
                Player.transform.position = new Vector3(_midCellsInitPos[_ind].x, _midCellsInitPos[_ind].y + XROrigin.CameraYOffset, _midCellsInitPos[_ind].z);

                StateManager.TeleportModeActivate();

                StateManager.ChangeState();

                SoundManager.PlaySound("OnSelectTeleportTile");

                return;
            }
            else _ind++;
        }
    }
}
