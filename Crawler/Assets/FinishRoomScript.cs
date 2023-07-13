using System.Collections;
using System;

using System.Collections.Generic;
using UnityEngine;

public class FinishRoomScript : MonoBehaviour
{
    public Transform Doors;
    public Action OnMazeLeave;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            OnMazeLeave();
        }
    }
    public void OpenDoor()
    {
        Doors.position += new Vector3(0, -10, 0);
    }
}
