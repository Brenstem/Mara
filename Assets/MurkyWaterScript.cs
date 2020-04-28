using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MurkyWaterScript : MonoBehaviour
{
    //göra så bossen/fiender kan ta skada av dem och ändra pölarnas weight för navmeshen, fast inte för de som svävar (?)
    [Tooltip("Vilka som ska kunna bli påvärkade av pölarna")]
    [SerializeField] private LayerMask _collisionLayers;
    [SerializeField] private float _damagePerSecond = 1f;


    //[SerializeField] private bool _destroyAfterCertainTime = false;
    //[Tooltip("Om Destroy After Certain Time är true spelar detta värde inte roll")]
    //[SerializeField] private float _timeToLive = 0f;

    [NonSerialized] private bool _destroyAfterCertainTime;
    [NonSerialized] private float _timeToLive;

    //kanske kommer behöva lägga in en base offset som flyttar upp objektet lite 

    public float timeToLive
    {
        get { return _timeToLive; }
        set
        {
            if (!_destroyAfterCertainTime)
                _destroyAfterCertainTime = true;
            _timeToLive = value;
        }
    }

    [NonSerialized] private Timer timer;

    void Start()
    {
        //print("nu körs start för murky water");
        if (_destroyAfterCertainTime)
        {
            //print("kms in " + _timeToLive);
            timer = new Timer(_timeToLive);
        }
        else
        {
            //print("IM GONNA LIVE FOREVER!!!");
        }
    }


    void Update()
    {
        if (_destroyAfterCertainTime)
        {
            timer.Time += Time.deltaTime;

            //kanske lägga till att den typ börjar bubbla eller något, när det är någon sec tills den försvinner

            if (timer.Expired)
            {
                print("nu ska murky water dö");
                //kanske spela någon animation här för att förtydliga att den försvinner
                Destroy(this.transform.parent.gameObject);
            }
        }
    }

	private void OnTriggerStay(Collider other) {
		print("nu ser jag något");
		if (_collisionLayers == (_collisionLayers | 1 << other.gameObject.layer)) {
			if (other.CompareTag("Player")) {
				other.GetComponent<PlayerInsanity>().IncrementInsanity(_damagePerSecond);
			}
		}
	}
}
