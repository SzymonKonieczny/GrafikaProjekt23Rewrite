using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crawler;

namespace Crawler {
    enum TileType {
        // Symmetrical
        Empty,
        // CorridorAxis
        Hall_I_Z,
        Hall_I_X,
        // CorridorAxis1_CorridorAxis2
        Hall_L_Xpos_Zneg, // 0 degs
        Hall_L_Xneg_Zneg, // 90 degs
        Hall_L_Xneg_Zpos, // 180 degs
        Hall_L_Xpos_Zpos, // 270 degs
        // LongCorridorAxis_ShortCorridorAxis
        Hall_T_Z_Xneg,
        Hall_T_X_Zpos,
        Hall_T_Z_Xpos,
        Hall_T_X_Zneg,
        // Symmetrical
        Hall_X,
        // RoomInteriorDirection
        Room_I_Zneg,
        Room_I_Xneg,
        Room_I_Zpos,
        Room_I_Xpos,
        // EntranceCorridorAxis
        Room_T_Xpos,
        Room_T_Zneg,
        Room_T_Xneg,
        Room_T_Zpos,
        // RoomExteriorDirection_RoomExteriorDirection
        Room_L_Xpos_Zneg,
        Room_L_Xneg_Zneg,
        Room_L_Xneg_Zpos,
        Room_L_Xpos_Zpos,
        // Symmetrical
        Room_Floor
    }

    struct Tile {
        public Vector3 Position;
        public TileType Type;

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
        public void SetPosition(Vector2Int pos)
        {
            this.Position.x = pos.x;
            this.Position.z = pos.y;
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

        public Tile(TileType t)
        {
            this.Type = t;
            this.Position = new Vector3(0, 0, 0);
        }
        public Tile(TileType t, Vector2 p)
        {
            this.Type = t;
            this.Position.x = p.x;
            this.Position.z = p.y;
            this.Position.y = 0;
        }
        public Tile(TileType t, Vector3 p)
        {
            this.Type = t;
            this.Position = p;
        }
    };
}

public class WFC : MonoBehaviour, IRoomPlacer
{
    [SerializeField] List<GameObject> RoomPrefabs = new List<GameObject>();
    [SerializeField] Dictionary<Vector2Int, GameObject> PlacedRooms = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] Dictionary<Vector2Int, List<Tile>> WFCMap;
    [SerializeField] Dictionary<Vector2Int, Tile> FinalWFCMap;

    //          tiletype  direction    possible
    Dictionary<(TileType, Vector2Int), List<TileType>> PossibleNeighbours = new Dictionary<(TileType, Vector2Int), List<TileType>>();

    [SerializeField] List<Tile> TileTemplates = new List<Tile>(25);
    [SerializeField] Vector2Int MapSize = new Vector2Int(15, 15);
    public float GridSize = 18;
    public bool WFCDelay = true;

    public int PlacedFloors = 0;

    private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

    public List<Transform> GetRoomTransforms() {
        Debug.LogError("TODO: GetRoomTransforms()");
        return new List<Transform>();
    }

    private void InitTemplates() {
        // TileTemplates.Clear();
        foreach(TileType tt in (TileType[]) Enum.GetValues(typeof(TileType)))
        {
            TileTemplates.Add(new Tile(tt));
        }
    }

    private void InitMap(int width, int height)
    {
        InitPossibleNeighbours();
        InitTemplates();
        Dictionary<Vector2Int, List<Tile>> Map = new Dictionary<Vector2Int, List<Tile>>(height*width);

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

    private void InitPossibleNeighbours()
    {
        this.PossibleNeighbours = HF.GetPossibilities();
    }

    private List<TileType> GetPossibleTileNeighbours(TileType t, Vector2Int d)
    {
        var ret = new List<TileType>();
        PossibleNeighbours.TryGetValue((t, d), out ret);
        return ret;
    }

    private List<TileType> GetPossibleNeighbours(Vector2Int Origin, Vector2Int Direction)
    {
        List<TileType> ret = new List<TileType>();
        List<Tile> OriginTiles = new List<Tile>();
        try {
            WFCMap.TryGetValue(Origin, out OriginTiles);
        } catch (KeyNotFoundException)
        {
            Debug.LogError($"GetPossibleNeighbours() invalid coordinates: [{Origin.x},{Origin.y}]");
            return ret;
        }

        foreach(Tile t in OriginTiles)
        {
            foreach(TileType tt in GetPossibleTileNeighbours(t.Type, Direction))
            {
                ret.Add(tt);
            }
        }

        return ret;
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
        Vector2Int MinEntropyIdx = GetMinEntropyIdx();
        List<Tile> MinEntropyCell = WFCMap[MinEntropyIdx];
        if (MinEntropyCell.Count <= 0)
        {
            // Debug.LogError($"Iteration() found 0 tiles at [{MinEntropyIdx.x},{MinEntropyIdx.y}]");
        }
        Tile SelectedTile = MinEntropyCell[UnityEngine.Random.Range(0, MinEntropyCell.Count)];
        FinalWFCMap.Add(MinEntropyIdx, SelectedTile);
        SelectedTile.SetPosition(MinEntropyIdx);
        PlaceRoom(SelectedTile);

        // WFCMap[MinEntropyIdx] = new List<Tile>();
        WFCMap[MinEntropyIdx].Clear();
        WFCMap[MinEntropyIdx].Add(SelectedTile);

        return MinEntropyIdx;
    }

    private void Spread(Vector2Int Coords)
    {
        Stack<Vector2Int> CoordsStack = new Stack<Vector2Int>();

        CoordsStack.Push(Coords);
        while (CoordsStack.Count > 0)
        {
            var CurrCoords = CoordsStack.Pop();

            foreach(Vector2Int dir in HF.ValidCoords(Coords: CurrCoords, MaxH: MapSize.y, MaxW: MapSize.x))
            {
                var OtherCoords = CurrCoords + dir;
                var OtherPossibilities = WFCMap[OtherCoords];
                if (OtherPossibilities.Count <= 0)
                {
                    // Debug.LogError($"Spread() found 0 tiles at [{OtherCoords.x},{OtherCoords.y}]");
                    // continue;
                }
                var ValidPossibilities = GetPossibleNeighbours(Origin: CurrCoords, Direction: dir);
                foreach(var CompareTile in OtherPossibilities)
                {
                    if ((!ValidPossibilities.Contains(CompareTile.Type)) && (!CoordsStack.Contains(OtherCoords)))
                    {
                        CoordsStack.Push(OtherCoords);
                        // break;
                    }
                }
                WFCMap[OtherCoords].RemoveAll((ct) => {
                    return (!ValidPossibilities.Contains(ct.Type));
                });
            }
        }
    }

    public IEnumerator GenerateWFCMap()
    {
        InitMap(MapSize.x, MapSize.y);

        // Vector2Int p = new Vector2Int(3, 3);
        // WFCMap[p] = new List<Tile>();
        // WFCMap[p].Add(new Tile(TileType.Room_Floor));
        // Spread(p);

        do {
            Spread(Iteration());
            yield return new WaitForSeconds(WFCDelay ? 0.075f : 0f);
        } while(Continue());

        FinalWFCMap = new Dictionary<Vector2Int, Tile>(MapSize.x * MapSize.y);
        foreach(KeyValuePair<Vector2Int, List<Tile>> Cell in WFCMap)
        {
            if (Cell.Value.Count == 0)
            {
                // Debug.LogError(string.Format(
                //     "Cell without any tiles possible at {0}, {1}!",
                //     Cell.Key.x,
                //     Cell.Key.y
                // ));
                foreach(var go in PlacedRooms)
                {
                    Destroy(go.Value);
                }
                PlacedRooms.Clear();
                FinalWFCMap.Clear();
                WFCMap.Clear();
                PlacedFloors = 0;
                StartCoroutine(GenerateWFCMap());
                yield break;
            } 
            else
            {
                Tile MapTile = Cell.Value[0];
                FinalWFCMap.Add(Cell.Key, MapTile);
            }
        }

        if (PlacedFloors <= 4 || (PlacedFloors >= 8 && false) && false)
        {
            // foreach(var go in PlacedRooms)
            // {
            //     Destroy(go.Value);
            // }
            // PlacedRooms.Clear();
            // FinalWFCMap.Clear();
            // WFCMap.Clear();
            // PlacedFloors = 0;
            // StartCoroutine(GenerateWFCMap());
            yield break;
        }

        if (FinalWFCMap.Count == MapSize.x * MapSize.y)
        {
            sw.Stop();
            Debug.Log($"WFC Finished in {(float)sw.ElapsedMilliseconds/1000} seconds.");
        }
    }

    
    private void PlaceRoom(Tile tile)
    {
        if (tile.Type == TileType.Room_Floor) PlacedFloors += 1;
        if ((int)tile.Type > RoomPrefabs.Count)
        {
            int count = Enum.GetNames(typeof(TileType)).Length;
            Debug.LogError("You forgot to add the prefab to the list!");
            Debug.LogWarning($"Total count of prefabs should be: {count}");
        }
        GameObject go = Instantiate(RoomPrefabs[(int)tile.Type]);
        go.transform.position = tile.GetPositionScaled(GridSize);

        if ((int)tile.Type >= 12) // if is room component
        {
            go.transform.localScale = new Vector3(2, 2, 2);
        }
        // if ((int)tile.Type == 24)
        // {
        //     go.transform.localScale = new Vector3(4, 1, 4);
        // }
      
        PlacedRooms.Add(tile.GetPositionVec2(), go);
    }


    void Start()
    {
        PlacedRooms = new Dictionary<Vector2Int, GameObject>();
        FinalWFCMap = new Dictionary<Vector2Int, Tile>();
        sw.Start();
        StartCoroutine(GenerateWFCMap());
    }
}
