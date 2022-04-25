using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : InteractableObject
{
    public Leaves leaves;
    public Sprite overview;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        leaves = Instantiate(leaves, transform.position, Quaternion.identity);
        leaves.SetTree(this);
    }

    protected override IEnumerator animateDeath() {
        yield return StartCoroutine(FadeOut());
        GameObject.Destroy(leaves.gameObject);
        gameObject.SetActive(false);
    }

    protected override IEnumerator FadeOut()
     {
         float alphaVal = spriteRenderer.material.color.a;
         Color tmp = spriteRenderer.material.color;
 
         while (spriteRenderer.material.color.a > 0)
         {
             alphaVal -= 0.2f*Time.fixedDeltaTime;
             tmp.a = alphaVal;
             spriteRenderer.material.color = tmp;
             leaves.GetComponent<SpriteRenderer>().material.color = tmp;
 
             yield return null; // update interval
         }
     }
 
    protected override IEnumerator FadeIn()
     {
         float alphaVal = spriteRenderer.material.color.a;
         Color tmp = spriteRenderer.material.color;
 
         while (spriteRenderer.material.color.a < 1)
         {
             alphaVal += 0.2f*Time.fixedDeltaTime;
             tmp.a = alphaVal;
             spriteRenderer.material.color = tmp;
             leaves.GetComponent<SpriteRenderer>().material.color = tmp;
 
             yield return null; // update interval
         }
     }
}
