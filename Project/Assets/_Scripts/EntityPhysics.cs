using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DitzelGames.FastIK;

public class EntityPhysics : EntityComponent
{

    public Collider worldCollider;
    public PhysicMaterial highFrictionMat;
    public PhysicMaterial noFrictionMat;
    public LayerMask layerMask_water;
    public LayerMask layerMask_walkable;

    public Rigidbody rb;
    public Animator animator;
    public Animator iKTargetAnimator;
    public Transform gyro;
    public Transform body;
    public Transform[] bodyPartTs, bodyPartTs_legs, bodyPartTs_upperBody;
    public Transform hips, head, handRight, handLeft, fingerRight, fingerLeft, footRight, footLeft, toeRight, toeLeft;
    public Transform groundSense, wallSense, waterSense, obstacleHeightSense, kneeHeightT;
    public RaycastHit groundInfo, wallInfo, waterInfo;
    public static float BASE_CASTDISTANCE_GROUND_PLAYER = .3f;
    public static float BASE_CASTDISTANCE_GROUND_NPC = .3f;
    public static float BASE_CASTDISTANCE_WALL = 1f;
    float groundCastDistance;
    float distanceFromGround;

    public static float BASE_FORCE_JUMP = 2800f;
    public static float BASE_FORCE_THROW = 400f;
    public static float BASE_ACCELERATION = 12f;
    public static float BASE_MAX_SPEED = 11f;
    public static float BASE_COOLDOWN_JUMP = .15f;
    public static float WEAPON_CHARGETIME_MAX = 3f;
    public static float WEAPON_HITTIME_MAX = .25f;


    public Vector3 moveDir;
    public bool onWalkableGround;
    public bool jumping, jumpOpposite, sprinting;
    public float offWallTime, offWaterTime, jumpTime, airTime, groundTime;
    public float acceleration;
    public float maxSpeed_run, maxSpeed_sprint, maxSpeed_climb, maxSpeed_swim;


    public static float landScrunch_recoverySpeed = .75f;
    public static float landScrunch_airTimeThreshhold = 1.2f;
    public float landScrunch;
    public static float crouchSpeed = .75f;
    public static float crouch_airTimeThreshhold = 1.2f;
    public float crouch;
    public bool handFree_right, handFree_left;
    public bool insideCamp;


    public float ROTATION_Y_THIS;
    public float ROTATION_Y_LAST;
    public bool GROUNDTOUCH, WALLTOUCH, IN_WATER;


    // ik
    public bool ikEnabled;
    public FastIKFabric ikScript_hips, ikScript_footLeft, ikScript_footRight, ikScript_toeRight, ikScript_toeLeft, ikScript_handRight, ikScript_handLeft, ikScript_fingerRight, ikScript_fingerLeft;
    public FastIKFabric[] ikScripts, ikScripts_legs, ikScripts_upperBody;
    public Transform ikParent;
    public Transform basePositionHips, basePositionFootRight, basePositionFootLeft, basePositionHandRight, basePositionHandLeft;
    public Transform targetHips, targetFootRight, targetFootLeft, targetToeRight, targetToeLeft, targetHandRight, targetHandLeft, targetFingerRight, targetFingerLeft;
    public Vector3 plantPosFootRight, plantPosFootLeft, plantPosHandRight, plantPosHandLeft;
    public float updateTime_footRight, updateTime_footLeft, updateTime_handRight, updateTime_handLeft, updateTime_hips;

    // ikProfile settings
    public IkProfile ikProfile;
    public bool quadripedal;
    public bool hasFingersAndToes;
    public float runCycle_strideFrequency;
    public float runCycle_lerpTightness;
    public float runCycle_limbVerticalDisplacement;
    public float runCycle_limbForwardReachDistance;



    // other settings
    float weaponChargeTime;
    bool weaponCharging;
    float weaponChargeAmount;
    float attackHitTime;
    public bool attackCanHit;
    public bool attackHit;





    protected override void Awake(){

        base.Awake();

    }

    void Start(){

        body = Utility.FindDeepChildWithTag(this.transform, "Body");
        worldCollider = body.GetComponent<CapsuleCollider>();
        highFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/HighFriction");
        noFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/NoFriction");
        layerMask_water = LayerMask.GetMask("Water");
        layerMask_walkable = LayerMask.GetMask("Default", "Terrain", "Feature", "Item");
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        gyro = Utility.FindDeepChild(this.transform, "Gyro");

        ikProfile = entityInfo.speciesInfo.ikProfile;
        quadripedal = ikProfile.quadripedal;
        hasFingersAndToes = ikProfile.hasFingersAndToes;
        runCycle_strideFrequency = ikProfile.runCycle_strideFrequency;
        runCycle_lerpTightness = ikProfile.runCycle_lerpTightness;
        runCycle_limbVerticalDisplacement = ikProfile.runCycle_limbVerticalDisplacement;
        runCycle_limbForwardReachDistance = ikProfile.runCycle_limbForwardReachDistance;

        hips = Utility.FindDeepChild(body, ikProfile.name_hips);
        head = Utility.FindDeepChild(body, ikProfile.name_head);
        handRight = Utility.FindDeepChild(body, ikProfile.name_handRight);
        handLeft = Utility.FindDeepChild(body, ikProfile.name_handLeft);
        footRight = Utility.FindDeepChild(body, ikProfile.name_footRight);
        footLeft = Utility.FindDeepChild(body, ikProfile.name_footLeft);
        if(hasFingersAndToes){
            toeRight = Utility.FindDeepChild(body, ikProfile.name_toeRight);
            toeLeft = Utility.FindDeepChild(body, ikProfile.name_toeLeft);
            fingerRight = Utility.FindDeepChild(body, ikProfile.name_fingerRight);
            fingerLeft = Utility.FindDeepChild(body, ikProfile.name_fingerLeft);
        }

        bodyPartTs = new Transform[]{ handRight, handLeft, fingerRight, fingerLeft, footRight, footLeft, toeRight, toeLeft };
        bodyPartTs_legs = new Transform[]{ footRight, footLeft, toeRight, toeLeft };
        bodyPartTs_upperBody = new Transform[]{ handRight, handLeft, fingerRight, fingerLeft };
        groundSense = Utility.FindDeepChild(transform, "GroundSense");
        obstacleHeightSense = Utility.FindDeepChild(transform, "ObstacleHeightSense");
        wallSense = Utility.FindDeepChild(transform, "WallSense");
        waterSense = Utility.FindDeepChild(transform, "WaterSense");
        kneeHeightT = Utility.FindDeepChild(transform, "KneeHeight");
        if(tag == "Player"){
            groundCastDistance = BASE_CASTDISTANCE_GROUND_PLAYER;
        }else if(tag == "Npc"){
            groundCastDistance = BASE_CASTDISTANCE_GROUND_NPC;
        }

        ikEnabled = true;
        ikParent = Utility.FindDeepChild(transform, "IKTargets");
        basePositionHips = ikParent.Find("BasePositionHips");
        basePositionFootRight = ikParent.Find("BasePositionFootRight");
        basePositionFootLeft = ikParent.Find("BasePositionFootLeft");
        targetHips = ikParent.Find("TargetHips");
        targetFootRight = ikParent.Find("TargetFootRight");
        targetFootLeft = ikParent.Find("TargetFootLeft");
        targetHandRight = ikParent.Find("TargetHandRight");
        targetHandLeft = ikParent.Find("TargetHandLeft");
        basePositionHandRight = ikParent.Find("BasePositionHandRight");
        basePositionHandLeft = ikParent.Find("BasePositionHandLeft");
        ikScript_hips = hips.GetComponent<FastIKFabric>();
        ikScript_footRight = footRight.GetComponent<FastIKFabric>();
        ikScript_footLeft = footLeft.GetComponent<FastIKFabric>();
        ikScript_handRight = handRight.GetComponent<FastIKFabric>();
        ikScript_handLeft = handLeft.GetComponent<FastIKFabric>();
        if(hasFingersAndToes){
            targetToeRight = ikParent.Find("TargetToeRight");
            targetToeLeft = ikParent.Find("TargetToeLeft");
            targetFingerRight = ikParent.Find("TargetFingerRight");
            targetFingerLeft = ikParent.Find("TargetFingerLeft");
            ikScript_toeRight = toeRight.GetComponent<FastIKFabric>();
            ikScript_toeLeft = toeLeft.GetComponent<FastIKFabric>();
            ikScript_fingerRight = fingerRight.GetComponent<FastIKFabric>();;
            ikScript_fingerLeft = fingerLeft.GetComponent<FastIKFabric>();
        }
        ikScripts = new FastIKFabric[]{ ikScript_hips, ikScript_footRight, ikScript_footLeft, ikScript_toeRight, ikScript_toeLeft, ikScript_handRight, ikScript_handLeft, ikScript_fingerRight, ikScript_fingerLeft };
        ikScripts_legs = new FastIKFabric[]{ ikScript_footRight, ikScript_footLeft, ikScript_toeRight, ikScript_toeLeft };
        ikScripts_upperBody = new FastIKFabric[]{ ikScript_handRight, ikScript_handLeft, ikScript_fingerRight, ikScript_fingerLeft };
        iKTargetAnimator = ikParent.GetComponent<Animator>();

        acceleration = Stats.GetStatValue(entityStats.statsCombined, Stats.StatType.Speed) * .5f * BASE_ACCELERATION;
        maxSpeed_run = Stats.GetStatValue(entityStats.statsCombined, Stats.StatType.Speed) * .5f * BASE_MAX_SPEED;
        maxSpeed_sprint = maxSpeed_run * 1.5f;
        maxSpeed_climb = maxSpeed_run * .25f;
        maxSpeed_swim = maxSpeed_run * .75f;

        plantPosFootRight = targetFootRight.position;
        plantPosFootLeft = targetFootLeft.position;
        plantPosHandRight = targetHandRight.position;
        plantPosHandLeft = targetHandLeft.position;
        updateTime_footRight = 0f;
        updateTime_footLeft = .5f;
        updateTime_handRight = .5f;
        updateTime_handLeft = 0f;
        updateTime_hips = 0f;

        if(!quadripedal){
            handFree_right = handFree_left = true;
        }
        else{
            handFree_right = handFree_left = false;
        }

        // find attackCollisionDetectors
        foreach(AttackCollisionDetector acd in GetComponentsInChildren<AttackCollisionDetector>()){
            acd.SetOwner(entityHandle);
        }

        // disable all self collision
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach(Collider col0 in allColliders){
            foreach(Collider col1 in allColliders){
                Physics.IgnoreCollision(col0, col1, true);
            }
        }

        ToggleIK(true);
        UpdateIKForCarryingItems();
    }

    public void ToggleIK(bool value){
        if(ikEnabled != value){
            ikEnabled = value;
            foreach(FastIKFabric script in ikScripts){
                script.enabled = value;
            }
        }
        

    }


    // check if foot is behind threshhold, if so set new plant point
    public void IKUpdate(){
        if (ikEnabled)
        {

            // if(IN_WATER){
            //     ikScript_footLeft.enabled = false;
            //     ikScript_footRight.enabled = false;
            //     ikScript_toeLeft.enabled = false;
            //     ikScript_toeRight.enabled = false;
            //     ikScript_fingerRight.enabled = false;
            //     ikScript_fingerLeft.enabled = false;
            // }
            // else{
            //     ikScript_footLeft.enabled = true;
            //     ikScript_footRight.enabled = true;
            //     ikScript_toeLeft.enabled = true;
            //     ikScript_toeRight.enabled = true;
            //     ikScript_fingerRight.enabled = true;
            //     ikScript_fingerLeft.enabled = true;
            // }

            bool moving = IsMoving();
            bool groundIsClose = GroundIsClose();

            UpdateLimbPositions(IN_WATER);

            if (groundIsClose || IN_WATER)
            {
                // not in the air
                if (moving || IN_WATER)
                {
                    // moving

                    // check if plant points need update
                    if (updateTime_footRight >= 1f)
                    {
                        CyclePlantPosition(targetFootRight, basePositionFootRight, ref plantPosFootRight, ref updateTime_footRight, IN_WATER);
                    }
                    if (updateTime_footLeft >= 1f)
                    {
                        CyclePlantPosition(targetFootLeft, basePositionFootLeft, ref plantPosFootLeft, ref updateTime_footLeft, IN_WATER);
                    }

                    // if quadripedal, check for hand placement update as well
                    if(quadripedal){
                        if (updateTime_handRight >= 1f)
                        {
                            CyclePlantPosition(targetHandRight, basePositionHandRight, ref plantPosHandRight, ref updateTime_handRight, IN_WATER);
                        }
                        if (updateTime_handLeft >= 1f)
                        {
                            CyclePlantPosition(targetHandLeft, basePositionHandLeft, ref plantPosHandLeft, ref updateTime_handLeft, IN_WATER);
                        }
                    }

                }
                else
                {
                    // not moving
                    SetPlantPosition(targetFootLeft, basePositionFootLeft, Vector3.zero, ref plantPosFootLeft);
                    SetPlantPosition(targetFootRight, basePositionFootRight, GetHorizVelocity().normalized * -.2f, ref plantPosFootRight);
                    if(quadripedal){
                        SetPlantPosition(targetHandLeft, basePositionHandLeft, Vector3.zero, ref plantPosHandLeft);
                        SetPlantPosition(targetHandRight, basePositionHandRight, GetHorizVelocity().normalized * -.2f, ref plantPosHandRight);
                    }
                }
            }
            else
            {
                // in the air
                SetPlantPosition(targetFootLeft, basePositionFootLeft, Vector3.up * .1f + entityOrientation.body.right * 0f, ref plantPosFootLeft);
                SetPlantPosition(targetFootRight, basePositionFootRight, Vector3.up * .3f + entityOrientation.body.forward * .5f + entityOrientation.body.right * .1f, ref plantPosFootRight);
                updateTime_footRight = .2f;
                updateTime_footLeft = .7f;
                if(quadripedal){
                    SetPlantPosition(targetHandLeft, basePositionHandLeft, Vector3.up * .1f + entityOrientation.body.right * 0f, ref plantPosHandLeft);
                    SetPlantPosition(targetHandRight, basePositionHandRight, Vector3.up * .3f + entityOrientation.body.forward * .5f + entityOrientation.body.right * .1f, ref plantPosHandRight);
                    updateTime_handRight = .2f;
                    updateTime_handLeft = .7f;
                }
            }

            // update plant positions for accuracy
            UpdatePlantPosition(targetFootRight, basePositionFootRight, ref plantPosFootRight, groundIsClose);
            UpdatePlantPosition(targetFootLeft, basePositionFootLeft, ref plantPosFootLeft, groundIsClose);
            if(quadripedal){
                UpdatePlantPosition(targetHandRight, basePositionHandRight, ref plantPosHandRight, groundIsClose);
                UpdatePlantPosition(targetHandLeft, basePositionHandLeft, ref plantPosHandLeft, groundIsClose);
            }
            ApplyFootPositionRestraints();

        }
        else{
            if(!IN_WATER){
                ToggleIK(true);
                IKUpdate();
                UpdateIKForCarryingItems();
            }
        }
        
        float footPlantTimeUpdateSpeed = runCycle_strideFrequency * Time.deltaTime;
        float hipsUpdateSpeed = footPlantTimeUpdateSpeed / 6f;
        updateTime_footRight += footPlantTimeUpdateSpeed;
        updateTime_footLeft += footPlantTimeUpdateSpeed;
        updateTime_handRight += footPlantTimeUpdateSpeed;
        updateTime_handLeft += footPlantTimeUpdateSpeed;
        updateTime_hips += hipsUpdateSpeed;


    }

    public void UpdateLimbPositions(bool water){

        // hips
        if(updateTime_hips > 1f){
            updateTime_hips = updateTime_hips - 1f;
        }
        targetHips.position = basePositionHips.position + Vector3.up * GetRunCyclePhase(updateTime_hips, 0f) * .2f;


        // feet and toes
        float changePositionSpeed = runCycle_lerpTightness * Time.deltaTime;
        Vector3 vertFootLeft, vertFootRight;
        if(IsMoving()){
            // moving
            vertFootLeft = Vector3.up * GetRunCycleVerticality(updateTime_footLeft, water);
            vertFootRight = Vector3.up * GetRunCycleVerticality(updateTime_footRight, water);
            Vector3 toeForward = (entityOrientation.body.forward + transform.forward).normalized;
            if (hasFingersAndToes)
            {
                targetToeRight.position = targetFootRight.position + toeForward + Vector3.down * (GetRunCyclePhase(updateTime_footRight, 0f) + .2f);
                targetToeLeft.position = targetFootLeft.position + toeForward + Vector3.down * (GetRunCyclePhase(updateTime_footLeft, 0f) + .2f);
                if (quadripedal)
                {
                    targetFingerRight.position = targetFingerRight.position + toeForward + Vector3.down * (GetRunCyclePhase(updateTime_footLeft, 0f) + .2f);
                    targetFingerLeft.position = targetFingerLeft.position + toeForward + Vector3.down * (GetRunCyclePhase(updateTime_footRight, 0f) + .2f);
                }
            }

            
        }
        else{
            // not moving
            vertFootLeft = vertFootRight = Vector3.up * GetRunCycleVerticality(.65f, water);
            if (hasFingersAndToes)
            {
                targetToeRight.position = targetFootRight.position + entityOrientation.body.forward.normalized + Vector3.down;
                targetToeLeft.position = targetFootLeft.position + entityOrientation.body.forward.normalized + Vector3.down;
                if (quadripedal)
                {
                    targetFingerRight.position = targetHandRight.position + entityOrientation.body.forward.normalized + Vector3.down;
                    targetFingerLeft.position = targetHandLeft.position + entityOrientation.body.forward.normalized + Vector3.down;
                }
            }
            

        }
        targetFootRight.position = Vector3.Lerp(targetFootRight.position, plantPosFootRight, changePositionSpeed) + vertFootRight;
        targetFootLeft.position = Vector3.Lerp(targetFootLeft.position, plantPosFootLeft, changePositionSpeed) + vertFootLeft;
        if(quadripedal){
            targetHandRight.position = Vector3.Lerp(targetHandRight.position, plantPosHandRight, changePositionSpeed) + vertFootLeft;
            targetHandLeft.position = Vector3.Lerp(targetHandLeft.position, plantPosHandLeft, changePositionSpeed) + vertFootRight;
        }

        
    }

    float GetRunCycleVerticality(float updateTime, bool water)
    {
        float verticalityBase = water ? .006f : .015f;
        verticalityBase *= runCycle_limbVerticalDisplacement;
        return (verticalityBase + .025f * Mathf.InverseLerp(0f, 2f, rb.velocity.y)) * Mathf.Pow(GetRunCyclePhase(updateTime, 0f), 1f);
    }

    // .5 is stance phase, -.5 is swing phase
    float GetRunCyclePhase(float updateTime, float offset)
    {
        return Mathf.Cos(updateTime * 2f * Mathf.PI + offset) + 1;
    }



    public void CyclePlantPosition(Transform targetIk, Transform baseTransform, ref Vector3 plantPos, ref float updateTime, bool water){

        float forwardReachDistance = water ? 0f : runCycle_limbForwardReachDistance * (GetHorizVelocity().magnitude / maxSpeed_sprint) + (entityOrientation.bodyLean * .1f);
        plantPos = baseTransform.position + GetHorizVelocity().normalized * 2.2f * forwardReachDistance;
        updateTime = Mathf.Max(updateTime, 1f) - 1f;
    }
    public void UpdatePlantPosition(Transform targetIk, Transform baseTransform, ref Vector3 plantPos, bool onGround){
        // move plantPos down until hits terrain
        if (onGround)
        {
            RaycastHit hit;
            if (Physics.Raycast(plantPos, Vector3.down, out hit, 100f, layerMask_walkable))
            {
                plantPos.y = hit.point.y + distanceFromGround;
            }
        }
        Vector3 pos = targetIk.position;
        pos.y = Mathf.Max(pos.y, baseTransform.position.y);
        targetIk.position = pos;
    }

    public void ApplyFootPositionRestraints(){
        Vector3 pos;

        pos = targetFootRight.position;
        pos.y = Mathf.Min(pos.y, kneeHeightT.position.y);
        targetFootRight.position = pos;

        pos = targetFootLeft.position;
        pos.y = Mathf.Min(pos.y, kneeHeightT.position.y);
        targetFootLeft.position = pos;

        if (quadripedal)
        {
            pos = targetHandRight.position;
            pos.y = Mathf.Min(pos.y, kneeHeightT.position.y);
            targetHandRight.position = pos;

            pos = targetHandLeft.position;
            pos.y = Mathf.Min(pos.y, kneeHeightT.position.y);
            targetHandLeft.position = pos;
        }
    }

    public void SetPlantPosition(Transform targetIk, Transform targetTransform, Vector3 offset, ref Vector3 positionPointer){
        Vector3 pos = targetTransform.position + offset;
        RaycastHit hit;
        if(Physics.Raycast(pos, Vector3.up, out hit, 1f, layerMask_walkable)){
            positionPointer = hit.point;
        }
        else{
            positionPointer = pos;
        }
        positionPointer = pos;
        
    }

    public void UpdateIKForCarryingItems(){

        if(quadripedal){
            return;
        }

        GameObject objectRight = entityItems.weaponEquipped_object;
        GameObject objectLeft = entityItems.holding_object;


        // right hand
        if(objectRight != null){
            ikScript_handRight.enabled = true;
            handFree_right = false;
            ikScript_handRight.Target = objectRight.transform.Find("IKTargetT_Right");
        }
        else{
            ikScript_handRight.enabled = false;
            ikScript_handRight.Target = ikParent.Find("TargetHandRight");
            handFree_right = true;
        }
        
        // left hand
        if(objectLeft != null){
            ikScript_handLeft.enabled = true;
            ikScript_handLeft.Target = objectLeft.transform.Find("IKTargetT_Left");
            handFree_left = false;
        }
        else{

            // if hand is free, support right hand with holding the weapon, if equipped
            if(!handFree_right && entityItems.weaponEquipped_item.holdStyle.Equals(Item.ItemHoldStyle.Axe)){
                ikScript_handLeft.enabled = true;
                ikScript_handLeft.Target = objectRight.transform.Find("IKTargetT_Left");
            }
            else{
                ikScript_handLeft.enabled = false;
                ikScript_handLeft.Target = ikParent.transform.Find("TargetHandLeft");
            }
            handFree_left = true;
        }


        // iKPackage.ikTarget.position = ... set ik target to handle on holding item
    }


    // ----------


    public void Move(Vector3 direction, float speed){
        float speedStat = speed * Stats.GetStatValue(entityStats.statsCombined, Stats.StatType.Speed);
        sprinting = entityBehavior.urgent || (isLocalPlayer && entityUserInput.pressSprint);
        Vector3 move = transform.TransformDirection(direction).normalized * speedStat;
        rb.AddForce(move * speedStat, ForceMode.Force);
        
    }

    public void Jump(float power){
        if(!jumping){
            
            StartCoroutine(_Jump(power));
        }
    }
    public void Jump(){
        if(!jumping){
            StartCoroutine(_Jump(BASE_FORCE_JUMP));
        }
    }
    IEnumerator _Jump(float power){
        jumping = true;
        jumpOpposite = !jumpOpposite;
        if(groundTime <= BASE_COOLDOWN_JUMP){
            float t = BASE_COOLDOWN_JUMP - groundTime;
            yield return new WaitForSecondsRealtime(t);
        }
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;
        Vector3 direction = Vector3.up;
        rb.AddForce(direction * power, ForceMode.Force);
        jumpTime = 0;
        groundTime = 0;
        yield return new WaitForSecondsRealtime(.2f);
        jumping = false;
        yield return null;
    }
    public void Vault(){
        Jump(BASE_FORCE_JUMP*.7f);
    }

    public bool CanJump(){
        if(Physics.Raycast(groundSense.position, Vector3.down, groundCastDistance + .3f, layerMask_walkable)){
            return true;
        }
        return false;
    }

    public void SetHeadTarget(Vector3 position){
        head.rotation = Quaternion.LookRotation(position, Vector3.up);
    }


    // -------------

    // attacking

    public void LaunchProjectile(Item item, Transform referenceWeaponT, bool pointTowardsDirection){

        StartCoroutine(_LaunchProjectile());

        IEnumerator _LaunchProjectile(){

            GameObject projectile = Utility.InstantiatePrefabSameName(item.worldObjectPrefab);
            AttackCollisionDetector acd =  projectile.transform.Find("HitZone").GetComponent<AttackCollisionDetector>();
            acd.SetOwner(entityHandle);
            acd.SetIsProjectile(true);
            projectile.transform.SetPositionAndRotation(referenceWeaponT.position, referenceWeaponT.rotation);
            Utility.ToggleObjectPhysics(projectile, true, true, true, true);
            Utility.IgnorePhysicsCollisions(projectile, gameObject.GetComponentInChildren<Collider>());
            Utility.IgnorePhysicsCollisions(projectile, entityItems.weaponEquipped_object.GetComponentsInChildren<Collider>());

            Vector3 throwDir = entityOrientation.body.forward + (Vector3.up * 0f);
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            projectileRb.centerOfMass = Vector3.up * .622f;
            projectileRb.angularDrag = 5f;
            float addForceTime = .2f;
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            float force = BASE_FORCE_THROW * Mathf.Lerp(1f, 1.5f, Mathf.InverseLerp(0f, BASE_MAX_SPEED, GetHorizVelocity().magnitude));
            projectileRb.velocity = rb.velocity;

            while(timer.ElapsedMilliseconds / 1000f < addForceTime){
                projectileRb.AddForce(throwDir * force);
                yield return null;
            }
            timer.Stop();


            


            yield return new WaitForSeconds(.075f);
            //entityAnimation.SetFreeBodyRotationMode(true);
            //entityAnimation.SetBodyRotationMode(EntityAnimation.BodyRotationMode.Target, null);


        }

        
    }


    public void Attack(AttackType attackType, Transform target){

        switch (attackType) {
            case (AttackType.Weapon) :
                OnWeaponAttack(target);
                break;
            case (AttackType.Bite) :
                AttackBite(target);
                break;
            case (AttackType.Swipe) :
                AttackSwipe(target);
                break;
            case (AttackType.HeadButt) :
                AttackHeadButt(target);
                break;
            case (AttackType.Stomp) :
                AttackStomp(target);
                break;
        }
    }

    void OnWeaponAttack(Transform target)
    {
        Item weapon = entityItems.weaponEquipped_item;
        bool ranged = entityItems.rangedMode;
        bool charging = weaponChargeTime == 0f;

        if (weaponChargeTime == 0f)
        {
            OnWeaponChargeBegin(weapon, ranged);
        }
        else
        {
            OnWeaponRelease(weapon, ranged);
        }

        StartAttackAnimation(charging, weapon, ranged);
        
    }
    void OnWeaponChargeBegin(Item weapon, bool ranged){
        weaponCharging = true;
        weaponChargeAmount = .001f;
    }
    void OnWeaponRelease(Item weapon, bool ranged){
        weaponCharging = false;
        weaponChargeAmount = Mathf.InverseLerp(0f, WEAPON_CHARGETIME_MAX, weaponChargeTime);
        weaponChargeTime = 0f;
        if(!ranged)
        {
            BeginAttackHitTime();
        }
        if(weapon.holdStyle.Equals(Item.ItemHoldStyle.Spear) && ranged)
        {
            // if ranged mode and using spear, launch the weapon
            LaunchProjectile(weapon, entityItems.weaponEquipped_object.transform, true);
        }
    }

    void StartAttackAnimation(bool charging, Item weapon, bool ranged)
    {
        if(weapon.holdStyle.Equals(Item.ItemHoldStyle.Spear))
        {
            if (charging)
            {
                if (ranged)
                {
                    entityItems.itemOrientationAnimator.SetTrigger("ChargeSpearRanged");
                }
                else
                {
                    entityItems.itemOrientationAnimator.SetTrigger("ChargeSpearMelee");
                }
            }
            else
            {
                if (ranged)
                {
                    entityItems.itemOrientationAnimator.SetTrigger("ReleaseSpearRanged");
                }
                else
                {
                    entityItems.itemOrientationAnimator.SetTrigger("ReleaseSpearMelee");
                }
            }
            
            
        }
        else if(weapon.holdStyle.Equals(Item.ItemHoldStyle.Axe))
        {
            if (charging)
            {
                if (ranged)
                {
                    entityItems.itemOrientationAnimator.SetTrigger("ChargeAxeRanged");
                }
                else
                {
                    entityItems.itemOrientationAnimator.SetTrigger("ChargeAxeMelee");
                }
            }
            else
            {
                if (ranged)
                {
                    entityItems.itemOrientationAnimator.SetTrigger("ReleaseAxeRanged");
                }
                else
                {
                    entityItems.itemOrientationAnimator.SetTrigger("ReleaseAxeMelee");
                }
            }
        }
    }

    void BeginAttackHitTime(){
        attackHitTime = 0f;
        attackCanHit = true;
    }
    void StopAttackHitTime(){
        attackCanHit = false;
        if(attackHit){
            attackHit = false;
        }
    }
    public void OnAttackHit(Collider collider){
        GameObject hitObject = collider.gameObject;
        //Debug.Log(hitObject.layer);

        if(hitObject.layer == LayerMask.NameToLayer("Creature") || hitObject.layer == LayerMask.NameToLayer("Feature"))
        {
            //Log("HIT!!!! " + collider.gameObject.name);
            collider.gameObject.GetComponentInParent<EntityHitDetection>().OnHit(this.entityHandle);

            // apply fixed weapon position effect if applicable
            if (entityItems != null)
            {
                StartCoroutine(FixWeaponPosition(entityItems.weaponEquipped_object, collider.transform, .45f));
            }

            StopAttackHitTime();
        }
        else if(hitObject.layer == LayerMask.NameToLayer("Item"))
        {
            Log("HIT!!!! " + collider.gameObject.name);
            ItemHitDetection ihd = collider.gameObject.GetComponentInParent<ItemHitDetection>();
            if(ihd != null){
                ihd.OnHit(this.entityHandle);
            }
        }
        
        
        

    }
    IEnumerator FixWeaponPosition(GameObject weapon, Transform targetT, float time)
    {

        Rigidbody rbTarget = targetT.gameObject.GetComponentInParent<Rigidbody>();
        Rigidbody rbWeapon = weapon.GetComponent<Rigidbody>();
        if (rbTarget == null)
        {
            rbTarget = targetT.gameObject.AddComponent<Rigidbody>();
            rbTarget.constraints = RigidbodyConstraints.FreezeAll;
        }
        if (rbWeapon == null)
        {
            rbWeapon = weapon.gameObject.AddComponent<Rigidbody>();
        }
        //rbWeapon.constraints = RigidbodyConstraints.FreezeAll;
        SpringJoint j = weapon.AddComponent<SpringJoint>();
        j.connectedBody = rbTarget;
        j.spring = 1;
        entityItems.ToggleItemOrientationUpdate(false);

        Transform handleT = weapon.transform.Find("IKTargetT_Right");

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        while(timer.ElapsedMilliseconds / 1000f < time && Vector3.Distance(handRight.position, handleT.position) < .2f){
            yield return null;
        }
        timer.Stop();
        Destroy(j);
        entityItems.ToggleItemOrientationUpdate(true);
    }

    public void OnWeaponHitRemove(){ 
        
    }

    void AttackBite(Transform target){
        // todo: bite attack
    }   

    void AttackSwipe(Transform target){
        // todo: swipe attack
        StartCoroutine(_AttackSwipe());

        IEnumerator _AttackSwipe(){
            BeginAttackHitTime();
            Lunge(target.position - transform.position);
            iKTargetAnimator.enabled = true;
            iKTargetAnimator.SetTrigger("AttackSwipe");
            yield return new WaitForSeconds(.25f);
            iKTargetAnimator.enabled = false;
            StopAttackHitTime();
        }

    }

    void AttackHeadButt(Transform target){
        // todo: head butt attack
    }

    void AttackStomp(Transform target){
        // todo: stomp attack
    }


    public void Lunge(Vector3 direction){

        StartCoroutine(_Lunge());

        IEnumerator _Lunge()
        {
            Vector3 dir = ((direction).normalized + Vector3.up * .5f) * 1000f * entityBehavior.behaviorProfile.lungePower;
            for (int i = 0; i < 50f; ++i)
            {
                rb.AddForce(dir);
                yield return null;
            }
        }
    }
    


    // ----------------------------------------------------------

    void CheckGround(){
        Vector3 vel = rb.velocity;
        if(Physics.Raycast(groundSense.position, Vector3.down, out groundInfo, 100f, layerMask_walkable)){
            distanceFromGround = Vector3.Distance(groundInfo.point, transform.position);
            if(distanceFromGround < groundCastDistance){
                if(!GROUNDTOUCH){
                    GROUNDTOUCH = true;
                    vel.y = 0f;
                    rb.velocity = vel;
                }
                groundTime += Time.fixedDeltaTime;
                onWalkableGround = groundInfo.normal.y >= ChunkGenerator.GrassNormal - .2f;
            }
            else{
                if(GROUNDTOUCH){
                    GROUNDTOUCH = false;
                    groundTime = 0f;
                    airTime = 0f;
                }
                airTime += Time.fixedDeltaTime;
                onWalkableGround = false;
            }
        }
    }

    void CheckWall(){
        bool w = false;
        if(offWallTime > .4f){
            if(moveDir.magnitude > 0){
                if(Physics.Raycast(wallSense.position, transform.forward, out wallInfo, BASE_CASTDISTANCE_WALL) || Physics.Raycast(wallSense.position, entityOrientation.body.forward, out wallInfo, BASE_CASTDISTANCE_WALL)){
                    string tag = wallInfo.collider.gameObject.tag;
                    if(tag != "Npc" && tag != "Player" && tag != "Body"){
                        w = true;
                    }
                }
            }
        }
        
        if(w){
            WALLTOUCH = true;
        }
        else{
            if(WALLTOUCH){
                offWallTime = 0f;
                Vault();
            }
            WALLTOUCH = false;
        }
    }

    void CheckWater(){
        float y = transform.position.y;
        float waterY = ChunkGenerator.SeaLevel*ChunkGenerator.ElevationAmplitude;
        bool w = y <= waterY - .5f;
        if(w){    
            if(!IN_WATER){
                IN_WATER = true;
                //animator.SetTrigger("Water");
                entityOrientation.SetBodyRotationMode(EntityOrientation.BodyRotationMode.Target, null);
            }
            ApplyFlotationForce(waterY - y);
        }
        else if(y > waterY){
            if(IN_WATER){
                IN_WATER = false;
                offWaterTime = 0f;
                //animator.SetTrigger("Land");
                entityOrientation.SetBodyRotationMode(EntityOrientation.BodyRotationMode.Target, entityOrientation.bodyRotationTarget);
            }
        }
    }

    public void SetAnimationLayerWeight(string position, float value){
        animator.SetLayerWeight(animator.GetLayerIndex(position), value);
    }

    void CheckScrunch(){
        if(groundTime < 1f - landScrunch_recoverySpeed){
            landScrunch = Mathf.Sin(Mathf.InverseLerp(0f, 1f - landScrunch_recoverySpeed, groundTime) * Mathf.PI * 1.25f);
            float fromAirTime = Mathf.Lerp(0f, 1f, this.airTime / landScrunch_airTimeThreshhold);
            landScrunch = Mathf.Lerp(0f, Mathf.Max(fromAirTime), landScrunch);
        }else{
            landScrunch = 0f;
        }
    }


    void CheckCrouch(){

    }

    public void OnCrouchInput(){
        crouch += crouchSpeed * Time.deltaTime;
        if(crouch > 1f){
            crouch = 1f;
        }
    }



    void CheckPhysicMaterial(){
        if(moveDir.magnitude > 0f){
            //worldCollider.sharedMaterial = noFrictionMat;
            worldCollider.sharedMaterial = highFrictionMat;
        }
        else{
            worldCollider.sharedMaterial = highFrictionMat;
        }
    }

    void LimitSpeed(){

        Vector3 horvel = rb.velocity;
        float ySpeed = horvel.y;
        horvel.y = 0f;

        float max;
        if(WALLTOUCH){
            max = 0f;
            if(ySpeed > maxSpeed_climb/.5f){
                if(ySpeed > maxSpeed_climb){
                    ySpeed = maxSpeed_climb;
                }
            }
            else{
                ySpeed = maxSpeed_climb/.5f;
            }
            
        }else{
            if(IN_WATER){
                max = maxSpeed_swim;
            }
            else{
                max = sprinting ? maxSpeed_sprint : maxSpeed_run;
                if(!jumping && ySpeed > maxSpeed_climb){
                    ySpeed = maxSpeed_climb; 
                }
            }
        }
        if(!jumping){
            //max = Mathf.Lerp(max, max *= 1.5f - Mathf.InverseLerp(-3f, 3f, rb.velocity.y), 30f * Time.deltaTime);
        }

        if(horvel.magnitude > max){
            horvel = horvel.normalized * max;
            horvel.y = ySpeed;
            rb.velocity = horvel;
        }

        
    }

    void ApplyFlotationForce(float distanceFromSurface){
        rb.AddForce(Physics.gravity * 2f * (Mathf.InverseLerp(0f, 20f, distanceFromSurface) + .5f) * -1f, ForceMode.Force);
    }

    void SetGravity(){
        if((GROUNDTOUCH || WALLTOUCH) && !IN_WATER && moveDir.magnitude > 0f){
            rb.useGravity = !onWalkableGround;
        }
        else{ rb.useGravity = true; }
    }


    Vector3 GetHorizVelocity(){
        Vector3 horvel = rb.velocity;
        float ySpeed = horvel.y;
        horvel.y = 0f;
        return horvel;
    }

    public bool IsMoving(){
        return GetHorizVelocity().magnitude > .1f;
    }

    bool GroundIsClose(){

        return Physics.OverlapSphere(groundSense.position, .75f, layerMask_walkable).Length > 0;

    }


    void FixedUpdate()
    {
       ROTATION_Y_LAST = transform.rotation.y;

        CheckPhysicMaterial();
        Move(moveDir, acceleration);
        CheckGround();
        //CheckWall();
        CheckWater();
        //CheckScrunch();
        CheckCrouch();
        LimitSpeed();
        SetGravity();
    }

    void Update(){

        float dTime = Time.deltaTime;

        jumpTime += dTime;
        offWallTime += dTime;
        offWaterTime += dTime;

        if(crouch > 0f){
            crouch -= (crouchSpeed / 2f) * dTime;
        }
        else if(crouch < 0f){
            crouch = 0f;
        }

        if(Input.GetKeyUp(KeyCode.P)){
            acceleration *= 2f;
            maxSpeed_run *= 2f;
        }
        if(Input.GetKeyUp(KeyCode.O)){
            acceleration /= 2f;
            maxSpeed_run /= 2f;
        }

        if(weaponCharging){
            weaponChargeTime += dTime;
        }
        if(attackCanHit){
            //Log("Weapon can hit");
            attackHitTime += dTime;
            if(attackHitTime >= WEAPON_HITTIME_MAX){
                StopAttackHitTime();
                //Log("Weapon cannot hit");
            }
        }

        //SetHeadTarget(Camera.main.transform.position + Camera.main.transform.forward * 1000f);
        
        IKUpdate();
    }


    void OnTriggerEnter(Collider col){
        int layer = col.gameObject.layer;
        if(layer == LayerMask.NameToLayer("Feature")){
            worldCollider.sharedMaterial = noFrictionMat;
        }

        // check if crossing into own camp border
        else if(layer == LayerMask.NameToLayer("CampBorder"))
        {
            ObjectReference objectReference = col.gameObject.GetComponent<ObjectReference>();
            if(objectReference != null)
            {
                if(objectReference.GetObjectReference() == entityInfo.faction.camp)
                {
                    entityItems.OnCampBorderCross();
                }
            }
        }
        
    }
    void OnTriggerExit(Collider col){
        if(col.gameObject.layer == LayerMask.NameToLayer("Feature")){
            worldCollider.sharedMaterial = highFrictionMat;
        }
    }
}

