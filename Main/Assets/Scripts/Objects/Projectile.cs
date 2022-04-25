using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float moveTime;
    private float inverseMoveTime;
    protected Rigidbody2D rb2D;

    // Start is called before the first frame update
    void Start()
    {
        inverseMoveTime = 1 / moveTime;
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Shoot(Vector2 end)
    {
        float sqrRemainingDistance = ((Vector2)transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector2 newPosition = Vector2.MoveTowards(transform.position, end, inverseMoveTime * Time.fixedDeltaTime);
            // rb2D.MovePosition(newPosition);
            transform.position = newPosition;
            sqrRemainingDistance = ((Vector2)transform.position - end).sqrMagnitude;
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
            yield return null;
        }
        Destroy(gameObject);
    }
}
