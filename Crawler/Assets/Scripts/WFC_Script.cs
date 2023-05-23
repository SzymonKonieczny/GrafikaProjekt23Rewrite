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

    enum SocketType
    {
        Empty_S,
        Hall_0,     // on X axis
        Hall_1,     // on Z axis
    }

    struct Tile 
    {
        public Vector2Int Position;
        public TileType Type;
        public Quaternion Rotation;

        public List<SocketType> Sockets;

        void SetRotation(int rotIdx)
        {
            SetRotationVec3(new Vector3(0, rotIdx * 90, 0));
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
                Sockets.Add(SocketType.Empty_S); // Z+
                Sockets.Add(SocketType.Empty_S); // X+
                Sockets.Add(SocketType.Empty_S); // Z-
                Sockets.Add(SocketType.Empty_S); // X-
                break;
            case TileType.Hall_I:
                Sockets.Add(SocketType.Hall_0);  // Z+
                Sockets.Add(SocketType.Empty_S); // X+
                Sockets.Add(SocketType.Hall_0);  // Z-
                Sockets.Add(SocketType.Empty_S); // X-
                break;
            case TileType.Hall_L:
                Sockets.Add(SocketType.Empty_S); // Z+
                Sockets.Add(SocketType.Hall_1);  // X+
                Sockets.Add(SocketType.Hall_0);  // Z-
                Sockets.Add(SocketType.Empty_S); // X-
                break;
            case TileType.Hall_T:
                Sockets.Add(SocketType.Hall_0);  // Z+
                Sockets.Add(SocketType.Empty_S); // X+
                Sockets.Add(SocketType.Hall_0);  // Z-
                Sockets.Add(SocketType.Hall_1);  // X-
                break;
            case TileType.Hall_X:
                Sockets.Add(SocketType.Hall_0);  // Z+
                Sockets.Add(SocketType.Hall_1);  // X+
                Sockets.Add(SocketType.Hall_0);  // Z-
                Sockets.Add(SocketType.Hall_1);  // X-
                break;
            default:
                throw new System.Exception(string.Format("{0} NOT IMPLEMENTED YET", type.ToString()));
            }

            List<SocketType> DefaultSockets = new List<SocketType>(Sockets);

            // TODO: check if indexing is right :)
            // (the order was 0 3 2 1 on the right side in the UE5)
            // (idk why lol)
            Sockets[0] = DefaultSockets[(0+Rot)%4];
            Sockets[1] = DefaultSockets[(1+Rot)%4];
            Sockets[2] = DefaultSockets[(2+Rot)%4];
            Sockets[3] = DefaultSockets[(3+Rot)%4];

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
        }
    }

}

public class WFC_Script : MonoBehaviour
{
    [SerializeField] List<GameObject> RoomPrefabs = new List<GameObject>();
    [SerializeField] Dictionary<Vector2Int, GameObject> PlacedRooms = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] Dictionary<Vector2Int, List<Tile>> WFCMap;
    [SerializeField] Dictionary<Vector2Int, Tile> FinalWFCMap;

    [SerializeField] List<Tile> TileTemplates = new List<Tile>();

    private void InitTemplates() {
        for(int i = 0; i < 4; ++i)
        {
            TileTemplates.Add(new Tile(TileType.Hall_I, i));
            TileTemplates.Add(new Tile(TileType.Hall_L, i));
            TileTemplates.Add(new Tile(TileType.Hall_T, i));
            TileTemplates.Add(new Tile(TileType.Hall_X, i));
            // TODO: Add the rest of the TileTypes
        }
        TileTemplates.Add(new Tile(TileType.Empty, 0));
        Console.WriteLine("[INFO] Check if all TileTypes have been inserted");
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
            if(Cell.Value.Count > 1) { return true; }
        }
        return false;
    }

    private Vector2Int GetMinEntropyIdx()
    {
        Vector2Int CurrIdx = new Vector2Int(0, 0);
        int CurrMin = TileTemplates.Count;
        int Entropy = 0;
        int x = 0, y = 0;

        foreach(KeyValuePair<Vector2Int, List<Tile>> Cell in WFCMap)
        {
            Entropy = Cell.Value.Count;
            if (Entropy < CurrMin) {
                CurrMin = Entropy;
                CurrIdx = Cell.Key;
            }
        }

        return CurrIdx;
    }

    private void Iteration()
    {
        Tile SelectedTile;
        Vector2Int MinEntropyIdx = GetMinEntropyIdx();
        List<Tile> MinEntropyCell = WFCMap[GetMinEntropyIdx()];
        SelectedTile = MinEntropyCell[UnityEngine.Random.Range(0, MinEntropyCell.Count)];
        WFCMap[MinEntropyIdx] = new List<Tile>();
        WFCMap[MinEntropyIdx].Add(SelectedTile);
          
    }

    private bool Spread()
    {
        List<Tile> CellToCompare;
        List<SocketType> AdjacentSockets= new List<SocketType>();
        bool Changed = false;
        int PrevSize = 0;
        Vector2Int CompareCoords = new Vector2Int(0, 0);

        foreach(KeyValuePair<Vector2Int, List<Tile>> Cell in WFCMap)
        {
            if (Cell.Value.Count == 1) { continue; }

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

    public void GenerateWFCMap()
    {
        InitMap();
        while(Continue()) {
            while(Spread()) {}
            Iteration();
        }

        FinalWFCMap = new Dictionary<Vector2Int, Tile>();
        foreach(KeyValuePair<Vector2Int, List<Tile>> Cell in WFCMap)
        {
            FinalWFCMap.Add(Cell.Value[0]);
        }
    }

    
    private void PlaceRoom(Tile tile)
    {
        GameObject go = Instantiate(RoomPrefabs[(int)tile.Type]);
        go.transform.position = new Vector3(tile.Position.x, 0, tile.Position.y);
        PlacedRooms.Add(tile.Position, go);
   
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }


    
}
