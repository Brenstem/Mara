using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveWallsTriggerScript : MonoBehaviour
{
    [SerializeField] GameObject[] wallsToHide;
    [SerializeField] GameObject[] wallsToAppear;

    GameObject[][] allWalls = new GameObject[2][];

    int sign = 0;

    private LayerMask _triggerDetectionLayers;

    private void Awake()
    {
        allWalls[0] = wallsToHide;
        allWalls[1] = wallsToAppear;
        _triggerDetectionLayers = (_triggerDetectionLayers | 1 << GlobalState.state.Player.gameObject.gameObject.layer);

    }

    private void OnTriggerEnter(Collider other)
    {

        if (_triggerDetectionLayers == (_triggerDetectionLayers | 1 << other.gameObject.layer))
        {
            for (int i = 0; i < allWalls[sign].Length; i++)
            {
                allWalls[sign][i].SetActive(false);
            }

            sign = (sign + 1) % 2;

            for (int i = 0; i < allWalls[sign].Length; i++)
            {
                allWalls[sign][i].SetActive(true);
            }
        }
    }
}
