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
        RoomEntrance,
        RoomWall,
        RoomCorner,
        RoomInterior,
    }

    [System.Serializable]
    enum SocketType
    {
        Empty_S,
        Hall_0,
        Hall_1,
        Wall_0,
        Wall_1,
        Wall_2,
        Wall_3,
        RoomInterior,
    }

    [System.Serializable]
    struct Tile 
    {
        public Vector2Int Position;
        public TileType Type;
        public Quaternion Rotation;
        public int RotationInt;

        public List<SocketType> Sockets;

        SocketType RotateSocket(SocketType Socket, int Rotation)
        {
            List<SocketType> Sockets = new List<SocketType>();
            Sockets.Add(SocketType.Wall_0);
            Sockets.Add(SocketType.Wall_3);
            Sockets.Add(SocketType.Wall_2);
            Sockets.Add(SocketType.Wall_1);

            if (!Sockets.Contains(Socket)) return Socket;

            int CurrentIdx = Sockets.LastIndexOf(Socket);
            return Sockets[(CurrentIdx + Rotation) % 4];
        }
            
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
            RotationInt = Rot;
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
            case TileType.RoomEntrance:
                Sockets.Add(SocketType.RoomInterior);  // X-
                Sockets.Add(SocketType.Wall_0);        // Z-
                Sockets.Add(SocketType.Hall_1);        // X+
                Sockets.Add(SocketType.Wall_0);        // Z+
                break;
            case TileType.RoomWall:
                Sockets.Add(SocketType.Wall_3);        // X+
                Sockets.Add(SocketType.Empty_S);       // Z-
                Sockets.Add(SocketType.Wall_3);        // X-
                Sockets.Add(SocketType.RoomInterior);  // Z+
                break;
            case TileType.RoomCorner:
                Sockets.Add(SocketType.Empty_S);  // X+
                Sockets.Add(SocketType.Empty_S);  // Z-
                Sockets.Add(SocketType.Wall_2);   // X-
                Sockets.Add(SocketType.Wall_1);   // Z+
                break;
            case TileType.RoomInterior:
                Sockets.Add(SocketType.RoomInterior);  // X+
                Sockets.Add(SocketType.RoomInterior);  // Z-
                Sockets.Add(SocketType.RoomInterior);  // X-
                Sockets.Add(SocketType.RoomInterior);  // Z+
                break;
            default:
                throw new System.Exception(string.Format("{0} NOT IMPLEMENTED YET", type.ToString()));
            }

            List<SocketType> DefaultSockets = new List<SocketType>(Sockets);
            
            Sockets[0] = DefaultSockets[(4-Rot)%4];
            Sockets[3] = DefaultSockets[(7-Rot)%4];
            Sockets[2] = DefaultSockets[(6-Rot)%4];
            Sockets[1] = DefaultSockets[(5-Rot)%4];

            for (int i = 0; i < 4; ++i)
            {
                if (Sockets[i] == SocketType.Empty_S || Sockets[i] == SocketType.Hall_0 || Sockets[i] == SocketType.Hall_1)
                {
                    if (Rot % 2 == 0) { break; }

                    if (Sockets[i] == SocketType.Hall_0) {
                        Sockets[i] = SocketType.Hall_1;
                    }
                    else if (Sockets[i] == SocketType.Hall_1) {
                        Sockets[i] = SocketType.Hall_0;
                    }
                }
                else
                {
                    Sockets[i] = RotateSocket(Sockets[i], Rot);
                }
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


    public int PriorityEmpty = 4;
    public int PriorityI = 1;
    public int PriorityL = 3;
    public int PriorityT = 3;
    public int PriorityX = 1;
    public int PriorityRoom = 4;


    [SerializeField] List<Tile> WCFMapListCopy = new List<Tile>();
    [SerializeField] List<Tile> TileTemplates = new List<Tile>();


    [SerializeField] Vector2Int MapSize = new Vector2Int(10,10);
    public float GridSize = 2;


    private void InitTemplates() {


        for (int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < PriorityEmpty; ++j)
                TileTemplates.Add(new Tile(TileType.Empty, i));
            for (int j = 0; j < PriorityI; ++j)
                TileTemplates.Add(new Tile(TileType.Hall_I, i));
            for (int j = 0; j < PriorityL; ++j)
                TileTemplates.Add(new Tile(TileType.Hall_L, i));
            for (int j = 0; j < PriorityT; ++j)
                TileTemplates.Add(new Tile(TileType.Hall_T, i));
            for (int j = 0; j < PriorityX; ++j)
                TileTemplates.Add(new Tile(TileType.Hall_X, i));
            for (int j = 0; j < PriorityRoom; ++j)
            {
                TileTemplates.Add(new Tile(TileType.RoomInterior, i));
                TileTemplates.Add(new Tile(TileType.RoomCorner, i));
                TileTemplates.Add(new Tile(TileType.RoomEntrance, i));
                TileTemplates.Add(new Tile(TileType.RoomWall, i));
            }
        }
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
        if (tile.Type != TileType.Hall_I && tile.RotationInt%2 == 0)
            go.transform.Rotate(new Vector3(0, 1, 0), 180);

        if (tile.Type == TileType.RoomEntrance || tile.Type == TileType.RoomWall || tile.Type == TileType.RoomCorner)
            go.transform.localScale=new Vector3(2, 2, 2);

        // if (tile.Type == TileType.RoomCorner)
        //     go.transform.Rotate(new Vector3(0, 1, 0), -90);
      
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
