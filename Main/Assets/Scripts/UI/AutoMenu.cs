using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;
using System;
using UnityEngine.SocialPlatforms;
using UnityEditor;

public class AutoMenu : MonoBehaviour
{

    public void Auto()
    {
        GameManager.instance.auto = !GameManager.instance.auto;
    }
}
 