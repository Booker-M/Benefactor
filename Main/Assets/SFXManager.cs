using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public AudioSource SFXSource;
    public static SFXManager instance = null;

    public float lowPitchRange = .95f;
    public float highPitchRange = 1.05f;

    // SFX List
    public AudioClip FootstepsApproaching;
    public AudioClip FootstepsLeaving;
    public AudioClip DoorOpening;
    public AudioClip Punch1;
    public AudioClip Punch2;


    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void PlaySingle(string clip)
    {
        switch (clip)
        {
            case "FootstepsApproaching":
                SFXSource.clip = FootstepsApproaching;
                break;
            case "FootstepsLeaving":
                SFXSource.clip = FootstepsLeaving;
                break;
            case "DoorOpening":
                SFXSource.clip = DoorOpening;
                break;
            case "Punch":
                SFXSource.clip = Punch1;
                break;
            case "Punch2":
                SFXSource.clip = Punch2;
                break;
            default:
                Debug.Log("No SFX found for " + clip);
                break;
        }
        SFXSource.Play();
    }

}
