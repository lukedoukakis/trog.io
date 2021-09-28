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
    public static int RackCapacity_Wood = 30;
    public static int RackCapacity_Bone = 100;

    // --

    public Camp camp;
    public Enum itemType;
    public int capacity;
    public List<GameObject> objects;
    public GameObject worldObject;
    public Transform worldObject_orientationParent;
    public bool onDemandPlacement;  // whether new racks can be created when this one is filled up; if false, objects cannot be placed when attempting to place on this rack
    public bool freezeObjects; // if true, physics are deactivated for objects added to the rack
    public bool singlePlacementPoint; // if true, objects are placed at a single position on the rack, rather than finding an open position

    public void SetObjectRack(Camp camp, Enum itemType){
        this.camp = camp;
        this.itemType = itemType;
        GameObject worldObjectPrefab;
        switch(itemType){
            case Item.ItemType.Food :
                this.capacity = RackCapacity_Food;
                worldObjectPrefab = CampResources.PREFAB_RACK_FOOD;
                onDemandPlacement = true;
                freezeObjects = true;
                singlePlacementPoint = false;
                break;
            case Item.ItemType.Weapon :
                this.capacity = RackCapacity_Weapons;
                worldObjectPrefab = CampResources.PREFAB_RACK_WEAPONS;
                onDemandPlacement = true;
                freezeObjects = true;
                singlePlacementPoint = false;
                break;
            case Item.ItemType.Clothing : 
                this.capacity = RackCapacity_Clothing;
                worldObjectPrefab = CampResources.PREFAB_RACK_CLOTHING;
                onDemandPlacement = true;
                freezeObjects = true;
                singlePlacementPoint = false;
                break;
            case Item.ItemType.Wood : 
                this.capacity = RackCapacity_Wood;
                worldObjectPrefab = CampResources.PREFAB_RACK_WOOD;
                onDemandPlacement = true;
                freezeObjects = false;
                singlePlacementPoint = true;
                break;
            case Item.ItemType.Bone : 
                this.capacity = RackCapacity_Bone;
                worldObjectPrefab = CampResources.PREFAB_RACK_BONE;
                onDemandPlacement = true;
                freezeObjects = false;
                singlePlacementPoint = true;
                break;
            case Item.ItemType.Any : 
                this.capacity = 3;
                worldObjectPrefab = CampResources.PREFAB_WORKBENCH;
                onDemandPlacement = false;
                freezeObjects = true;
                singlePlacementPoint = false;
                break;
            default:
                this.capacity = RackCapacity_Food;
                worldObjectPrefab = CampResources.PREFAB_RACK_FOOD;
                onDemandPlacement = true;
                Debug.Log("unsupported itemType for ItemRack");
                break;
        }
        this.worldObject = Utility.InstantiatePrefabSameName(worldObjectPrefab);
        this.worldObject_orientationParent = Utility.FindDeepChild(this.worldObject.transform, "ItemOrientations");
        ScriptableObjectReference reference = this.worldObject.AddComponent<ScriptableObjectReference>();
        reference.SetScriptableObjectReference(this);

        objects = new List<GameObject>();
    }



    public virtual void AddObjects(Item item, ref int countToAdd){
        int c = countToAdd;
        for(int i = 0; i < c; ++i){ 
            if(!IsFull()){

                // if room in the rack, add the item to it
                GameObject o = Utility.InstantiatePrefabSameName(item.worldObject);
                objects.Add(o);
                SetObjectOrientation(o);
                o.GetComponent<ScriptableObjectReference>().SetScriptableObjectReference(this);
                --countToAdd;
            }
            else{

                // else, call to add remaining count to other racks if onDemandPlacement is true
                if(onDemandPlacement){
                    camp.AddObjectsAnyRack(item, ref countToAdd);
                    break;
                }
                else{
                    Debug.Log("Did not add object to rack, as the rack is full and the call did not prompt to search for another.");
                }
            }
        }
    }

    public virtual void RemoveObjects(Item item, ref int countToRemove){


        //Debug.Log("Removing " + countToRemove + " " + item.nme);

        // count the number of occurences of the item, and remove that many from the objects
        GameObject[] matches = objects.Where(o => o.name == item.nme).ToArray();
        int occurences = matches.Length;
        if (occurences > 0)
        {
            int c = countToRemove;
            for (int i = 0; i < Math.Min(occurences, c); ++i)
            {
                GameObject o = matches[occurences - 1];
                objects.Remove(o);
                GameObject.Destroy(o);
                --countToRemove;
            }
        }
    
        if(countToRemove > 0){
            // call to remove remaining count
            camp.RemoveObjectsAnyRack(item, ref countToRemove);
        }
    }   

    // set a given object's orientation to fit properly in the rack
    public void SetObjectOrientation(GameObject o){

        List<Transform> orientations = worldObject_orientationParent.GetComponentsInChildren<Transform>().Where(ortn => ortn.tag == "Orientation").ToList();

        for(int i = 0; i < capacity; ++i)
        {
            Transform orientation = orientations[i];
            //Transform orientation = worldObject_orientationParent.Find("ItemOrientation" + i);
            
            //Debug.Log(orientation.childCount);
            if (orientation.childCount == 0)
            {
                o.transform.position = orientation.position;
                o.transform.rotation = orientation.rotation;
                o.transform.parent = orientation;
                if(freezeObjects){
                    Utility.ToggleObjectPhysics(o, false, true, true, false);
                }
                else{
                    Rigidbody rb = o.GetComponent<Rigidbody>();
                    rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    //rb.drag = 5f;
                    //rb.angularDrag = 5f;
                }
                break;
                
                //FixedJoint joint = o.AddComponent<FixedJoint>();
            }
            
        }
    }


    public bool IsFull(){
        return objects.Count >= capacity;
    }

    public bool IsEmpty(){
        return objects.Count == 0;
    }

}
