using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoomPlacer
{
    public List<Transform> GetRoomTransforms();
    public IEnumerator GenerateMap();
}
