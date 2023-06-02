using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MagicGemScript : MonoBehaviour, IInteractible
{
    [SerializeField]Item item;
    [SerializeField]  bool canInteract = true;
    [SerializeField] Rigidbody rb;
    public void Interact(PlayerMovement player)
    {
        if (!canInteract) return;

        player.AttatchToHand(transform);
        player.SetHandItem(item);

    }

    public void setInteractible(bool b)
    {
        canInteract = b;
        rb.isKinematic = !b;
    }
}
