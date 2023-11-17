using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;

public class MeasurePoints : MonoBehaviour
{

    public InputAction MeasurePoint;

    public MainManager MainManager;
    public MenuManager MenuManager;
    public Window_graph Window_Graph;

    public TMP_Text PropertyTextCanvas;

    Ray _ray;
    PointExt _measuredPointsExt, _selectedPoint;
    PointExt[] _closerPointsExt;

    public float MaxDistance = 0.2f;

    public GameObject SpherePrefab;
    public GameObject Sphere;

    [SerializeField] private Material sphereMaterial;

    private void Update()
    {
        if(Window_Graph.gameObject.activeInHierarchy) PropertyTextCanvas.text = "";
    }

    void Start()
    {
        MainManager = GameObject.FindGameObjectWithTag("MainManager").GetComponent<MainManager>();
        MenuManager = GameObject.FindGameObjectWithTag("MenuManager").GetComponent<MenuManager>();

        Sphere = Instantiate(SpherePrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        Sphere.SetActive(false);

        MeasurePoint.performed +=
            context =>
            {
                RaycastResult _raycastResult = new RaycastResult();

                XRRayInteractor m_XRRayInteractor = GetComponent<XRRayInteractor>();

                m_XRRayInteractor.TryGetCurrentUIRaycastResult(out _raycastResult);

                if (_raycastResult.gameObject != null)
                {
                    if (_raycastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                    {
                        return;
                    }
                }

                _ray = new Ray(this.transform.position, this.transform.forward);
                _closerPointsExt = MainManager.PointCloud.FinalOctree.GetNearby(_ray, MaxDistance);

                if (_closerPointsExt.Length > 0)
                {
                    _measuredPointsExt = MainManager.PointCloud.ClosestPoint(transform, _closerPointsExt);

                    if (_selectedPoint.Equals(_measuredPointsExt) && Sphere.activeSelf)
                    {
                        Sphere.SetActive(false);
                        return;
                    }

                    _selectedPoint = _measuredPointsExt;

                    Sphere.transform.position = _measuredPointsExt.Point.Position;
                    float _sphereScale = MainManager.PointCloud.PointCloudRendererPC.pointSize * 5f;
                    Sphere.transform.localScale = new Vector3(_sphereScale, _sphereScale, _sphereScale);
                    if (!Sphere.activeSelf) Sphere.SetActive(true);

                    if (Window_Graph.gameObject.activeInHierarchy)
                    {
                        
                        List<float> parameterValueList = new List<float>();

                        int prop_number = _measuredPointsExt.prop_package.Length;//AGO23
                        for (int indexal = 0; indexal < prop_number; indexal++) //AGO23
                        {//AGO23
                            parameterValueList.Add(Mathf.Round(_measuredPointsExt.prop_package[indexal] * 100.0f) * 0.01f);//AGO23

                        }//AGO23

                     //   parameterValueList.Add(Mathf.Round(_measuredPointsExt.Prop1 * 100.0f) * 0.01f); //AGO23
                     //   parameterValueList.Add(Mathf.Round(_measuredPointsExt.Prop2 * 100.0f) * 0.01f); //AGO23
                     //   parameterValueList.Add(Mathf.Round(_measuredPointsExt.Prop3 * 100.0f) * 0.01f); //AGO23
                     //   parameterValueList.Add(Mathf.Round(_measuredPointsExt.Prop4 * 100.0f) * 0.01f); //AGO23
                     //   parameterValueList.Add(Mathf.Round(_measuredPointsExt.Prop5 * 100.0f) * 0.01f); //AGO23
                     //   parameterValueList.Add(Mathf.Round(_measuredPointsExt.Prop6 * 100.0f) * 0.01f); //AGO23

                        Window_Graph.ShowGraph(parameterValueList, (int _i) => "Time " + (_i + 1), (float _f) => System.Math.Round(_f, 1) + " ºC");
                        
                    }
                    else
                    {
                        PropertyTextCanvas.text = _selectedPoint.prop_package[MainManager.Property].ToString("N1") + " ºC"; //AGO23
                        

                    }
                }
            };
    }
    public void UpdateMaxSearchingDistance(float newDistance)
    {
        MaxDistance = newDistance;
    }
    void OnEnable()
    {
        MeasurePoint.Enable();
    }

    void OnDisable()
    {
        MeasurePoint.Disable();
    }
}
