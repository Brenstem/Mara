using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour {

	[SerializeField] GameObject target;
	[SerializeField] float speed;

    void Start()
    {
        
    }

    void Update()
    {
		transform.RotateAround(target.transform.position, Vector3.up, speed * Time.deltaTime);
	}
}
