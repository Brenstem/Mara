using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiral : MonoBehaviour
{
	[SerializeField] GameObject target;
	[SerializeField] float speed;

	// Start is called before the first frame update
	void Start()
    {
        
    }

	// Update is called once per frame
	void Update() {
		transform.RotateAround(target.transform.position, Vector3.right, speed * Time.deltaTime);
		//transform.Rotate(speed * Time.deltaTime, 0, 0, Space.Self);
	}
}

