using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float playerHealth;
    //public float playerRotation;
    public float[] playerPosition;
    public float[] playerRotation;

    public PlayerData(float health, Transform spawnPosition)
    {
        playerHealth = health;

        playerPosition = new float[3];
        playerPosition[0] = spawnPosition.transform.position.x;
        playerPosition[1] = spawnPosition.transform.position.y;
        playerPosition[2] = spawnPosition.transform.position.z;

        //playerRotation = GlobalState.state.Player.gameObject.transform.rotation.eulerAngles.y;

        playerRotation = new float[3];
        playerRotation[0] = spawnPosition.transform.rotation.eulerAngles.x;
        playerRotation[1] = spawnPosition.transform.rotation.eulerAngles.y;
        playerRotation[2] = spawnPosition.transform.rotation.eulerAngles.z;
    }
}
