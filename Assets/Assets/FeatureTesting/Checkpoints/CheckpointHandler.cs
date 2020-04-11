using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointHandler : MonoBehaviour
{
    [SerializeField]
    float respawnTime;

    private CheckpointData _activeCheckPoint;
    private GameObject _player;
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

    private void Update()
    {
        if (_respawnTimer != null)
        {
            _respawnTimer.Time += Time.deltaTime;

            if (_respawnTimer.Expired())
            {
                Spawn();
                _respawnTimer = null;
            }
        }
    }


    // Save checkpoint data to struct on checkpoint activation
    public void ActivateCheckpoint(GameObject player, Transform respawnPosition)
    {
        print("Checkpoint Set");

        _player = player;
        Transform position = respawnPosition;
        float insanity = player.GetComponent<PlayerInsanity>().GetInsanity();
        _activeCheckPoint = new CheckpointData(position, insanity);
    }

    // Respawn player using checkpoint data
    private void Spawn()
    {
        print("Respawning Player");

        _player.transform.position = _activeCheckPoint.pos.position;
        _player.transform.rotation = _activeCheckPoint.pos.rotation;
        _player.GetComponent<PlayerInsanity>().SetInsanity(_activeCheckPoint.ins);
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
