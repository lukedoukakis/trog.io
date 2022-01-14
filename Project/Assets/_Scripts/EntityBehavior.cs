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
    public Transform followPositionTransform;
    public Tent restingTent;
    public bool isPlayer;
    public bool isAtHome;
    public Vector3 move;
    public bool urgent;
    public Vector3 followOffset;


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

    public static float DISTANCE_STEP_BACK = 3f;
    public static float DISTANCE_STEP_SIDE = 1f;

    public static float STAMINA_LOSS_RATE = LightingController.SECONDS_PER_DAY * (1f / 3f);
    public static float STAMINA_GAIN_RATE = LightingController.SECONDS_PER_DAY * (3f);
    public static float HEALTH_GAIN_RATE = STAMINA_GAIN_RATE;
    

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
    public static readonly float HOME_RESET_TIME_MIN = 5f;
    public static readonly float HOME_RESET_TIME_MAX = 10f;



    public ActionParameters activeAction;
    public List<ActionParameters> actionQueue;


    public Dictionary<string, ActionParameters> actionLayers;
    public Dictionary<string, IEnumerator> coroutineLayers;


    // pre-established action sequences
    public ActionSequence entityActionSequence_AssertStanding, entityActionSequence_AssertSquatting;

    public System.Diagnostics.Stopwatch homeResetTimer;
    public float homeResetTime;


    protected override void Awake()
    {

        this.fieldName = "entityBehavior";

        base.Awake();

        isPlayer = tag == "Player";
        followPositionTransform = new GameObject().transform;
        SetRestingTent(null);
        followOffset = Utility.GetHorizontalVector(Utility.GetRandomVectorHorizontal(2.5f));
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

        entityActionSequence_AssertStanding = ActionSequence.CreateActionSequence(entityPhysics.AssertStanding);
        entityActionSequence_AssertSquatting = ActionSequence.CreateActionSequence(entityPhysics.AssertSquatting);

        homeResetTimer = new System.Diagnostics.Stopwatch();
        ResetHomeResetTimer();

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
    public void InsertActionAndExecuteAsap(ActionParameters apToInsert, bool clear)
    {
        TerminateActionLayer("Command");
        if(clear){
            if(actionQueue.Count > 0){
                ClearActionQueue();
            }
        }
        InsertAction(apToInsert);
        bool interrupt = DecideInterrupt(apToInsert.interruptionTier);
        if(interrupt)
        {
            OnActionInterrupt();
            //Debug.Log("going to call nextaction... actions count: " + actions.Count);
            NextAction();
            //Debug.Log("InsertActionImmediate() done");
        }
    }

    public bool DecideInterrupt(InterruptionTier otherEventInterruptionTier)
    {
        if(activeAction == null)
        {
            return true;
        }

        if((int)otherEventInterruptionTier >= (int)activeAction.interruptionTier)
        {
            return true;
        }
        return true;
    }

    public void ClearActionQueue(){
        actionQueue.Clear();
    }


    void OnEmptyActionQueue()
    {
        //Debug.Log("ACTION QUEUE EMPTY");

        // if is a faction follower, go home or follower leader depending on where they are
        if (entityInfo.isFactionFollower)
        {
            //Debug.Log("ACTION QUEUE EMPTY");
            AssertWeaponChargedStatus(false);
            EntityHandle factionLeader = entityInfo.faction.leaderHandle;

            // if leader is in camp, take action in priority:
                // 1) rest if needed
                // 2) find weapon
                // 2) follow home position
            ActionParameters actionToTake;
            if(entityInfo.faction.leaderInCamp)
            {
                bool needsSleep = entityStats.stamina < entityStats.maxStamina;
                if(needsSleep)
                {
                    // todo: sleep action
                    actionToTake = ActionParameters.GenerateActionParameters("Go Rest", entityHandle);
                    if(actionToTake.targetedWorldObject == null)
                    {
                        actionToTake = ActionParameters.GenerateActionParameters("Go Home", entityHandle);
                    }
                }
                else
                {
                    if((!entityItems.HasWeapon()) && (entityInfo.faction.leaderInCamp) && (entityInfo.faction.GetItemCount(ItemType.Weapon) > 0))
                    {
                        actionToTake = ActionParameters.GenerateActionParameters("Find Weapon", entityHandle);
                    }
                    else
                    {
                        actionToTake = ActionParameters.GenerateActionParameters("Go Home", entityHandle);
                    }
                    
                }
                InsertAction(actionToTake);
            }
            else
            {
                ActionParameters followLeader = ActionParameters.GenerateActionParameters("Follow Faction Leader", entityHandle);
                InsertAction(followLeader);
            }
            
        }
        // if not a faction follower
        else
        {
            // if has a camp, chill in the camp
            if(entityInfo.faction.camp != null && false)
            {
                ActionParameters goHome = ActionParameters.GenerateActionParameters("Go Home", entityHandle);
                InsertAction(goHome);
            }
            else
            {
                ActionParameters goTo = ActionParameters.GenerateActionParameters("Go To Random Nearby Spot", entityHandle);
                ActionParameters idle = ActionParameters.GenerateActionParameters("Idle For 5 Seconds", entityHandle);
                InsertAction(goTo);
                InsertAction(idle);
            }       
        }
    }

    // select and execute the next action in the queue
    public ActionParameters NextAction()
    {
    
        timeSince_creatureSense = BASE_TIMESTEP_CREATURESENSE;

        // if no specified action, come up with a default
        if(actionQueue.Count == 0)
        {
            OnEmptyActionQueue();
        }
        activeAction = actionQueue[0];
        actionQueue.RemoveAt(0);
        ExecuteAction(activeAction);
        return activeAction;
    }
    public void OnActionInterrupt(){

    }

    public void ExecuteAction(ActionParameters ap)
    {

        Transform t = null;
        if(ap.targetedWorldObject == null){ t = null; }else{ t = ap.targetedWorldObject.transform; }
        entityOrientation.SetBodyRotationMode(ap.bodyRotationMode, t);
        if(ap.actionSequenceBeforeBeginning != null)
        {
            ap.actionSequenceBeforeBeginning.Execute();
        }

        //Debug.Log("EXECUTING ACTION: " + a.type.ToString());

        switch(ap.type){
            case ActionType.Idle :
                Idle(ap);
                break;
            case ActionType.GoTo :
                GoTo(ap);
                break;
            case ActionType.Follow :
                Follow(ap);
                break;
            case ActionType.RunFrom :
                RunFrom(ap);
                break;
            case ActionType.CollectItem :
                CollectItem(ap);
                break;
            case ActionType.CollectItemSameType :
                CollectItemSameType(ap);
                break;
            case ActionType.Pickup :
                Pickup(ap);
                break;
            case ActionType.Chase :
                Chase(ap);
                break;
            case ActionType.Attack :
                Attack(ap);
                break;
            case ActionType.AttackRecover :
                AttackRecover(ap);
                break;
            case ActionType.Build :
                Build(ap);
                break;
            case ActionType.Hunt :
                Hunt(ap);
                break;
            case ActionType.StepBack :
                StepBack(ap);
                break;
            case ActionType.StepSide :
                StepSide(ap);
                break;
            default:
                Debug.Log("ObjectBehavior: called action not a defined action (" + ap.type + ")... idling.");
                break;
        }

        urgent = ap.urgent;
    }

    public void StopActions()
    {
        foreach(string layerName in coroutineLayers.Keys.ToArray())
        {
            TerminateActionLayer(layerName);
        }

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

    public void GoTo(ActionParameters ap){

        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", ap, _GoTo());
        //Debug.Log("STARTING GOTO");

        IEnumerator _GoTo()
        {

            //Debug.Log("GOTO");

            if (ap.targetedWorldObject != null)
            {
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                float maxTime = ap.maxTime;
                if (maxTime == -1f) { maxTime = float.MaxValue; }

                Transform targetT = ap.targetedWorldObject.transform;
                Vector3 targetPos;
                while (targetT != null)
                {
                    targetPos = targetT.position + transform.TransformDirection(ap.offset);
                    if (IsAtPosition(targetPos, ap.distanceThreshold))
                    {
                        timer.Stop();
                        break;
                    }
                    else if ((timer.ElapsedMilliseconds / 1000f) > maxTime)
                    {
                        timer.Stop();
                        //ClearActionQueue();
                        break;
                    }
                    else
                    {
                        //Debug.Log(timer.ElapsedMilliseconds / 1000f);
                        move = GetNavigationDirection(targetPos, false);
                        entityPhysics.moveDir = move;
                    }
                    yield return null;
                }
            }
            NextAction();
        }
    }

    public void Follow(ActionParameters ap){

        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", ap, _Follow(ap, false));
    }


    public void RunFrom(ActionParameters ap){
        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", ap, _Follow(ap, true));
    }
    IEnumerator _Follow(ActionParameters ap, bool reverse)
    {

        //Debug.Log("FOLLOW");

        if (ap.targetedWorldObject == null)
        {
            NextAction();
        }

        Transform targetT = ap.targetedWorldObject.transform;
        
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        float maxTime = ap.maxTime == -1f ? float.MaxValue : ap.maxTime;

        // repeats until action layer is cancelled
        bool followCondition;
        Vector3 targetPos;
        while (targetT != null)
        {
            //Debug.Log(ap.offset);


            if(ap.endCondition != null)
            {
                bool end = ap.endCondition();
                if(end)
                {
                    //Debug.Log("END CONDITION TRUE");
                    timer.Stop();
                    NextAction();
                }
            }



            targetPos = targetT.position + transform.TransformDirection(ap.offset);

            followCondition = reverse ? Vector3.Distance(transform.position, targetPos) <= ap.distanceThreshold : ((timer.ElapsedMilliseconds / 1000f <= maxTime) && Vector3.Distance(transform.position, targetPos) > ap.distanceThreshold);

            if(reverse)
            {
                if(Vector3.Distance(transform.position, targetPos) >= ap.distanceThreshold)
                {
                    timer.Stop();
                    NextAction();
                }
                else
                {
                    move = GetNavigationDirection(targetPos, reverse);
                    entityPhysics.moveDir = move;
                }
            }
            else
            {
                // if max time reached, next action
                if(timer.ElapsedMilliseconds / 1000f > maxTime)
                {
                    timer.Stop();
                    NextAction();
                }

                // if distance within a certain threshhold of the target, don't move, call target's action if it exists, and apply actionWhenAchieved
                if(Vector3.Distance(transform.position, targetPos) <= ap.distanceThreshold)
                {
                    move = Vector3.zero;
                    TargetPositionComponent tpc = ap.targetedWorldObject.GetComponentInParent<TargetPositionComponent>();
                    if(tpc != null)
                    {
                        tpc.OnTargetPositionReached(entityHandle);
                    }
                    if(ap.actionSequenceWhenAchieved != null)
                    {
                        ap.actionSequenceWhenAchieved.Execute();
                    }
                }
                // else, move towards the target
                else
                {
                    move = GetNavigationDirection(targetPos, reverse);
                    entityPhysics.moveDir = move;
                }
            }

            yield return null;

        }
        NextAction();
    }

    public void Chase(ActionParameters a)
    {

        EquipOptimalWeaponForTarget(a);
    
        //Debug.Log("CHASE");
        AssertWeaponChargedStatus(true);
        ActionParameters goToTarget = ActionParameters.GenerateActionParameters(entityHandle, ActionType.GoTo, a.targetedWorldObject, Vector3.zero, -1, null, null, a.maxTime, DISTANCE_THRESHOLD_MELEE_ATTACK, BodyRotationMode.Target, InterruptionTier.BeenHit, true, null, entityActionSequence_AssertStanding, null);
        ActionParameters attackTarget = ActionParameters.GenerateActionParameters(entityHandle, ActionType.Attack, a.targetedWorldObject, Vector3.zero, -1, null, null, -1, -1, BodyRotationMode.Target, InterruptionTier.Nothing, true, null, entityActionSequence_AssertStanding, null);
        InsertAction(attackTarget);
        InsertAction(goToTarget);
        NextAction();
    }

    void Attack(ActionParameters ap){

        // if attack rate allows and target isn't null, attack
        if (ap.targetedWorldObject != null)
        {
            TerminateActionLayer("Hands");
            BeginActionLayer("Hands", ap, _Attack());
        }
        else
        {
            ClearActionQueue();
            NextAction();
        }
        

        IEnumerator _Attack()
        {

            //Debug.Log("ATTACK");

            AssertWeaponChargedStatus(true);

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

            if (ap.targetedWorldObject != null)
            {
                // execute attack
                entityPhysics.Attack(attackType, ap.targetedWorldObject.transform);
                timeSince_attack = 0f;
                ActionParameters attackRecover = ActionParameters.GenerateActionParameters(entityHandle, ActionType.AttackRecover, ap.targetedWorldObject, Vector3.zero, -1, null, null, -1, -1, BodyRotationMode.Target, InterruptionTier.BeenHit, true, null, entityActionSequence_AssertStanding, null);
                if (ap.targetedWorldObject != null)
                {
                    InsertAction(attackRecover);
                }
            }
            else
            {
                AssertWeaponChargedStatus(false);
            }

            NextAction();

        }
    }

    public void EquipOptimalWeaponForTarget(ActionParameters ap)
    {

        if(entityItems == null || ap.targetedWorldObject == null){ return; }

        // find the stats of the action parameter's target gameobject
        Stats targetStats;
        EntityStats targetEntityStats = ap.targetedWorldObject.GetComponent<EntityStats>();
        if(targetEntityStats != null)
        {
            targetStats = targetEntityStats.combinedStats; 
        }
        else
        {
            ItemHitDetection ihd = GetComponent<ItemHitDetection>();
            if(ihd != null)
            {
                targetStats = ihd.item.baseStats;
            }
            else
            {
                Item item = Item.GetItemByName(ap.targetedWorldObject.name);
                if (item != null)
                {
                    targetStats = item.baseStats;
                }
                else
                {
                    Debug.Log("Couldn't find stats for targeted object");
                    return;
                }
            }   
        }

        Dictionary<ItemDamageType, float> armorFromDamageTypeDict = new Dictionary<ItemDamageType, float>()
        {
            { ItemDamageType.Blunt, targetStats.armorBlunt },
            { ItemDamageType.Slash, targetStats.armorSlash },
            { ItemDamageType.Pierce, targetStats.armorPierce }
        };
        List<ItemDamageType> priorityDamageTypeList = new List<ItemDamageType>(armorFromDamageTypeDict.Keys);
        priorityDamageTypeList.OrderBy(idt => armorFromDamageTypeDict[idt]);

        ItemDamageType damageType_weaponEquipped = entityItems.weaponEquipped_item == null ? ItemDamageType.None : entityItems.weaponEquipped_item.damageType;
        ItemDamageType damageType_weaponUnequipped = entityItems.weaponUnequipped_item == null ? ItemDamageType.None : entityItems.weaponUnequipped_item.damageType;
        foreach(ItemDamageType idt in priorityDamageTypeList)
        {
            if(damageType_weaponEquipped == idt)
            {
                return;
            }
            else if(damageType_weaponUnequipped == idt)
            {
                entityItems.ToggleWeaponEquipped();
                return;
            }
        }


    }

    void AttackRecover(ActionParameters ap){

        TerminateActionLayer("Command");
        BeginActionLayer("Command", ap, _AttackRecover());

        IEnumerator _AttackRecover()
        {
            //Debug.Log("ATTACK RECOVER");

            if(ap.targetedWorldObject != null)
            {

                // if target is alive (hasn't been deleted), queue repeat attack
                ActionParameters chase = ActionParameters.GenerateActionParameters(entityHandle, ActionType.Chase, ap.targetedWorldObject, Vector3.zero, -1, null, null, CalculateChaseTime(), DISTANCE_THRESHOLD_MELEE_ATTACK, BodyRotationMode.Target, InterruptionTier.BeenHit, true, null, entityActionSequence_AssertStanding, null);
                InsertAction(chase);

                // queue attack recovery actions as defined in behavior profile
                foreach(ActionType actionType in behaviorProfile.attackRecoverySequence){
                    ActionParameters newAp = ActionParameters.GenerateActionParameters();
                    newAp.type = actionType;
                    newAp.targetedWorldObject = ap.targetedWorldObject;
                    newAp.urgent = true;
                    InsertAction(newAp);
                }

                AssertWeaponChargedStatus(true);

                NextAction();
                yield return null;

            }
            else{
                //Debug.Log("target is null");
                AssertWeaponChargedStatus(false);
                ClearActionQueue();
                NextAction();
            }
        }
    
    }

    void StepBack(ActionParameters ap)
    {
        Vector3 offset = Vector3.forward * -1f * DISTANCE_STEP_BACK;
        ActionParameters goTo = ActionParameters.GenerateActionParameters(entityHandle, ActionType.GoTo, ap.targetedWorldObject, offset, -1, null, null, .5f, DISTANCE_THRESHOLD_SAME_POINT, BodyRotationMode.Target, InterruptionTier.BeenHit, true, null, entityActionSequence_AssertStanding, null);
        InsertAction(goTo);
        NextAction();
    }

    void StepSide(ActionParameters ap)
    {
        Vector3 offset = (Utility.GetRandomBoolean() ? Vector3.right : Vector3.left) * DISTANCE_STEP_SIDE;
        ActionParameters goTo = ActionParameters.GenerateActionParameters(entityHandle, ActionType.GoTo, ap.targetedWorldObject, offset, -1, null, null, .5f, DISTANCE_THRESHOLD_SAME_POINT, BodyRotationMode.Target, InterruptionTier.BeenHit, true, null, entityActionSequence_AssertStanding, null);
        InsertAction(goTo);
        NextAction();
    }


    public void CollectItem(ActionParameters ap)
    {

        Item targetItem = ap.item_target;
        GameObject closestObject = FindClosestObject(targetItem, SENSE_DISTANCE_EARSHOT, false);
        CollectFoundObject(closestObject);
    }

    public void CollectItemSameType(ActionParameters ap)
    {
        Item targetItem = ap.item_target;
        GameObject closestObject = FindClosestObject(targetItem, SENSE_DISTANCE_EARSHOT, true);
        CollectFoundObject(closestObject);
    }


    void CollectFoundObject(GameObject targetedObject)
    {
        if(targetedObject == null)
        {
            // TODO: search in new area if nothing found
            //Debug.Log("Collect: nothing found");
            NextAction();
        }
        else{
            //Log("Collect: picking up object");
            entityInfo.faction.AddObjectTargeted(targetedObject);
            ActionParameters goToObject = ActionParameters.GenerateActionParameters(entityHandle, ActionType.GoTo, targetedObject, Vector3.zero, -1, Item.GetItemByName(targetedObject.name), null, -1, DISTANCE_THRESHOLD_SAME_SPOT, BodyRotationMode.Normal, InterruptionTier.SenseDanger, false, null, entityActionSequence_AssertStanding, null);
            ActionParameters pickupObject = ActionParameters.GenerateActionParameters(entityHandle, ActionType.Pickup, targetedObject, Vector3.zero, -1, Item.GetItemByName(targetedObject.name), null, -1, -1f, BodyRotationMode.Normal, InterruptionTier.Nothing, false, null, entityActionSequence_AssertStanding, null);
            InsertAction(pickupObject);
            InsertAction(goToObject);
            NextAction();
        }
    }

    public void Pickup(ActionParameters ap)
    {

        TerminateActionLayer("Hands");
        BeginActionLayer("Hands", ap, _Pickup());

        IEnumerator _Pickup()
        {
            if (ap.targetedWorldObject != null)
            {
                GameObject targetedObject = ap.targetedWorldObject;
                Item targetedItem = Item.GetItemByName(targetedObject.name);

                yield return new WaitForSecondsRealtime(.25f);
                if (targetedObject != null)
                {
                    entityItems.OnObjectTake(targetedObject, targetedObject.GetComponent<ObjectReference>().GetObjectReference());
                    yield return new WaitForSecondsRealtime(.25f);

                    entityInfo.faction.RemoveItemTargeted(targetedObject);
                }
            }

            NextAction();
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

    void BeginActionLayer(string layer, ActionParameters a, IEnumerator coroutine)
    {
        actionLayers[layer] = a;
        coroutineLayers[layer] = coroutine;
        StartCoroutine(coroutine);
    }


    Vector3 GetNavigationDirection(Vector3 targetPos, bool reverse){

        bool jumped = false;

        // set direction to face
        targetPos.y = transform.position.y;
        Vector3 targetDirection = reverse ? (transform.position - targetPos) : (targetPos - transform.position);
        Transform gyro = entityPhysics.gyro;
        gyro.LookAt(targetPos);
        if(reverse){ gyro.Rotate(Vector3.up * 180f); } // if reverse (running away from target), turn in y axis
        Quaternion rot = gyro.rotation;
        rot.x = 0;
        rot.z = 0;
        gyro.rotation = rot;
		
        float leftDistance, centerDistance, rightDistance;
        RaycastHit leftHitInfo, centerHitInfo, rightHitInfo;
		Vector3 path = transform.position - targetPos;
		path.y = 0;

			
		// if obstacle in front and it's not the player object
		if(SenseObstacle())
        {
				
			// if obstacle can't be jumped over, navigate around it
			if(!CanClearObstacle() || true){
				TurnTowardsMostOpenPath();
			}
			else{
				
				// if close enough to obstacle, attempt a jump
				if(Mathf.Min(Mathf.Min(leftDistance, centerDistance), rightDistance) < MAX_JUMPFROM_DISTANCE){
					
                    Debug.Log("Jump Attempt");

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
        

        return targetDirection;



        bool SenseObstacle(){

            // set raycasts to reach castDistance units away
            Transform gs = entityPhysics.groundSense;
            Vector3 moveDir = entityPhysics.moveDir;

            bool leftCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward + gyro.right * -4f, out leftHitInfo, SENSE_DISTANCE_OBSTACLE, LayerMaskController.WALKABLE);
            bool centerCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward * 4f, out centerHitInfo, SENSE_DISTANCE_OBSTACLE, LayerMaskController.WALKABLE);
            bool rightCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), gyro.forward + gyro.right * 4f, out rightHitInfo, SENSE_DISTANCE_OBSTACLE, LayerMaskController.WALKABLE);


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
            return hits >= 1;
            //return false;
            

        }

        bool CanClearObstacle()
        {
            //Transform ohs = entityPhysics.obstacleHeightSense;
            //return !Physics.BoxCast(ohs.position, new Vector3(entityPhysics.worldCollider.bounds.extents.x, .01f, .1f), gyro.forward, gyro.rotation, Mathf.Max(leftDistance, centerDistance, rightDistance));
        
            return Mathf.Max(rightHitInfo.point.y, centerHitInfo.point.y, leftHitInfo.point.y) < entityPhysics.obstacleHeightSense.position.y;
        
        }

        void TurnTowardsMostOpenPath(){

            if (leftDistance < rightDistance){
                targetDirection = gyro.forward + gyro.right*30f;
            }
            else{
                targetDirection = gyro.forward + gyro.right*-30f;
            }
        }
	}


    public List<GameObject> SenseSurroundingItems(Item targetItem, float senseDistance, bool senseSameType)
    {
        //Collider[] colliders = Physics.OverlapSphere(transform.position, senseDistance, LayerMaskController.ITEM).Where(col => col.gameObject.tag == "Item").ToArray();
        Collider[] colliders = Physics.OverlapSphere(transform.position, senseDistance, LayerMaskController.ITEM, QueryTriggerInteraction.Ignore);
        
        List<GameObject> foundObjects = new List<GameObject>();
        GameObject foundObject;
        Item foundItem;
        foreach(Collider col in colliders)
        {
            foundObject = col.gameObject;
            foundItem = Item.GetItemByName(foundObject.name);

            bool matchesParameters = senseSameType ? (foundItem.type.Equals(targetItem.type)) : foundItem.Equals(targetItem);
            if (matchesParameters)
            {
                //Debug.Log(foundObject.name);
                if (!entityInfo.faction.ItemIsTargetedByThisFaction(foundObject))
                {
                    if(!(foundObject.GetComponent<ObjectReference>().GetObjectReference() is EntityItems))
                    {
                        foundObjects.Add(foundObject);
                    }
                }
            }
            
        }
        //Debug.Log("Surroundings: " + sur);

        return foundObjects;
        
    }

    public GameObject FindClosestObject(Item targetItem, float searchDistance, bool senseSameType)
    {
        List<GameObject> foundObjects = SenseSurroundingItems(targetItem, searchDistance, senseSameType);
        if(foundObjects.Count > 0)
        {
            foundObjects = foundObjects.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).ToList();
            return foundObjects[0];
        }
        else
        {
            return null;
        }
        
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
        foreach(Collider col in colliders)
        {
            o = col.gameObject;
            foundHandle = o.GetComponentInParent<EntityHandle>();
            if(foundHandle != null)
            {
                if(!ReferenceEquals(foundHandle, entityHandle) && !ReferenceEquals(foundHandle.entityInfo.faction, entityInfo.faction))
                {
                    if((targetSpecies.Equals(Species.Any) || targetSpecies.Equals(foundHandle.entityInfo.species)))
                    {
                        foundHandles.Add(foundHandle);
                    }
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

        // timid nibbas
        if (behaviorProfile.behaviorType.Equals(BehaviorType.Timid))
        {
            foreach (EntityHandle handleOther in sensedCreatureHandles)
            {
                behaviorTypeOther = handleOther.entityInfo.speciesInfo.behaviorProfile.behaviorType;
                if (behaviorTypeOther.Equals(BehaviorType.Aggressive) || behaviorTypeOther.Equals(BehaviorType.Steadfast))
                {
                    distanceAway = Vector3.Distance(transform.position, handleOther.transform.position);
                    if(distanceAway < DISTANCE_THRESHOLD_FLEE){
                        reacting = true;
                        InsertActionAndExecuteAsap(ActionParameters.GenerateActionParameters(entityHandle, ActionType.RunFrom, handleOther.gameObject, Vector3.zero, -1, null, null, CalculateFleeTime(), DISTANCE_THRESHOLD_CHASE, BodyRotationMode.Normal, InterruptionTier.SenseDanger, true, null, entityActionSequence_AssertStanding, null), true);
                    }
                }
            }
        }

        // steadfast
        else if (behaviorProfile.behaviorType.Equals(BehaviorType.Steadfast))
        {
            foreach (EntityHandle handleOther in sensedCreatureHandles)
            {
                behaviorTypeOther = handleOther.entityInfo.speciesInfo.behaviorProfile.behaviorType;
                if (behaviorTypeOther.Equals(BehaviorType.Aggressive) || behaviorTypeOther.Equals(BehaviorType.Steadfast))
                {
                    distanceAway = Vector3.Distance(transform.position, handleOther.transform.position);
                    if(distanceAway < DISTANCE_THRESHOLD_STANDGROUND){
                        reacting = true;
                        InsertActionAndExecuteAsap(ActionParameters.GenerateActionParameters(entityHandle, ActionType.Chase, sensedCreatureHandles[0].gameObject, Vector3.zero, -1, null, null, CalculateChaseTime(), DISTANCE_THRESHOLD_LUNGEATTACK, BodyRotationMode.Target, InterruptionTier.BeenHit, true, null, entityActionSequence_AssertStanding, null), true);
                    }
                }
            }
        }

        // aggressive
        else if (behaviorProfile.behaviorType.Equals(BehaviorType.Aggressive)){
            reacting = true;
            InsertActionAndExecuteAsap(ActionParameters.GenerateActionParameters(entityHandle, ActionType.Chase, sensedCreatureHandles[0].gameObject, Vector3.zero, -1, null, null, CalculateChaseTime(), DISTANCE_THRESHOLD_LUNGEATTACK, BodyRotationMode.Target, InterruptionTier.BeenHit, true, null, entityActionSequence_AssertStanding, null), true);
        }

    }

    public void AssertWeaponChargedStatus(bool targetChargedValue)
    {
        if (entityItems != null)
        {
            if (entityItems.weaponEquipped_item != null)
            {
                if(entityPhysics.weaponCharging != targetChargedValue)
                {
                    entityPhysics.Attack(AttackType.Weapon, null);
                    timeSince_attack = 0f;
                }
            }
        }
    }

    public void OnRestFrame()
    {

        
        // increment rest if not fully rested
        if(!entityStats.IsStaminaFull() || !entityStats.IsHealthFull())
        {

            // make sure entity is squatting
            entityPhysics.AssertSquatting();

            // increment stamina
            entityStats.ApplyStaminaIncrement(STAMINA_GAIN_RATE * Time.deltaTime);

            // increment health
            entityStats.ApplyHealthIncrement(HEALTH_GAIN_RATE * Time.deltaTime, null);

            // if fully rested and healed, "exit" the tent by setting its tent reference to null, opening the tent's spot(s) to other npc's, and stand up
            if(entityStats.IsStaminaFull() && entityStats.IsHealthFull())
            {
                SetRestingTent(null);
                entityPhysics.AssertStanding();
            }
        }


    }

    public void UpdateStaminaLoss()
    {

        // if entity requires rest, update rest at a rate such that it reaches 0 after 24 in-game hours
        if(behaviorProfile.requiresRest)
        {
            if(!entityPhysics.isInsideCamp)
            {
                if(entityStats.stamina > 0f)
                {
                    entityStats.ApplyStaminaIncrement(STAMINA_LOSS_RATE * (1f / Stats.GetStatValue(entityStats.combinedStats, Stats.StatType.Stamina)) * Time.deltaTime * -1f);
                }
            }

            // if (!isLocalPlayer)
            // {
            //     Debug.Log("Rest level: " + rest);
            // }
        }
        //Debug.Log(entityStats.stamina);
    }

    public bool IsAtPosition(Vector3 position, float distanceThreshhold){
        return Vector3.Distance(transform.position, position) < distanceThreshhold;
    }

    public float CalculateChaseTime(){
        return BASE_TIME_CHASE * entityStats.combinedStats.stamina;
    }

    public float CalculateFleeTime(){
        return BASE_TIME_FLEE * entityStats.combinedStats.stamina;
    }

    public float CalculateAttackCooldownTime()
    {
        return BASE_TIMESTEP_ATTACK * entityStats.combinedStats.attackSpeed;
    }

    public float CalculateTimeUntilCanAttack()
    {
        return CalculateAttackCooldownTime() - timeSince_attack;
    }

    bool WithinActiveDistance(){
        return Vector3.Distance(GameManager.instance.localPlayer.transform.position, transform.position) <= DISTANCE_THRESHOLD_ACTIVE;
    }

    public bool IsTargetedObject(GameObject otherObject)
    {
        if(activeAction == null)
        {
            return true;
        }
        else
        {
            if(activeAction.targetedWorldObject == null)
            {
                return true;
            }
            else
            {
                return Utility.IsInHierarchy(otherObject.transform, activeAction.targetedWorldObject.transform);
            }

            //return ReferenceEquals(otherObject, activeAction.targetedWorldObject);
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
        bool reactingToSensedCreature;
        HandleCreatureSense(out reactingToSensedCreature);
    }


    void HandleCreatureSense(out bool sensed)
    {

        sensed = false;
        if (timeSince_creatureSense >= BASE_TIMESTEP_CREATURESENSE)
        {
            if (WithinActiveDistance())
            {
                //Debug.Log("Boutta sense creatures");
                sensed = true;
                ReactToNearbyCreatures(behaviorProfile.behaviorType, out sensed);
            }

            timeSince_creatureSense = 0;
        }

    }



    public GameObject ClaimOpenRestingTent()
    {

        if(restingTent != null)
        {
            return restingTent.worldObject;
        }

        Tent openTent = entityInfo.faction.camp.GetOpenTent();
        if(openTent != null)
        {
            SetRestingTent(openTent);
            return openTent.worldObject;
        }
        else
        {
            SetRestingTent(null);
            return null;
        }
    }


    public void ResetFollowPosition()
    {
        
        //Debug.Log("updating home position");

        if(entityInfo.faction.leaderInCamp)
        {
            //Debug.Log("(leader in camp)");
            Transform campPositionT = entityInfo.faction.camp.GetOpenTribeMemberStandPosition().transform;
            //Debug.Log(homeT.gameObject.name);
            //Debug.Log(campPositionT.gameObject.name);
            followPositionTransform.SetParent(campPositionT);
            followPositionTransform.position = campPositionT.position;
        }
        else
        {
            //Debug.Log("(leader NOT in camp)");

            // Transform directionalT = Utility.FindDeepChild(entityInfo.faction.leaderHandle.gameObject.transform, "DirectionalTs");
            // directionalT = directionalT.GetChild(UnityEngine.Random.Range(0, directionalT.childCount - 1));
            // homeT.SetParent(directionalT);
            // homeT.position = directionalT.position;

            followPositionTransform.SetParent(entityInfo.faction.leaderHandle.transform);
            followPositionTransform.position = entityInfo.faction.leaderHandle.transform.position;
            
        }

        entityPhysics.AssertStanding();
    }

    public void ResetFollowPositionIfReady()
    {
        if(homeResetTimer.ElapsedMilliseconds >= homeResetTime * 1000f)
        {
            ResetHomeResetTimer();
            ResetFollowPosition();
        }
    }

    void ResetHomeResetTimer()
    {
        homeResetTime = UnityEngine.Random.Range(HOME_RESET_TIME_MIN, HOME_RESET_TIME_MAX);
        homeResetTimer.Restart();
    }

    public void SetRestingTent(Tent tent)
    {
        if(restingTent != null)
        {
            restingTent.RemoveOccupant(entityHandle);
        }

        if(tent != null)
        {
            tent.AddOccupant(entityHandle);
        }

        restingTent = tent;

    }


    // Update is called once per frame
    void Update()
    {

        entityPhysics.moveDir = move;

        if (!isPlayer)
        {
            UpdateBehavior();
            UpdateStaminaLoss();
        }

        timeSince_creatureSense += Time.deltaTime;
        timeSince_attack += Time.deltaTime;


        
    }




}