using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTestPickUpScript : MonoBehaviour, IInteractible
{
    public void Interact(PlayerMovement player)
    {

        player.AttatchToHand(transform);
       
    }

    void IInteractible.Interact(PlayerMovement player)
    {
        throw new System.NotImplementedException();
    }

    void IInteractible.setInteractible(bool b)
    {
        throw new System.NotImplementedException();
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
