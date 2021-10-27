using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum ActionType{ Idle, GoTo, Follow, RunFrom, Collect, Pickup, Chase, Attack, AttackRecover, Build, Hunt, StepBack, StepSide }
public enum InteruptionTier{ Anything, SenseDanger, Hit, Nothing }

public class ActionParameters : ScriptableObject
{

    // doer of the action
    public EntityHandle doerHandle;

    // type of action
    public Enum type;

    // gameobject to interact with
    public GameObject targetedWorldObject;

    // offset vector
    public Vector3 offset;

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

    public Enum interruptionLevel;
    // is urgent, i.e. will the entity sprint to accomplish the action etc.
    public bool urgent;


    // maximum time to be spent executing the action
    public float maxTime;

    public static ActionParameters GenerateActionParameters(EntityHandle _doerHandle, Enum _type, GameObject _targetWorldObject, Vector3 _offset, int _number, Item _item_target, Item _item_result, float _maxTime, float _distanceThreshold, Enum _bodyRotationMode, Enum _interruptionLevel, bool _urgent)
    {
        ActionParameters a = ActionParameters.GenerateActionParameters();
        a.doerHandle = _doerHandle;
        a.type = _type;
        a.targetedWorldObject = _targetWorldObject;
        a.offset = _offset;
        a.number = _number;
        a.item_target = _item_target;
        a.item_result = _item_result;
        a.maxTime = _maxTime;
        a.distanceThreshold = _distanceThreshold;
        a.bodyRotationMode = _bodyRotationMode;
        a.interruptionLevel = _interruptionLevel;
        a.urgent = _urgent;

        return a;
    }

    // some predefined actions
    public static ActionParameters GenerateActionParameters(string command, EntityHandle doerHandle)
    {

        ActionParameters ap = ActionParameters.GenerateActionParameters();
        ap.doerHandle = doerHandle;
        switch(command)
        {
            case "Idle" :
                ap.type = ActionType.Idle;
                break;

            case "Go Home" :

                // set home transform
                Transform campT = doerHandle.entityInfo.faction.camp.GetOpenTribeMemberStandPosition();
                Transform newHomeT = new GameObject().transform;
                newHomeT.position = campT.position;
                newHomeT.transform.SetParent(campT);
                Destroy(ap.doerHandle.entityBehavior.homeT.gameObject);
                ap.doerHandle.entityBehavior.homeT = newHomeT;

                // set action parameters
                ap.type = ActionType.Follow;
                ap.targetedWorldObject = newHomeT.gameObject;
                ap.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_SAME_POINT;
                ap.urgent = true;
                break;

            case "Follow Player" :

                Transform directionalTs = Utility.FindDeepChild(GameManager.current.localPlayer.gameObject.transform, "DirectionalTs");
            
                ap.type = ActionType.Follow;
                ap.targetedWorldObject = directionalTs.GetChild(UnityEngine.Random.Range(0, directionalTs.childCount - 1)).gameObject;
                ap.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_SAME_SPOT;
                break;

            case "Follow Faction Leader" :

                Transform _directionalTs = Utility.FindDeepChild(doerHandle.entityInfo.faction.leaderHandle.gameObject.transform, "DirectionalTs");
            
                ap.type = ActionType.Follow;
                ap.targetedWorldObject = _directionalTs.GetChild(UnityEngine.Random.Range(0, _directionalTs.childCount - 1)).gameObject;
                ap.distanceThreshold = 5;
                ap.maxTime = -1f;
                ap.urgent = true;
                break;

            case "Run From Player" :

                ap.type = ActionType.RunFrom;
                ap.targetedWorldObject = GameManager.current.localPlayer.gameObject;
                ap.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_CHASE;
                ap.urgent = true;
                break;

            case "Attack Player" :

                ap.type = ActionType.Chase;
                ap.targetedWorldObject = GameManager.current.localPlayer.gameObject;
                ap.maxTime = doerHandle.entityBehavior.CalculateChaseTime();
                ap.urgent = true;
                break;

            case "Idle For 5 Seconds" :

                ap.type = ActionType.Idle;
                ap.maxTime = 5f;
                break;

            case "Go To Random Nearby Spot" :

                ap.type = ActionType.GoTo;
                GameObject temp = new GameObject();
                temp.transform.position = Utility.GetRandomVectorOffset(doerHandle.transform.position, 10f, true);
                ap.targetedWorldObject = temp;
                ap.maxTime = 10f;
                ap.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_SAME_SPOT;
                break;

            case "Collect Spear" :

                ap.type = ActionType.Collect;
                ap.item_target = Item.Spear;
                //Log(a.item_target.nme);
                break;

            case "Collect Stone" :

                ap.type = ActionType.Collect;
                ap.item_target = Item.Stone;
                //Log(a.item_target.nme);
                break;

            case "Attack TribeMember" :

                ap.type = ActionType.Chase;
                ap.urgent = true;
                EntityHandle[] members = doerHandle.entityInfo.faction.memberHandles.ToArray();
                foreach(EntityHandle h in members){
                    if(h != doerHandle){
                        ap.targetedWorldObject = h.gameObject;
                    }
                }
                //Log(a.item_target.nme);
                break;    

            default :
                Debug.Log("ObjectBehavior: no action for command specified: \"" + command + "\"");
                break;
        }
        //Debug.Log("CreateAction() done");
        return ap;
        
    }

    public static ActionParameters GenerateActionParameters()
    {

        ActionParameters ap = ScriptableObject.CreateInstance<ActionParameters>();
        ap.doerHandle = null;
        ap.type = null;
        ap.targetedWorldObject = null;
        ap.offset = Vector3.zero;
        ap.number = -1;
        ap.item_result = null;
        ap.item_target = null;
        ap.maxTime = -1;
        ap.distanceThreshold = -1;
        ap.bodyRotationMode = BodyRotationMode.Normal;
        ap.interruptionLevel = InteruptionTier.Anything;
        ap.urgent = false;
        return ap;
    }

    public static ActionParameters Clone(ActionParameters baseAp)
    {
        ActionParameters newAp = ScriptableObject.CreateInstance<ActionParameters>();

        newAp.doerHandle = baseAp.doerHandle;
        newAp.type = baseAp.type;
        newAp.targetedWorldObject = baseAp.targetedWorldObject;
        newAp.offset = baseAp.offset;
        newAp.number = baseAp.number;
        newAp.item_target = baseAp.item_target;
        newAp.maxTime = baseAp.maxTime;
        newAp.distanceThreshold = baseAp.distanceThreshold;
        newAp.bodyRotationMode = baseAp.bodyRotationMode;
        newAp.interruptionLevel = baseAp.interruptionLevel;
        newAp.urgent = baseAp.urgent;

        return newAp;
    }




    public override string ToString(){
        return Enum.GetName(typeof(ActionType), type);
    }
}
