using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScripit : MonoBehaviour
{
    public Material Red;
    public MeshRenderer MR;
    public void OpenDoor()
    {
        MR.material = Red;
    }
}
