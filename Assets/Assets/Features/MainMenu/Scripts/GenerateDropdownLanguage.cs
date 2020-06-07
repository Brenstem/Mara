using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateDropdownLanguage : MonoBehaviour
{
    private Dropdown _dropdown;

    public Dropdown Dropdown
    {
        get
        {
            return _dropdown;
        }
    }

    void Awake()
    {
        _dropdown = GetComponent<Dropdown>();
        if (_dropdown != null)
        {
            int currentindex = 0;
            _dropdown.ClearOptions();
            List<string> options = new List<string>();
            string[] languages = System.Enum.GetNames(typeof(GlobalState.LanguageEnum));

            foreach (var language in languages)
            {
                options.Add(language);
                currentindex++;
            }
            _dropdown.AddOptions(options);
            _dropdown.value = currentindex;
        }
        else
        {
            Debug.LogWarning("Dropdown component is missing!", this);
        }
    }

    public void OnChange(int index)
    {
        GlobalState.state.language = (GlobalState.LanguageEnum)index;
    }
}
