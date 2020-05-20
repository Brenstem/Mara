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

            int currentIndex = 0;
            bool incrementIndex = true;
            // Print the resolutions
            foreach (var res in resolutions)
            {
                options.Add(res.width + "x" + res.height + " : " + res.refreshRate);
                if (incrementIndex)
                {
                    if (Screen.currentResolution.height == res.height &&
                        Screen.currentResolution.width == res.width &&
                        Screen.currentResolution.refreshRate == res.refreshRate)
                    {
                        incrementIndex = false;
                    }
                    currentIndex++;
                }
                
            }

            _dropdown.AddOptions(options);
            _dropdown.value = currentIndex;
        }
        else
        {
            Debug.LogWarning("Dropdown component is missing!", this);
        }


    }
}
