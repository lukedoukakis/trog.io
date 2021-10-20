using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum ActionType{ Idle, GoTo, Follow, RunFrom, Collect, Pickup, Chase, Attack, AttackRecover, Build, Hunt, StepBack, StepSide }

public class ActionParameters : ScriptableObject
{

    // doer of the action
    public EntityHandle doerHandle;

    // type of action
    public Enum type;

    // gameobject to interact with
    public GameObject targetedWorldObject;

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

    public static ActionParameters GenerateActionParameters(EntityHandle _doerHandle, Enum _type, GameObject _obj, int _number, Item _item_target, Item _item_result, float _maxTime, float _distanceThreshold, Enum _bodyRotationMode, bool _urgent)
    {
        ActionParameters a = ActionParameters.GenerateActionParameters();
        a.doerHandle = _doerHandle;
        a.type = _type;
        a.targetedWorldObject = _obj;
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
    public static ActionParameters GenerateActionParameters(string command, EntityHandle doerHandle)
    {

        ActionParameters a = ActionParameters.GenerateActionParameters();
        a.doerHandle = doerHandle;
        switch(command)
        {
            case "Idle" :
                a.type = ActionType.Idle;
                break;

            case "Go Home" :

                Transform campT = doerHandle.entityInfo.faction.camp.GetOpenTribeMemberStandPosition();
                Transform newHomeT = new GameObject().transform;
                newHomeT.position = campT.position;
                newHomeT.transform.SetParent(campT);
                Destroy(a.doerHandle.entityBehavior.homeT.gameObject);
                a.doerHandle.entityBehavior.homeT = newHomeT;


                a.type = ActionType.Follow;
                a.targetedWorldObject = newHomeT.gameObject;
                a.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_SAME_POINT;
                a.urgent = false;
                break;

            case "Follow Player" :

                Transform directionalTs = Utility.FindDeepChild(GameManager.current.localPlayer.gameObject.transform, "DirectionalTs");
            
                a.type = ActionType.Follow;
                a.targetedWorldObject = directionalTs.GetChild(UnityEngine.Random.Range(0, directionalTs.childCount - 1)).gameObject;
                a.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_SAME_SPOT;
                break;

            case "Follow Faction Leader" :

                Transform _directionalTs = Utility.FindDeepChild(doerHandle.entityInfo.faction.leaderHandle.gameObject.transform, "DirectionalTs");
            
                a.type = ActionType.Follow;
                a.targetedWorldObject = _directionalTs.GetChild(UnityEngine.Random.Range(0, _directionalTs.childCount - 1)).gameObject;
                a.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_SAME_SPOT;
                a.maxTime = .5f;
                break;

            case "Run From Player" :

                a.type = ActionType.RunFrom;
                a.targetedWorldObject = GameManager.current.localPlayer.gameObject;
                a.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_CHASE;
                a.urgent = true;
                break;

            case "Attack Player" :

                a.type = ActionType.Chase;
                a.targetedWorldObject = GameManager.current.localPlayer.gameObject;
                a.maxTime = doerHandle.entityBehavior.CalculateChaseTime();
                a.urgent = true;
                break;

            case "Idle For 5 Seconds" :

                a.type = ActionType.Idle;
                a.maxTime = 5f;
                break;

            case "Go To Random Nearby Spot" :

                a.type = ActionType.GoTo;
                GameObject temp = new GameObject();
                temp.transform.position = Utility.GetRandomVectorOffset(doerHandle.transform.position, 10f, true);
                a.targetedWorldObject = temp;
                a.maxTime = 10f;
                a.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_SAME_SPOT;
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
                EntityHandle[] members = doerHandle.entityInfo.faction.memberHandles.ToArray();
                foreach(EntityHandle h in members){
                    if(h != doerHandle){
                        a.targetedWorldObject = h.gameObject;
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

    public static ActionParameters GenerateActionParameters()
    {

        ActionParameters a = ScriptableObject.CreateInstance<ActionParameters>();
        a.doerHandle = null;
        a.type = null;
        a.targetedWorldObject = null;
        a.number = -1;
        a.item_result = null;
        a.item_target = null;
        a.maxTime = -1;
        a.distanceThreshold = -1;
        a.bodyRotationMode = BodyRotationMode.Normal;
        a.urgent = false;
        return a;
    }

    public static ActionParameters Clone(ActionParameters baseAp)
    {
        ActionParameters newAp = ScriptableObject.CreateInstance<ActionParameters>();

        newAp.doerHandle = baseAp.doerHandle;
        newAp.type = baseAp.type;
        newAp.targetedWorldObject = baseAp.targetedWorldObject;
        newAp.number = baseAp.number;
        newAp.item_result = baseAp.item_result;
        newAp.item_target = baseAp.item_target;
        newAp.maxTime = baseAp.maxTime;
        newAp.distanceThreshold = baseAp.distanceThreshold;
        newAp.bodyRotationMode = baseAp.bodyRotationMode;
        newAp.urgent = baseAp.urgent;

        return newAp;
    }




    public override string ToString(){
        return Enum.GetName(typeof(ActionType), type);
    }
}
