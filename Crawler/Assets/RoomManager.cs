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
    List<Transform> Rooms;
    [SerializeField]Transform ChosenRoom;
    [SerializeField] Transform ChosenExit;

    void Start()
    {
        RoomPlacer = _RoomPlacer as IRoomPlacer;
        StartCoroutine(GenerateDungeon());
    }
    IEnumerator GenerateDungeon()
    {
        yield return StartCoroutine(RoomPlacer.GenerateMap());
        Rooms = RoomPlacer.GetRoomTransforms();
        ChosenRoom = Rooms[UnityEngine.Random.Range(0, Rooms.Count)];
        Rooms.Sort((Transform a, Transform b) =>
        {
            float distanceA = Vector3.Distance(a.position, ChosenRoom.transform.position);
            float distanceB = Vector3.Distance(b.position, ChosenRoom.transform.position);

            if (distanceA > distanceB) return 1;
            else return -1;
        });
        ChosenExit = Rooms[Rooms.Count-1];
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
