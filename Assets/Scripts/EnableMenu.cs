using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class EnableMenu : MonoBehaviour
{

    public InputAction ActivaMenu;

    public ActionBasedController controller;


    GameObject menu;


    void Awake()
    {
        controller = GetComponentInChildren<ActionBasedController>();

        menu = GameObject.FindGameObjectWithTag("Menu");

        ActivaMenu.performed +=
            context =>
            {
                if (menu.activeInHierarchy)
                {
                    menu.SetActive(false);
                    controller.hideControllerModel = false;
                }
                else
                {
                    menu.SetActive(true);
                    controller.hideControllerModel = true;

                }


            };
    }


    void OnEnable()
    {
        ActivaMenu.Enable();

    }

    void OnDisable()
    {
        ActivaMenu.Disable();
    }
}