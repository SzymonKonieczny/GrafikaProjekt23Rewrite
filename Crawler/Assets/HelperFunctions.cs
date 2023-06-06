using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crawler;

namespace Crawler {

    class HF {
        static public List<Vector2Int> ValidCoords(Vector2Int Coords, int MaxH, int MaxW)
        {
            List<Vector2Int> ret = new List<Vector2Int>();
            int cx = Coords.x;
            int cz = Coords.y;

            if (cz+1 < MaxH)
            {
                ret.Add(new Vector2Int(0, 1));
            }
            if (cx+1 < MaxW)
            {
                ret.Add(new Vector2Int(1, 0));
            }
            if (cz-1 >= 0)
            {
                ret.Add(new Vector2Int(0, -1));
            }
            if (cx-1 >= 0)
            {
                ret.Add(new Vector2Int(-1, 0));
            }

            return ret;
        }

        static public Dictionary<(TileType, Vector2Int), List<TileType>> GetPossibilities()
        {
            var PossibleNeighbours = new Dictionary<(TileType, Vector2Int), List<TileType>>();

            PossibleNeighbours.Add( // EMPTY Z+
                (TileType.Empty, new Vector2Int(0, 1)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_X,
                    TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xpos_Zpos,
                    TileType.Hall_T_X_Zpos,
                    TileType.Room_I_Zpos,
                    TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg,
                }
            ); // EMPTY Z+ END
            PossibleNeighbours.Add( // EMPTY X+
                (TileType.Empty, new Vector2Int(1, 0)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_Z,
                    TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                    TileType.Hall_T_Z_Xpos,
                    TileType.Room_I_Xpos,
                    TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xneg_Zneg,
                }
            ); // EMPTY X+ END
            PossibleNeighbours.Add( // EMPTY Z-
                (TileType.Empty, new Vector2Int(0, -1)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_X,
                    TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xpos_Zneg,
                    TileType.Hall_T_X_Zneg,
                    TileType.Room_I_Zneg,
                    TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos,
                }
            ); // EMPTY Z- END
            PossibleNeighbours.Add( // EMPTY X-
                (TileType.Empty, new Vector2Int(-1, 0)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_Z,
                    TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                    TileType.Hall_T_Z_Xneg,
                    TileType.Room_I_Xneg,
                    TileType.Room_L_Xpos_Zpos, TileType.Room_L_Xpos_Zneg,
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
                    // TileType.Room_I_Zpos,
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
                    // TileType.Room_I_Xpos,
                    TileType.Room_Floor
                }
            ); // X- END

            PossibleNeighbours.Add( // Z+
                (TileType.Room_I_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                    // TileType.Room_I_Zneg,
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
                    // TileType.Room_I_Xneg,
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
                    // TileType.Room_T_Xneg,
                    TileType.Room_Floor
                }
            ); // X- END

            PossibleNeighbours.Add( // Z+
                (TileType.Room_T_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                    // TileType.Room_T_Zpos,
                    TileType.Room_Floor
                }
            ); // Z+ END
            PossibleNeighbours.Add( // X+
                (TileType.Room_T_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                    // TileType.Room_I_Zpos,
                    // TileType.Room_T_Zneg,
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
                    // TileType.Room_T_Zneg,
                    TileType.Room_L_Xneg_Zneg
                }
            ); // X- END

            PossibleNeighbours.Add( // Z+
                (TileType.Room_T_Xneg, new Vector2Int(0, 1)), new List<TileType>() {
                    // TileType.Room_I_Xpos,
                    // TileType.Room_T_Xneg,
                    TileType.Room_L_Xneg_Zpos
                }
            ); // Z+ END
            PossibleNeighbours.Add( // X+
                (TileType.Room_T_Xneg, new Vector2Int(1, 0)), new List<TileType>() {
                    // TileType.Room_T_Xpos,
                    TileType.Room_Floor
                }
            ); // X+ END
            PossibleNeighbours.Add( // Z-
                (TileType.Room_T_Xneg, new Vector2Int(0, -1)), new List<TileType>() {
                    // TileType.Room_I_Xpos,
                    // TileType.Room_T_Xneg,
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
                    // TileType.Room_T_Zpos,
                    TileType.Room_L_Xpos_Zpos
                }
            ); // X+ END
            PossibleNeighbours.Add( // Z-
                (TileType.Room_T_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                    // TileType.Room_T_Zneg,
                    TileType.Room_Floor
                }
            ); // Z- END
            PossibleNeighbours.Add( // X-
                (TileType.Room_T_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                    // TileType.Room_I_Zneg,
                    // TileType.Room_T_Zpos,
                    TileType.Room_L_Xneg_Zpos
                }
            ); // X- END

            PossibleNeighbours.Add( // Z+
                (TileType.Room_L_Xpos_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                    TileType.Room_I_Xneg,
                    TileType.Room_T_Xpos,
                    // TileType.Room_L_Xpos_Zpos
                }
            ); // Z+ END
            PossibleNeighbours.Add( // X+
                (TileType.Room_L_Xpos_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_Z,
                    TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                    TileType.Hall_T_Z_Xpos,
                    TileType.Room_I_Xpos,
                    TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xneg_Zneg,
                }
            ); // X+ END
            PossibleNeighbours.Add( // Z-
                (TileType.Room_L_Xpos_Zneg, new Vector2Int(0, -1)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_X,
                    TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xpos_Zneg,
                    TileType.Hall_T_X_Zneg,
                    TileType.Room_I_Zneg,
                    TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos,
                }
            ); // Z- END
            PossibleNeighbours.Add( // X-
                (TileType.Room_L_Xpos_Zneg, new Vector2Int(-1, 0)), new List<TileType>() {
                    TileType.Room_I_Zpos,
                    TileType.Room_T_Zneg,
                    // TileType.Room_L_Xneg_Zneg
                }
            ); // X- END

            PossibleNeighbours.Add( // Z+
                (TileType.Room_L_Xneg_Zneg, new Vector2Int(0, 1)), new List<TileType>() {
                    TileType.Room_I_Xpos,
                    TileType.Room_T_Xneg,
                    // TileType.Room_L_Xneg_Zpos
                }
            ); // Z+ END
            PossibleNeighbours.Add( // X+
                (TileType.Room_L_Xneg_Zneg, new Vector2Int(1, 0)), new List<TileType>() {
                    TileType.Room_I_Zpos,
                    TileType.Room_T_Zneg,
                    // TileType.Room_L_Xpos_Zneg
                }
            ); // X+ END
            PossibleNeighbours.Add( // Z-
                (TileType.Room_L_Xneg_Zneg, new Vector2Int(0, -1)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_X,
                    TileType.Hall_L_Xneg_Zneg, TileType.Hall_L_Xpos_Zneg,
                    TileType.Hall_T_X_Zneg,
                    TileType.Room_I_Zneg,
                    TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xpos_Zpos,
                }
            ); // Z- END
            PossibleNeighbours.Add( // X-
                (TileType.Room_L_Xneg_Zneg, new Vector2Int(-1, 0)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_Z,
                    TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                    TileType.Hall_T_Z_Xneg,
                    TileType.Room_I_Xneg,
                    TileType.Room_L_Xpos_Zpos, TileType.Room_L_Xpos_Zneg,
                }
            ); // X- END

            PossibleNeighbours.Add( // Z+
                (TileType.Room_L_Xneg_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_X,
                    TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xpos_Zpos,
                    TileType.Hall_T_X_Zpos,
                    TileType.Room_I_Zpos,
                    TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg,
                }
            ); // Z+ END
            PossibleNeighbours.Add( // X+
                (TileType.Room_L_Xneg_Zpos, new Vector2Int(1, 0)), new List<TileType>() {
                    TileType.Room_I_Zneg,
                    TileType.Room_T_Zpos,
                    // TileType.Room_L_Xpos_Zpos
                }
            ); // X+ END
            PossibleNeighbours.Add( // Z-
                (TileType.Room_L_Xneg_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                    TileType.Room_I_Xpos,
                    TileType.Room_T_Xneg,
                    // TileType.Room_L_Xneg_Zneg
                }
            ); // Z- END
            PossibleNeighbours.Add( // X-
                (TileType.Room_L_Xneg_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_Z,
                    TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xneg_Zneg,
                    TileType.Hall_T_Z_Xneg,
                    TileType.Room_I_Xneg,
                    TileType.Room_L_Xpos_Zpos, TileType.Room_L_Xpos_Zneg,
                }
            ); // X- END

            PossibleNeighbours.Add( // Z+
                (TileType.Room_L_Xpos_Zpos, new Vector2Int(0, 1)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_X,
                    TileType.Hall_L_Xneg_Zpos, TileType.Hall_L_Xpos_Zpos,
                    TileType.Hall_T_X_Zpos,
                    TileType.Room_I_Zpos,
                    TileType.Room_L_Xneg_Zneg, TileType.Room_L_Xpos_Zneg,
                }
            ); // Z+ END
            PossibleNeighbours.Add( // X+
                (TileType.Room_L_Xpos_Zpos, new Vector2Int(1, 0)), new List<TileType>() {
                    TileType.Empty,
                    TileType.Hall_I_Z,
                    TileType.Hall_L_Xpos_Zpos, TileType.Hall_L_Xpos_Zneg,
                    TileType.Hall_T_Z_Xpos,
                    TileType.Room_I_Xpos,
                    TileType.Room_L_Xneg_Zpos, TileType.Room_L_Xneg_Zneg,
                }
            ); // X+ END
            PossibleNeighbours.Add( // Z-
                (TileType.Room_L_Xpos_Zpos, new Vector2Int(0, -1)), new List<TileType>() {
                    TileType.Room_I_Xneg,
                    TileType.Room_T_Xpos,
                    // TileType.Room_L_Xpos_Zneg
                }
            ); // Z- END
            PossibleNeighbours.Add( // X-
                (TileType.Room_L_Xpos_Zpos, new Vector2Int(-1, 0)), new List<TileType>() {
                    TileType.Room_I_Zneg,
                    TileType.Room_T_Zpos,
                    // TileType.Room_L_Xneg_Zpos
                }
            ); // X- END

            PossibleNeighbours.Add( // Z+
                (TileType.Room_Floor, new Vector2Int(0, 1)), new List<TileType>() {
                    TileType.Room_I_Zneg,
                    TileType.Room_T_Zpos,
                    // TileType.Room_Floor
                }
            ); // Z+ END
            PossibleNeighbours.Add( // X+
                (TileType.Room_Floor, new Vector2Int(1, 0)), new List<TileType>() {
                    TileType.Room_I_Xneg,
                    TileType.Room_T_Xpos,
                    // TileType.Room_Floor
                }
            ); // X+ END
            PossibleNeighbours.Add( // Z-
                (TileType.Room_Floor, new Vector2Int(0, -1)), new List<TileType>() {
                    TileType.Room_I_Zpos,
                    TileType.Room_T_Zneg,
                    // TileType.Room_Floor
                }
            ); // Z- END
            PossibleNeighbours.Add( // X-
                (TileType.Room_Floor, new Vector2Int(-1, 0)), new List<TileType>() {
                    TileType.Room_I_Xpos,
                    TileType.Room_T_Xneg,
                    // TileType.Room_Floor
                }
            ); // X- END

            return PossibleNeighbours;
        }
    }
}