using Pcx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum State { HighScale, LowScale }

public struct InitialTransform
{
    public Vector3 Position;
    public Quaternion Rotation;
}

public class StateManager : MonoBehaviour
{
    [Header("Game Objects")]
    public MainManager MainManager;
    public MenuManager MenuManager;
    public SoundManager SoundManager;

    private GameObject Exhibitor;
    private ExhibitorMonoBehaviour ExhibitorMono;
    private CapsuleCollider ExhibCollider;
    [HideInInspector]
    public  BoxCollider BoxCollider;
    private Rigidbody Rigidbody;
    private MeshRenderer MeshRenderer;
    private ActionBasedSnapTurnProvider SnapTurnProvider;
    private XRGrabInteractable XRGrabInteractable;
    private TeleportSurfaceMono TeleportSurfaceMono;
    [Space(10)]

    [Header("Current state")]
    public State State;
    [Space(10)]

    [Header("Public parameters")]
    public float ScaleReduceFactor = 0.01f;
    public float DistanceToCamera = 0.7f;
    [Space(10)]

    [Header("Public bools")]
    public bool IsTeleportModeActivated = false;
    public bool HasChanged = false;

    private InitialTransform _initialTransform;

    void Start()
    {
        Exhibitor = GameObject.FindGameObjectWithTag("Exhibitor");
        ExhibCollider = Exhibitor.GetComponent<CapsuleCollider>();
        ExhibitorMono = Exhibitor.GetComponent<ExhibitorMonoBehaviour>();
        BoxCollider = this.GetComponent<BoxCollider>();
        Rigidbody = this.GetComponent<Rigidbody>();
        MeshRenderer = this.GetComponent<MeshRenderer>();
        XRGrabInteractable = this.GetComponent<XRGrabInteractable>();
        SnapTurnProvider = GameObject.FindGameObjectWithTag("Player").GetComponent<ActionBasedSnapTurnProvider>();
        TeleportSurfaceMono = GetComponentInChildren<TeleportSurfaceMono>();

        State = State.HighScale;

        MeshRenderer.enabled = false;
        Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        BoxCollider.enabled = false;

        GetInitialPosition();
    }

    public void GetInitialPosition()
    {
        _initialTransform.Position = this.transform.position;
        _initialTransform.Rotation = this.transform.rotation;
    }

    public void SetInitialPosition()
    {
        this.transform.SetPositionAndRotation(_initialTransform.Position, _initialTransform.Rotation);
    }

    public void SetActualPosition(Camera Camera, float distanceToCamera)
    {
        this.transform.position = Camera.ViewportToWorldPoint(new Vector3(0.4f, 0.4f, distanceToCamera));
    }

    public void SetExhibitionPosition()
    {
        this.transform.localPosition = new Vector3(0f, (ExhibCollider.height * Exhibitor.transform.localScale.x) + (BoxCollider.size.y * ScaleReduceFactor), 0f);
        this.transform.rotation = _initialTransform.Rotation;
    }

    public void UpdateScale(State state)
    {
        if(state == State.HighScale)
        {
            this.transform.localScale = Vector3.one;
            State = State.HighScale;
        }
        else if(state == State.LowScale)
        {
            this.transform.localScale *= ScaleReduceFactor;
            State = State.LowScale;
        }
    }

    public void ChangeState()
    {
        if (State == State.HighScale) State = State.LowScale;
        else State = State.HighScale;

        OnEnterState(State);

        HasChanged = true;
    }

    public void OnEnterState(State state)
    {
        if (state == State.HighScale)
        {
            this.transform.parent = null;

            MeshRenderer.enabled = false;
            Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            BoxCollider.enabled = false;

            SetInitialPosition();

            GetComponentInChildren<PointCloudRenderer>().pointSize = GetComponentInChildren<PointCloudRenderer>().pointSize / ScaleReduceFactor;

            MenuManager.PointSizeSlider.enabled = true;

            ExhibitorMono.ResetPosButton.gameObject.SetActive(false);
            ExhibitorMono.TeleportButton.gameObject.SetActive(false);

            ExhibitorMono.gameObject.GetComponent<MeshRenderer>().enabled = false;

            SoundManager.PlaySound("MakeItBig");
        }
        else
        {
            this.transform.parent = Exhibitor.transform;

            MeshRenderer.enabled = true;
            Rigidbody.constraints = RigidbodyConstraints.None;
            Rigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            BoxCollider.enabled = true;

            SetExhibitionPosition();

            GetComponentInChildren<PointCloudRenderer>().pointSize = GetComponentInChildren<PointCloudRenderer>().pointSize * ScaleReduceFactor;

            MenuManager.PointSizeSlider.enabled = false;

            SphereCollider[] MaxMinPoints = this.GetComponentsInChildren<SphereCollider>();
            foreach(SphereCollider sphere in MaxMinPoints)
            {
                sphere.gameObject.SetActive(false);
            }

            ExhibitorMono.gameObject.GetComponent<MeshRenderer>().enabled = true;
            ExhibitorMono.ResetPosButton.gameObject.SetActive(true);
            ExhibitorMono.TeleportButton.gameObject.SetActive(true);

            MenuManager.ResetPlayerPos();

            SoundManager.PlaySound("MakeItLittle");
        }

        UpdateScale(state);
    }

    public void OnTeleportModeEnter()
    {
        SetExhibitionPosition();

        Rigidbody.isKinematic = true;

        BoxCollider.enabled = false;

        XRGrabInteractable.enabled = false;

        TeleportSurfaceMono.gameObject.SetActive(true);
    }

    public void OnTeleportModeExit()
    {
        Rigidbody.isKinematic = false;

        BoxCollider.enabled = true;

        SetExhibitionPosition();

        XRGrabInteractable.enabled = true;

        TeleportSurfaceMono.gameObject.SetActive(false);
    }

    public void TeleportModeActivate()
    {
        if (IsTeleportModeActivated)
        {
            IsTeleportModeActivated = false;
            OnTeleportModeExit();
        }
        else
        {
            IsTeleportModeActivated = true;
            OnTeleportModeEnter();
        }
    }

    public void OnSelected()
    {
        SnapTurnProvider.enabled = false;
    }

    public void OnDeselected()
    {
        SnapTurnProvider.enabled = true;
    }
}
