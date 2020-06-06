using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveWallsTriggerScript : MonoBehaviour
{
    [SerializeField] GameObject[] wallsToHide;
    [SerializeField] GameObject[] wallsToAppear;

    GameObject[][] allWalls = new GameObject[2][];

    int sign = 0;

    void Start()
    {
        allWalls[0] = wallsToHide;
        allWalls[1] = wallsToAppear;
    }

    private void OnTriggerEnter(Collider other)
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
