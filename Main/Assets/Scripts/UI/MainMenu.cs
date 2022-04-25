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

public class MainMenu : MonoBehaviour
{
    private float alphaIncrementRate = .02f;
    private float alphaIncrementAmount = 3f;
    private Image cover;

    void Start() {
        cover = GameObject.Find("Cover").GetComponent<Image>();
        cover.raycastTarget = false;
    }

    public void NewGame()
    {
        StartCoroutine(SwitchScene("PrologueCutscene"));
    }

    public void Continue()
    {
        StartCoroutine(SwitchScene("SampleScene"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator SwitchScene(String scene)
    {
        cover.raycastTarget = true;

        while (cover.color.a <= 1)
        {
            cover.color += new Color (0, 0, 0, alphaIncrementAmount*Time.deltaTime);
            yield return new WaitForSeconds(alphaIncrementRate);
        }

        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
