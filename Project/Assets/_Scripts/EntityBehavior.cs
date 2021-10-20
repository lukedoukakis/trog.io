using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum ActionPriority{ Back, Front, FrontImmediate }
public enum AttackType{ Weapon, Bite, Swipe, HeadButt, Stomp }

public class EntityBehavior : EntityComponent
{

    public BehaviorProfile behaviorProfile;
    public Transform homeT;
    public bool isPlayer;
    public bool isAtHome;
    public Vector3 move;
    public bool urgent;


    public static float RANDOM_OFFSET_RANGE = 1f;
    public static float DISTANCE_THRESHOLD_NONE = -1f;
    public static float DISTANCE_THRESHOLD_SAME_POINT = .3f;
    public static float DISTANCE_THRESHOLD_SAME_SPOT = 2f;
    public static float DISTANCE_THRESHOLD_MELEE_ATTACK = 1f;
    public static float DISTANCE_THRESHOLD_LUNGEATTACK = 10f;
    public static float DISTANCE_THRESHOLD_COMBAT = 15f;
    public static float DISTANCE_THRESHOLD_CHASE = 100f;
    public static readonly float DISTANCE_THRESHOLD_FLEE = 15f;
    public static readonly float DISTANCE_THRESHOLD_STANDGROUND = 5f;
    public static float DISTANCE_THRESHOLD_ACTIVE = 100f;
    

    Vector3 randomOffset;


    // sensing and movement parameters
    public float timeSince_creatureSense;
    public float timeSince_attack;
    public static readonly float BASE_TIMESTEP_CREATURESENSE = 1f;
    public static readonly float BASE_TIMESTEP_ATTACK = 1f;
    public static readonly float SENSE_DISTANCE_OBSTACLE = 3f;
    public static readonly float SENSE_DISTANCE_IMMEDIATE = 1f;
    public static readonly float SENSE_DISTANCE_SEARCH = 150f;
    public static readonly float SENSE_DISTANCE_EARSHOT = 500f;
    public static readonly float SENSE_DISTANCE_INFINITE = float.MaxValue;
    public static readonly float MAX_JUMPFROM_DISTANCE = 3f;
    public static readonly float DIRECT_ADJUST_ROTATION_SPEED = 1f;
    public static readonly float BASE_TIME_CHASE = 10f;
    public static readonly float BASE_TIME_FLEE = 10f;



    public ActionParameters activeAction;
    public List<ActionParameters> actionQueue;


    public Dictionary<string, ActionParameters> actionLayers;
    public Dictionary<string, IEnumerator> coroutineLayers;


    protected override void Awake(){

        base.Awake();

        isPlayer = tag == "Player";
        homeT = new GameObject().transform;
        actionQueue = new List<ActionParameters>();
        randomOffset = new Vector3(UnityEngine.Random.Range(RANDOM_OFFSET_RANGE*-1f, RANDOM_OFFSET_RANGE), 0f, UnityEngine.Random.Range(RANDOM_OFFSET_RANGE*-1f, 0));
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

    void Start()
    {
        timeSince_creatureSense = 0f;
        behaviorProfile = entityInfo.speciesInfo.behaviorProfile;

        if(!isPlayer)
        {
            NextAction();
        }
    }


    // add an action to the end of the queue
    public void AddAction(ActionParameters a){
        actionQueue.Add(a);
    }

    // insert an action to the front of the queue, to be executed when the current action is finished
    public void InsertAction(ActionParameters a){
        actionQueue.Insert(0, a);
    }

    // insert an action to the front of the queue and immediately execute
    public void InsertActionImmediate(ActionParameters a, bool clear){
        TerminateActionLayer("Command");
        if(clear){
            if(actionQueue.Count > 0){
                actionQueue.Clear();
            }
        }
        InsertAction(a);
        OnActionInterrupt();
        //Debug.Log("going to call nextaction... actions count: " + actions.Count);
        NextAction();
        //Debug.Log("InsertActionImmediate() done");
    }

    public void ClearActionQueue(){
        actionQueue.Clear();
    }



    // select and execute the next action in the queue... if list is empty, insert "go home" or "idle" action
    public ActionParameters NextAction()
    {
    
        timeSince_creatureSense = BASE_TIMESTEP_CREATURESENSE;

        // if no specified action, come up with a default
        if(actionQueue.Count == 0)
        {
            //Debug.Log("ACTION QUEUE EMPTY");
            // if is a faction follower, go home or follower leader depending on where they are
            if(entityInfo.isFactionFollower)
            {
                EntityHandle factionLeader = entityInfo.faction.leaderHandle;
                if(factionLeader == null)
                {
                    Debug.Log("faction leader null");
                }
                else if(factionLeader.entityPhysics.isInsideCamp)
                {
                    //Debug.Log("Going home");
                    ActionParameters goHome = ActionParameters.GenerateActionParameters("Go Home", entityHandle);
                    InsertAction(goHome);
                }
                else
                {
                    //Debug.Log("Following leader");
                    ActionParameters followFactionLeader = ActionParameters.GenerateActionParameters("Follow Faction Leader", entityHandle);
                    InsertAction(followFactionLeader);
                }
            }
            // if not a faction follower, wander around
            else
            {
                ActionParameters goTo = ActionParameters.GenerateActionParameters("Go To Random Nearby Spot", entityHandle);
                ActionParameters idle = ActionParameters.GenerateActionParameters("Idle For 5 Seconds", entityHandle);
                InsertAction(goTo);
                InsertAction(idle);
            }

        }
        activeAction = actionQueue[0];
        //Debug.Log("Active Action: " + activeAction.type);
        actionQueue.RemoveAt(0);
        ExecuteAction(activeAction);
        //Log("Action type: " + activeAction.ToString());
        return activeAction;
    }
    public void OnActionInterrupt(){

    }

    public void ExecuteAction(ActionParameters a){
        Transform t = null;
        if(a.targetedWorldObject == null){ t = null; }else{ t = a.targetedWorldObject.transform; }
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

            //Debug.Log("GOTO");

            if (a.targetedWorldObject != null)
            {
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                float maxTime = a.maxTime;
                if (maxTime == -1f) { maxTime = float.MaxValue; }

                Transform targetT = a.targetedWorldObject.transform;
                while (targetT != null)
                {
                    if (IsAtPosition(targetT.position, a.distanceThreshold))
                    {
                        timer.Stop();
                        //NextAction();
                        break;
                    }
                    else if ((timer.ElapsedMilliseconds / 1000f) > maxTime)
                    {
                        timer.Stop();
                        ClearActionQueue();
                        //NextAction();
                        break;
                    }
                    else
                    {
                        //Debug.Log(timer.ElapsedMilliseconds / 1000f);
                        move = GetNavigationDirection(targetT, false);
                        entityPhysics.moveDir = move;
                    }
                    //SetHeadLookAt(a.obj.transform.position, -1f);
                    yield return null;
                }
            }
            NextAction();
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

        //Debug.Log("FOLLOW");

        if (a.targetedWorldObject == null)
        {
            NextAction();
        }

        Transform targetT;
        if(reverse){
            targetT = a.targetedWorldObject.transform;
        }
        else{
            targetT = a.targetedWorldObject.transform;
        }

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        float maxTime = a.maxTime == -1f ? float.MaxValue : a.maxTime;

        // repeats until action layer is canceled
        bool followCondition;
        while (targetT != null)
        {

            followCondition = reverse ? Vector3.Distance(transform.position, targetT.position) <= a.distanceThreshold : (timer.ElapsedMilliseconds / 1000f <= maxTime);
            if (followCondition)
            {
                move = GetNavigationDirection(targetT, reverse);
                entityPhysics.moveDir = move;
            }
            else
            {
                move = Vector3.zero;

                if(reverse){
                    timer.Stop();
                    NextAction();
                }
    
            }

            yield return null;

        }
        NextAction();
    }

    public void Collect(ActionParameters a){

        Item i_target = a.item_target;
        //Log("target name: " + i_target.nme);

        List<GameObject> foundObjects = SenseSurroundingItems(i_target.type, i_target.nme, SENSE_DISTANCE_INFINITE);
        foundObjects = foundObjects.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).ToList();
        if(foundObjects.Count == 0){
            // TODO: search in new area if nothing found
            //Log("Collect: nothing found");
        }
        else{
            //Log("Collect: picking up object");
            GameObject target = foundObjects[0];
            entityInfo.faction.AddItemTargeted(target);
            ActionParameters goToObject = ActionParameters.GenerateActionParameters(entityHandle, ActionType.GoTo, target, -1, Item.GetItemByName(target.name), null, -1, DISTANCE_THRESHOLD_SAME_SPOT, BodyRotationMode.Normal, false);
            ActionParameters pickupObject = ActionParameters.GenerateActionParameters(entityHandle, ActionType.Pickup, target, -1, Item.GetItemByName(target.name), null, -1, -1f, BodyRotationMode.Normal, false);
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
            GameObject o = a.targetedWorldObject;
            if(i.type.Equals(ItemType.Weapon)){
                yield return new WaitForSecondsRealtime(.25f);
                TakeObject(o);
                yield return new WaitForSecondsRealtime(.25f);
            }
            else{
                Faction.OnItemPickup(i, o, entityInfo.faction);
            }
            entityInfo.faction.RemoveItemTargeted(o);
            
            NextAction();
        }

    }

    public void Chase(ActionParameters a)
    {
    
        //Debug.Log("CHASE");

        ActionParameters goToTarget = ActionParameters.GenerateActionParameters(entityHandle, ActionType.GoTo, a.targetedWorldObject, -1, null, null, a.maxTime, DISTANCE_THRESHOLD_MELEE_ATTACK, BodyRotationMode.Target, true);
        ActionParameters attackTarget = ActionParameters.GenerateActionParameters(entityHandle, ActionType.Attack, a.targetedWorldObject, -1, null, null, -1, -1, BodyRotationMode.Target, true);
        InsertAction(attackTarget);
        InsertAction(goToTarget);
        NextAction();
    }

    void Attack(ActionParameters a){

        // if attack rate allows and target isn't null, attack
        if (a.targetedWorldObject != null)
        {
            TerminateActionLayer("Hands");
            BeginActionLayer("Hands", a, _Attack());     
        }
        else
        {
            ClearActionQueue();
            NextAction();
        }
        

        IEnumerator _Attack()
        {

            //Debug.Log("ATTACK");

            // choose attack type
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

            move = Vector3.zero;
            yield return new WaitForSecondsRealtime(CalculateTimeUntilCanAttack());

            // execute attack
            entityPhysics.Attack(attackType, a.targetedWorldObject.transform);
            timeSince_attack = 0f;
            ActionParameters attackRecover = ActionParameters.GenerateActionParameters(entityHandle, ActionType.AttackRecover, a.targetedWorldObject, -1, null, null, -1, -1, BodyRotationMode.Target, true);
            if(a.targetedWorldObject != null){
                InsertAction(attackRecover);
            }
            NextAction();
            yield return null;
        }
    }

    void AttackRecover(ActionParameters a){

        TerminateActionLayer("Command");
        BeginActionLayer("Command", a, _AttackRecover());

        IEnumerator _AttackRecover()
        {
            //Debug.Log("ATTACK RECOVER");

            if(a.targetedWorldObject != null)
            {

                // if target is alive (hasn't been deleted), queue repeat attack
                ActionParameters chase = ActionParameters.GenerateActionParameters(entityHandle, ActionType.Chase, a.targetedWorldObject, -1, null, null, CalculateChaseTime(), DISTANCE_THRESHOLD_MELEE_ATTACK, BodyRotationMode.Target, true);
                InsertAction(chase);
                foreach(Enum actionType in behaviorProfile.attackRecoverySequence){
                    ActionParameters newAp = ActionParameters.GenerateActionParameters();
                    newAp.type = actionType;
                    newAp.targetedWorldObject = a.targetedWorldObject;
                    InsertAction(newAp);
                }

                // if has a weapon and the charging has not begun, call physics attack to begin charge
                // if(entityItems != null)
                // {
                //     if(entityItems.weaponEquipped_item != null && !entityPhysics.weaponCharging)
                //     {
                //         entityPhysics.Attack(AttackType.Weapon, a.targetedWorldObject.transform);
                //     }
                // }

                NextAction();
                yield return null;

            }
            else{
                Debug.Log("target is null");
                ClearActionQueue();
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
				if(Mathf.Min(Mathf.Min(leftDistance, centerDistance), rightDistance) < MAX_JUMPFROM_DISTANCE){
					if(entityPhysics.CanJump()){
						entityPhysics.Jump();
                        jumped = true;
					}
				}
			}
		}

        if(!jumped)
        {
            if (entityPhysics.isHandGrab)
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

            bool leftCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward + gyro.right*-2f, out leftHitInfo, SENSE_DISTANCE_OBSTACLE, LayerMaskController.WALKABLE);
            bool centerCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward, out centerHitInfo, SENSE_DISTANCE_OBSTACLE, LayerMaskController.WALKABLE);
            bool rightCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward + gyro.right*2f, out rightHitInfo, SENSE_DISTANCE_OBSTACLE, LayerMaskController.WALKABLE);


            // Debug.DrawRay(transform.position + new Vector3(0, .1f, 0), (gyro.forward + gyro.right*-2f).normalized*senseDistance_obstacle, Color.green, Time.deltaTime);
            // Debug.DrawRay(transform.position + new Vector3(0, .1f, 0), (gyro.forward).normalized*senseDistance_obstacle, Color.green, Time.deltaTime);
            // Debug.DrawRay(transform.position + new Vector3(0, .1f, 0), (gyro.forward + gyro.right*2f).normalized*senseDistance_obstacle, Color.green, Time.deltaTime);
            
            List<RaycastHit> hitInfos = new List<RaycastHit>();

            // set leftDistance, centerDistance, rightDistance
            if (leftCast)
            {
                if(IsTargetedObject(leftHitInfo.collider.gameObject))
                {
                    leftDistance = float.MaxValue;
                }
                else
                {
                    leftDistance = (leftHitInfo.point - transform.position).magnitude;
                    hitInfos.Add(leftHitInfo);
                }
            }
            else
            {
                leftDistance = float.MaxValue;
            }
            if (centerCast)
            {
                if(IsTargetedObject(centerHitInfo.collider.gameObject))
                {
                    centerDistance = float.MaxValue;
                }
                else
                {
                    centerDistance = (centerHitInfo.point - transform.position).magnitude;
                    hitInfos.Add(centerHitInfo);
                }
            }
            else
            {
                centerDistance = float.MaxValue;
            }
            if (rightCast)
            {
                if(IsTargetedObject(rightHitInfo.collider.gameObject))
                {
                    rightDistance = float.MaxValue;
                }
                else
                {
                    rightDistance = (rightHitInfo.point - transform.position).magnitude;
                    hitInfos.Add(rightHitInfo);
                }
                
            }
            else
            {
                rightDistance = float.MaxValue;
            }

            // return true if any of the raycasts hit something besides a tribe member
            int hits = 0;
            foreach(RaycastHit hitInfo in hitInfos){
                string tag = hitInfo.collider.gameObject.tag;
                if(hitInfo.normal.y < .5f){
                    ++hits;
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
        entityItems.OnObjectTake(o, o.GetComponent<ObjectReference>().GetObjectReference());
    }


    public List<GameObject> SenseSurroundingItems(Enum type, string name, float distance){
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, LayerMaskController.ITEM);
       
        //string sur = "";
        List<GameObject> foundObjects = new List<GameObject>();
        GameObject o;
        Item i;
        foreach(Collider col in colliders){
            o = col.gameObject;
            i = Item.GetItemByName(o.name);
            if(type == null || i.type == type){
                if(name == null || o.name == name){
                    if(!entityInfo.faction.ItemIsTargetedByThisFaction(o)){  
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

        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, LayerMaskController.CREATURE);
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
    public void ReactToNearbyCreatures(BehaviorType behaviorType, out bool reacting){

        reacting = false;
        List<EntityHandle> sensedCreatureHandles = SenseSurroundingCreatures(Species.Any, 30f);
        //Debug.Log(sensedCreatureHandles.Count);
        if(sensedCreatureHandles.Count == 0){
            return;
        }
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
                    if(distanceAway < DISTANCE_THRESHOLD_FLEE){
                        reacting = true;
                        InsertActionImmediate(ActionParameters.GenerateActionParameters(entityHandle, ActionType.RunFrom, handleOther.gameObject, -1, null, null, CalculateFleeTime(), DISTANCE_THRESHOLD_CHASE, BodyRotationMode.Normal, true), true);
                    }
                }
            }
        }
        if (behaviorProfile.behaviorType.Equals(BehaviorType.Steadfast))
        {
            foreach (EntityHandle handleOther in sensedCreatureHandles)
            {
                behaviorTypeOther = handleOther.entityInfo.speciesInfo.behaviorProfile.behaviorType;
                if (behaviorTypeOther.Equals(BehaviorType.Aggressive))
                {
                    distanceAway = Vector3.Distance(transform.position, handleOther.transform.position);
                    if(distanceAway < DISTANCE_THRESHOLD_STANDGROUND){
                        reacting = true;
                        InsertActionImmediate(ActionParameters.GenerateActionParameters(entityHandle, ActionType.Chase, sensedCreatureHandles[0].gameObject, -1, null, null, CalculateChaseTime(), DISTANCE_THRESHOLD_LUNGEATTACK, BodyRotationMode.Target, true), true);
                    }
                }
            }
        }
        else if (behaviorProfile.behaviorType.Equals(BehaviorType.Aggressive)){
            reacting = true;
            InsertActionImmediate(ActionParameters.GenerateActionParameters(entityHandle, ActionType.Chase, sensedCreatureHandles[0].gameObject, -1, null, null, CalculateChaseTime(), DISTANCE_THRESHOLD_LUNGEATTACK, BodyRotationMode.Target, true), true);
        }

    }


    // returns true if the entity isn't "busy" with something
    public bool NotBusy(){
        List<ActionParameters> aList = new List<ActionParameters>(actionQueue);
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

    public bool IsAtPosition(Vector3 position, float distanceThreshhold){
        return Vector3.Distance(transform.position, position) < distanceThreshhold;
    }

    public float CalculateChaseTime(){
        return BASE_TIME_CHASE * entityStats.statsCombined.stamina;
    }

    public float CalculateFleeTime(){
        return BASE_TIME_FLEE * entityStats.statsCombined.stamina;
    }

    public float CalculateAttackCooldownTime()
    {
        return BASE_TIMESTEP_ATTACK * entityStats.statsCombined.attackSpeed;
    }

    public float CalculateTimeUntilCanAttack()
    {
        return CalculateAttackCooldownTime() - timeSince_attack;
    }

    bool WithinActiveDistance(){
        return Vector3.Distance(GameManager.current.localPlayer.transform.position, transform.position) <= DISTANCE_THRESHOLD_ACTIVE;
    }

    public bool IsTargetedObject(GameObject otherObject)
    {
        if(activeAction == null)
        {
            return true;
        }
        else
        {
            return ReferenceEquals(otherObject, activeAction.targetedWorldObject);
        }
    }

    public GameObject GetTargetedObject()
    {
        return activeAction == null ? null : activeAction.targetedWorldObject;
    }



    // --

    void UpdateBehavior()
    {
        if(entityInfo.isFactionFollower)
        {
            // follow leader behavior
            HandleFollowerBehavior();
        }
        else
        {
            // wild behavior
            HandleIndependentBehavior();
        }
    }


    void HandleIndependentBehavior()
    {
        bool reactingToSensedCreature;
        HandleCreatureSense(out reactingToSensedCreature);
    }

    void HandleFollowerBehavior()
    {
        // By default, follow the leader. When the player hits something, attack that thing. Sense for creatures with steadfast behavior.

        // order of priority:
        //     1. react to nearby creatures with steadfast behavior
        //     2. react to leader's activity
        //     3. follow leader (Follow if outside camp, go home if inside camp)

        bool reactingToSensedCreature;
        HandleCreatureSense(out reactingToSensedCreature);

        if(!reactingToSensedCreature)
        {

        }
    }


    void HandleCreatureSense(out bool sensed)
    {
        timeSince_creatureSense += Time.deltaTime;

        sensed = false;
        if (timeSince_creatureSense >= BASE_TIMESTEP_CREATURESENSE)
        {
            if (WithinActiveDistance() && NotBusy())
            {
                //Debug.Log("Boutta sense creatures");
                sensed = true;
                ReactToNearbyCreatures(behaviorProfile.behaviorType, out sensed);
            }

            timeSince_creatureSense = timeSince_creatureSense - BASE_TIMESTEP_CREATURESENSE;
        }

    }



    // Update is called once per frame
    void Update()
    {

        entityPhysics.moveDir = move;

        if (!isPlayer)
        {
            UpdateBehavior();
        }

        timeSince_attack += Time.deltaTime;
        

        


        
    }




}