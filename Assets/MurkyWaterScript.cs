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


    [SerializeField] private bool _destroyAfterCertainTime = false;
    [Tooltip("Om Destroy After Certain Time är true spelar detta värde inte roll")]
    [SerializeField] private float _timeToLive = 0f;


    [NonSerialized] private Timer timer;

    //får inte har en konstruktor för unity är dumma och låter mig inte göra som jag vill, får göra i awake eller något istället
    public MurkyWaterScript(float timeToLive = 0f, bool destroyAfterCertainTime = false)
    {
        if (destroyAfterCertainTime && timeToLive < 0.01f)
        {
            Debug.LogError("timeToLive värdet är för litet för att det ska kunna förstöras efter en viss mängd tid");
        }
        else
        {
            _destroyAfterCertainTime = destroyAfterCertainTime;
            _timeToLive = timeToLive;
        }
    }

    void Start()
    {
        print("nu körs start för murky water");
        if (_destroyAfterCertainTime)
        {
            timer = new Timer(_timeToLive);
        }
    }


    void Update()
    {

        if (_destroyAfterCertainTime)
        {
            timer.Time += Time.deltaTime;

            //kanske lägga till att den typ börjar bubbla eller något, när det är någon sec tills den försvinner

            if (timer.Expired())
            {
                //kanske spela någon animation här för att förtydliga att den försvinner
                Destroy(this.gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        print("nu ser jag något");
        if (_collisionLayers == (_collisionLayers | 1 << other.gameObject.layer))
        {
            //gör skada/slow/blind eller whatever som ska hända
            print("thing found, do damage or watever");
        }
    }
}
