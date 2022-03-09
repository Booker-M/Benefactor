using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    // Essentially a copy of SFX Manager, but this will repeat indefinately until a scene is over
    // Use the introManager "fadeout" statement to stop this audio
    public AudioSource AmbienceSource;
    public static AmbienceManager instance = null;

    public float lowPitchRange = .95f;
    public float highPitchRange = 1.05f;

    // Ambient SFX List
    public AudioClip Town;
    public AudioClip Bar;

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
            case "Town":
                AmbienceSource.clip = Town;
                break;
            case "Bar":
                AmbienceSource.clip = Bar;
                break;
            default:
                Debug.Log("No Ambient SFX found for " + clip);
                break;
        }
        AmbienceSource.Play();
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
