using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Keyboard : MonoBehaviour
{
    public MainManager MainManager;
    public ConfigurationMenu ConfigurationMenu;

    public TMP_InputField TextInput { get; set; }
    public TMP_Text NewTextValue;

    private string _savedValue;
    private int _state = 0; //0 -> idle, 1 -> steps, 2 -> max value, 3 -> min value

    private void Start()
    {
        MainManager = GameObject.FindGameObjectWithTag("MainManager").GetComponent<MainManager>();
    }

    public void SaveValue(string value)
    {
        _savedValue += value;
        NewTextValue.text = "New value:\n " + _savedValue;
    }

    public void PrintValue()
    {
        TextInput.text = _savedValue;

        switch (_state)
        {
            case 0:
                 //
                break;
            case 1:
                ConfigurationMenu.UpdateSteps(int.Parse(_savedValue));
                break;
            case 2:
                ConfigurationMenu.UpdateMaxScale(int.Parse(_savedValue));
                break;
            case 3:
                ConfigurationMenu.UpdateMinScale(int.Parse(_savedValue));
                break;
            case 4:
                ConfigurationMenu.ChangeMaxRange(int.Parse(_savedValue));
                break;
            case 5:
                ConfigurationMenu.ChangeMinRange(int.Parse(_savedValue));
                break;
        }

        _savedValue = "";
        NewTextValue.text = "New value:\n - ";
    }

    public void SelectNumberofStepsInput(TMP_InputField input)
    {
        TextInput = input;

        _state = 1;
    }

    public void SelectMaxScaleInput(TMP_InputField input)
    {
        TextInput = input;

        _state = 2;
    }
    
    public void SelectMinScaleInput(TMP_InputField input)
    {
        TextInput = input;

        _state = 3;
    }
    
    public void SelectMaxRangeInput(TMP_InputField input)
    {
        TextInput = input;

        _state = 4;
    }
    
    public void SelectMinRangeInput(TMP_InputField input)
    {
        TextInput = input;

        _state = 5;
    }

    public void SetInputFieldState(int state)
    {
        _state = state;
    }

    public void DeleteAll()
    {
        TextInput.text = "...";
        _savedValue = "";
        NewTextValue.text = "-";
    }
}
