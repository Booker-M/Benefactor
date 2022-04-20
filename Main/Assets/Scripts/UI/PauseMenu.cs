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

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance = null;

    public GameObject pauseMenu;
    public Button pauseButton;
    private bool paused;

    void Start() {
        Resume();
    }

    void Awake() {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        
        // DontDestroyOnLoad(gameObject);
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        paused = true;
        pauseButton.interactable = false;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        paused = false;
        pauseButton.interactable = true;
    }

    public void Exit() {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public bool IsPaused() {
        return paused;
    }
}
 