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

    private List<TileType> GetPossibleTileNeighbours(TileType t, Vector2Int d)
    {
        var ret = new List<TileType>();
        //                                       tiletype  direction    possible
        var PossibleNeighbours = new Dictionary<(TileType, Vector2Int), List<TileType>>();

        PossibleNeighbours.Add( // EMPTY Z+
            (TileType.Empty, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xpos_Zpos,
                TileType.Hall_T_X_Zpos,
                TileType.Room_I_Zpos,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos,
            }
        ); // EMPTY Z+ END
        PossibleNeighbours.Add( // EMPTY X+
            (TileType.Empty, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_Z_Xpos,
                TileType.Room_I_Xpos,
                TileType.Room_L_Xpos_Zpos, TileType.Room_L_Xpos_Zneg,
            }
        ); // EMPTY X+ END
        PossibleNeighbours.Add( // EMPTY Z-
            (TileType.Empty, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_X_Zneg,
                TileType.Room_I_Zneg,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg,
            }
        ); // EMPTY Z- END
        PossibleNeighbours.Add( // EMPTY X-
            (TileType.Empty, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xneg,
                TileType.Room_I_Xneg,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xneg_Zneg,
            }
        ); // EMPTY X- END

        PossibleNeighbours.Add( // HALL_I_X Z+
            (TileType.Hall_I_X, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_X_Zpos,
                TileType.Room_I_Zpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg
            }
        ); // HALL_I_X Z+ END
        PossibleNeighbours.Add( // HALL_I_X X+
            (TileType.Hall_I_X, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xneg,
                TileType.Hall_X,
                TileType.Room_T_Xneg
            }
        ); // HALL_I_X X+ END
        PossibleNeighbours.Add( // HALL_I_X Z-
            (TileType.Hall_I_X, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zneg,
                TileType.Room_I_Zneg,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos
            }
        ); // HALL_I_X Z- END
        PossibleNeighbours.Add( // HALL_I_X X-
            (TileType.Hall_I_X, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xpos,
                TileType.Hall_X,
                TileType.Room_T_Xpos
            }
        ); // HALL_I_X X- END

        PossibleNeighbours.Add( // HALL_I_Z Z+
            (TileType.Hall_I_Z, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zneg,
                TileType.Hall_X,
                TileType.Room_T_Zneg
            }
        ); // HALL_I_Z Z+ END
        PossibleNeighbours.Add( // HALL_I_Z X+
            (TileType.Hall_I_Z, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xpos_Zpos,
                TileType.Hall_T_Z_Xpos,
                TileType.Room_I_Xpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xneg_Zpos
            }
        ); // HALL_I_Z X+ END
        PossibleNeighbours.Add( // HALL_I_Z Z-
            (TileType.Hall_I_Z, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Zpos
            }
        ); // HALL_I_Z Z- END
        PossibleNeighbours.Add( // HALL_I_Z X-
            (TileType.Hall_I_Z, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xneg,
                TileType.Room_I_Xneg,
                TileType.Room_L_Xpos_Zneg, TileType.Room_L_Xpos_Zpos
            }
        ); // HALL_I_Z X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Hall_L_Xpos_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_X_Zpos,
                TileType.Room_I_Zpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Hall_L_Xpos_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xneg,
                TileType.Hall_X,
                TileType.Room_T_Xneg
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Hall_L_Xpos_Zneg, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Hall_L_Xpos_Zneg, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xneg,
                TileType.Room_I_Xneg,
                TileType.Room_L_Xpos_Zneg, TileType.Room_L_Xpos_Zpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Hall_L_Xneg_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_X_Zpos,
                TileType.Room_I_Zpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Hall_L_Xneg_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xpos_Zpos,
                TileType.Hall_T_Z_Xpos,
                TileType.Room_I_Xpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xneg_Zpos
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Hall_L_Xneg_Zneg, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Hall_L_Xneg_Zneg, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Xpos,
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Hall_L_Xneg_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zneg,
                TileType.Hall_X,
                TileType.Room_T_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Hall_L_Xneg_Zpos, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xpos_Zpos,
                TileType.Hall_T_Z_Xpos,
                TileType.Room_I_Xpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xneg_Zpos
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Hall_L_Xneg_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zneg,
                TileType.Room_I_Zneg,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Hall_L_Xneg_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Xpos,
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Hall_L_Xpos_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zneg,
                TileType.Hall_X,
                TileType.Room_T_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Hall_L_Xpos_Zpos, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xneg,
                TileType.Hall_X,
                TileType.Room_T_Xneg
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Hall_L_Xpos_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zneg,
                TileType.Room_I_Zneg,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Hall_L_Xpos_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xneg,
                TileType.Room_I_Xneg,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xneg_Zneg,
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Hall_T_Z_Xneg, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zneg,
                TileType.Hall_X,
                TileType.Room_T_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Hall_T_Z_Xneg, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xpos_Zpos,
                TileType.Hall_T_Z_Xpos,
                TileType.Room_I_Xpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xneg_Zpos
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Hall_T_Z_Xneg, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Hall_T_Z_Xneg, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xpos,
                TileType.Hall_X,
                TileType.Room_T_Xpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Hall_T_X_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zneg,
                TileType.Hall_X,
                TileType.Room_T_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Hall_T_X_Zpos, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xneg,
                TileType.Hall_X,
                TileType.Room_T_Xneg
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Hall_T_X_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zneg,
                TileType.Room_I_Zneg,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Hall_T_X_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xpos,
                TileType.Hall_X,
                TileType.Room_T_Xpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Hall_T_Z_Xpos, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zneg,
                TileType.Hall_X,
                TileType.Room_T_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Hall_T_Z_Xpos, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xneg,
                TileType.Hall_X,
                TileType.Room_T_Xneg
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Hall_T_Z_Xpos, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Hall_T_Z_Xpos, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xneg,
                TileType.Room_I_Xneg,
                TileType.Room_L_Xpos_Zneg, TileType.Room_L_Xpos_Zpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Hall_T_X_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_X_Zpos,
                TileType.Room_I_Zpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Hall_T_X_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xneg,
                TileType.Hall_X,
                TileType.Room_T_Xneg
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Hall_T_X_Zneg, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Hall_T_X_Zneg, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xpos,
                TileType.Hall_X,
                TileType.Room_T_Xpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Hall_X, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zneg,
                TileType.Hall_X,
                TileType.Room_T_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Hall_X, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xneg,
                TileType.Hall_X,
                TileType.Room_T_Xneg
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Hall_X, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Hall_X, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xpos,
                TileType.Hall_X,
                TileType.Room_T_Xpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_I_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_X_Zpos,
                TileType.Room_I_Zpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_I_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                // TileType.Room_I_Zneg,
                // TileType.Room_T_Zpos,
                TileType.Room_L_Xpos_Zpos
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_I_Zneg, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Room_Floor
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_I_Zneg, new Vector2Int(-1, 0)), new List<TileType>() {
                // TileType.Room_I_Zneg,
                // TileType.Room_T_Zpos,
                TileType.Room_L_Xneg_Zpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_I_Xneg, new Vector2Int(0, 1)), new List<TileType>() {
                // TileType.Room_I_Xneg,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xpos_Zpos
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_I_Xneg, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xpos_Zpos,
                TileType.Hall_T_Z_Xpos,
                TileType.Room_I_Xpos,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xneg_Zpos
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_I_Xneg, new Vector2Int(0, -1)), new List<TileType>() {
                // TileType.Room_I_Xneg,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xpos_Zneg
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_I_Xneg, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Room_Floor
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_I_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Room_Floor
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_I_Zpos, new Vector2Int(1, 0)), new List<TileType>() {
                // TileType.Room_I_Zpos,
                // TileType.Room_T_Zneg,
                TileType.Room_L_Xpos_Zneg
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_I_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zneg,
                TileType.Room_I_Zneg,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_I_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                // TileType.Room_I_Zpos,
                // TileType.Room_T_Zneg,
                TileType.Room_L_Xneg_Zneg
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_I_Xpos, new Vector2Int(0, 1)), new List<TileType>() {
                // TileType.Room_I_Xpos,
                // TileType.Room_T_Xneg,
                TileType.Room_L_Xneg_Zpos
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_I_Xpos, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Room_Floor
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_I_Xpos, new Vector2Int(0, -1)), new List<TileType>() {
                // TileType.Room_I_Xpos,
                // TileType.Room_T_Xneg,
                TileType.Room_L_Xneg_Zneg
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_I_Xpos, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xneg,
                TileType.Room_I_Xneg,
                TileType.Room_L_Xpos_Zneg, TileType.Room_L_Xpos_Zpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_T_Xpos, new Vector2Int(0, 1)), new List<TileType>() {
                // TileType.Room_I_Xneg,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xpos_Zpos
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_T_Xpos, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xneg,
                TileType.Hall_X,
                TileType.Room_T_Xneg
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_T_Xpos, new Vector2Int(0, -1)), new List<TileType>() {
                // TileType.Room_I_Xneg,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xpos_Zneg
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_T_Xpos, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Room_Floor
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_T_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Room_Floor
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_T_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                // TileType.Room_I_Zpos,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xpos_Zneg
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_T_Zneg, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xneg_Zpos,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zpos,
                TileType.Hall_X,
                TileType.Room_T_Zpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_T_Zneg, new Vector2Int(-1, 0)), new List<TileType>() {
                // TileType.Room_I_Zpos,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xneg_Zneg
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_T_Xneg, new Vector2Int(0, 1)), new List<TileType>() {
                // TileType.Room_I_Xpos,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xneg_Zpos
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_T_Xneg, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Room_Floor
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_T_Xneg, new Vector2Int(0, -1)), new List<TileType>() {
                // TileType.Room_I_Xpos,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xneg_Zneg
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_T_Xneg, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Hall_I_X,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_X_Zpos, TileType.Hall_T_X_Zneg, TileType.Hall_T_Z_Xpos,
                TileType.Hall_X,
                TileType.Room_T_Xpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_T_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zneg, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xpos, TileType.Hall_T_Z_Xneg, TileType.Hall_T_X_Zneg,
                TileType.Hall_X,
                TileType.Room_T_Zneg
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_T_Zpos, new Vector2Int(1, 0)), new List<TileType>() {
                // TileType.Room_I_Zneg,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xpos_Zpos
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_T_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Room_Floor
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_T_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                // TileType.Room_I_Zneg,
                // TileType.Room_T_Xpos,
                TileType.Room_L_Xneg_Zpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_L_Xpos_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Room_I_Xneg,
                TileType.Room_T_Xpos,
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_L_Xpos_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_Z_Xpos,
                TileType.Room_I_Xpos,
                TileType.Room_L_Xpos_Zpos, TileType.Room_L_Xpos_Zneg,
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_L_Xpos_Zneg, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_X_Zneg,
                TileType.Room_I_Zneg,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg,
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_L_Xpos_Zneg, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Room_I_Zpos,
                TileType.Room_T_Zneg,
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_L_Xneg_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Room_I_Xpos,
                TileType.Room_T_Xneg,
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_L_Xneg_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Room_I_Zpos,
                TileType.Room_T_Zneg,
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_L_Xneg_Zneg, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_X_Zneg,
                TileType.Room_I_Zneg,
                TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg,
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_L_Xneg_Zneg, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xneg,
                TileType.Room_I_Xneg,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xneg_Zneg,
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_L_Xneg_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xpos_Zpos,
                TileType.Hall_T_X_Zpos,
                TileType.Room_I_Zpos,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos,
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_L_Xneg_Zpos, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Room_I_Zneg,
                TileType.Room_T_Zpos,
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_L_Xneg_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Room_I_Xpos,
                TileType.Room_T_Xneg,
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_L_Xneg_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                TileType.Hall_T_Z_Xneg,
                TileType.Room_I_Xneg,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xneg_Zneg,
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_L_Xpos_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_X,
                TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xpos_Zpos,
                TileType.Hall_T_X_Zpos,
                TileType.Room_I_Zpos,
                TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos,
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_L_Xpos_Zpos, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Empty,
                TileType.Hall_I_Z,
                TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                TileType.Hall_T_Z_Xpos,
                TileType.Room_I_Xpos,
                TileType.Room_L_Xpos_Zpos, TileType.Room_L_Xpos_Zneg,
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_L_Xpos_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Room_I_Xneg,
                TileType.Room_T_Xpos
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_L_Xpos_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Room_I_Zneg,
                TileType.Room_T_Zpos
            }
        ); // X- END

        PossibleNeighbours.Add( // Z+
            (TileType.Room_Floor, new Vector2Int(0, 1)), new List<TileType>() {
                TileType.Room_I_Zneg,
                TileType.Room_T_Zpos
            }
        ); // Z+ END
        PossibleNeighbours.Add( // X+
            (TileType.Room_Floor, new Vector2Int(1, 0)), new List<TileType>() {
                TileType.Room_I_Xneg,
                TileType.Room_T_Xpos
            }
        ); // X+ END
        PossibleNeighbours.Add( // Z-
            (TileType.Room_Floor, new Vector2Int(0, -1)), new List<TileType>() {
                TileType.Room_I_Zpos,
                TileType.Room_T_Zneg
            }
        ); // Z- END
        PossibleNeighbours.Add( // X-
            (TileType.Room_Floor, new Vector2Int(-1, 0)), new List<TileType>() {
                TileType.Room_I_Xpos,
                TileType.Room_T_Xneg
            }
        ); // X- END

        PossibleNeighbours.TryGetValue((t, d), out ret);
        return ret;
    }

    private IEnumerable<Tile> GetPossibleNeighbours(Vector2Int Origin, Vector2Int Direction)
    {
        List<Tile> OriginTiles = new List<Tile>();
        try {
            WFCMap.TryGetValue(Origin, out OriginTiles);
        } catch (KeyNotFoundException)
        {
            Debug.LogError($"GetPossibleNeighbours() found 0 tiles at [{Origin.x},{Origin.y}]");
            yield break;
        }

        foreach(Tile t in OriginTiles)
        {
            foreach(Tile n in GetPossibleTileNeighbours(t, Direction))
            {
                yield return n;
            }
        }
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
