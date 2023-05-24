using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTestPickUpScript : MonoBehaviour, IInteractible
{
    public void Interact(Look Look, MovementScript movement)
    {

        Look.AttatchToHand(transform);
       
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
