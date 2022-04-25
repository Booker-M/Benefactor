using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    private float moveSpeed;
    public bool followMouse;
    protected GameObject toFollow;
    protected Vector2 minPosition = new Vector2(8.5f, 4.5f);
    private MenuManager menuManager;

    // Start is called before the first frame update
    void Start()
    {
        // transform.position = new Vector3(minPosition.x, minPosition.y, -10);
        transform.position = new Vector3(15, minPosition.y, -10);
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseMenu.instance.IsPaused() && (followMouse || toFollow != null)) {
            Vector3 target = new Vector3();
            if (followMouse)
            {
                Vector2 mouseScreenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                double moveFraction = 0.1; //only move camera if the mouse is this close to edge of screen
                if (mouseScreenPosition.x > Screen.width * moveFraction && mouseScreenPosition.x < Screen.width * (1-moveFraction) &&
                    mouseScreenPosition.y > Screen.height * moveFraction && mouseScreenPosition.y < Screen.height * (1-moveFraction))
                {
                    return;
                }

                Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
                target = mouseWorldPosition;
            }
            else
                target = toFollow.transform.position;
            target.z = -10;
            float distance = Math.Abs(transform.position.x - target.x) + Math.Abs(transform.position.y - target.y);
            if (distance < 0.03)
            {
                transform.position = new Vector3(target.x, target.y, -10);
            }
            else
            {
                moveSpeed = Math.Max(distance * 2, 0.03f);
                Vector3 newPosition = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                BoardManager board = GameManager.instance.GetComponent<BoardManager>();
                Vector2 maxPosition = new Vector2((float)board.columns - minPosition.x - 1, (float)board.rows - minPosition.y - 1);
                if (newPosition.x < minPosition.x * (Camera.main.orthographicSize/5))
                    newPosition.x = minPosition.x * (Camera.main.orthographicSize/5);
                if (newPosition.y < minPosition.y * (Camera.main.orthographicSize/5))
                    newPosition.y = minPosition.y * (Camera.main.orthographicSize/5);
                if (newPosition.x > maxPosition.x * (Camera.main.orthographicSize/5))
                    newPosition.x = maxPosition.x * (Camera.main.orthographicSize/5);
                if (newPosition.y > maxPosition.y * (Camera.main.orthographicSize/5))
                    newPosition.y = maxPosition.y * (Camera.main.orthographicSize/5);
                transform.position = newPosition;
            }
        }
    }

    public void Target(GameObject newTarget)
    {
        toFollow = newTarget;
    }

    public void FollowMouse()
    {
        followMouse = true;
        // Debug.Log("FOLLOW MOUSE");
    }

    public void FollowTarget()
    {
        followMouse = false;
        menuManager.HideMouseIndicator();
        // Debug.Log("FOLLOW TARGET");
    }

}
