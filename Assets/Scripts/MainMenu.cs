using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class MainMenu : MonoBehaviour
{
    public ActionBasedController Controller;

    [Header("Canvas")]
    public GameObject MainMenuGO;
    public GameObject MenuGO;
    public GameObject ConfigurationMenuGO;
    public GameObject ElevatorScrollbarGO;
    public GameObject WindowGraphGO;
    [Space(10)]

    [Header("Buttons")]
    public GameObject MenuButton;
    public GameObject ElevatorButton;
    public GameObject WindowGraphButton;
    [Space(10)]

    [Header("Input Actions")]
    public InputAction EnableMainMenu;

    void Awake()
    {
        MenuButton.SetActive(false);
        ElevatorButton.SetActive(false);
        WindowGraphButton.SetActive(false);

        SetActionPerformed();
    }

    private void Start()
    {
        DeactivateEverything();
    }

    public void ActivateMenu()
    {
        WindowGraphGO.SetActive(false);
        ElevatorScrollbarGO.SetActive(false);
        MenuGO.SetActive(true);

        MenuButton.SetActive(false);
        ElevatorButton.SetActive(false);
        WindowGraphButton.SetActive(false);
    }

    public void ActivateElevator()
    {
        WindowGraphGO.SetActive(false);
        ElevatorScrollbarGO.SetActive(true);
        MenuGO.SetActive(false);
        ConfigurationMenuGO.SetActive(false);

        MenuButton.SetActive(false);
        ElevatorButton.SetActive(false);
        WindowGraphButton.SetActive(false);
    }

    public void ActivateWindowGraph()
    {
        WindowGraphGO.SetActive(true);
        ElevatorScrollbarGO.SetActive(false);
        MenuGO.SetActive(false);
        ConfigurationMenuGO.SetActive(false);

        MenuButton.SetActive(false);
        ElevatorButton.SetActive(false);
        WindowGraphButton.SetActive(false);
    }

    public void DeactivateEverything()
    {
        WindowGraphGO.SetActive(false);
        ElevatorScrollbarGO.SetActive(false);
        MenuGO.SetActive(false);
        ConfigurationMenuGO.SetActive(false);

        MenuButton.SetActive(true);
        ElevatorButton.SetActive(true);
        WindowGraphButton.SetActive(true);

        MainMenuGO.SetActive(false);
    }

    void SetActionPerformed()
    {
        EnableMainMenu.performed +=
            context =>
            {
                if (MainMenuGO.activeInHierarchy)
                {
                    DeactivateEverything();
                    Controller.hideControllerModel = false;
                    Controller.GetComponent<XRRayInteractor>().enabled = true;
                }
                else
                {
                    MainMenuGO.SetActive(true);
                    Controller.hideControllerModel = true;
                    Controller.GetComponent<XRRayInteractor>().enabled = false;
                }
            };
    }


    void OnEnable()
    {
        EnableMainMenu.Enable();

    }

    void OnDisable()
    {
        EnableMainMenu.Disable();
    }
}
