using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SpiralCounterMusic : MonoBehaviour
{
    

    private Transform player;
    private Transform distanceToPlayer;
    private float playerVerticalDistance;
    private float spiralTopVerticalDistance;
    [EventRef]
    //[SerializeField] string InsanityEventAudio;
    EventInstance spiralEvent;

    // Start is called before the first frame update
    private float TheMeme()
    {
        return playerVerticalDistance + spiralTopVerticalDistance * 100;
    }
    void Start()
    {
        distanceToPlayer = FindObjectOfType<Transform>();
        spiralTopVerticalDistance = distanceToPlayer.position.y;
        //spiralEvent = 
    }

    // Update is called once per frame
    void Update()
    {
        player = GlobalState.state.Player.GetComponent<Transform>();

        playerVerticalDistance = player.position.y;

        Debug.Log(playerVerticalDistance + spiralTopVerticalDistance);
     
         





    }
    public void PlayerInsanityAudio(float insanityPercentage)
    {
       // InsanityEvent = RuntimeManager.CreateInstance(InsanityEventAudio); // Create a new FMOD::Studio::EventInstance.
      //  InsanityEvent.setParameterByName("InsanityBar", insanityPercentage); // string-värdet är parameternamnet och insanitypercentage är float-värdet
      //  InsanityEvent.start(); // spelar upp ljudet
    }

}
