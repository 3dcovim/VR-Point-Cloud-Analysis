using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExhibitorMonoBehaviour : MonoBehaviour
{
    public Button ResetPosButton;
    public Button TeleportButton;

    private void Awake()
    {
        Button[] _buttonArray = this.GetComponentsInChildren<Button>();
        foreach (var button in _buttonArray)
        {
            if (button.gameObject.name == "ResetPosButton") ResetPosButton = button;
            else if (button.gameObject.name == "TeleportButton") TeleportButton = button;
        }

        ResetPosButton.gameObject.SetActive(false);
        TeleportButton.gameObject.SetActive(false);

        this.GetComponent<MeshRenderer>().enabled = false;
    }
}
