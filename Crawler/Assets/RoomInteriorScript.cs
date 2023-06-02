using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum RoomSocket
    {
        KeyHole,
        Gem,
        Door
    }

public class RoomInteriorScript : RoomBaseScript
{
    public Transform KeyHoleSocket;
    public Transform GemSocket;

    public GameObject PlaceInRoom(GameObject prefab, RoomSocket roomSocket)
        {
            GameObject go=null;
            switch (roomSocket)
            {
                case RoomSocket.KeyHole:
                    go  = Instantiate(prefab);
                    go.transform.position = KeyHoleSocket.position;
                     return go;
                    break;
                case RoomSocket.Gem:
                    go = Instantiate(prefab);
                    go.transform.position = GemSocket.position;
                    return go;
                    break;
                case RoomSocket.Door:
                    go = Instantiate(prefab);
                    go.transform.position = KeyHoleSocket.position;
                 return go;
                break;
            default:
                return go;
        }

        }
}
