using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuResume : MonoBehaviour
{
    public void Unpause()
    {
        GlobalState.state.Unpause();
    }
}
