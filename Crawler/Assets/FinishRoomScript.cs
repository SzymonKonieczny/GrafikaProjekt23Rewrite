using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishRoomScript : MonoBehaviour
{
    public Transform Doors;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {

        }
    }
    public void OpenDoor()
    {
        Doors.position += new Vector3(0, -10, 0);
    }
}
