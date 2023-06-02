using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RoomManager : MonoBehaviour
{
   // [RequiredInterface(typeof(IRoomPlacer))]
    public WFC_Script _RoomPlacer;
    public IRoomPlacer RoomPlacer;
        // Start is called before the first frame update

    void Start()
    {
        RoomPlacer = _RoomPlacer as IRoomPlacer;
        StartCoroutine(GenerateDungeon());
    }
    IEnumerator GenerateDungeon()
    {
        yield return StartCoroutine(RoomPlacer.GenerateMap());


    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
