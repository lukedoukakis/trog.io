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


    public Transform t_left_hug;
    public Transform t_left_underArm;
    public Transform t_spear;
    public Transform t_axe;
    public Transform t_left_overShoulder;
    public Transform t_back;
    public Transform t_left_current;


    public Transform itemT;
    public Transform rightHandT;
    










    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityItems = this;

        itemT = transform.Find("ItemT");
        rightHandT = Utility.FindDeepChild(transform, "B-palm_01_R");

        foreach(Transform t in itemT){
            switch(t.gameObject.name){
                case "Hug" :
                    t_left_hug = t;
                    break;
                case "UnderArm" :
                    t_left_underArm = t;
                    break;
                case "Spear" :
                    t_spear = t;
                    break;
                case "Axe" :
                    t_axe = t;
                    break;
                case "OverShoulder" :
                    t_left_overShoulder = t;
                    break;
                case "Backpack" :
                    t_back = t;
                    break;
                default:
                    break;
            }
        }

        Log(rightHandT.name);

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
                t_left_current = t_left_hug;
                break;
            case (int)Item.HoldStyle.UnderArm :
                t_left_current = t_left_underArm;
                break;
            case (int)Item.HoldStyle.OverShoulder :
                t_left_current = t_left_overShoulder;
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
            hold.transform.position = Vector3.Lerp(hold.transform.position, t_left_current.position, objSpeed);
        }
        if(weapon_equipped != null){
            GameObject weap_on = weapon_equipped.Item2;
        }
        if(weapon_unequipped != null){
            GameObject weap_off = weapon_unequipped.Item2;

        }

        //weap_on.transform.position = Vector3.Lerp(weap_on.transform.position, , objSpeed);

    }

}
