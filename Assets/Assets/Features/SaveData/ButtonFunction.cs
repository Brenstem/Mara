using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFunction : MonoBehaviour
{

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

        float rotation;
        rotation = data.playerRotation;

        Debug.Log(rotation);

        GlobalState.state.Player.gameObject.transform.position = position;
        GlobalState.state.Player.gameObject.transform.Rotate(new Vector3(0, rotation, 0));

        GlobalState.state.Player.gameObject.GetComponent<CharacterController>().enabled = true;
    }
}
