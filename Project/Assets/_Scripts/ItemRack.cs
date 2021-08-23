using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRack : MonoBehaviour
{
    
    public static GameObject FoodRack = Resources.Load<GameObject>("Camp/Food Rack");
    public static GameObject WeaponsRack = Resources.Load<GameObject>("Camp/Weapons Rack");
    public static GameObject ClothingRack = Resources.Load<GameObject>("Camp/Clothing Rack");
    public static int RackCapacity_Food = 6;
    public static int RackCapacity_Weapons = 6;
    public static int RackCapacity_Clothing = 6;

    // --

    public Camp camp;
    public Enum itemType;
    public int capacity;
    public List<GameObject> items;
    public GameObject worldObject;


    public void SetItemRack(Camp camp, Enum itemType, List<GameObject> objects){
        this.camp = camp;
        this.itemType = itemType;
        switch(itemType){
            case Item.Type.Food :
                this.capacity = RackCapacity_Food;
                this.worldObject = Instantiate(FoodRack);
                break;
            case Item.Type.Weapon :
                this.capacity = RackCapacity_Weapons;
                this.worldObject = Instantiate(WeaponsRack);
                break;
            case Item.Type.Clothing : 
                this.capacity = RackCapacity_Clothing;
                this.worldObject = Instantiate(ClothingRack);
                break;
            default:
                Debug.Log("unsupported itemType for ItemRack");
                break;
        }

        items = new List<GameObject>();
        AddItems(objects);
    }

    public void AddItems(List<GameObject> objectsToAdd){
        foreach(GameObject o in objectsToAdd){
            
            bool fit = items.Count < capacity;
            if(fit){
                items.Add(o);
                objectsToAdd.Remove(o);
                SetItemPosition(o);
            }
            else{
                camp.OnRackCapacityReached(this.itemType, objectsToAdd);
                break;
            }
        }
    }

    public void SetItemPosition(GameObject o){
        Utility.ToggleObjectPhysics(o, false);
        int index = items.Count - 1;
        Transform orientation = Utility.FindDeepChild(worldObject.transform, "ItemPositionIndex" + index);
        o.transform.position = orientation.position;
        o.transform.rotation = orientation.rotation;
    }

}
