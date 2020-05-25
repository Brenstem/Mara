using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSave : MonoBehaviour
{
    public void Save()
    {
        MenuInputResource.SaveOverrides();
    }
}
