using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRack : ScriptableObject
{
    
    
    public static int RackCapacity_Food = 6;
    public static int RackCapacity_Weapons = 6;
    public static int RackCapacity_Clothing = 6;

    // --

    public Camp camp;
    public Enum itemType;
    public int capacity;
    public List<GameObject> objects;
    public GameObject worldObject;


    public void SetObjectRack(Camp camp, Enum itemType){
        this.camp = camp;
        this.itemType = itemType;
        switch(itemType){
            case Item.Type.Food :
                this.capacity = RackCapacity_Food;
                this.worldObject = Instantiate(CampResources.Prefab_FoodRack);
                break;
            case Item.Type.Weapon :
                this.capacity = RackCapacity_Weapons;
                this.worldObject = Instantiate(CampResources.Prefab_WeaponsRack);
                break;
            case Item.Type.Clothing : 
                this.capacity = RackCapacity_Clothing;
                this.worldObject = Instantiate(CampResources.Prefab_ClothingRack);
                break;
            default:
                Debug.Log("unsupported itemType for ItemRack");
                break;
        }
        objects = new List<GameObject>();
    }

    public void AddObjects(List<GameObject> objectsToAdd){

        foreach(GameObject o in objectsToAdd.ToArray()){

            if(!IsFull()){
                //Debug.Log("AddItems(): Adding item: " + o.name);
                objects.Add(o);
                objectsToAdd.Remove(o);
                SetObjectOrientation(o);
            }
            else{
                camp.OnRackCapacityReached(this.itemType, objectsToAdd);
                break;
            }
        }
    }

    // set a given object's orientation to fit properly in the rack
    public void SetObjectOrientation(GameObject o){
        int index = objects.Count - 1;
        string orientationName = "ItemOrientation" + index;
        Debug.Log("SetItemOrientation(): orientation name: " + orientationName);
        Transform orientation = Utility.FindDeepChild(worldObject.transform, "ItemOrientation" + index);
        o.transform.position = orientation.position;
        o.transform.rotation = orientation.rotation;

        //Utility.ToggleObjectPhysics(o, false);
        SpringJoint joint = o.AddComponent<SpringJoint>();
        joint.spring = 1000f;
        joint.damper = .2f;
    }


    public bool IsFull(){
        return objects.Count >= capacity;
    }

}
