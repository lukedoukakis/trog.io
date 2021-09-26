using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampResources : MonoBehaviour
{
    
    public static GameObject Prefab_CampLayout = Resources.Load<GameObject>("Camp/Camp Layout");
    public static GameObject Prefab_BonfireUnlit = Resources.Load<GameObject>("Camp/Bonfire");
    public static GameObject Prefab_BonfireLit = Resources.Load<GameObject>("Camp/Bonfire");
    public static GameObject Prefab_Workbench = Resources.Load<GameObject>("Camp/Workbench");
    public static GameObject Prefab_FoodRack = Resources.Load<GameObject>("Camp/Food Rack");
    public static GameObject Prefab_WeaponsRack = Resources.Load<GameObject>("Camp/Weapons Rack");
    public static GameObject Prefab_ClothingRack = Resources.Load<GameObject>("Camp/Clothing Rack");
    public static GameObject Prefab_MiscLargeRack = Resources.Load<GameObject>("Camp/MiscLarge Rack");
    public static GameObject Prefab_Tent = Resources.Load<GameObject>("Camp/Tent");




    public static LayerMask LayerMask_Terrain = LayerMask.GetMask("Terrain");
}
