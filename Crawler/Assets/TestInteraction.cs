using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteraction : MonoBehaviour, IInteractible
{
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Interact(PlayerMovement player)
   {
        rb.velocity = new Vector3(0, 5, 0);
   }

    void IInteractible.Interact(PlayerMovement player)
    {
        throw new System.NotImplementedException();
    }

    void IInteractible.setInteractible(bool b)
    {
        throw new System.NotImplementedException();
    }
}
