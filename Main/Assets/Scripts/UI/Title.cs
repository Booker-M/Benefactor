using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    private RectTransform transform;
    private float velocity;
    private bool up;

    public float maxVelocity;
    public float delta;

    private void Start()
    {
        transform = GetComponent<RectTransform>();
        up = true;
        velocity = 0;
    }

    private void Hover() {
        transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + velocity * Time.deltaTime);
        if (up) {
            velocity = velocity + delta;
            if (velocity >= maxVelocity) {
                up = false;
                velocity = maxVelocity;
            }
        } else {
            velocity = velocity - delta;
            if (velocity <= maxVelocity * -1) {
                up = true;
                velocity = maxVelocity * -1;
            }
        }
    }

    private void Update()
    {
        Hover();
    }

}
