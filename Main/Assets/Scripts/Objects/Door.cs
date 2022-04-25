using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractableObject
{
    public bool locked;
    public bool takesKey;
    public DoorSprite doorSprite;
    public Lever trigger;
    public float openTime;

    public GameObject edge;
    public GameObject backEdge;

    protected bool open = false;
    protected bool side = false;
    protected Animator animator;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        animator.SetBool("Open", false);
        SetSprite();
    }

    void SetSprite()
    {
        bool isUp = false;
        bool isDown = false;
        bool isLeft = false;
        bool isRight = false;
        bool isRightFront = false;
        bool isLeftFront = false;
       
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(0, 1), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<Wall>() != null)
                    isUp = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(0, -1), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<Wall>() != null)
                    isDown = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(-1, 0), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                Wall wall = hitCollider.GetComponent<Wall>();
                if (wall != null) {
                    isLeft = true;
                    isLeftFront = wall.GetInFront();
                }
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(1, 0), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                Wall wall = hitCollider.GetComponent<Wall>();
                if (wall != null) {
                    isRight = true;
                    isRightFront = wall.GetInFront();
                }
            }

        if (isRight && isLeft)
            animator.SetBool("Front", (isRightFront || isLeftFront));
        else if (isUp && isDown) {
            animator.SetBool("Side", true);
            side = true;
            edge = Instantiate(edge, transform.position - new Vector3(0,1,0.1f), Quaternion.identity);
            backEdge = Instantiate(backEdge, transform.position + new Vector3(0,0,0.9f), Quaternion.identity);
        }
    }

    public IEnumerator Toggle()
    {
        if (open)
            Close();
        else
            Open();

        yield return new WaitForSeconds(openTime);
    }

    void Open()
    {
        open = true;
        walkOver = true;
        animator.SetBool("Open", true);
        GameManager.instance.UpdateNode(transform.position, true, 0);
        SoundManager.instance.OpenDoor();
    }

    void Close()
    {
        open = false;
        walkOver = false;
        animator.SetBool("Open", false);
        UpdatePosition();
        SoundManager.instance.CloseDoor();
    }

    public void Unlock()
    {
        locked = false;
        SoundManager.instance.Unlock();
    }

    public bool IsOpen()
    {
        return open;
    }

    public override SortedSet<String> GetActions()
    {
        receiveActions = base.GetActions();
        if (!locked)
            receiveActions.Add("Door");
        else if (takesKey)
            receiveActions.Add("Unlock");

        return receiveActions;
    }

    protected override void UpdatePosition()
    {
        GameManager.instance.UpdateNode(transform.position, !locked || damageable, -2);
    }

    public void SetupTrigger(Vector2 position)
    {
        trigger = Instantiate(trigger, position, Quaternion.identity);
        trigger.SetTarget(this);
    }

    public Lever GetTrigger()
    {
        return trigger;
    }

    protected override void ErasePosition()
    {
        if (side) {
            GameObject.Destroy(edge.gameObject);
            GameObject.Destroy(backEdge.gameObject);
        }
        base.ErasePosition();
    }
}
