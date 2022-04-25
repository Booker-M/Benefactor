using UnityEngine;
using System.Collections;
 
 public class AnimatedWeapon : MonoBehaviour {
     public float delay;
     public GameObject ammo;
     protected Animator animator;
 
     // Use this for initialization
     void Start () {
        //  animator = GetComponent<Animator>();
     }

    public void Animate(bool isMale, string direction) {
        if (ammo != null) {
            AnimatedWeapon animatedWeapon = Instantiate(ammo.GetComponent<AnimatedWeapon>(), this.transform);
            animatedWeapon.Animate(isMale, direction);
        }
        animator = GetComponent<Animator>();
        animator.SetBool("male", isMale);
        animator.SetTrigger(direction);
        Destroy(gameObject, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay);
    }
 }