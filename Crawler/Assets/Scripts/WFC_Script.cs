using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crawler;
namespace Crawler
{
    enum RoomType
    {
        Hall,
        RoomCorner
    }
    struct Tile 
    {

        public Vector2Int Position;
        public RoomType Room;
        public Quaternion Rotation;
        void SetRotation(Vector3 Rot)
        {
            Rotation = Quaternion.Euler(Rot);
        }
    }
}
public class WFC_Script : MonoBehaviour
{
    [SerializeField] List<GameObject> RoomPrefabs = new List<GameObject>();
    [SerializeField] Dictionary<Vector2Int, GameObject> PlacedRooms = new Dictionary<Vector2Int, GameObject>();
    
    private void PlaceRoom(Tile tile)
    {
        GameObject go = Instantiate(RoomPrefabs[(int)tile.Room]);
        go.transform.position = new Vector3(tile.Position.x, 0, tile.Position.y);
        PlacedRooms.Add(tile.Position, go);
   
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }


    
}
