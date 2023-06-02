using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemKeyHoleScript : MonoBehaviour, IInteractible
{
    [SerializeField] Transform GemSocket;
    [SerializeField] Item RequiredItem;
    bool canInteract = true;
    public Action OnKeyInserted;
    public void Interact(PlayerMovement player)
    {
        Debug.Log("Interacting with the heyhole");
        Item playersItem = player.GetHandItem();
        if (!canInteract) return;
        if(playersItem == null)
        {

            return;
        }
        if(playersItem.ItemID == RequiredItem.ItemID)
        {
            Transform PlayersItemTransform = player.TakeAwayHeldItem();
            if(PlayersItemTransform != null)
            {
                IInteractible gem = PlayersItemTransform.GetComponent<MagicGemScript>();
                gem.setInteractible(false);
                PlayersItemTransform.position = GemSocket.position;
                PlayersItemTransform.parent = GemSocket;
                this.setInteractible(false);
                OnKeyInserted();
            }
        }
    }

    public void setInteractible(bool b)
    {
        canInteract = b;
    }
}
