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

        Vector3 position;
        position.x = data.playerPosition[0];
        position.y = data.playerPosition[1];
        position.z = data.playerPosition[2];

        Debug.Log(position);

        GlobalState.state.PlayerGameObject.transform.position = position;
    }
}
