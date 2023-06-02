using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crawler;

namespace Crawler {
    enum TileType {
        Empty,
        Hall_I,
        Hall_L,
        Hall_T,
        Hall_X,
        Room_T,
        Room_I,
        Room_L,
        Room_Floor
    }

    struct Tile {
        public Vector3 Position;
        public Quaternion Rotation;
        public TileType Type;

        public void SetRotation(int Rotation)
        {
            this.Rotation = Quaternion.Euler(new Vector3(0, (Rotation%4)*90, 0));
        }
        public void SetPosition(float x, float z)
        {
            this.Position = new Vector3(x, 0, z);
        }
        public void SetPosition(float x, float y, float z)
        {
            this.Position = new Vector3(x, y, z);
        }
        public void SetPosition(Vector3 pos)
        {
            this.Position = pos;
        }

        public Vector3 GetPositionScaled(float GridSize)
        {
            return this.Position * GridSize;
        }
        public Vector2Int GetPositionVec2()
        {
            Vector2Int ret = new Vector2Int((int)this.Position.x, (int)this.Position.z);
            return ret;
        }

        public Tile(TileType t, int r)
        {
            this.Type = t;
            this.Position = new Vector3(0, 0, 0);
            this.Rotation = new Quaternion();
            this.SetRotation(r);
        }
        public Tile(TileType t, int r, Vector2 p)
        {
            this.Type = t;
            this.Position.x = p.x;
            this.Position.z = p.y;
            this.Position.y = 0;
            this.Rotation = new Quaternion();
            this.SetRotation(r);
        }
        public Tile(TileType t, int r, Vector3 p)
        {
            this.Type = t;
            this.Position = p;
            this.Rotation = new Quaternion();
            this.SetRotation(r);
        }
    };
}

public class WFC : MonoBehaviour, IRoomPlacer
{
    [SerializeField] List<GameObject> RoomPrefabs = new List<GameObject>();
    [SerializeField] Dictionary<Vector2Int, GameObject> PlacedRooms = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] Dictionary<Vector2Int, List<Tile>> WFCMap;
    [SerializeField] Dictionary<Vector2Int, Tile> FinalWFCMap;


    [SerializeField] List<Tile> TileTemplates = new List<Tile>();
    [SerializeField] Vector2Int MapSize = new Vector2Int(15, 15);
    public float GridSize = 18;
    public bool WFCDelay = true;

    public int PlacedFloors = 0;

    public List<Transform> GetRoomTransforms() {
        Debug.LogError("TODO: GetRoomTransforms()");
        return new List<Transform>();
    }

    private void InitTemplates() {
    }

    private void InitMap(int width, int height)
    {
        InitTemplates();
        Dictionary<Vector2Int, List<Tile>> Map = new Dictionary<Vector2Int, List<Tile>>();

        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                Map.Add(new Vector2Int(x, y), new List<Tile>(TileTemplates));
            }
        }

        WFCMap = new Dictionary<Vector2Int, List<Tile>>(Map);
    }

    private bool Continue()
    {
        foreach(KeyValuePair<Vector2Int, List<Tile>> Cell in WFCMap)
        {
            if (Cell.Value.Count > 1) { return true; }
        }
        return false;
    }

    private Vector2Int GetMinEntropyIdx()
    {
        Vector2Int CurrIdx = new Vector2Int(0, 0);
        int CurrMin = TileTemplates.Count;
        int Entropy = 0;

        foreach(KeyValuePair<Vector2Int, List<Tile>> Cell in WFCMap)
        {
            if (Cell.Value.Count <= 1) continue;
            Entropy = Cell.Value.Count;
            if (Entropy < CurrMin) {
                CurrMin = Entropy;
                CurrIdx = Cell.Key;
            }
        }

        return CurrIdx;
    }

    private Vector2Int Iteration()
    {
        Tile SelectedTile;
        Vector2Int MinEntropyIdx;
        List<Tile> MinEntropyCell;
        MinEntropyIdx = GetMinEntropyIdx();
        MinEntropyCell = WFCMap[GetMinEntropyIdx()];
        SelectedTile = MinEntropyCell[UnityEngine.Random.Range(0, MinEntropyCell.Count)];
        FinalWFCMap.Add(MinEntropyIdx, SelectedTile);
        PlaceRoom(SelectedTile);

        WFCMap[MinEntropyIdx] = new List<Tile>();
        WFCMap[MinEntropyIdx].Add(SelectedTile);

        return MinEntropyIdx;
    }

    private void Spread(Vector2Int IdxToCheck)
    {
        if (!WFCMap.ContainsKey(IdxToCheck))
        {
            return;
        }
    }

    public IEnumerator GenerateWFCMap()
    {
        InitMap(MapSize.x, MapSize.y);

        // Vector2Int p = new Vector2Int(3, 3);
        // WFCMap[p] = new List<Tile>();
        // WFCMap[p].Add(new Tile(TileType.RoomInterior, 0));
        // Spread(p);
        // p = new Vector2Int(8, 8);
        // WFCMap[p] = new List<Tile>();
        // WFCMap[p].Add(new Tile(TileType.RoomInterior, 0));
        // Spread(p);

        do {
            Spread(Iteration());
            yield return new WaitForSeconds(WFCDelay ? 0.075f : 0f);
        } while(Continue());

        FinalWFCMap = new Dictionary<Vector2Int, Tile>();
        foreach(KeyValuePair<Vector2Int, List<Tile>> Cell in WFCMap)
        {
            if (Cell.Value.Count == 0)
            {
                Debug.LogError(string.Format(
                    "Cell without any tiles possible at {0}, {1}!",
                    Cell.Key.x,
                    Cell.Key.y
                ));
                foreach(var go in PlacedRooms)
                {
                    Destroy(go.Value);
                }
                PlacedRooms.Clear();
                FinalWFCMap.Clear();
                WFCMap.Clear();
                StartCoroutine(GenerateWFCMap());
                yield return new WaitForSeconds(WFCDelay ? 0.075f : 0f);
                break;
            }
            Tile MapTile = Cell.Value[0];
            FinalWFCMap.Add(Cell.Key, MapTile);
        }

        Debug.Log("WFC Finished");
    }

    
    private void PlaceRoom(Tile tile)
    {
        if (tile.Type == TileType.Room_Floor) PlacedFloors += 1;
        GameObject go = Instantiate(RoomPrefabs[(int)tile.Type]);
        go.transform.position = tile.GetPositionScaled(GridSize);
        go.transform.rotation = tile.Rotation;

        if ((int)tile.Type >= 5) // if is room component
        {
            go.transform.localScale=new Vector3(2, 2, 2);
        }
      
        PlacedRooms.Add(tile.GetPositionVec2(), go);
    }


    void Start()
    {
        PlacedRooms = new Dictionary<Vector2Int, GameObject>();
        FinalWFCMap = new Dictionary<Vector2Int, Tile>();

        StartCoroutine(GenerateWFCMap());
        // SpawnMap();
    }
}
