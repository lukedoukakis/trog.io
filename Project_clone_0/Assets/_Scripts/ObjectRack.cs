using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectRack : CampComponent
{
    
    public static float OBJECT_MOVEMENT_ANIMATION_SPEED = 15f;
    public static float OBJECT_PLACEMENT_DELAY_TIMESTEP = .1f;
    
    public static int RackCapacity_Food = 6;
    public static int RackCapacity_Weapons = 6;
    public static int RackCapacity_Clothing = 6;
    public static int RackCapacity_Wood = 30;
    public static int RackCapacity_Bone = 28;

    // --

    public Camp camp;
    public Enum itemType;
    public int capacity;
    public List<GameObject> objects;
    public Transform worldObject_orientationParent;
    List<Transform> orientations;
    public bool onDemandPlacement;  // whether new racks can be created when this one is filled up; if false, objects cannot be placed when attempting to place on this rack
    public bool allowObjectPhysics; // if true, physics are deactivated for objects added to the rack
    public bool allowLateralTranslation;
    public bool allowRotation;


    public void SetObjectRack(Camp camp, Enum itemType){
        this.camp = camp;
        this.itemType = itemType;
        GameObject worldObjectPrefab;
        switch(itemType){
            case Item.ItemType.Food :
                this.capacity = RackCapacity_Food;
                worldObjectPrefab = CampResources.PREFAB_RACK_FOOD;
                onDemandPlacement = true;
                allowObjectPhysics = false;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            case Item.ItemType.Weapon :
                this.capacity = RackCapacity_Weapons;
                worldObjectPrefab = CampResources.PREFAB_RACK_WEAPONS;
                onDemandPlacement = true;
                allowObjectPhysics = false;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            case Item.ItemType.Clothing : 
                this.capacity = RackCapacity_Clothing;
                worldObjectPrefab = CampResources.PREFAB_RACK_CLOTHING;
                onDemandPlacement = true;
                allowObjectPhysics = false;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            case Item.ItemType.Wood : 
                this.capacity = RackCapacity_Wood;
                worldObjectPrefab = CampResources.PREFAB_RACK_WOOD;
                onDemandPlacement = true;
                allowObjectPhysics = true;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            case Item.ItemType.Bone : 
                this.capacity = RackCapacity_Bone;
                worldObjectPrefab = CampResources.PREFAB_RACK_BONE;
                onDemandPlacement = true;
                allowObjectPhysics = true;
                allowLateralTranslation = true;
                allowRotation = true;
                break;
            case Item.ItemType.Any : 
                this.capacity = 3;
                worldObjectPrefab = CampResources.PREFAB_WORKBENCH;
                onDemandPlacement = false;
                allowObjectPhysics = false;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            default:
                this.capacity = RackCapacity_Food;
                worldObjectPrefab = CampResources.PREFAB_RACK_FOOD;
                onDemandPlacement = true;
                allowObjectPhysics = false;
                allowLateralTranslation = false;
                allowRotation = false;
                Debug.Log("unsupported itemType for ItemRack");
                break;
        }
        SetWorldObject(Utility.InstantiatePrefabSameName(worldObjectPrefab));
        this.worldObject_orientationParent = Utility.FindDeepChild(this.worldObject.transform, "ItemOrientations");
        this.orientations = worldObject_orientationParent.GetComponentsInChildren<Transform>().Where(ortn => ortn.tag == "Orientation").ToList();
        if(allowRotation){
            foreach(Transform orientation in orientations){
                orientation.rotation = Utility.GetRandomRotation(360f);
            }
        }
        ObjectReference reference = this.worldObject.AddComponent<ObjectReference>();
        reference.SetObjectReference(this);
        objects = new List<GameObject>();
    }



    public virtual void AddObjects(Item item, ref int countToAdd, Transform originT, ref int newRacksCount){

        int c = countToAdd;
        for(int countAdded = 0; countAdded < c; ++countAdded){ 
            if(!IsFull()){

                // if room in the rack, add the item to it
                GameObject o = Utility.InstantiatePrefabSameName(item.worldObject);
                objects.Add(o);
                SetObjectOrientation(o, originT, (OBJECT_PLACEMENT_DELAY_TIMESTEP * countAdded) + (Camp.CAMP_COMPONENT_PLACING_TIME_GAP * newRacksCount));
                o.GetComponent<ObjectReference>().SetObjectReference(this);
                --countToAdd;
            }
            else{

                // else, call to add remaining count to other racks if onDemandPlacement is true
                if(onDemandPlacement){
                    camp.AddObjectsAnyRack(item, ref countToAdd, originT, ref newRacksCount);
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
    public void SetObjectOrientation(GameObject o, Transform originT, float delay){


        StartCoroutine(_SetObjectOrientation(o));

        IEnumerator _SetObjectOrientation(GameObject _o)
        {
            for (int i = capacity - 1; i >= 0; --i)
            {

                Transform orientation = orientations[i];

                if (orientation.childCount == 0)
                {
                    _o.transform.parent = orientation;

                    // animate movement of object to rack
                    Utility.ToggleObjectPhysics(o, false, false, false, false);
                    o.SetActive(false);
                    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                    timer.Start();
                    while (timer.ElapsedMilliseconds / 1000f < delay)
                    {
                        yield return null;
                    }
                    timer.Stop();
                    o.transform.position = originT.transform.position;
                    o.SetActive(true);
                    Vector3 curOrPos, lastOrPos;
                    lastOrPos = orientation.position;
                    curOrPos = Vector3.zero;
                    while(Vector3.Distance(_o.transform.position, orientation.position) > .1f || Vector3.Distance(curOrPos, lastOrPos) > .01f)
                    {
                        curOrPos = orientation.position;
                        _o.transform.position = Vector3.Lerp(_o.transform.position, orientation.position, OBJECT_MOVEMENT_ANIMATION_SPEED * Time.deltaTime);
                        o.transform.Rotate(Vector3.right * 10f);
                        lastOrPos = curOrPos;
                        yield return null;
                    }
                    _o.transform.position = orientation.position;
                    _o.transform.rotation = orientation.rotation;

                    // set physics accordingly to anchor the object in place
                    if (allowObjectPhysics)
                    {
                        Utility.ToggleObjectPhysics(_o, true, true, true, true);

                        Rigidbody rb = _o.GetComponent<Rigidbody>();
                        if (!allowLateralTranslation)
                        {
                            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                        }
                        if (!allowRotation || true)
                        {
                            rb.constraints = rb.constraints | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                        }

                        //rb.drag = 1000f;
                        //rb.angularDrag = 1000f;

                    }
                    else{
                        Utility.ToggleObjectPhysics(_o, true, true, false, false);
                    }

                    break;

                }

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
