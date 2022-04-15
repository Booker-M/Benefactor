using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackdropManager : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

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
                spriteRenderer.sprite = RangerTower;
                break;
            case "ForestMorning":
                spriteRenderer.sprite = ForestMorning;
                break;
            case "ForestDay":
                spriteRenderer.sprite = ForestDay;
                break;
            case "ForestNight":
                spriteRenderer.sprite = ForestNight;
                break;
            case "Crater":
                spriteRenderer.sprite = Crater;
                break;
            default:
                Debug.LogError("Invalid portrait name: " + newBackdropName);
                break;
        }

    }

}
