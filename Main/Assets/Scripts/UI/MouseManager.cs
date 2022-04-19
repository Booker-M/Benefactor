using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;
using System;
using UnityEngine.SocialPlatforms;

public class MouseManager : MonoBehaviour
{
    Vector2 currentMouseCoords;
    public static MouseManager instance = null;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        currentMouseCoords = new Vector2{};
    }

    public bool GetNextCharacter(List<Player> characters)
    {
        if (PauseMenu.instance.IsPaused())
            return true;

        UpdateMousePosition();
        
        foreach (Player character in characters)
        {
            if ((Vector2)character.transform.position == currentMouseCoords)
            {
                MenuManager.instance.HighlightPath(new Vector2[] { currentMouseCoords });

                if (Input.GetMouseButtonDown(0))
                {
                    GameManager.instance.activeCharacter = character;
                    GameManager.instance.CameraTarget(character.gameObject);
                    MenuManager.instance.HideIndicators();
                    return false;
                }
            }
            else
                MenuManager.instance.UnhighlightPath(new Vector2[] { character.transform.position });
        }

        return true;
    }

    public bool GetMoveInput(Character character, Dictionary<Vector2, Vector2[]> paths)
    {
        if (PauseMenu.instance.IsPaused())
            return true;
        UpdateMousePosition();

        if (paths.ContainsKey(currentMouseCoords))
        {
            Vector2[] path;
            paths.TryGetValue(currentMouseCoords, out path);
            character.SetPathToObjective(path);
            MenuManager.instance.HighlightPath(character.GetPathToObjective());

            if (Input.GetMouseButtonDown(0))
            {
                MenuManager.instance.HideIndicators();
                return false;
            }
        }
        else
        {
            MenuManager.instance.UnhighlightPaths();
        }

        return true;
    }

    public bool GetTargetInput(Character character, List<InteractableObject> objects)
    {
        if (PauseMenu.instance.IsPaused())
            return true;
        UpdateMousePosition();
        
        foreach (InteractableObject o in objects)
        {
            if ((Vector2)o.transform.position == currentMouseCoords || 
                (o.gameObject.GetComponent<Bed>() != null && (Vector2)o.transform.position - new Vector2(0,1) == currentMouseCoords))
            {
                MenuManager.instance.HighlightPath(new Vector2[] { currentMouseCoords });

                if (Input.GetMouseButtonDown(0))
                {
                    character.SetCurrentObjectiveTarget(o);
                    GameManager.instance.CameraTarget(o.gameObject);
                    MenuManager.instance.HideIndicators();
                    return false;
                }
            }
            else
                MenuManager.instance.UnhighlightPath(new Vector2[] { o.transform.position });
        }

        return true;
    }

    public Vector2 UpdateMousePosition()
    {
        Vector2 mouseScreenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 coords = new Vector2((int)(mouseWorldPosition.x + 0.5), (int)(mouseWorldPosition.y + 0.5));
        if (currentMouseCoords != coords) {
            MenuManager.instance.HideMouseIndicator();
            currentMouseCoords = coords;
            MenuManager.instance.ShowMouseIndicator(currentMouseCoords);
        }
        return coords;
    }
}
