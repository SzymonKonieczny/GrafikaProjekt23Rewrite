using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crawler;

namespace Crawler
{
    enum TileType
    {
        Empty,
        Hall_I,
        Hall_L,
        Hall_T,
        Hall_X,
        RoomCorner,
        RoomEntrance,
        RoomWall,
    }
    [System.Serializable]
    enum SocketType
    {
        Empty_S,
        Hall_0,     // on Z axis
        Hall_1,     // on X axis
    }

        [System.Serializable]
    struct Tile 
    {
        public Vector2Int Position;
        public TileType Type;
        public Quaternion Rotation;

        public List<SocketType> Sockets;
            
        void SetRotation(int rotIdx)
        {
            SetRotationVec3(new Vector3(0, -(rotIdx%4) * 90, 0));
        }
        void SetRotationVec3(Vector3 Rot)
        {
            Rotation = Quaternion.Euler(Rot);
        }

        public Tile(TileType type, int Rot)
        {
            Position = new Vector2Int();
            Rotation = new Quaternion();
            Sockets = new List<SocketType>();
            Type = type;
            SetRotation(Rot);

            switch(type)
            {
            case TileType.Empty:
                Sockets.Add(SocketType.Empty_S); // X+
                Sockets.Add(SocketType.Empty_S); // Z-
                Sockets.Add(SocketType.Empty_S); // X-
                Sockets.Add(SocketType.Empty_S); // Z+
                break;
            case TileType.Hall_I:
                Sockets.Add(SocketType.Empty_S); // X+
                Sockets.Add(SocketType.Hall_0);  // Z-
                Sockets.Add(SocketType.Empty_S); // X-
                Sockets.Add(SocketType.Hall_0);  // Z+
                break;
            case TileType.Hall_L:
                Sockets.Add(SocketType.Empty_S); // X+
                Sockets.Add(SocketType.Empty_S); // Z-
                Sockets.Add(SocketType.Hall_1);  // X-
                Sockets.Add(SocketType.Hall_0);  // Z+
                break;
            case TileType.Hall_T:
                Sockets.Add(SocketType.Hall_1);  // X+
                Sockets.Add(SocketType.Hall_0);  // Z-
                Sockets.Add(SocketType.Empty_S); // X-
                Sockets.Add(SocketType.Hall_0);  // Z+
                break;
            case TileType.Hall_X:
                Sockets.Add(SocketType.Hall_1);  // X+
                Sockets.Add(SocketType.Hall_0);  // Z-
                Sockets.Add(SocketType.Hall_1);  // X-
                Sockets.Add(SocketType.Hall_0);  // Z+
                break;
            default:
                throw new System.Exception(string.Format("{0} NOT IMPLEMENTED YET", type.ToString()));
            }

            List<SocketType> DefaultSockets = new List<SocketType>(Sockets);

            if (Rot != 0)
            {
                // Debug.LogWarning(string.Format("tile: {0} | rotation: {1}", type.ToString(), Rot));
                // Debug.Log(string.Format("X+: {0} | Z-: {1} | X-: {2} | Z+: {3}", Sockets[0].ToString(), Sockets[1].ToString(), Sockets[2].ToString(), Sockets[3].ToString()));
            }
            
            Sockets[0] = DefaultSockets[(4-Rot)%4];
            Sockets[3] = DefaultSockets[(7-Rot)%4];
            Sockets[2] = DefaultSockets[(6-Rot)%4];
            Sockets[1] = DefaultSockets[(5-Rot)%4];

            for (int i = 0; i < 4; ++i)
            {
                if (Rot % 2 == 0) { break; }

                if (Sockets[i] == SocketType.Hall_0) {
                    Sockets[i] = SocketType.Hall_1;
                }
                else if (Sockets[i] == SocketType.Hall_1) {
                    Sockets[i] = SocketType.Hall_0;
                }
            }

            if (Rot != 0)
            {
                // Debug.Log(string.Format("X+: {0} | Z-: {1} | X-: {2} | Z+: {3}", Sockets[0].ToString(), Sockets[1].ToString(), Sockets[2].ToString(), Sockets[3].ToString()));
            }
        }
    }

}

public class WFC_Script : MonoBehaviour
{
    [SerializeField] List<GameObject> RoomPrefabs = new List<GameObject>();
    [SerializeField] Dictionary<Vector2Int, GameObject> PlacedRooms = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] Dictionary<Vector2Int, List<Tile>> WFCMap;
    [SerializeField] Dictionary<Vector2Int, Tile> FinalWFCMap;


    [SerializeField] List<Tile> WCFMapListCopy = new List<Tile>();
    [SerializeField] List<Tile> TileTemplates = new List<Tile>();


    [SerializeField] Vector2Int MapSize = new Vector2Int(10,10);
    public float GridSize = 2;


    private void InitTemplates() {
        for(int i = 0; i < 4; ++i)
        {
            TileTemplates.Add(new Tile(TileType.Empty, i));
            TileTemplates.Add(new Tile(TileType.Hall_I, i));
            // TileTemplates.Add(new Tile(TileType.Hall_L, i));
            TileTemplates.Add(new Tile(TileType.Hall_T, i));
            // TileTemplates.Add(new Tile(TileType.Hall_X, i));
            // TODO: Add the rest of the TileTypes
        }
        Debug.LogWarning("Check if all TileTypes have been inserted");
    }

    private void InitMap(int width, int height)
    {
        InitTemplates();
        Dictionary<Vector2Int, List<Tile>> Map = new Dictionary<Vector2Int, List<Tile>>();
        List<Tile> EmptyCell = new List<Tile>();
        EmptyCell.Add(new Tile(TileType.Empty, 0));

        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                Map.Add(new Vector2Int(x, y), new List<Tile>(TileTemplates));
                // if (x == 0 || y == 0 || x == width-1 || y == height-1)
                // {
                //     Map[new Vector2Int(x, y)].Add(new Tile(TileType.Empty, 0));
                // }
                // else
                // {
                //     Map.Add(new Vector2Int(x, y), new List<Tile>(TileTemplates));
                // }
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
        Vector2Int CurrIdx = new Vector2Int(1, 1);
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
        Vector2Int MinEntropyIdx = GetMinEntropyIdx();
        List<Tile> MinEntropyCell = WFCMap[GetMinEntropyIdx()];
        SelectedTile = MinEntropyCell[UnityEngine.Random.Range(0, MinEntropyCell.Count)];

        SelectedTile.Position = MinEntropyIdx;
        FinalWFCMap.Add(MinEntropyIdx, SelectedTile);
        WCFMapListCopy.Add(SelectedTile);
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

        List<Tile> CellToCompare = new List<Tile>();
        List<SocketType> AvailableSockets = new List<SocketType>();
        bool Changed = false;
        int PrevSize = 0;
        Vector2Int CompareIdx = new Vector2Int(0, 0);
        int x = IdxToCheck.x, z = IdxToCheck.y;

        // 1. go to each direction from Idx
        CompareIdx = new Vector2Int(x+1, z);
        if (WFCMap.ContainsKey(CompareIdx))
        {
            CellToCompare = WFCMap[CompareIdx];
            // 2. check adjacent sockets
            foreach (Tile tile in WFCMap[IdxToCheck])
            {
                AvailableSockets.Add(tile.Sockets[0]);
            }

            // 3. if sockets dont match, remove tiles 
            PrevSize = WFCMap[CompareIdx].Count;
            // TODO: Check if it works correctly!!!
            WFCMap[CompareIdx].RemoveAll((t) => {
                return !AvailableSockets.Contains(t.Sockets[2]);
            });

            // 4. if removal was done, call Spread() on this Idx
            if (WFCMap[CompareIdx].Count != PrevSize)
            {
                Spread(CompareIdx);
            }
            AvailableSockets.Clear();
        }

        CompareIdx = new Vector2Int(x, z-1);
        if (WFCMap.ContainsKey(CompareIdx))
        {
            CellToCompare = WFCMap[CompareIdx];
            // 2. check adjacent sockets
            foreach (Tile tile in WFCMap[IdxToCheck])
            {
                AvailableSockets.Add(tile.Sockets[1]);
            }

            // 3. if sockets dont match, remove tiles 
            PrevSize = WFCMap[CompareIdx].Count;
            // TODO: Check if it works correctly!!!
            WFCMap[CompareIdx].RemoveAll((t) => {
                return !AvailableSockets.Contains(t.Sockets[3]);
            });

            // 4. if removal was done, call Spread() on this Idx
            if (WFCMap[CompareIdx].Count != PrevSize)
            {
                Spread(CompareIdx);
            }
            AvailableSockets.Clear();
        }

        CompareIdx = new Vector2Int(x-1, z);
        if (WFCMap.ContainsKey(CompareIdx))
        {
            CellToCompare = WFCMap[CompareIdx];
            // 2. check adjacent sockets
            foreach (Tile tile in WFCMap[IdxToCheck])
            {
                AvailableSockets.Add(tile.Sockets[2]);
            }

            // 3. if sockets dont match, remove tiles 
            PrevSize = WFCMap[CompareIdx].Count;
            // TODO: Check if it works correctly!!!
            WFCMap[CompareIdx].RemoveAll((t) => {
                return !AvailableSockets.Contains(t.Sockets[0]);
            });

            // 4. if removal was done, call Spread() on this Idx
            if (WFCMap[CompareIdx].Count != PrevSize)
            {
                Spread(CompareIdx);
            }
            AvailableSockets.Clear();
        }

        CompareIdx = new Vector2Int(x, z+1);
        if (WFCMap.ContainsKey(CompareIdx))
        {
            CellToCompare = WFCMap[CompareIdx];
            // 2. check adjacent sockets
            foreach (Tile tile in WFCMap[IdxToCheck])
            {
                AvailableSockets.Add(tile.Sockets[3]);
            }

            // 3. if sockets dont match, remove tiles 
            PrevSize = WFCMap[CompareIdx].Count;
            // TODO: Check if it works correctly!!!
            WFCMap[CompareIdx].RemoveAll((t) => {
                return !AvailableSockets.Contains(t.Sockets[1]);
            });

            // 4. if removal was done, call Spread() on this Idx
            if (WFCMap[CompareIdx].Count != PrevSize)
            {
                Spread(CompareIdx);
            }
            AvailableSockets.Clear();
        }
    }

    private bool SpreadOld()
    {
        List<Tile> CellToCompare = new List<Tile>();
        List<SocketType> AdjacentSockets= new List<SocketType>();
        bool Changed = false;
        int PrevSize = 0;
        Vector2Int CompareCoords = new Vector2Int(0, 0);
        int x = 0, y = 0;

        foreach(KeyValuePair<Vector2Int, List<Tile>> Cell in WFCMap)
        {
            if (Cell.Value.Count == 1) { continue; }

            x = Cell.Key.x;
            y = Cell.Key.y;

            CompareCoords = new Vector2Int(x, y-1);
            if (WFCMap.ContainsKey(CompareCoords)) {
                CellToCompare = WFCMap[CompareCoords];
                foreach (Tile tile in CellToCompare)
                {
                    AdjacentSockets.Add(tile.Sockets[0]);
                }
                PrevSize = Cell.Value.Count;
                for (int i = 0; i < Cell.Value.Count; ++i) {
					bool IsCompatible = false;
					for (int j = 0; j < AdjacentSockets.Count; ++j) {
						IsCompatible |= Cell.Value[i].Sockets[2] == AdjacentSockets[j];
					}
					if (!IsCompatible) {
                        Cell.Value.RemoveAt(i--);
					}
				}
				Changed |= PrevSize != Cell.Value.Count;            		// this is for checking if any change was made
				AdjacentSockets.Clear();							        // clear adjacent sockets list
            }

            CompareCoords = new Vector2Int(x-1, y);
            if (WFCMap.ContainsKey(new Vector2Int(x-1, y))) {
                CellToCompare = WFCMap[CompareCoords];
                foreach (Tile tile in CellToCompare)
                {
                    AdjacentSockets.Add(tile.Sockets[1]);
                }
                PrevSize = Cell.Value.Count;
                for (int i = 0; i < Cell.Value.Count; ++i) {
					bool IsCompatible = false;
					for (int j = 0; j < AdjacentSockets.Count; ++j) {
						IsCompatible |= Cell.Value[i].Sockets[3] == AdjacentSockets[j];
					}
					if (!IsCompatible) {
                        Cell.Value.RemoveAt(i--);
					}
				}
				Changed |= PrevSize != Cell.Value.Count;            		// this is for checking if any change was made
				AdjacentSockets.Clear();							        // clear adjacent sockets list
            }

            CompareCoords = new Vector2Int(x, y+1);
            if (WFCMap.ContainsKey(new Vector2Int(x, y+1))) {
                CellToCompare = WFCMap[CompareCoords];
                foreach (Tile tile in CellToCompare)
                {
                    AdjacentSockets.Add(tile.Sockets[2]);
                }
                PrevSize = Cell.Value.Count;
                for (int i = 0; i < Cell.Value.Count; ++i) {
					bool IsCompatible = false;
					for (int j = 0; j < AdjacentSockets.Count; ++j) {
						IsCompatible |= Cell.Value[i].Sockets[0] == AdjacentSockets[j];
					}
					if (!IsCompatible) {
                        Cell.Value.RemoveAt(i--);
					}
				}
				Changed |= PrevSize != Cell.Value.Count;            		// this is for checking if any change was made
				AdjacentSockets.Clear();							        // clear adjacent sockets list
            }

            CompareCoords = new Vector2Int(x+1, y);
            if (WFCMap.ContainsKey(new Vector2Int(x+1, y))) {
                CellToCompare = WFCMap[CompareCoords];
                foreach (Tile tile in CellToCompare)
                {
                    AdjacentSockets.Add(tile.Sockets[3]);
                }
                PrevSize = Cell.Value.Count;
                for (int i = 0; i < Cell.Value.Count; ++i) {
					bool IsCompatible = false;
					for (int j = 0; j < AdjacentSockets.Count; ++j) {
						IsCompatible |= Cell.Value[i].Sockets[1] == AdjacentSockets[j];
					}
					if (!IsCompatible) {
                        Cell.Value.RemoveAt(i--);
					}
				}
				Changed |= PrevSize != Cell.Value.Count;            		// this is for checking if any change was made
				AdjacentSockets.Clear();							        // clear adjacent sockets list
            }
        }
        return Changed;
    }

    public IEnumerator GenerateWFCMap()
    {
        InitMap(MapSize.x, MapSize.y);

        do {
            Spread(Iteration());
            yield return new WaitForSeconds(0.125f);
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
                continue;
            }
            Tile MapTile = Cell.Value[0];
            MapTile.Position = Cell.Key;
            FinalWFCMap.Add(Cell.Key, MapTile);
        }

        Debug.Log("WFC Finished");
    }

    
    private void PlaceRoom(Tile tile)
    {
        GameObject go = Instantiate(RoomPrefabs[(int)tile.Type]);
        go.transform.position = new Vector3(tile.Position.x*GridSize, 0, tile.Position.y*GridSize);
        go.transform.rotation = tile.Rotation;

        // if(tile.Type != TileType.Empty || tile.Type != TileType.Hall_I)
        //     go.transform.Rotate(new Vector3(0, 1, 0), 180);
        if(tile.Type == TileType.Hall_L)
            go.transform.Rotate(new Vector3(0, 1, 0), 180);
      
        PlacedRooms.Add(tile.Position, go);
    }

    void SpawnMap()
    {
        foreach(KeyValuePair<Vector2Int, Tile> Cell in FinalWFCMap)
        {
            PlaceRoom(Cell.Value);
        }
    }



    void Start()
    {
        PlacedRooms = new Dictionary<Vector2Int, GameObject>();
        FinalWFCMap = new Dictionary<Vector2Int, Tile>();

        StartCoroutine(GenerateWFCMap());
        // SpawnMap();
    }


    
}
