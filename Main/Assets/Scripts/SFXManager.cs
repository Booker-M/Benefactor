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
    public AudioClip Fall;
    public AudioClip Crash;
    public AudioClip FootstepsApproaching;
    public AudioClip FootstepsLeaving;
    public AudioClip DoorOpening;
    public AudioClip Punch1;
    public AudioClip Punch2;

    public AudioClip Die;
    public AudioClip Charge;
    public AudioClip Miss;

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
            case "Fall":
                SFXSource.clip = Fall;
                break;
            case "Crash":
                SFXSource.clip = Crash;
                break;
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
            case "Die":
                SFXSource.clip = Die;
                break;
            case "Charge":
                SFXSource.clip = Charge;
                break;
            case "Miss":
                SFXSource.clip = Miss;
                break;
            default:
                Debug.Log("No SFX found for " + clip);
                break;
        }
        SFXSource.Play();
    }

    public void PlaySingle(AudioClip clip)
    {
        SFXSource.clip = clip;
        SFXSource.Play();
    }

    //public void RandomizeSfx(params AudioClip[] clips)
    //{
    //    int randomIndex = Random.Range(0, clips.Length);
    //    float randomPitch = Random.Range(lowPitchRange, highPitchRange);

    //    SFXSource.pitch = randomPitch;
    //    SFXSource.clip = clips[randomIndex];
    //    SFXSource.Play();
    //}

}
