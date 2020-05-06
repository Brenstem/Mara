using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSwitch : MonoBehaviour
{
    [SerializeField] private GameObject _menuSwitchDestination;
    public void OnClick()
    {
        print("click");
        MenuHandler.state.ChangeMenu(_menuSwitchDestination);
    }
}
