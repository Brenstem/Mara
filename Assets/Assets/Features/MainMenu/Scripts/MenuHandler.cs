using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject _currentMenu;

    private void Awake()
    {

    }

    private void Start()
    {
        if (_currentMenu != null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            EnableMenu(_currentMenu);
        }
    }

    private void EnableMenu(GameObject gameObject)
    {
        if (!GlobalState.state.GameStarted)
        {
            gameObject.SetActive(true);
        }
    }

    private void DisableMenu(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    public void ChangeMenu(GameObject gameObject)
    {
        if (gameObject != _currentMenu)
        {
            DisableMenu(_currentMenu);
            _currentMenu = gameObject;
            EnableMenu(_currentMenu);
        }
        else
        {
            Debug.LogWarning("Trying to change to already active menu!", this);
        }
    }
}
