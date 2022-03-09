using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitManager : MonoBehaviour
{
    Image ImageComponent;

    public Sprite Unknown;
    public Sprite Ranger;
    public Sprite Policeman;
    public Sprite Policewoman;
    public Sprite MaleScientist;
    public Sprite FemaleScientist;
    public Sprite MaleCultist;
    public Sprite FemaleCultist;
    public Sprite OlderBrother;
    public Sprite YoungerBrother;

    public void Awake()
    {
        ImageComponent = GetComponent<Image>();
    }

    public void changePortrait(string newPortraitName)
    {
        switch (newPortraitName)
        {
            case "Unknown": 
                ImageComponent.sprite = Unknown;
                break;
            case "Ranger": 
                ImageComponent.sprite = Ranger;
                break;
            case "Policeman": 
                ImageComponent.sprite = Policeman;
                break;
            case "Policewoman": 
                ImageComponent.sprite = Policewoman;
                break;
            case "MaleScientist": 
                ImageComponent.sprite = MaleScientist;
                break;
            case "FemaleScientist": 
                ImageComponent.sprite = FemaleScientist;
                break;
            case "MaleCultist": 
                ImageComponent.sprite = MaleCultist;
                break;
            case "FemaleCultist": 
                ImageComponent.sprite = FemaleCultist;
                break;
            case "OlderBrother": 
                ImageComponent.sprite = OlderBrother;
                break;
            case "YoungerBrother":
                ImageComponent.sprite = YoungerBrother;
                break;
            default:
                Debug.LogError("Invalid portrait name: " + newPortraitName);
                break;

        }
    }

}
