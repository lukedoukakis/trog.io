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
    public static int RackCapacity_Stone = 28;

    // --

    public Camp camp;
    public Enum itemType;
    public int capacity;
    public List<GameObject> objectsOnRack;
    public Transform worldObject_orientationParent;
    List<Transform> orientations;
    public bool onDemandPlacement;  // whether new racks can be created when this one is filled up; if false, objects cannot be placed when attempting to place on this rack
    public bool allowObjectPhysics; // if true, physics are deactivated for objects added to the rack
    public bool allowItemHoverTriggers; // if true, hover triggers on placed items are not deactivated and can be grabbed individually
    public bool allowLateralTranslation;
    public bool allowRotation;


    public void SetObjectRack(Camp camp, Enum itemType){
        this.camp = camp;
        this.itemType = itemType;
        GameObject worldObjectPrefab;
        switch(itemType){
            case ItemType.Food :
                this.capacity = RackCapacity_Food;
                worldObjectPrefab = CampResources.PREFAB_RACK_FOOD;
                onDemandPlacement = true;
                allowObjectPhysics = false;
                allowItemHoverTriggers = true;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            case ItemType.Weapon :
                this.capacity = RackCapacity_Weapons;
                worldObjectPrefab = CampResources.PREFAB_RACK_WEAPONS;
                onDemandPlacement = true;
                allowObjectPhysics = false;
                allowItemHoverTriggers = true;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            case ItemType.Clothing : 
                this.capacity = RackCapacity_Clothing;
                worldObjectPrefab = CampResources.PREFAB_RACK_CLOTHING;
                onDemandPlacement = true;
                allowObjectPhysics = false;
                allowItemHoverTriggers = true;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            case ItemType.Wood : 
                this.capacity = RackCapacity_Wood;
                worldObjectPrefab = CampResources.PREFAB_RACK_WOOD;
                onDemandPlacement = true;
                allowObjectPhysics = true;
                allowItemHoverTriggers = false;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            case ItemType.Bone : 
                this.capacity = RackCapacity_Bone;
                worldObjectPrefab = CampResources.PREFAB_RACK_BONE;
                onDemandPlacement = true;
                allowObjectPhysics = true;
                allowItemHoverTriggers = false;
                allowLateralTranslation = true;
                allowRotation = true;
                break;
            case ItemType.Stone : 
                this.capacity = RackCapacity_Stone;
                worldObjectPrefab = CampResources.PREFAB_RACK_STONE;
                onDemandPlacement = true;
                allowObjectPhysics = true;
                allowItemHoverTriggers = false;
                allowLateralTranslation = true;
                allowRotation = true;
                break;
            case ItemType.Any : 
                this.capacity = 3;
                worldObjectPrefab = CampResources.PREFAB_WORKBENCH;
                onDemandPlacement = false;
                allowObjectPhysics = false;
                allowItemHoverTriggers = true;
                allowLateralTranslation = false;
                allowRotation = false;
                break;
            default:
                this.capacity = RackCapacity_Food;
                worldObjectPrefab = CampResources.PREFAB_RACK_FOOD;
                onDemandPlacement = true;
                allowObjectPhysics = false;
                allowItemHoverTriggers = true;
                allowLateralTranslation = false;
                allowRotation = false;
                Debug.Log("unsupported itemType for ItemRack");
                break;
        }
        SetWorldObject(Utility.InstantiateSameName(worldObjectPrefab));
        this.worldObject_orientationParent = Utility.FindDeepChild(this.worldObject.transform, "ItemOrientations");
        this.orientations = worldObject_orientationParent.GetComponentsInChildren<Transform>().Where(ortn => ortn.tag == "Orientation").ToList();
        if(allowRotation){
            foreach(Transform orientation in orientations){
                orientation.rotation = Utility.GetRandomRotation(360f);
            }
        }
        ObjectReference reference = this.worldObject.AddComponent<ObjectReference>();
        reference.SetObjectReference(this);
        objectsOnRack = new List<GameObject>();
    }



    public virtual void AddObjects(Item item, ref int countToAdd, Transform originT, ref int newRacksCount){

        int c = countToAdd;
        for(int countAdded = 0; countAdded < c; ++countAdded){ 
            if(!IsFull()){

                // if room in the rack, add the item to it
                if(item.worldObjectPrefab == null)
                {
                     Debug.Log("Instantiating item, but worldObject null: " + item.nme);
                }
                GameObject newWorldObject = Utility.InstantiateSameName(item.worldObjectPrefab);
                newWorldObject.GetComponent<ObjectReference>().SetObjectReference(this);
                SetObjectOrientation(newWorldObject, originT, (OBJECT_PLACEMENT_DELAY_TIMESTEP * countAdded) + (Camp.CAMP_COMPONENT_PLACING_TIME_GAP * newRacksCount));
                objectsOnRack.Add(newWorldObject);
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

    public virtual void RemoveObjects(Item item, ref int countToRemove, bool moveToAnotherPlace, object destination)
    {


        //Debug.Log("Removing " + countToRemove + " " + item.nme);

        // count the number of occurences of the item, and remove that many from the objects
        GameObject[] matches = objectsOnRack.Where(o => o.name == item.nme).ToArray();
        int occurences = matches.Length;

        Debug.Log("occurences: " + occurences);

        // if there are any instances of the object on this rack
        if (occurences > 0)
        {
            int c = countToRemove;
            for (int i = 0; i < Math.Min(occurences, c); ++i)
            {
                GameObject foundObject = matches[occurences - 1];

                // if a destination rack is desired, add the item to that rack ("moving" the item)
                if (moveToAnotherPlace)
                {
    
                    // if moving to another ObjectRack, destroy the object add a new one to faction with specified rack
                    if(destination is ObjectRack)
                    {
                        Transform tempT = new GameObject().transform;
                        tempT.SetPositionAndRotation(foundObject.transform.position, foundObject.transform.rotation);
                        float delay = i * ObjectRack.OBJECT_PLACEMENT_DELAY_TIMESTEP;
                        Utility.DestroyInSeconds(tempT.gameObject, delay + 5f);
                        camp.faction.AddItemOwned(item, 1, (ObjectRack)destination, tempT, delay);
                        
                        objectsOnRack.Remove(foundObject);
                        GameObject.Destroy(foundObject);

                    }

                    // if moving to an EntityItems, call the EntityItems to take the object
                    else if(destination is EntityItems)
                    {
                        Transform tempT = new GameObject().transform;
                        tempT.SetPositionAndRotation(foundObject.transform.position, foundObject.transform.rotation);
                        float delay = i * ObjectRack.OBJECT_PLACEMENT_DELAY_TIMESTEP;
                        Utility.DestroyInSeconds(tempT.gameObject, delay + 5f); 
                          
                        EntityItems ei = (EntityItems)destination;
                        ei.OnObjectTake(foundObject, this);


                        //ei.OnObjectTake(foundObject, null);
                    }
                    
                }

                // if not moving to another location, remove and destroy the object
                else
                {
                    objectsOnRack.Remove(foundObject);
                    GameObject.Destroy(foundObject);
                }

                --countToRemove;
            }
        }
    
        if(countToRemove > 0){
            if(camp.faction.GetItemCount(item) > 0)
            {
                // call to remove remaining count
                camp.RemoveObjectsAnyRack(item, ref countToRemove, moveToAnotherPlace, destination);
            }
        }
    }


    // set a given object's orientation to fit properly in the rack
    public void SetObjectOrientation(GameObject o, Transform originT, float delay)
    {

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
                    Utility.ToggleObjectPhysics(_o, false, false, false, false);
                    _o.SetActive(false);
                    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                    timer.Start();
                    while (timer.ElapsedMilliseconds / 1000f < delay)
                    {
                        yield return null;
                    }
                    timer.Stop();
                    _o.transform.position = originT.transform.position;
                    _o.SetActive(true);
                    Vector3 curOrPos, lastOrPos;
                    lastOrPos = orientation.position;
                    curOrPos = Vector3.zero;
                    while(Vector3.Distance(_o.transform.position, orientation.position) > .1f || Vector3.Distance(curOrPos, lastOrPos) > .01f)
                    {
                        curOrPos = orientation.position;
                        _o.transform.position = Vector3.Lerp(_o.transform.position, orientation.position, OBJECT_MOVEMENT_ANIMATION_SPEED * Time.deltaTime);
                        _o.transform.Rotate(Vector3.right * 20f);
                        lastOrPos = curOrPos;
                        yield return null;
                    }
                    _o.transform.position = orientation.position;
                    _o.transform.rotation = orientation.rotation;

                    // set physics accordingly to anchor the object in place
                    if (allowObjectPhysics)
                    {
                        Rigidbody rb = _o.GetComponent<Rigidbody>();
                        if (!allowLateralTranslation)
                        {
                            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                        }
                        if (!allowRotation || true)
                        {
                            rb.constraints = rb.constraints | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                        }
                    }

                    Utility.ToggleObjectPhysics(_o, allowObjectPhysics, allowItemHoverTriggers, allowObjectPhysics, allowObjectPhysics);

                    break;

                }
            }
        }
    }


    public bool IsFull(){
        return objectsOnRack.Count >= capacity;
    }

    public bool IsEmpty(){
        return objectsOnRack.Count == 0;
    }

}
