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
        public TileType Tile;
        public Quaternion Rotation;

        public List<SocketType> Sockets;

        void SetRotation(int rotIdx) => SetRotationVec3(new Vector3(0, rotIdx*90, 0));
        void SetRotationVec3(Vector3 Rot)
        {
            Rotation = Quaternion.Euler(Rot);
        }

        public Tile(TileType type, int Rotation)
        {
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
            Sockets[0] = DefaultSockets[(0+Rotation)%4];
            Sockets[1] = DefaultSockets[(1+Rotation)%4];
            Sockets[2] = DefaultSockets[(2+Rotation)%4];
            Sockets[3] = DefaultSockets[(3+Rotation)%4];

            for (uint i = 0; i < 4; ++i)
            {
                if (Rotation % 2 == 0) { break; }

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
        for(uint i = 0; i < 4; ++i)
        {
            TileTemplates.Add(Tile(TileType.Hall_I, i));
            TileTemplates.Add(Tile(TileType.Hall_L, i));
            TileTemplates.Add(Tile(TileType.Hall_T, i));
            TileTemplates.Add(Tile(TileType.Hall_X, i));
            // TODO: Add the rest of the TileTypes
        }
        TileTemplates.Add(Tile(TileType.Empty, i));
        Console.WriteLine("[INFO] Check if all TileTypes have been inserted");
    }

    private void InitMap(usize width, usize height)
    {
        InitTemplates();
        Dictionary<Vector2Int, List<Tile>> Map;
        for (uint y = 0; y < height; ++y) {
            for (uint x = 0; x < width; ++x) {
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
        uint CurrMin = TileTemplates.Count;
        uint Entropy = 0;
        uint x = 0, y = 0;

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
        List<Tile> MinEntropyCell = WFCMap[GetMinEntropyIdx()];
        SelectedTile = MinEntropyCell[Random.Next(MinEntropyCell.Count)];
        WFCMap[MinEntropyIdx] = new List<Tile>(SelectedTile);
    }

    private bool Spread()
    {
        List<Tile> CellToCompare;
        List<SocketType> AdjacentSockets;
        bool Changed = false;
        uint PrevSize = 0;
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
                for (uint i = 0; i < Cell.Value.Count; ++i) {
					bool IsCompatible = false;
					for (uint j = 0; j < AdjacentSockets.Count; ++j) {
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
                for (uint i = 0; i < Cell.Value.Count; ++i) {
					bool IsCompatible = false;
					for (uint j = 0; j < AdjacentSockets.Count; ++j) {
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
                for (uint i = 0; i < Cell.Value.Count; ++i) {
					bool IsCompatible = false;
					for (uint j = 0; j < AdjacentSockets.Count; ++j) {
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
                for (uint i = 0; i < Cell.Value.Count; ++i) {
					bool IsCompatible = false;
					for (uint j = 0; j < AdjacentSockets.Count; ++j) {
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
    }

    public void GenerateWFCMap()
    {
        InitMap();
        while(Continue()) {
            while(Spread()) {}
            Iteration();
        }

        foreach(KeyValuePair<Vector2Int, List<Tile>> Cell in WFCMap)
        {
            FinalWFCMap.Add(Cell.Value[0]);
        }
    }

    
    private void PlaceRoom(Tile tile)
    {
        GameObject go = Instantiate(RoomPrefabs[(int)tile.Tile]);
        go.transform.position = new Vector3(tile.Position.x, 0, tile.Position.y);
        PlacedRooms.Add(tile.Position, go);
   
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }


    
}
