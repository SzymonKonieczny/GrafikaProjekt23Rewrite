using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
public class MovementScript : MonoBehaviour
{

    public CharacterController controller;

    public Camera cam;
    public float speed = 10;
    public float SprintMultiplier=2;
    float SprintMultiplier_with_check=1;
    public float Gravity = -1.81f;
    public Transform GroundCheck;
    public float GroundDistancs = 0.5f;
    public LayerMask groundMask;
    public int maxHP;
  


    int HP;
    bool isGrounded;
    float x;
    Vector3 velocity;
    float z;
    Vector3 move;

    // Update is called once per frame
    private void Start()
    {

        Debug.Log("Starting Movement Sciprt");
    }
    void Update()
    {
      isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistancs, groundMask);
        if(isGrounded && velocity.y <0)
           {
           velocity.y = 0f;
           }

   
        x = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");
        move = transform.right * x + transform.forward * z;
        


      
      


        controller.Move(move * speed * SprintMultiplier_with_check * Time.deltaTime);

        if (isGrounded && Input.GetKey(KeyCode.Space)) velocity.y = 5f;

        velocity.y += Gravity * Time.deltaTime ; 

       controller.Move(velocity * Time.deltaTime);
    }
  
}
