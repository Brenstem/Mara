using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GenerateDropdownQuality : MonoBehaviour
{
    private Dropdown _dropdown;

    void Awake()
    {
        _dropdown = GetComponent<Dropdown>();
        if (_dropdown != null)
        {
            _dropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (string item in QualitySettings.names)
            {
                options.Add(item);
            }
            _dropdown.AddOptions(options);
        }
        else
        {
            Debug.LogWarning("Dropdown component is missing!", this);
        }
    }
}
