using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float playerHealth;
    public float playerRotation;
    public float[] playerPosition;

    public PlayerData()
    {
        playerHealth = GlobalState.state.Player.GetComponent<PlayerInsanity>().CurrentHealth;

        playerPosition = new float[3];
        playerPosition[0] = GlobalState.state.Player.gameObject.transform.position.x;
        playerPosition[1] = GlobalState.state.Player.gameObject.transform.position.y;
        playerPosition[2] = GlobalState.state.Player.gameObject.transform.position.z;


        playerRotation = GlobalState.state.Player.gameObject.transform.rotation.eulerAngles.y;
    }


}
