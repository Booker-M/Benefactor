using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackdropManager : MonoBehaviour
{
    public Image ImageComponent;

    public Sprite RangerTower;
    public Sprite ForestMorning;
    public Sprite ForestDay;
    public Sprite ForestNight;
    public Sprite Crater;

    public void changeBackdrop(string newBackdropName)
    {
        switch (newBackdropName)
        {
            case "RangerTower":
                ImageComponent.sprite = RangerTower;
                break;
            case "ForestMorning":
                ImageComponent.sprite = ForestMorning;
                break;
            case "ForestDay":
                ImageComponent.sprite = ForestDay;
                break;
            case "ForestNight":
                ImageComponent.sprite = ForestNight;
                break;
            case "Crater":
                ImageComponent.sprite = Crater;
                break;
            default:
                Debug.LogError("Invalid portrait name: " + newBackdropName);
                break;
        }

    }

}
