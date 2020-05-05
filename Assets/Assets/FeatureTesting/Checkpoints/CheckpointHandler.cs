using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointHandler : MonoBehaviour
{
    [SerializeField]
    float respawnTime;

    private CheckpointData _activeCheckPoint;
    private Timer _respawnTimer;

    // Subscribe to player death event
    private void OnEnable()
    {
        PlayerInsanity.onPlayerDeath += RespawnPlayer;
    }

    private void OnDisable()
    {
        PlayerInsanity.onPlayerDeath -= RespawnPlayer;
    }

    private void Start()
    {
        _activeCheckPoint = new CheckpointData(this.transform, 0);
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
            insanity = GlobalState.state.PlayerGameObject.GetComponent<PlayerInsanity>().CurrentHealth;
        }

        _activeCheckPoint = new CheckpointData(position, insanity);
    }

    // Respawn player using checkpoint data
    private void Spawn()
    {
        GlobalState.state.Player.IsDying = false;
        GlobalState.state.PlayerGameObject.GetComponent<CharacterController>().enabled = false;
        GlobalState.state.Player.EnableCombatController();
        GlobalState.state.Player.EnableMovementController();
        GlobalState.state.Player.EnableLockonFunctionality();
        GlobalState.state.Player.ResetCombatController();
        GlobalState.state.Player.ResetMovementController();
        GlobalState.state.PlayerGameObject.transform.position = _activeCheckPoint.pos.position;
        GlobalState.state.PlayerGameObject.transform.rotation = _activeCheckPoint.pos.rotation;
        GlobalState.state.PlayerGameObject.GetComponent<PlayerInsanity>().SetInsanity(_activeCheckPoint.ins);
        GlobalState.state.PlayerGameObject.GetComponent<CharacterController>().enabled = true;
        GlobalState.state.Player.ResetAnim();
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
