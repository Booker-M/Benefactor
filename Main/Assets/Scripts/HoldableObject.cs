using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldableObject : MonoBehaviour
{
    public Sprite icon;
    public string type; //"Medicine", "Tool", "Melee", "Gun", "Ammo", "Throwable", "Trap"
    public string name;
    public int uses; //# of uses before breaking or amount of ammo for guns
    public int amount; //amount of health to heal or to damage
    public int accuracy;
    public int critical; 
    public int range; //distance to shoot or throw an object
    public int blastRadius; //blast radius, 1 normally, over 1 for area of effect throwables
    public int copies;
    public int weight;
    public GameObject effect;
    public GameObject projectile;
    public bool power;

    // Start is called before the first frame update
    void Start()
    {

    }
}
