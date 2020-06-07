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
            _dropdown.value = QualitySettings.GetQualityLevel();
        }
        else
        {
            Debug.LogWarning("Dropdown component is missing!", this);
        }
    }

    public void OnChange(int index)
    {
        QualitySettings.SetQualityLevel(index);
        SaveData.Save_Data(new OptionData(null));
    }
}
