using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointHandler : MonoBehaviour
{
    [SerializeField]
    float respawnTime;

    private PlayerData _activeCheckPoint;
    private Timer _respawnTimer;
    private Quaternion _rotationQ;

    // Subscribe to player death event
    private void OnEnable()
    {
        PlayerAnimationEventHandler.onPlayerDeath += RespawnPlayer;
    }

    private void OnDisable()
    {
        PlayerAnimationEventHandler.onPlayerDeath -= RespawnPlayer;
    }

    private void Start()
    {
        _activeCheckPoint = new PlayerData(0, this.transform);
    }

    private void LateUpdate()
    {
        if (_respawnTimer != null)
        {
            _respawnTimer.Time += Time.deltaTime;

            if (_respawnTimer.Expired)
            {
                Spawn();
                _respawnTimer = null;
            }
        }
    }


    // Save checkpoint data to struct on checkpoint activation
    public void ActivateCheckpoint(Transform respawnPosition, bool useMaxHealth)
    {
        print("Checkpoint Set");

        float insanity;

        Transform position = respawnPosition;

        if (useMaxHealth)
        {
            insanity = 0;
        }
        else
        {
            insanity = GlobalState.state.Player.gameObject.GetComponent<PlayerInsanity>().CurrentHealth;
        }


        _activeCheckPoint = new PlayerData(insanity , position);
        SaveData.Save_Data(_activeCheckPoint);
    }

    // Respawn player using checkpoint data
    private void Spawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        /*
        PlayerData data = (PlayerData)SaveData.Load_Data("player");

        GlobalState.state.Player.gameObject.GetComponent<CharacterController>().enabled = false;

        Vector3 position = new Vector3(data.playerPosition[0], data.playerPosition[1], data.playerPosition[2]);
        Vector3 rotation = new Vector3(data.playerRotation[0], data.playerRotation[1], data.playerRotation[2]);

        _rotationQ.eulerAngles = rotation;

        GlobalState.state.Player.gameObject.GetComponent<PlayerRevamp>().stateMachine.ChangeState(new IdleState());

        GlobalState.state.Player.gameObject.transform.position = position;
        GlobalState.state.Player.gameObject.transform.rotation = _rotationQ;

        GlobalState.state.Player.gameObject.GetComponent<PlayerInsanity>().SetInsanity(data.playerHealth);

        GlobalState.state.Player.gameObject.GetComponent<CharacterController>().enabled = true;
        */
    }

    void RespawnPlayer()
    {
        _respawnTimer = new Timer(respawnTime);
    }
}

public struct CheckpointData 
{
    public CheckpointData(Transform position, float insanity)
    {
        pos = position;
        ins = insanity;
    }

    public Transform pos;
    public float ins;
}
