using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GenerateDropdownFullscreen : MonoBehaviour
{
    private Dropdown _dropdown;

    void Awake()
    {
        _dropdown = GetComponent<Dropdown>();
        if (_dropdown != null)
        {
            _dropdown.ClearOptions();
            List<string> options = new List<string>();

            Resolution[] resolutions = Screen.resolutions;

            // Print the resolutions
            foreach (var res in resolutions)
            {
                options.Add(res.width + "x" + res.height + " : " + res.refreshRate);
            }

            _dropdown.AddOptions(options);
        }
        else
        {
            Debug.LogWarning("Dropdown component is missing!", this);
        }


    }
}
