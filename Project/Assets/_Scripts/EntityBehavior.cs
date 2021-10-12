﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum ActionPriority{ Back, Front, FrontImmediate }
public enum AttackType{ Weapon, Bite, Swipe, HeadButt, Stomp }

public class EntityBehavior : EntityComponent
{

    public bool isPlayer;
    public BehaviorProfile behaviorProfile;
    public Transform homeT;
    public bool isAtHome;
    public Vector3 move;
    public bool urgent;


    public static float randomOffsetRange = 1f;
    public static float distanceThreshold_none = -1f;
    public static float distanceThreshold_point = .3f;
    public static float distanceThreshold_spot = 2f;
    public static float distanceThreshhold_lungeAttack = 10f;
    public static float distanceThreshold_combat = 15f;
    public static float distanceThreshhold_pursuit = 100f;
    public static float distanceThreshhold_active = 100f;

    Vector3 randomOffset;


    // sensing and movement parameters
    public float timeSince_creatureSense;
    public float timeSince_attack;
    public static readonly float baseTimeStep_creatureSense = 1f;
    public static readonly float baseTimeStep_attack = 1f;
    public static readonly float senseDistance_obstacle = 3f;
    public static readonly float senseDistance_immediate = 1f;
    public static readonly float senseDistance_search = 150f;
    public static readonly float senseDistance_earshot = 500f;
    public static readonly float senseDistance_infinite = 5000f;
    public static readonly float maxJumpFromDistance = 3f;
    public static readonly float rotationSpeed = 1f;
    public static readonly float baseTime_chase = 10f;
    public static readonly float baseTime_flee = 10f;



    public ActionParameters activeAction;
    public List<ActionParameters> actions;


    public Dictionary<string, ActionParameters> actionLayers;
    public Dictionary<string, IEnumerator> coroutineLayers;



    protected override void Awake(){

        base.Awake();

        isPlayer = tag == "Player";
        homeT = new GameObject().transform;
        randomOffset = new Vector3(UnityEngine.Random.Range(randomOffsetRange*-1f, randomOffsetRange), 0f, UnityEngine.Random.Range(randomOffsetRange*-1f, 0));
        actionLayers = new Dictionary<string, ActionParameters>{
            {"Command", null},
            {"Movement", null},
            {"Hands", null},
            {"Head", null}
        };
        coroutineLayers = new Dictionary<string, IEnumerator>{
            {"Command", null},
            {"Movement", null},
            {"Hands", null},
            {"Head", null }
        };

    }

    void Start(){
        timeSince_creatureSense = 0f;
        behaviorProfile = entityInfo.speciesInfo.behaviorProfile;
    }


    // add an action to the end of the queue
    public void AddAction(ActionParameters a){
        actions.Add(a);
    }

    // insert an action to the front of the queue, to be executed when the current action is finished
    public void InsertAction(ActionParameters a){
        actions.Insert(0, a);
    }

    // insert an action to the front of the queue and immediately execute
    public void InsertActionImmediate(ActionParameters a, bool clear){
        TerminateActionLayer("Command");
        if(clear){
            if(actions.Count > 0){
                actions.Clear();
            }
        }
        InsertAction(a);
        OnActionInterrupt();
        //Debug.Log("going to call nextaction... actions count: " + actions.Count);
        NextAction();
        //Debug.Log("InsertActionImmediate() done");
    }

    public void ClearActions(){
        actions.Clear();
    }



    // select and execute the next action in the queue... if list is empty, insert "go home" or "idle" action
    public ActionParameters NextAction(){
        timeSince_creatureSense = baseTimeStep_creatureSense;
        if(actions.Count == 0){
            //Debug.Log("Actions empty -> goto/idle sequence");
            ActionParameters goTo = ActionParameters.GenerateActionParameters("Go To Random Nearby Spot", entityHandle);
            ActionParameters idle = ActionParameters.GenerateActionParameters("Idle For 5 Seconds", entityHandle);
            InsertAction(goTo);
            InsertAction(idle);
        }
        activeAction = actions[0];
        actions.RemoveAt(0);
        ExecuteAction(activeAction);
        //Log("Action type: " + activeAction.ToString());
        return activeAction;
    }
    public void OnActionInterrupt(){

    }

    public void ExecuteAction(ActionParameters a){
        Transform t = null;
        if(a.obj == null){ t = null; }else{ t = a.obj.transform; }
        entityOrientation.SetBodyRotationMode(a.bodyRotationMode, t);
        switch(a.type){
            case ActionType.Idle :
                Idle(a);
                break;
            case ActionType.GoTo :
                GoTo(a);
                break;
            case ActionType.Follow :
                Follow(a);
                break;
            case ActionType.RunFrom :
                RunFrom(a);
                break;
            case ActionType.Collect :
                Collect(a);
                break;
            case ActionType.Pickup :
                Pickup(a);
                break;
            case ActionType.Chase :
                Chase(a);
                break;
            case ActionType.Attack :
                Attack(a);
                break;
            case ActionType.AttackRecover :
                AttackRecover(a);
                break;
            case ActionType.Build :
                Build(a);
                break;
            case ActionType.Hunt :
                Hunt(a);
                break;
            default:
                Debug.Log("ObjectBehavior: called action not a defined action (" + a.type + ")... idling.");
                break;
        }

        urgent = a.urgent;
    }



    


    public void Idle(ActionParameters a){
        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", a, _Idle());

        IEnumerator _Idle(){

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            float maxTime = a.maxTime;

            move = Vector3.zero;
            while(true){

                if(maxTime == -1f || (timer.ElapsedMilliseconds / 1000f) > maxTime){
                    timer.Stop();
                    NextAction();
                    break;
                }

                yield return null;
            }
        }
    }

    public void GoTo(ActionParameters a){

        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", a, _GoTo());
        //Debug.Log("STARTING GOTO");

        IEnumerator _GoTo()
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            float maxTime = a.maxTime;
            if(maxTime == -1f){ maxTime = float.MaxValue; }

            while(true)
            {
                if(a.obj == null){
                    timer.Stop();
                    NextAction();
                    break;
                }
                else if(IsAtPosition(a.obj.transform.position, a.distanceThreshold)){
                    timer.Stop();
                    NextAction();
                    break;
                }
                else if((timer.ElapsedMilliseconds / 1000f) > maxTime){
                    timer.Stop();
                    ClearActions();
                    NextAction();
                    break;
                }
                else{
                    //Debug.Log(timer.ElapsedMilliseconds / 1000f);
                    move = GetNavigationDirection(a.obj.transform, false);
                    entityPhysics.moveDir = move;
                }
                //SetHeadLookAt(a.obj.transform.position, -1f);
                yield return null;
            }
        }
    }

    public void Follow(ActionParameters a){

        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", a, _Follow(a, false));
    }

    public void RunFrom(ActionParameters a){
        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", a, _Follow(a, true));
    }
    IEnumerator _Follow(ActionParameters a, bool reverse)
    {

        //Debug.Log("_Follow()");

        Transform targetT;
        if(reverse){
            targetT = a.obj.transform;
        }
        else{
            targetT = a.obj.transform;
        }

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        float maxTime = a.maxTime;
        if(maxTime == -1f){ maxTime = float.MaxValue; }

        // repeats until action layer is canceled
        bool followCondition;
        while (targetT != null)
        {

            followCondition = (reverse ? (Vector3.Distance(transform.position, targetT.position) <= a.distanceThreshold) : Vector3.Distance(transform.position, targetT.position) > a.distanceThreshold) && (timer.ElapsedMilliseconds / 1000f) <= maxTime;
            if (followCondition)
            {
                move = GetNavigationDirection(targetT, reverse);
                entityPhysics.moveDir = move;
            }
            else
            {
                entityPhysics.moveDir = Vector3.zero;

                if(reverse){
                    timer.Stop();
                    NextAction();
                }
    
            }

            if (!reverse && urgent) { SetHeadLookAt(a.obj.transform.position, -1f); }

            yield return null;

        }
        NextAction();
    }

    public void Collect(ActionParameters a){

        Item i_target = a.item_target;
        //Log("target name: " + i_target.nme);

        List<GameObject> foundObjects = SenseSurroundingItems(i_target.type, i_target.nme, senseDistance_infinite);
        foundObjects = foundObjects.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).ToList();
        if(foundObjects.Count == 0){
            // TODO: search in new area if nothing found
            //Log("Collect: nothing found");
        }
        else{
            //Log("Collect: picking up object");
            GameObject target = foundObjects[0];
            Faction.AddItemTargeted(entityInfo.faction, target);
            ActionParameters goToObject = ActionParameters.GenerateActionParameters(ActionType.GoTo, target, -1, Item.GetItemByName(target.name), null, -1, distanceThreshold_spot, EntityOrientation.BodyRotationMode.Normal, false);
            ActionParameters pickupObject = ActionParameters.GenerateActionParameters(ActionType.Pickup, target, -1, Item.GetItemByName(target.name), null, -1, -1f, EntityOrientation.BodyRotationMode.Normal, false);
            ActionParameters followPlayer = ActionParameters.GenerateActionParameters("Follow Player", entityHandle);
            InsertAction(pickupObject);
            InsertAction(goToObject);
            NextAction();
        }
    }

    public void Pickup(ActionParameters a){

        TerminateActionLayer("Hands");
        BeginActionLayer("Hands", a, _Pickup());

        IEnumerator _Pickup(){
            Item i = a.item_target;
            GameObject o = a.obj;
            if(i.type.Equals(Item.ItemType.Weapon)){
                yield return new WaitForSecondsRealtime(.25f);
                TakeObject(o);
                yield return new WaitForSecondsRealtime(.25f);
            }
            else{
                Faction.OnItemPickup(i, o, entityInfo.faction);
            }
            Faction.RemoveItemTargeted(o, entityInfo.faction);
            
            NextAction();
        }

    }

    public void Chase(ActionParameters a){
        GameObject target = a.obj;
        ActionParameters goToTarget = ActionParameters.GenerateActionParameters(ActionType.GoTo, target, -1, null, null, a.maxTime, distanceThreshold_spot, EntityOrientation.BodyRotationMode.Target, true);
        ActionParameters attackTarget = ActionParameters.GenerateActionParameters(ActionType.Attack, target, -1, null, null, -1, distanceThreshold_spot, EntityOrientation.BodyRotationMode.Target, true);
        InsertAction(attackTarget);
        InsertAction(goToTarget);
        NextAction();
    }

    void Attack(ActionParameters a){

        // if attack rate allows and target isn't null, attack
        if (a.obj != null)
        {
            if (timeSince_attack >= baseTimeStep_attack * entityStats.statsCombined.attackSpeed)
            {
                timeSince_attack = 0f;
                TerminateActionLayer("Hands");
                BeginActionLayer("Hands", a, _Attack());
            }
        }
        else{
            NextAction();
        }
        

        IEnumerator _Attack()
        {
            List<AttackType> attackTypes = behaviorProfile.attackTypes;
            AttackType attackType;
            if(attackTypes.Contains(AttackType.Weapon))
            {
                attackType = AttackType.Weapon;
            }
            else
            {
                attackType = attackTypes[UnityEngine.Random.Range(0, behaviorProfile.attackTypes.Count)];
            }
            entityPhysics.Attack(attackType, a.obj.transform);
            ActionParameters attackRecover = ActionParameters.GenerateActionParameters(ActionType.AttackRecover, a.obj, -1, null, null, -1, distanceThreshold_spot, EntityOrientation.BodyRotationMode.Target, true);
            yield return new WaitForSecondsRealtime(.2f);
            if(a.obj != null){
                InsertAction(attackRecover);
            }
            NextAction();
        }
    }

    void AttackRecover(ActionParameters a){

        TerminateActionLayer("Command");
        BeginActionLayer("Command", a, _AttackRecover());

        IEnumerator _AttackRecover(){

            GameObject target = a.obj;

            if(target != null){

                // if target is alive (hasn't been deleted), queue repeat attack

                ActionParameters repeatAttack = ActionParameters.GenerateActionParameters(ActionType.Chase, a.obj, -1, null, null, GetChaseTime(), distanceThreshhold_lungeAttack, EntityOrientation.BodyRotationMode.Target, true);
                InsertAction(repeatAttack);
                foreach(ActionParameters ap in behaviorProfile.attackRecoverySequence){
                    ap.obj = a.obj;
                    InsertAction(ap);
                }
                yield return new WaitForSecondsRealtime(1f * (1f - entityStats.statsCombined.attackSpeed));
                NextAction();

            }
            else{
                Debug.Log("target has been killed");
                NextAction();
            }
        }
        
        
    }

    public void Build(ActionParameters a){

    }

    public void Hunt(ActionParameters a){

    }


    void TerminateActionLayer(string layer){
        IEnumerator current = coroutineLayers[layer];
        if(current != null){
            StopCoroutine(current);
        }
        actionLayers[layer] = null;
        coroutineLayers[layer] = null;
    }

    void BeginActionLayer(string layer, ActionParameters a, IEnumerator coroutine){
        actionLayers[layer] = a;
        coroutineLayers[layer] = coroutine;
        StartCoroutine(coroutine);
    }


    Vector3 GetNavigationDirection(Transform targetT, bool reverse){

        bool jumped = false;

        // set direction to face
        Vector3 targetDirection = reverse ? (transform.position - targetT.position) : (targetT.position - transform.position);
        Transform gyro = entityPhysics.gyro;
        gyro.LookAt(targetT);
        if(reverse){ gyro.Rotate(Vector3.up * 180f); } // if reverse (running away from target), turn in y axis
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
					if(entityPhysics.CanJump()){
						entityPhysics.Jump();
                        jumped = true;
					}
				}
			}
		}

        if(!jumped)
        {
            if (entityPhysics.handGrab)
            {
                if (entityPhysics.CanJump())
                {
                    entityPhysics.Jump();
                    jumped = true;
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
            Transform gs = entityPhysics.groundSense;
            Vector3 moveDir = entityPhysics.moveDir;

            bool leftCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward + gyro.right*-2f, out leftHitInfo, senseDistance_obstacle);
            bool centerCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward, out centerHitInfo, senseDistance_obstacle);
            bool rightCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward + gyro.right*2f, out rightHitInfo, senseDistance_obstacle);


            // Debug.DrawRay(transform.position + new Vector3(0, .1f, 0), (gyro.forward + gyro.right*-2f).normalized*senseDistance_obstacle, Color.green, Time.deltaTime);
            // Debug.DrawRay(transform.position + new Vector3(0, .1f, 0), (gyro.forward).normalized*senseDistance_obstacle, Color.green, Time.deltaTime);
            // Debug.DrawRay(transform.position + new Vector3(0, .1f, 0), (gyro.forward + gyro.right*2f).normalized*senseDistance_obstacle, Color.green, Time.deltaTime);
            
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
                if(hitInfo.normal.y < .5f && tag != "Npc" && tag != "Player" && tag != "Body"){
                    hits++;
                }
            }
            return hits >= 2;
            //return false;
            

        }

        bool CanClearObstacle(){
            Transform ohs = entityPhysics.obstacleHeightSense;
            return !Physics.BoxCast(ohs.position, new Vector3(entityPhysics.worldCollider.bounds.extents.x, .01f, .1f), gyro.forward, gyro.rotation, Mathf.Max(leftDistance, centerDistance, rightDistance));
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

    public void TakeObject(GameObject o){
        //Log("TakeObject()");
        entityItems.OnObjectInteract(o, o.GetComponent<ObjectReference>().GetObjectReference());
    }


    public List<GameObject> SenseSurroundingItems(Enum type, string name, float distance){
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, LayerMask.GetMask("Item"));
       
        //string sur = "";
        List<GameObject> foundObjects = new List<GameObject>();
        GameObject o;
        Item i;
        foreach(Collider col in colliders){
            o = col.gameObject;
            i = Item.GetItemByName(o.name);
            if(type == null || i.type == type){
                if(name == null || o.name == name){
                    if(!Faction.ItemIsTargetedByFaction(o, entityInfo.faction)){  
                        foundObjects.Add(o);
                        //sur += o.name + ", ";
                    }
                }
            }
        }
        //Debug.Log("Surroundings: " + sur);


        return foundObjects;
        
    }


    public List<EntityHandle> SenseSurroundingFeatures(Species species){
        
        // todo: sense surrounding features
        return new List<EntityHandle>();
    }


    public List<EntityHandle> SenseSurroundingCreatures(Species targetSpecies, float distance){

        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, LayerMask.GetMask("Creature"));
        //Debug.Log("sense distance: " + distance + "... creatures found: " + colliders.Length);

        List<EntityHandle> foundHandles = new List<EntityHandle>();
        GameObject o;
        EntityHandle foundHandle;
        foreach(Collider col in colliders){
            o = col.gameObject;
            foundHandle = o.GetComponentInParent<EntityHandle>();
            if(foundHandle != null){
                if(!foundHandle.entityInfo.species.Equals(entityInfo.species) && (targetSpecies.Equals(Species.Any) || targetSpecies.Equals(foundHandle.entityInfo.species))){
                    foundHandles.Add(foundHandle);
                }
            }
        }
        
        return foundHandles;
    }

    // check surroundings for creatures and act accordingly based on behavior type
    public void CheckForCreaturesUpdate(){
        BehaviorType behaviorType = behaviorProfile.behaviorType;
        if(behaviorType.Equals(BehaviorType.Steadfast)){ return; }
        List<EntityHandle> sensedCreatureHandles = SenseSurroundingCreatures(Species.Any, 30f);
        //Debug.Log(sensedCreatureHandles.Count);
        if(sensedCreatureHandles.Count == 0){ return; }
        sensedCreatureHandles = sensedCreatureHandles.OrderBy(handle => Vector3.Distance(transform.position, handle.transform.position)).ToList();

        float distanceAway;
        BehaviorType behaviorTypeOther;
        if (behaviorProfile.behaviorType.Equals(BehaviorType.Timid))
        {
            foreach (EntityHandle handleOther in sensedCreatureHandles)
            {
                behaviorTypeOther = handleOther.entityInfo.speciesInfo.behaviorProfile.behaviorType;
                if (behaviorTypeOther.Equals(BehaviorType.Aggressive))
                {
                    distanceAway = Vector3.Distance(transform.position, handleOther.transform.position);
                    if(distanceAway < 15f){
                        InsertActionImmediate(ActionParameters.GenerateActionParameters(ActionType.RunFrom, handleOther.gameObject, -1, null, null, GetFleeTime(), distanceThreshhold_pursuit, EntityOrientation.BodyRotationMode.Normal, true), true);
                    }
                    else{
                        SetHeadLookAt(handleOther.transform.position, baseTimeStep_creatureSense);
                    }
                }
            }
        }
        else if (behaviorProfile.behaviorType.Equals(BehaviorType.Aggressive)){
            InsertActionImmediate(ActionParameters.GenerateActionParameters(ActionType.Chase, sensedCreatureHandles[0].gameObject, -1, null, null, GetChaseTime(), distanceThreshhold_lungeAttack, EntityOrientation.BodyRotationMode.Target, true), true);
        }

    }


    // returns true if the entity isn't "busy" with something
    public bool NotBusy(){
        List<ActionParameters> aList = new List<ActionParameters>(actions);
        aList.Add(activeAction);
        if (aList.Any())
        {
            foreach (ActionParameters a in aList.Where(a => a != null).ToList())
            {
                //Debug.Log(a.type.ToString());
                if (!a.type.Equals(ActionType.Idle) && !a.type.Equals(ActionType.GoTo))
                {
                    //Debug.Log("   => creature is busy");
                    return false;
                }
            }
            return true;
        }
        return true;
    }

    // turn head towards position for x time
    public void SetHeadLookAt(Vector3 lookAtPos, float time){

        float lookAtForce = 2f;

        if(time == -1f)
        {
            entityPhysics.head.rotation = Quaternion.Slerp(entityPhysics.head.rotation, Quaternion.LookRotation((lookAtPos - Vector3.up * 30f) - entityPhysics.head.position, Vector3.up), lookAtForce * Time.deltaTime);
        }
        else
        {
            StartCoroutine(_SetHeadLookAt());
        }

        IEnumerator _SetHeadLookAt()
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            float maxTime = time;
            while(timer.ElapsedMilliseconds / 1000f < time){
                entityPhysics.head.rotation = Quaternion.Slerp(entityPhysics.head.rotation, Quaternion.LookRotation((lookAtPos - Vector3.up * 30f) - entityPhysics.head.position, Vector3.up), lookAtForce * Time.deltaTime);
                yield return null;
            }
        }
    }

    public bool IsAtPosition(Vector3 position, float distanceThreshhold){
        return Vector3.Distance(transform.position, position) < distanceThreshhold;
    }

    public float GetChaseTime(){
        return baseTime_chase * entityStats.statsCombined.stamina;
    }

    public float GetFleeTime(){
        return baseTime_flee * entityStats.statsCombined.stamina;
    }

    bool WithinActiveDistance(){
        return Vector3.Distance(GameManager.current.localPlayer.transform.position, transform.position) <= distanceThreshhold_active;
    }



    void HandleCreatureSense()
    {
        timeSince_creatureSense += Time.deltaTime;
        if (timeSince_creatureSense >= baseTimeStep_creatureSense)
        {
            if (WithinActiveDistance() && NotBusy())
            {
                //Debug.Log("Boutta sense creatures");
                CheckForCreaturesUpdate();
            }
            timeSince_creatureSense = timeSince_creatureSense - baseTimeStep_creatureSense;
        }
        timeSince_attack += Time.deltaTime;
    }   


    // Update is called once per frame
    void Update()
    {

        entityPhysics.moveDir = move;

        if (!isPlayer)
        {
            HandleCreatureSense();
        }
        

        


        
    }




}