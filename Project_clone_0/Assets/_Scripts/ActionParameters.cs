using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum ActionType{ Idle, GoTo, Follow, RunFrom, Collect, Pickup, Chase, Attack, AttackRecover, Build, Hunt }

public class ActionParameters : ScriptableObject
{

    // type of action
    public Enum type;

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
    public Enum bodyRotationMode;

    // is urgent, i.e. will the entity sprint to accomplish the action etc.
    public bool urgent;


    // maximum time to be spent executing the action
    public float maxTime;

    public static ActionParameters GenerateActionParameters(Enum _type, GameObject _obj, int _number, Item _item_target, Item _item_result, float _maxTime, float _distanceThreshold, Enum _bodyRotationMode, bool _urgent){
        ActionParameters a = ActionParameters.GenerateActionParameters();
        a.type = _type;
        a.obj = _obj;
        a.number = _number;
        a.item_target = _item_target;
        a.item_result = _item_result;
        a.maxTime = _maxTime;
        a.distanceThreshold = _distanceThreshold;
        a.bodyRotationMode = _bodyRotationMode;
        a.urgent = _urgent;

        return a;
    }

    // some predefined actions
    public static ActionParameters GenerateActionParameters(string command, EntityHandle calleeHandle){


        Transform calleeHomeT = calleeHandle.entityBehavior.homeT;
        Destroy(calleeHomeT.gameObject);
        calleeHandle.entityBehavior.homeT = new GameObject().transform;
        calleeHomeT = calleeHandle.entityBehavior.homeT;

        ActionParameters a = ActionParameters.GenerateActionParameters();
        switch(command){
            case "Idle" :
                a.type = ActionType.Idle;
                break;
            case "Go Home" :
                a.type = ActionType.Follow;
                Transform newHomeT = calleeHandle.entityInfo.faction.camp.GetOpenTribeMemberStandPosition();
                calleeHomeT.transform.position = newHomeT.position;
                calleeHomeT.transform.SetParent(newHomeT);
                a.obj = calleeHomeT.gameObject;
                a.distanceThreshold = EntityBehavior.distanceThreshold_point;
                a.urgent = false;
                break;
            case "Follow Player" :

                Transform directionalTs = Utility.FindDeepChild(GameManager.current.localPlayer.gameObject.transform, "DirectionalTs");
            
                a.type = ActionType.Follow;
                a.obj = directionalTs.GetChild(UnityEngine.Random.Range(0, directionalTs.childCount - 1)).gameObject;
                a.distanceThreshold = EntityBehavior.distanceThreshold_spot;
                break;
            case "Run From Player" :
                a.type = ActionType.RunFrom;
                a.obj = GameManager.current.localPlayer.gameObject;
                a.distanceThreshold = EntityBehavior.distanceThreshhold_pursuit;
                a.urgent = true;
                break;
            case "Attack Player" :
                a.type = ActionType.Chase;
                a.obj = GameManager.current.localPlayer.gameObject;
                a.maxTime = calleeHandle.entityBehavior.GetChaseTime();
                a.urgent = true;
                break;
            case "Idle For 5 Seconds" :
                a.type = ActionType.Idle;
                a.maxTime = 5f;
                break;
            case "Go To Random Nearby Spot" :
                a.type = ActionType.GoTo;
                GameObject temp = new GameObject();
                temp.transform.position = Utility.GetRandomVectorOffset(calleeHandle.transform.position, 10f, true);
                a.obj = temp;
                a.maxTime = 10f;
                a.distanceThreshold = EntityBehavior.distanceThreshold_spot;
                break;
            case "Collect Item" :
                a.type = ActionType.Collect;
                // TODO: finish params
                break;
             case "Attack Entity" :
                a.type = ActionType.Chase;
                a.urgent = true;
                // TODO: finish params
                break;

            case "Collect Spear" :
                a.type = ActionType.Collect;
                a.item_target = Item.Spear;
                //Log(a.item_target.nme);
                break;
            case "Collect Stone" :
                a.type = ActionType.Collect;
                a.item_target = Item.Stone;
                //Log(a.item_target.nme);
                break;
            case "Attack TribeMember" :
                a.type = ActionType.Chase;
                a.urgent = true;
                // a.obj = GameObject.FindGameObjectWithTag("Player");

                EntityHandle[] members = calleeHandle.entityInfo.faction.members.ToArray();
                foreach(EntityHandle h in members){
                    if(h != calleeHandle){
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

    public static ActionParameters GenerateActionParameters(){
        ActionParameters a = ScriptableObject.CreateInstance<ActionParameters>();
        a.type = null;
        a.obj = null;
        a.number = -1;
        a.item_result = null;
        a.item_target = null;
        a.maxTime = -1;
        a.distanceThreshold = -1;
        a.bodyRotationMode = EntityOrientation.BodyRotationMode.Normal;
        a.urgent = false;
        return a;
    }




    public override string ToString(){
        return Enum.GetName(typeof(ActionType), type);
    }
}
