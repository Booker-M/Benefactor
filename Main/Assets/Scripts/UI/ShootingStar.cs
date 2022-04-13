using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootingStar : MonoBehaviour
{
    private RectTransform position;
    private Count xRange;
    private Count yRange;
    private float speed;
    private bool falling;

    public float yVelocityInitial;
    private float yVelocity;
    public float xVelocity;

    public class Count
    {
        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    private void Start()
    {
        RectTransform transform = GetComponent<RectTransform>();
        xRange = new Count(-Screen.width/3, Screen.width);
        yRange = new Count(0, Screen.height);
        speed = Random.Range(0.7f, 2.5f);
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn(bool start = true) {
        falling = false;
        if (!start)
            yield return new WaitForSeconds(1.0f);
        GetComponent<TrailRenderer>().emitting = false;
        transform.localPosition = new Vector2(Random.Range(xRange.minimum, xRange.maximum), yRange.maximum);
        yield return new WaitForSeconds(Random.Range(0.0f,15.0f));
        GetComponent<TrailRenderer>().Clear();
        GetComponent<TrailRenderer>().emitting = true;
        yVelocity = yVelocityInitial;
        falling = true;
    }

    private void Fall() {
        transform.localPosition = new Vector2(transform.localPosition.x + xVelocity * speed * Time.deltaTime, transform.localPosition.y + yVelocity * speed * Time.deltaTime);
        yVelocity = yVelocity * (1 + 0.4f*Time.deltaTime);
    }

    private void Update()
    {
        if (transform.localPosition.x < xRange.maximum && transform.localPosition.y > yRange.minimum) {
            if (falling)
                Fall();
        } else {
            if (falling)
                StartCoroutine(Spawn(false));
        }
    }



}
