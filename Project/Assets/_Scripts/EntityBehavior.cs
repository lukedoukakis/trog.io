﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityBehavior : EntityComponent
{

    public Transform home;
    public Vector3 move;


    public static float randomOffsetRange = 1f;
    public static float distanceThreshold_none = -1f;
    public static float distanceThreshold_point = .1f;
    public static float distanceThreshold_spot = 2f;
    public static float distanceThreshold_combat = 1.5f;

    Vector3 randomOffset;


    // sensing and movement parameters
    public static float senseDistance_obstacle = 3f;
    public static float senseDistance_immediate = .25f;
    public static float senseDistance_search = 15f;
    public static float senseDistance_earshot = 50f;
    public static float senseDistance_infinite = 500f;
    public static float maxJumpFromDistance = 2f;
    public static float rotationSpeed = 1f;



    public Action activeAction;
    public List<Action> actions;
    public enum Priority{
        Back, Front, FrontImmediate
    }


    public Dictionary<string, Action> actionLayers;
    public Dictionary<string, IEnumerator> coroutineLayers;



    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityBehavior = this;
        home = GameObject.FindGameObjectWithTag("Home").transform;
        randomOffset = new Vector3(UnityEngine.Random.Range(randomOffsetRange*-1f, randomOffsetRange), 0f, UnityEngine.Random.Range(randomOffsetRange*-1f, 0));
        actionLayers = new Dictionary<string, Action>{
            {"Command", null},
            {"Movement", null},
            {"Hands", null},
        };
        coroutineLayers = new Dictionary<string, IEnumerator>{
            {"Command", null},
            {"Movement", null},
            {"Hands", null},
        };
    }

    void Start(){

    }


    // add an action to the end of the queue
    public void AddAction(Action a){
        actions.Add(a);
    }

    // insert an action to the front of the queue, to be executed when the current action is finished
    public void InsertAction(Action a){
        actions.Insert(0, a);
    }

    // insert an action to the front of the queue and immediately execute
    public void InsertActionImmediate(Action a, bool clear){
        TerminateActionLayer("Command");
        if(clear){
            if(actions.Count > 0){
                actions.Clear();
            }
        }
        InsertAction(a);
        OnActionInterrupt();
        NextAction();
        //Debug.Log("InsertActionImmediate() done");
    }



    // select and execute the next action in the queue... if list is empty, insert "go home" or "idle" action
    public Action NextAction(){
        if(actions.Count == 0){
            Log("Actions empty -> idling");
            Action idle = Action.GenerateAction("Idle", handle);
            InsertAction(idle);
        }
        activeAction = actions[0];
        actions.RemoveAt(0);
        ExecuteAction(activeAction);
        Log("Action type: " + activeAction.ToString());
        return activeAction;
    }
    public void OnActionInterrupt(){

    }

    public void ExecuteAction(Action a){
        Transform t = null;
        if(a.obj == null){ t = null; }else{ t = a.obj.transform; }
        handle.entityAnimation.SetBodyRotationMode(a.bodyRotationMode, t);

        switch(a.type){
            case (int)Action.ActionTypes.Idle :
                Idle(a);
                break;
            case (int)Action.ActionTypes.GoTo :
                GoTo(a);
                break;
            case (int)Action.ActionTypes.Follow :
                Follow(a);
                break;
            case (int)Action.ActionTypes.Collect :
                Collect(a);
                break;
            case (int)Action.ActionTypes.Pickup :
                Pickup(a);
                break;
            case (int)Action.ActionTypes.Attack :
                Attack(a);
                break;
            case (int)Action.ActionTypes.Swing :
                Swing(a);
                break;
            case (int)Action.ActionTypes.AttackRecover :
                AttackRecover(a);
                break;
            case (int)Action.ActionTypes.Build :
                Build(a);
                break;
            case (int)Action.ActionTypes.Hunt :
                Hunt(a);
                break;
            default:
                Debug.Log("ObjectBehavior: called action not a defined action (" + a.type + ")... idling.");
                break;
        }
    }



    


    public void Idle(Action a){
        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", a, _Idle());

        IEnumerator _Idle(){
            move = Vector3.zero;
            while(true){
                yield return null;
            }
        }
    }

    public void GoTo(Action a){

        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", a, _GoTo());

        IEnumerator _GoTo()
        {

            Transform targetT = a.obj.transform;
            while (true)
            {
                if (!IsAtPosition(targetT.position, a.distanceThreshold)){
                    move = GetNavigationDirection(targetT);
                    handle.entityPhysics.moveDir = move;
                }
                else
                {
                    NextAction();
                }
                yield return null;
            }
        }
    }

    public void Follow(Action a){

        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", a, _Follow());

        IEnumerator _Follow()
        {

            Transform targetT;
            if(a.bodyRotationMode == (int)EntityAnimation.BodyRotationMode.Normal){
                //targetT = a.obj.transform;
                Transform directionalTs = Utility.FindDeepChild(a.obj.transform, "DirectionalTs");
                targetT = directionalTs.GetChild(UnityEngine.Random.Range(0, directionalTs.childCount - 1));
            }
            else{
                Transform directionalTs = Utility.FindDeepChild(a.obj.transform, "DirectionalTs");
                targetT = directionalTs.GetChild(UnityEngine.Random.Range(0, directionalTs.childCount - 1));
            }
            

            while (true)
            {

                if(Vector3.Distance(transform.position, targetT.position) > a.distanceThreshold){
                    move = GetNavigationDirection(targetT);
                    handle.entityPhysics.moveDir = move;
                }
                else{
                    handle.entityPhysics.moveDir = Vector3.zero;
                }
                yield return null;

            }
        }
    }

    public void Collect(Action a){

        Item i_target = a.item_target;
        Log("target name: " + i_target.nme);

        List<GameObject> foundObjects = SenseSurroundingItems(i_target.type, i_target.nme, senseDistance_infinite, handle.entityInfo.faction.warringFactions);
        if(foundObjects.Count == 0){
            // TODO: search in new area if nothing found
            Log("Collect: nothing found");
        }
        else{
            Log("Collect: picking up object");
            GameObject target = foundObjects[0];
            Faction.AddItemTargeted(target, handle.entityInfo.faction);
            Action goToObject = Action.GenerateAction((int)(Action.ActionTypes.GoTo), target, -1, Item.GetItemByName(target.name), null, -1, distanceThreshold_spot, (int)EntityAnimation.BodyRotationMode.Normal);
            Action pickupObject = Action.GenerateAction((int)(Action.ActionTypes.Pickup), target, -1, Item.GetItemByName(target.name), null, -1, -1f, (int)EntityAnimation.BodyRotationMode.Normal);
            Action followPlayer = Action.GenerateAction("Follow Player", handle);
            InsertAction(pickupObject);
            InsertAction(goToObject);
            NextAction();
        }
    }

    public void Pickup(Action a){

        TerminateActionLayer("Hands");
        BeginActionLayer("Hands", a, _Pickup());

        IEnumerator _Pickup(){
            GameObject target = a.obj;
            Faction.AddItemOwned(target, handle.entityInfo.faction);
            Faction.RemoveItemTargeted(target, handle.entityInfo.faction);
            yield return new WaitForSecondsRealtime(.25f);
            TakeFromGround(target);
            yield return new WaitForSecondsRealtime(.25f);
            NextAction();
        }

    }

    public void Attack(Action a){
        GameObject target = a.obj;
        Action goToTarget = Action.GenerateAction((int)(Action.ActionTypes.GoTo), target, -1, null, null, -1, distanceThreshold_spot, (int)EntityAnimation.BodyRotationMode.Target);
        Action swingAtTarget = Action.GenerateAction((int)(Action.ActionTypes.Swing), target, -1, null, null, -1, distanceThreshold_spot, (int)EntityAnimation.BodyRotationMode.Target);
        InsertAction(swingAtTarget);
        InsertAction(goToTarget);
        NextAction();
    }

    void Swing(Action a){
        
        TerminateActionLayer("Hands");
        BeginActionLayer("Hands", a, _Swing());

        IEnumerator _Swing(){
            handle.entityAnimation.UseWeapon();
            Action attackRecover = Action.GenerateAction((int)(Action.ActionTypes.AttackRecover), a.obj, -1, null, null, -1, distanceThreshold_spot, (int)EntityAnimation.BodyRotationMode.Target);
            InsertAction(attackRecover);
            yield return null;
            NextAction();
        } 
    }

    void AttackRecover(Action a){

        TerminateActionLayer("Command");
        BeginActionLayer("Command", a, _AttackRecover());

        IEnumerator _AttackRecover(){

            GameObject target = a.obj;
            EntityStatus targetStatus = target.GetComponent<EntityStatus>();
            if(true){ // TODO: if target is alive
                
                
                Action followTarget = Action.GenerateAction((int)(Action.ActionTypes.Follow), a.obj, -1, null, null, -1, distanceThreshold_combat, (int)EntityAnimation.BodyRotationMode.Target);
                Action repeatAttack = Action.GenerateAction((int)(Action.ActionTypes.Attack), a.obj, -1, null, null, -1, distanceThreshold_spot, (int)EntityAnimation.BodyRotationMode.Target);
                
                InsertAction(followTarget);
                NextAction();
                yield return new WaitForSecondsRealtime(.3f);
                InsertAction(repeatAttack);
                NextAction();


            }
        }
        
        
    }

    public void Build(Action a){

    }

    public void Hunt(Action a){

    }


    void TerminateActionLayer(string layer){
        IEnumerator current = coroutineLayers[layer];
        if(current != null){
            StopCoroutine(current);
        }
        actionLayers[layer] = null;
        coroutineLayers[layer] = null;
    }

    void BeginActionLayer(string layer, Action a, IEnumerator coroutine){
        actionLayers[layer] = a;
        coroutineLayers[layer] = coroutine;
        StartCoroutine(coroutine);
    }


    Vector3 GetNavigationDirection(Transform targetT){

        Vector3 targetDirection = targetT.position - transform.position;
        Transform gyro = handle.entityPhysics.gyro;
        gyro.LookAt(targetT);
        Quaternion rot = gyro.rotation;
        rot.x = 0;
        rot.z = 0;
        gyro.rotation = rot;
		
        float leftDistance, centerDistance, rightDistance;
        RaycastHit leftHitInfo, centerHitInfo, rightHitInfo;
		Vector3 path = transform.position - targetT.position;
		path.y = 0;

			
		// if obstacle in front and it's not the player object
		if(SenseObstacle()){
				
			// if obstacle can't be jumped over, navigate around it
			if(!CanClearObstacle()){
				TurnTowardsMostOpenPath();
			}
			else{	
				
				// if close enough to obstacle and on the ground, jump
				if(Mathf.Min(Mathf.Min(leftDistance, centerDistance), rightDistance) < maxJumpFromDistance){
					if(handle.entityPhysics.CanJump()){
						handle.entityPhysics.Jump();
					}
				}
			}
		}
        
        Rigidbody targetRb = targetT.GetComponent<Rigidbody>();
        Vector3 tp = targetT.position;
        tp += targetT.TransformDirection(randomOffset);
        //RotateToward(tp, .2f);
        return targetDirection;



        bool SenseObstacle(){

            // set raycasts to reach castDistance units away
            Transform gs = handle.entityPhysics.groundSense;
            Vector3 moveDir = handle.entityPhysics.moveDir;

            bool leftCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward + gyro.right*-2f, out leftHitInfo, senseDistance_obstacle);
            bool centerCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward, out centerHitInfo, senseDistance_obstacle);
            bool rightCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward + gyro.right*2f, out rightHitInfo, senseDistance_obstacle);


            Debug.DrawRay(transform.position + new Vector3(0, .1f, 0), (gyro.forward + gyro.right*-2f).normalized*senseDistance_obstacle, Color.green, Time.deltaTime);
            Debug.DrawRay(transform.position + new Vector3(0, .1f, 0), (gyro.forward).normalized*senseDistance_obstacle, Color.green, Time.deltaTime);
            Debug.DrawRay(transform.position + new Vector3(0, .1f, 0), (gyro.forward + gyro.right*2f).normalized*senseDistance_obstacle, Color.green, Time.deltaTime);
            
            List<RaycastHit> hitInfos = new List<RaycastHit>();

            // set leftDistance, centerDistance, rightDistance
            if (leftCast){ leftDistance = (leftHitInfo.point - transform.position).magnitude; hitInfos.Add(leftHitInfo); }
            else{ leftDistance = int.MaxValue; }
            if (centerCast){ centerDistance = (centerHitInfo.point - transform.position).magnitude; hitInfos.Add(centerHitInfo); }
            else{ centerDistance = int.MaxValue; }
            if (rightCast){ rightDistance = (rightHitInfo.point - transform.position).magnitude; hitInfos.Add(rightHitInfo); }
            else{ rightDistance = int.MaxValue; }

            // return true if any of the raycasts hit something besides a tribe member
            int hits = 0;
            foreach(RaycastHit hitInfo in hitInfos){
                string tag = hitInfo.collider.gameObject.tag;
                if(tag != "TribeMember" && tag != "Player"){
                    hits++;
                }
            }
            return hits >= 2;

        }

        bool CanClearObstacle(){
            Transform ohs = handle.entityPhysics.obstacleHeightSense;
            return !Physics.BoxCast(ohs.position, new Vector3(handle.entityPhysics.hitbox.bounds.extents.x, .01f, .1f), gyro.forward, gyro.rotation, Mathf.Max(leftDistance, centerDistance, rightDistance));
        }

        void TurnTowardsMostOpenPath(){

            if (leftDistance < rightDistance){
                targetDirection = gyro.forward + gyro.right*3f;
            }
            else{
                targetDirection = gyro.forward + gyro.right*-3f;
            }
        }
	}

    void TakeFromGround(GameObject o){
        Log("TakeFromGround()");
        Item item = Item.GetItemByName(o.name);
        Tuple<Item, GameObject> pair = new Tuple<Item, GameObject>(item, o);
        switch(item.type){
            case (int)Item.Type.Misc:
                handle.entityItems.SetHolding(pair);
                break;
            case (int)Item.Type.Weapon:
                handle.entityItems.SetWeapon(pair);
                break;
            case (int)Item.Type.Container:
                handle.entityItems.SetHolding(pair);
                break;
            case (int)Item.Type.Pocket:
                handle.entityItems.PocketItem(item);
                break;
        }
        handle.entityAnimation.Pickup(item);
        //o.transform.position = o.transform.position += new Vector3(UnityEngine.Random.Range(-30f, 30f), 1f, UnityEngine.Random.Range(-30f, 30f));
    }
    public List<GameObject> SenseSurroundingItems(int type, string name, float distance, List<Faction> forbiddenFacs){
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, LayerMask.GetMask("Item"));
       
        string sur = "";
        List<GameObject> foundObjects = new List<GameObject>();
        GameObject o;
        Item i;
        bool forbid;
        foreach(Collider col in colliders){
            o = col.gameObject;
            i = Item.GetItemByName(o.name);
            forbid = false;
            if(type == -1 || i.type == type){
                if(name == null || o.name == name){
                    if(!Faction.ItemIsTargetedByFaction(o, handle.entityInfo.faction)){
                        foreach(Faction fac in forbiddenFacs){
                            if(Faction.ItemIsOwnedByFaction(o, fac)){
                                forbid = true;
                            }
                        }
                        if(!forbid){
                            foundObjects.Add(o);
                            sur += o.name + ", ";
                        }
                    }
                }
            }
        }
        //Debug.Log("Surroundings: " + sur);

        // order by proximity to entity
        foundObjects = foundObjects.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).ToList();


        return foundObjects;
        
    }



    public bool IsAtPosition(Vector3 position, float distanceThreshhold){
        return Vector3.Distance(transform.position, position) < distanceThreshhold;
    }


    // Update is called once per frame
    void Update()
    {   

        handle.entityPhysics.moveDir = move;


        if(tag == "Player"){
            if(Input.GetKeyUp(KeyCode.K)){
                MainCommand.current.SendCommand("Collect Spear");
            }
            if(Input.GetKeyUp(KeyCode.L)){
                MainCommand.current.SendCommand("Collect Stone");
            }

            if(Input.GetKeyUp(KeyCode.Mouse0)){
                handle.entityAnimation.UseWeapon();
            }
        }
        else{
             if(Input.GetKeyUp(KeyCode.Q)){
                MainCommand.current.SendCommand("Attack TribeMember");
            }
        }

        


        
    }




}
