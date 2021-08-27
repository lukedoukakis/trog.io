using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        GameObject worldObjectPrefab;
        switch(itemType){
            case Item.Type.Food :
                this.capacity = RackCapacity_Food;
                worldObjectPrefab = CampResources.Prefab_FoodRack;
                break;
            case Item.Type.Weapon :
                this.capacity = RackCapacity_Weapons;
                worldObjectPrefab = CampResources.Prefab_WeaponsRack;
                break;
            case Item.Type.Clothing : 
                this.capacity = RackCapacity_Clothing;
                worldObjectPrefab = CampResources.Prefab_ClothingRack;
                break;
            default:
                this.capacity = RackCapacity_Food;
                worldObjectPrefab = CampResources.Prefab_FoodRack;
                Debug.Log("unsupported itemType for ItemRack");
                break;
        }
        this.worldObject = Utility.InstantiatePrefabSameName(worldObjectPrefab);
        ScriptableObjectReference reference = this.worldObject.AddComponent<ScriptableObjectReference>();
        reference.SetScriptableObject(this);

        objects = new List<GameObject>();
    }



    public void AddObjects(Item item, ref int countToAdd){
        int c = countToAdd;
        for(int i = 0; i < c; ++i){ 
            if(!IsFull()){

                // if room in the rack, add the item to it
                Debug.Log("Count to add: " + countToAdd);
                GameObject o = Utility.InstantiatePrefabSameName(item.gameobject);
                objects.Add(o);
                SetObjectOrientation(o);
                InteractableObject.SetAttachedObject(o, this.worldObject);
                --countToAdd;
            }
            else{

                // else, call to add remaining count to other racks
                camp.AddObjectsAnyRack(item, ref countToAdd);
                break;
            }
        }
    }

    public void RemoveObjects(Item item, ref int countToRemove){

        // count the number of occurences of the item, and remove that many from the objects
        int occurences = objects.Where(o => o.name == item.nme).Count();
        for(int i = 0; i < occurences; ++i){
            objects.RemoveAt(objects.FindLastIndex(x => x.name.Equals(item.name)));
            --countToRemove;
        }

        // call to remove remaining count
        camp.RemoveObjectsAnyRack(item, ref countToRemove);
    }   

    // set a given object's orientation to fit properly in the rack
    public void SetObjectOrientation(GameObject o){
        int index = objects.Count - 1;
        string orientationName = "ItemOrientation" + index;
        Debug.Log("SetItemOrientation(): orientation name: " + orientationName);
        Transform orientation = Utility.FindDeepChild(worldObject.transform, "ItemOrientation" + index);
        o.transform.position = orientation.position;
        o.transform.rotation = orientation.rotation;
        SpringJoint joint = o.AddComponent<SpringJoint>();
        joint.spring = 1000f;
        joint.damper = .2f;
    }


    public bool IsFull(){
        return objects.Count >= capacity;
    }

    public bool IsEmpty(){
        return objects.Count == 0;
    }

}
