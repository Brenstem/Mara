using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSwitch : MonoBehaviour
{
    [SerializeField] private MenuHandler _menuHandler;
    [SerializeField] private GameObject _menuSwitchDestination;

    private void Awake()
    {
        _menuHandler = GetComponentInParent<MenuHandler>();
    }

    public void OnClick()
    {
        _menuHandler.ChangeMenu(_menuSwitchDestination);
    }
}
