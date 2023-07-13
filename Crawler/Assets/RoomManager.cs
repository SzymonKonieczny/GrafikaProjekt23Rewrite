using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.AI.Navigation;


public class RoomManager : MonoBehaviour
{
   // [RequiredInterface(typeof(IRoomPlacer))]
    public WFC _RoomPlacer;
    public IRoomPlacer RoomPlacer;
    // Start is called before the first frame update
    public List<Transform> Rooms;
    [SerializeField]RoomInteriorScript ChosenRoom;
    [SerializeField] RoomInteriorScript ChosenExit;
    [SerializeField] RoomInteriorScript ChosenGemRoom;

    [SerializeField] NavMeshSurface FloorNavigationMesh;
    [SerializeField]GameObject GemPrefab;
    [SerializeField] GameObject KeyHolePrefab;
    [SerializeField] GameObject DoorPrefab;

    [SerializeField] GameObject Player;

    [SerializeField] GameObject EnemyPrefab;



    [SerializeField] FinishRoomScript doors;
    public VictoryUI VictoryScreen;

    public LoadingUI LoadingScreen;
    public float GameStartTimestamp;

    public static RoomManager instance;
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        Time.timeScale = 1;
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    void Start()
    {
        RoomPlacer = _RoomPlacer as IRoomPlacer;
        StartCoroutine(GenerateDungeon());  
    }


    IEnumerator GenerateDungeon()
    {
        HashSet<int> OccupiedRooms = new HashSet<int>();
        LoadingScreen.gameObject.SetActive(true);
        LoadingScreen.SetSliderCompletion(0.1f);
        LoadingScreen.SetText("Generating Level ...");
        yield return new WaitForSeconds(0.1f);


        yield return StartCoroutine(RoomPlacer.GenerateMap());

        LoadingScreen.SetSliderCompletion(0.5f);
        LoadingScreen.SetText("Generating Rooms ...");

        yield return new WaitForSeconds(0.1f);

        Rooms = RoomPlacer.GetRoomTransforms();
        int index = UnityEngine.Random.Range(0, Rooms.Count - 1);
        Debug.Log("Choosing room nr" + index);
        ChosenRoom = Rooms[index].GetComponent<RoomInteriorScript>();
        Rooms.Sort((Transform a, Transform b) =>
        {
            float distanceA = Vector3.Distance(a.position, ChosenRoom.transform.position);
            float distanceB = Vector3.Distance(b.position, ChosenRoom.transform.position);

            if (distanceA > distanceB) return 1;
            else return -1;
        });

        ChosenExit = Rooms[Rooms.Count-1].GetComponent<RoomInteriorScript>();

        ChosenGemRoom = Rooms[Rooms.Count/2].GetComponent<RoomInteriorScript>();

        ChosenGemRoom.PlaceInRoom(GemPrefab, RoomSocket.Gem);
       GameObject Keyhole =  ChosenRoom.PlaceInRoom(KeyHolePrefab, RoomSocket.KeyHole);
        if(Keyhole!=null)
        {
            GemKeyHoleScript GemHole = Keyhole.GetComponent<GemKeyHoleScript>();
            GemHole.OnKeyInserted = this.OnKeyInsterted;
        }

        GameObject DoorObj = ChosenExit.PlaceInRoom(DoorPrefab, RoomSocket.Door);
        if (DoorObj != null)
        {
             doors = DoorObj.GetComponent<FinishRoomScript>();
            doors.OnMazeLeave = OnMazeLeave;
        }

        LoadingScreen.SetSliderCompletion(0.7f);
        LoadingScreen.SetText("Generating NavigationMesh ...");
        yield return new WaitForSeconds(0.1f);

        FloorNavigationMesh.BuildNavMesh();


        LoadingScreen.SetSliderCompletion(0.99f);
        LoadingScreen.SetText("Generating Enemies ...");
        yield return new WaitForSeconds(0.1f);

        SpawnEnemies(index, Rooms.Count - 3, Rooms.Count / 2);
        OccupiedRooms.Add(index);
        OccupiedRooms.Add(Rooms.Count - 3);
        OccupiedRooms.Add(Rooms.Count / 2);

        index = GetPlayerSpawn(OccupiedRooms);
        Player.transform.SetPositionAndRotation(Rooms[index].position + new Vector3(1,1,1) , Player.transform.rotation);

        LoadingScreen.gameObject.SetActive(false);
        GameStartTimestamp = Time.time;

    }
    int GetPlayerSpawn(HashSet<int> OccupiedRooms)
    {
        int res= UnityEngine.Random.Range(0, Rooms.Count - 1);
        while (OccupiedRooms.Contains(res))
        {
            res = UnityEngine.Random.Range(0, Rooms.Count - 1);
        }
        return res;
    }
    void SpawnEnemies(int a, int b, int c)
    {
        Instantiate(EnemyPrefab, Rooms[a].position, Rooms[a].rotation);

        Instantiate(EnemyPrefab, Rooms[b].position, Rooms[b].rotation);

        Instantiate(EnemyPrefab, Rooms[c].position, Rooms[c].rotation);
    }
    public void OnKeyInsterted()
    {
        doors.OpenDoor();
    }
    public void OnMazeLeave()
    {
        Time.timeScale = 0;
        VictoryScreen.gameObject.SetActive(true);
        VictoryScreen.setText("You escaped in : " + (int)( Time.time- GameStartTimestamp) + "seconds!");
        
    }

    public GameOverUI GameOverScreen;
    public void onCaught()
    {
        GameOverScreen.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
