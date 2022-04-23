using UnityEngine;
using System.Collections;
 
 public class AnimatedWeapon : MonoBehaviour {
     public float delay = 0f;
     protected Animator animator;
 
     // Use this for initialization
     void Start () {
        //  animator = GetComponent<Animator>();
     }

    public void Animate(bool isMale, string direction) {
        animator = GetComponent<Animator>();
        animator.SetBool("male", isMale);
        animator.SetTrigger(direction);
        Destroy(gameObject, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay);
    }
 }