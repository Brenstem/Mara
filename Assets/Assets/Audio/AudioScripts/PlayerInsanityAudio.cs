using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class PlayerInsanityAudio : MonoBehaviour
{
    [EventRef]
    public string InsanityEventAudio;
    EventInstance InsanityEvent;
    private PlayerInsanity playerInsanity;
    float insanitypercentage;
   

    // Start is called before the first frame update
    void Start()
    {
        playerInsanity = playerInsanity.GetComponent<PlayerInsanity>();
     
    }

    // Update is called once per frame
    void Update()
    {
        insanitypercentage = playerInsanity.GetInsanityPercentage();
    }
    public void PlayerInsanity()
    {
        InsanityEvent = RuntimeManager.CreateInstance(InsanityEventAudio); // Create a new FMOD::Studio::EventInstance.
        InsanityEvent.setParameterByName("InsanityBar", insanitypercentage); // 0f behöver ersättas av floatvärdet i getinsanitypercentage (tror jag gjort rätt)
        if(insanitypercentage > 1)
        {
            InsanityEvent.start();
        }
        if(insanitypercentage < 1)
        {
            InsanityEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        //InsanityEvent.start(); startar ljudet
    }
}
