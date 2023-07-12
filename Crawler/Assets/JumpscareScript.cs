using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpscareScript : MonoBehaviour
{
    public Animator animator;
    private void Awake()
    {
        animator.SetBool("Jumpscare", true);
        animator.SetBool("Idle", false); //cannot go from idle to jumpscare
    }
}
