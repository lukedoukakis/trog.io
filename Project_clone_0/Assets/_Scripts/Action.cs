using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Action : ScriptableObject
{

    // type of action
    public int type;

    // gameobject to interact with
    public GameObject obj;

    // ambiguous number of things
    public int number;

    // target item
    public Item item_target;

    // resultant item
    public Item item_result;

    // distance at which we reached the target, if there is one
    public float distanceThreshold;

    // body rotation mode
    public int bodyRotationMode;

    // is urgent, i.e. will the entity sprint to accomplish the action etc.
    public bool urgent;


    // maximum time to be spent executing the action
    public int maxSeconds;

    public static Action GenerateAction(int _type, GameObject _obj, int _number, Item _item_target, Item _item_result, int _maxSeconds, float _distanceThreshold, int _bodyRotationMode, bool _urgent){
        Action a = ScriptableObject.CreateInstance<Action>();
        a.type = _type;
        a.obj = _obj;
        a.number = _number;
        a.item_target = _item_target;
        a.item_result = _item_result;
        a.maxSeconds = _maxSeconds;
        a.distanceThreshold = _distanceThreshold;
        a.bodyRotationMode = _bodyRotationMode;
        a.urgent = _urgent;

        return a;
    }
    public static Action GenerateAction(){
        Action a = ScriptableObject.CreateInstance<Action>();
        a.type = -1;
        a.obj = null;
        a.number = -1;
        a.item_result = null;
        a.item_target = null;
        a.maxSeconds = -1;
        a.distanceThreshold = -1;
        a.bodyRotationMode = (int)EntityAnimation.BodyRotationMode.Normal;
        a.urgent = false;
        return a;
    }

    // some predefined actions
    public static Action GenerateAction(string command, EntityHandle handle){

        Action a = Action.GenerateAction();
        switch(command){
            case "Idle" :
                a.type = (int)Action.ActionTypes.Idle;
                break;
            case "Go Home" :
                a.type = (int)Action.ActionTypes.GoTo;
                a.obj = handle.entityBehavior.home.gameObject;
                a.distanceThreshold = EntityBehavior.distanceThreshold_spot;
                break;
            case "Follow Player" :
                a.type = (int)Action.ActionTypes.Follow;
                a.obj = Player.current.gameObject;
                a.distanceThreshold = EntityBehavior.distanceThreshold_spot;
                break;
            case "Collect Item" :
                a.type = (int)Action.ActionTypes.Collect;
                // TODO: finish params
                break;
             case "Attack Entity" :
                a.type = (int)Action.ActionTypes.Attack;
                a.urgent = true;
                // TODO: finish params
                break;

            case "Collect Spear" :
                a.type = (int)Action.ActionTypes.Collect;
                a.item_target = Item.Spear;
                //Log(a.item_target.nme);
                break;
            case "Collect Stone" :
                a.type = (int)Action.ActionTypes.Collect;
                a.item_target = Item.Stone;
                //Log(a.item_target.nme);
                break;
            case "Attack TribeMember" :
                a.type = (int)Action.ActionTypes.Attack;
                a.urgent = true;
                // a.obj = GameObject.FindGameObjectWithTag("Player");

                EntityHandle[] members = handle.entityInfo.faction.members.ToArray();
                foreach(EntityHandle h in members){
                    if(h != handle){
                        a.obj = h.gameObject;
                    }
                }


                //Log(a.item_target.nme);
                break;         
            default :
            Debug.Log("ObjectBehavior: no action for command specified: \"" + command + "\"");
                break;
        }
        //Debug.Log("CreateAction() done");
        return a;
        
    }



    public enum ActionTypes{
        Idle,
        GoTo,
        Follow,
        Collect,
        Pickup,
        Attack,
        Swing,
        AttackRecover,
        Build,
        Hunt
    }



    public override string ToString(){
        return Enum.GetName(typeof(ActionTypes), type);
    }
}
