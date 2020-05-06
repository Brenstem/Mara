using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public PlayerData()
    {
        playerHealth = GlobalState.state.Player.GetComponent<PlayerInsanity>().CurrentHealth;

        playerPosition = new float[3];
        playerPosition[0] = GlobalState.state.Player.gameObject.transform.position.x;
        playerPosition[1] = GlobalState.state.Player.gameObject.transform.position.y;
        playerPosition[2] = GlobalState.state.Player.gameObject.transform.position.z;
    }

    public float playerHealth;
    public float[] playerPosition;

}
