using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFunction : MonoBehaviour
{
    
    Quaternion rotationQ;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("i saved");
            Save();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            Load();
        }
    }

    public void Save()
    {
        SaveData.SavePlayer();
    }
    public void Load()
    {
        PlayerData data = SaveData.LoadPlayer();

        GlobalState.state.Player.gameObject.GetComponent<CharacterController>().enabled = false;

        Vector3 position;
        position.x = data.playerPosition[0];
        position.y = data.playerPosition[1];
        position.z = data.playerPosition[2];
        
        Vector3 rotation;
        rotation.x = data.playerRotation[0];
        rotation.y = data.playerRotation[1];
        rotation.z = data.playerRotation[2];

        GlobalState.state.Player.gameObject.transform.position = position;
        //av någon anledning får man inte sätta transform.rotation.eulerangles, men man får sätta transform.rotation...
        rotationQ.eulerAngles = rotation;
        GlobalState.state.Player.gameObject.transform.rotation = rotationQ;

        GlobalState.state.Player.gameObject.GetComponent<CharacterController>().enabled = true;
    }
}
