using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum ActionType{ Idle, GoTo, Follow, RunFrom, Collect, Pickup, Chase, Attack, AttackRecover, Build, Hunt, StepBack, StepSide }
public enum InterruptionTier{ Anything, SenseDanger, BeenHit, Nothing }

public class ActionParameters : ScriptableObject
{

    // doer of the action
    public EntityHandle doerHandle;

    // type of action
    public ActionType type;

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
    public BodyRotationMode bodyRotationMode;

    public InterruptionTier interruptionTier;
    // is urgent, i.e. will the entity sprint to accomplish the action etc.
    public bool urgent;


    // maximum time to be spent executing the action
    public float maxTime;

    public static ActionParameters GenerateActionParameters(EntityHandle _doerHandle, ActionType _type, GameObject _targetWorldObject, Vector3 _offset, int _number, Item _item_target, Item _item_result, float _maxTime, float _distanceThreshold, BodyRotationMode _bodyRotationMode, InterruptionTier _interruptionTier, bool _urgent)
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
        a.interruptionTier = _interruptionTier;
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

                ap.type = ActionType.Follow;
                ap.targetedWorldObject = doerHandle.entityBehavior.homeT.gameObject;
                ap.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_SAME_POINT;
                ap.maxTime = 1f;
                ap.urgent = false;
                break;
            
            case "Go Rest" :

                ap.type = ActionType.Follow;
                ap.targetedWorldObject = doerHandle.entityBehavior.FindOpenRestingLocation().gameObject;
                ap.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_SAME_POINT;
                ap.maxTime = 1f;
                ap.urgent = false;
                break;

            case "Follow Faction Leader" :

                ap.type = ActionType.Follow;
                ap.targetedWorldObject = doerHandle.entityBehavior.homeT.gameObject;
                ap.offset = ap.doerHandle.entityBehavior.followOffset;
                ap.distanceThreshold = 2.5f;
                ap.maxTime = 1f;
                ap.urgent = false;
                break;

            case "Run From Player" :

                ap.type = ActionType.RunFrom;
                ap.targetedWorldObject = GameManager.instance.localPlayer.gameObject;
                ap.distanceThreshold = EntityBehavior.DISTANCE_THRESHOLD_CHASE;
                ap.urgent = true;
                break;

            case "Attack Player" :

                ap.type = ActionType.Chase;
                ap.targetedWorldObject = GameManager.instance.localPlayer.gameObject;
                ap.maxTime = doerHandle.entityBehavior.CalculateChaseTime();
                ap.urgent = true;
                break;

            case "Idle For 5 Seconds" :

                ap.type = ActionType.Idle;
                ap.maxTime = 5f;
                break;

            // TODO: idle until something is true

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
                ap.item_target = Item.SpearStone;
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
        ap.type = ActionType.Idle;
        ap.targetedWorldObject = null;
        ap.offset = Vector3.zero;
        ap.number = -1;
        ap.item_result = null;
        ap.item_target = null;
        ap.maxTime = -1;
        ap.distanceThreshold = -1;
        ap.bodyRotationMode = BodyRotationMode.Normal;
        ap.interruptionTier = InterruptionTier.Anything;
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
        newAp.interruptionTier = baseAp.interruptionTier;
        newAp.urgent = baseAp.urgent;

        return newAp;
    }




    public override string ToString(){
        return Enum.GetName(typeof(ActionType), type);
    }
}
