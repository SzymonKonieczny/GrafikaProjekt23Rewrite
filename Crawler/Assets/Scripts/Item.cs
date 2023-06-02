using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseItem", menuName = "ScriptableObjects/Items")]
public class Item : ScriptableObject
{
    public int ItemID = 0;
    public string ItemName = "Name";
}
