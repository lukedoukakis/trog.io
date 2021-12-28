using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampResources : MonoBehaviour
{
    
    public static GameObject PREFAB_CAMPLAYOUT = Resources.Load<GameObject>("Camp/Camp Layout");
    public static GameObject PREFAB_BONFIRE_UNLIT = Resources.Load<GameObject>("Camp/Bonfire");
    public static GameObject PREFAB_BONFIRE_LIT = Resources.Load<GameObject>("Camp/Bonfire");
    public static GameObject PREFAB_WORKBENCH = Resources.Load<GameObject>("Camp/Workbench");
    public static GameObject PREFAB_RACK_FOOD = Resources.Load<GameObject>("Camp/Food Rack");
    public static GameObject PREFAB_RACK_WEAPONS = Resources.Load<GameObject>("Camp/Weapons Rack");
    public static GameObject PREFAB_RACK_Pelt = Resources.Load<GameObject>("Camp/Pelt Rack");
    public static GameObject PREFAB_RACK_WOOD = Resources.Load<GameObject>("Camp/Wood Rack");
    public static GameObject PREFAB_RACK_BONE = Resources.Load<GameObject>("Camp/Bone Rack");
    public static GameObject PREFAB_RACK_STONE = Resources.Load<GameObject>("Camp/Stone Rack");
    public static GameObject PREFAB_TENT = Resources.Load<GameObject>("Camp/Tent");




    public static LayerMask LayerMask_Terrain = LayerMask.GetMask("Terrain");
}
