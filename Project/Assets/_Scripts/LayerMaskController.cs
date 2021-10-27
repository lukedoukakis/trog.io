using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMaskController : MonoBehaviour
{

    public static LayerMask INTERACTABLE = LayerMask.GetMask("HoverTrigger");
    public static LayerMask TERRAIN = LayerMask.GetMask("Terrain");
    public static LayerMask WATER = LayerMask.GetMask("Water");
    public static LayerMask WALKABLE = LayerMask.GetMask("Terrain", "Feature", "Creature");
    public static LayerMask SWINGABLE = LayerMask.GetMask("Terrain", "Feature");
    public static LayerMask HITTABLE = LayerMask.GetMask("Feature", "Creature", "Item");
    public static LayerMask FEATURE = LayerMask.GetMask("Feature");
    public static LayerMask CREATURE = LayerMask.GetMask("Creature");
    public static LayerMask ITEM = LayerMask.GetMask("Item");
    public static LayerMask CLEAR_ON_CAMP_PLACEMENT = LayerMask.GetMask("Feature", "SmallFeature");
}
