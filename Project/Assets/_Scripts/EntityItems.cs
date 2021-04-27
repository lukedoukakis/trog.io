using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityItems : EntityComponent
{


    // Item the entity is currently holding - can only hold one item at a time - if held item is switched out, move to pockets if pocketable
    public Tuple<Item, GameObject> holding;

    // currently equipped weapon - when not holding, attached to character model
    public Tuple<Item, GameObject> weapon_equipped;
    public Tuple<Item, GameObject> weapon_unequipped;

    // items in the entity's pockets
    public ItemCollection pockets;


    public Transform t_hand_left;
    public Transform t_upperArm_left;
    public Transform t_shoulder_left;
    public Transform t_back;
    public Transform t_left_current;


    public Transform itemT;
    public Transform t_hand_right;
    










    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityItems = this;

        itemT = transform.Find("ItemT");
        t_hand_left = Utility.FindDeepChild(transform, "B-palm_01_L");
        t_upperArm_left = Utility.FindDeepChild(transform, "B-forearm_L");
        t_shoulder_left = Utility.FindDeepChild(transform, "B-upper_arm_L");
        t_hand_right = Utility.FindDeepChild(transform, "B-palm_01_R");
        t_back = Utility.FindDeepChild(transform, "B-upperChest");


        //Log(t_hand_right.name);

    }


    void Start()
    {
        
    }


    public void SetHolding(Tuple<Item, GameObject> itemObjectPair){
        if(holding != null){
            DropHolding();
        }
        holding = itemObjectPair;
        TogglePhysics(holding.Item2, false);
        switch(holding.Item1.holdStyle){
            case (int)Item.HoldStyle.Hug :
                t_left_current = t_hand_left;
                break;
            case (int)Item.HoldStyle.UnderArm :
                t_left_current = t_hand_left;
                break;
            case (int)Item.HoldStyle.OverShoulder :
                t_left_current = t_hand_left;
                break;
            default:
                break;
        }
        
    }
    public void DropHolding(){
        TogglePhysics(holding.Item2, true);
        holding = null;
    }

    public void SetWeapon(Tuple<Item, GameObject> itemObjectPair){
        if(weapon_equipped != null){
            if(weapon_unequipped != null){
                DropUnequippedWeapon();
            }
            SetUnequippedWeapon(itemObjectPair);
        }
        else{
            SetEquippedWeapon(itemObjectPair);
        }
    }
    public void DropUnequippedWeapon(){
        TogglePhysics(weapon_equipped.Item2, true);
        weapon_unequipped = null;
    }
    public void SetUnequippedWeapon(Tuple<Item, GameObject> itemObjectPair){
        weapon_unequipped = itemObjectPair;
        TogglePhysics(itemObjectPair.Item2, false);
    }
    public void SetEquippedWeapon(Tuple<Item, GameObject> itemObjectPair){
        weapon_equipped = itemObjectPair;
        TogglePhysics(itemObjectPair.Item2, false);
    }

    public void PocketItem(Item i){
        pockets.AddItem(i);
    }


    void TogglePhysics(GameObject o, bool value){
        o.GetComponent<BoxCollider>().enabled = value;
    }


    void Update(){

        float objSpeed = 30f * Time.deltaTime;

        if(holding != null){
            GameObject hold = holding.Item2;
            hold.transform.position = t_left_current.position + t_left_current.forward*hold.GetComponent<BoxCollider>().size.z/4f;
            //hold.transform.position = Vector3.Lerp(hold.transform.position, t_left_current.position, objSpeed);
        }
        if(weapon_equipped != null){
            GameObject weap_on = weapon_equipped.Item2;
            weap_on.transform.position = t_hand_right.position;
            weap_on.transform.rotation = t_hand_right.rotation;
        }
        if(weapon_unequipped != null){
            GameObject weap_off = weapon_unequipped.Item2;
            weap_off.transform.position = t_back.position;

        }

        //weap_on.transform.position = Vector3.Lerp(weap_on.transform.position, , objSpeed);

    }

}
