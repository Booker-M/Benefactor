using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roof : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private float curOpacity;
    private float targetOpacity;
    private int roofIndex;

    public Sprite flat;
    public Sprite front;
    public Sprite left;
    public Sprite right;
    public Sprite middle;
    public Sprite frontInnerCornerLeft;
    public Sprite frontInnerCornerRight;
    public Sprite frontOuterCornerLeft;
    public Sprite frontOuterCornerRight;
    public Sprite backCornerLeft;
    public Sprite backCornerRight;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        SetSprite();
        Show();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(targetOpacity - curOpacity) < 1)
            curOpacity = targetOpacity;
        else
        {
            if (curOpacity < targetOpacity)
                curOpacity += 200 * Time.deltaTime;
            else
                curOpacity -= 200 * Time.deltaTime;
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, curOpacity / 100);
    }

    void SetSprite()
    {
        bool isUp = false;
        bool isDown = false;
        bool isLeft = false;
        bool isRight = false;
        bool isDownLeft = false;
        bool isDownRight = false;

        if (isDown && isUp && isLeft && isRight && !isDownLeft)
            spriteRenderer.sprite = frontInnerCornerLeft;
        else if (isDown && isUp && isLeft && isRight && !isDownRight)
            spriteRenderer.sprite = frontInnerCornerRight;
        else if (isDown && isLeft && isRight)
            spriteRenderer.sprite = flat;
        else if (isUp && isLeft && isRight)
            spriteRenderer.sprite = front;
        else if (isUp && isDown && isRight)
            spriteRenderer.sprite = left;
        else if (isUp && isDown && isLeft)
            spriteRenderer.sprite = right;
        else if (isUp && isRight)
            spriteRenderer.sprite = frontOuterCornerLeft;
        else if (isUp && isLeft)
            spriteRenderer.sprite = frontOuterCornerRight;
        else if (isDown && isRight)
            spriteRenderer.sprite = backCornerLeft;
        else if (isDown && isLeft)
            spriteRenderer.sprite = backCornerRight;
    }

    public void hideRoof()
    {
        if (!GetOverPlayer())
        {
            checkRoofs();
        }
    }

    public void showRoof()
    {
        if (GetOverPlayer())
        {
            checkRoofs();
        }
    }

    public void checkRoofs()
    {
        bool roofOverPlayer = false;
        // foreach (Roof roof in GameManager.instance.Roofs[roofIndex])
        // {
        //     if (roof.GetOverPlayer())
        //         roofOverPlayer = true;
        // }
        if (roofOverPlayer)
            HideRoofs();
        else
            ShowRoofs();
    }

    void HideRoofs()
    {
        // foreach (Roof roof in GameManager.instance.Roofs[roofIndex])
        //     roof.Hide();
    }

    void ShowRoofs()
    {
        // foreach (Roof roof in GameManager.instance.Roofs[roofIndex])
        //     roof.Show();
    }

    public void Hide()
    {
        targetOpacity = 0f;
    }

    public void Show()
    {
        targetOpacity = 100f;
    }

    public bool Hidden()
    {
        return targetOpacity == 0f;
    }

    public void setRoofIndex(int index)
    {
        roofIndex = index;
    }

    public bool GetOverPlayer()
    {
        foreach (Player player in GameManager.instance.characters) {
            if (player.playable && (Vector2)transform.position == (Vector2)player.transform.position) {
                return true;
            }
        }
        return false;
    }
}
