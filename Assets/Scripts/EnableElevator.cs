using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class EnableElevator : MonoBehaviour
{

    public InputAction EnableMenu;

    public ActionBasedController Controller;


    GameObject ElevatorCanvas;


    void Awake()
    {
        Controller = GetComponentInChildren<ActionBasedController>();

        ElevatorCanvas = GameObject.FindGameObjectWithTag("Menu");

        EnableMenu.performed +=
            context =>
            {
                if (ElevatorCanvas.activeInHierarchy)
                {
                    ElevatorCanvas.SetActive(false);
                    Controller.hideControllerModel = false;
                }
                else
                {
                    ElevatorCanvas.SetActive(true);
                    Controller.hideControllerModel = true;

                }


            };
    }


    void OnEnable()
    {
        EnableMenu.Enable();

    }

    void OnDisable()
    {
        EnableMenu.Disable();
    }
}
