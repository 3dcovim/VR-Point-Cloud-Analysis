using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ValueToText : MonoBehaviour
{

    TextMeshProUGUI _text;
    [SerializeField] Slider _slider; 
    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void ShowValue(string textPrevio)
    {
        _text.text = textPrevio + " (" + _slider.value + ")";
    }
}
