using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RoomManager : MonoBehaviour
{
   // [RequiredInterface(typeof(IRoomPlacer))]
    public WFC _RoomPlacer;
    public IRoomPlacer RoomPlacer;
    // Start is called before the first frame update
    [SerializeField] List<Transform> Rooms;
    [SerializeField]RoomInteriorScript ChosenRoom;
    [SerializeField] RoomInteriorScript ChosenExit;
    [SerializeField]GameObject GemPrefab;
    [SerializeField] GameObject KeyHolePrefab;
    [SerializeField] GameObject DoorPrefab;

    DoorScripit doors;

    void Start()
    {
        RoomPlacer = _RoomPlacer as IRoomPlacer;
        StartCoroutine(GenerateDungeon());
    }
    IEnumerator GenerateDungeon()
    {
        yield return StartCoroutine(RoomPlacer.GenerateMap());
        Rooms = RoomPlacer.GetRoomTransforms();
        ChosenRoom = Rooms[UnityEngine.Random.Range(0, Rooms.Count-1)].GetComponent<RoomInteriorScript>();
        Rooms.Sort((Transform a, Transform b) =>
        {
            float distanceA = Vector3.Distance(a.position, ChosenRoom.transform.position);
            float distanceB = Vector3.Distance(b.position, ChosenRoom.transform.position);

            if (distanceA > distanceB) return 1;
            else return -1;
        });
        ChosenExit = Rooms[Rooms.Count-1].GetComponent<RoomInteriorScript>();
        ChosenRoom.PlaceInRoom(GemPrefab, RoomSocket.Gem);
       GameObject Keyhole =  ChosenRoom.PlaceInRoom(KeyHolePrefab, RoomSocket.KeyHole);
        if(Keyhole!=null)
        {
            GemKeyHoleScript GemHole = Keyhole.GetComponent<GemKeyHoleScript>();
            GemHole.OnKeyInserted = this.OnKeyInsterted;
        }

        GameObject DoorObj = ChosenExit.PlaceInRoom(DoorPrefab, RoomSocket.Door);
        if (DoorObj != null)
        {
             doors = DoorObj.GetComponent<DoorScripit>();
        }
    }
    public void OnKeyInsterted()
    {
        doors.OpenDoor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
