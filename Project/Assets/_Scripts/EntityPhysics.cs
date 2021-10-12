using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DitzelGames.FastIK;
using System.Linq;

public class EntityPhysics : EntityComponent
{

    public Collider worldCollider;
    public PhysicMaterial highFrictionMat;
    public PhysicMaterial noFrictionMat;
    public LayerMask layerMask_water;
    public LayerMask layerMask_walkable;
    public LayerMask layerMask_feature;

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

    public static float BASE_FORCE_JUMP = 280f;
    public static float BASE_FORCE_THROW = 400f;
    public static float BASE_ACCELERATION = 40f;
    public static float BASE_MAX_SPEED = 20f;
    public static float BASE_COOLDOWN_JUMP = .15f;
    public static float WEAPON_CHARGETIME_MAX = 3f;
    public static float WEAPON_HITTIME_MAX = .25f;


    public Vector3 moveDir;
    public bool onWalkableGround;
    public bool jumping, jumpOffLeft, jumpOffRight, sprinting;
    Vector3 jumpPoint;
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
    public bool handGrab;
    public float speedLimitModifier_launch;


    public Vector3 velHoriz_this, velHoriz_last, velHoriz_delta;
    public float changeInVelocityFactor;
    public bool GROUNDTOUCH, WALLTOUCH, IN_WATER;


    // ik
    public bool ikEnabled;
    public FastIKFabric ikScript_hips, ikScript_footLeft, ikScript_footRight, ikScript_toeRight, ikScript_toeLeft, ikScript_handRight, ikScript_handLeft, ikScript_fingerRight, ikScript_fingerLeft;
    public FastIKFabric[] ikScripts, ikScripts_legs, ikScripts_upperBody;
    public Transform ikParent;
    public Transform basePositionHips, basePositionFootRight, basePositionFootLeft, basePositionHandRight, basePositionHandLeft;
    public Transform targetHips, targetFootRight, targetFootLeft, targetToeRight, targetToeLeft, targetHandRight, targetHandLeft, targetFingerRight, targetFingerLeft;
    public Vector3 plantPosFootRight, plantPosFootLeft, plantPosHandRight, plantPosHandLeft;
    public Transform poleHandRight;
    public Transform polePosition_handRight_underhandGrip, polePosition_handRight_overhandGrip;
    public Transform polePositionTarget_handRight;
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
    public bool meleeAttackCanHit;
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
        layerMask_feature = LayerMask.GetMask("Feature");
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        gyro = Utility.FindDeepChild(this.transform, "Gyro");
        speedLimitModifier_launch = 1f;

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

        acceleration = Stats.GetStatValue(entityStats.statsCombined, Stats.StatType.Agility) * BASE_ACCELERATION;
        maxSpeed_run = Stats.GetStatValue(entityStats.statsCombined, Stats.StatType.Speed) * BASE_MAX_SPEED;
        maxSpeed_sprint = maxSpeed_run * 1.5f;
        maxSpeed_climb = maxSpeed_run * .25f;
        maxSpeed_swim = maxSpeed_run * .75f;

        plantPosFootRight = targetFootRight.position;
        plantPosFootLeft = targetFootLeft.position;
        plantPosHandRight = targetHandRight.position;
        plantPosHandLeft = targetHandLeft.position;
        if(quadripedal)
        {
            updateTime_footRight = .25f;
            updateTime_footLeft = 0f;
            updateTime_handRight = .5f;
            updateTime_handLeft = .75f;
            updateTime_hips = 0f;
        }
        else
        {
            updateTime_footRight = 0f;
            updateTime_footLeft = .5f;
            updateTime_handRight = .5f;
            updateTime_handLeft = 0f;
            updateTime_hips = 0f;
        }
        
        if(!quadripedal){
            poleHandRight = ikParent.Find("PoleHandRight");
            polePosition_handRight_underhandGrip = ikParent.Find("PolePositionHandRightUnderhand");
            polePosition_handRight_overhandGrip = ikParent.Find("PolePositionHandRightOverhand");
            polePositionTarget_handRight = polePosition_handRight_underhandGrip;

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

        if(!quadripedal)
        {
            UpdateIKForCarryingItems();
        }

        velHoriz_this = velHoriz_last = velHoriz_delta = Vector3.zero;
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
    public void IKUpdate()
    {

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

            if(!quadripedal)
            {
                handGrab = HandleHandGrab(handRight, targetHandRight, basePositionHandRight, ref plantPosHandRight, ikScript_handRight, (body.right + body.forward).normalized, ref updateTime_handRight, handFree_right, IN_WATER);
                handGrab = handGrab || HandleHandGrab(handLeft, targetHandLeft, basePositionHandLeft, ref plantPosHandLeft, ikScript_handLeft, ((body.right * -1f) + body.forward).normalized, ref updateTime_handLeft, handFree_left, IN_WATER);
            }


            // if on the ground
            if (groundIsClose || IN_WATER)
            {

                if (moving || IN_WATER)
                {
                    // moving

                    // check if plant points need update
                    if (updateTime_footRight >= 1f)
                    {
                        CycleFootPlantPosition(targetFootRight, basePositionFootRight, ref plantPosFootRight, ref updateTime_footRight, IN_WATER);
                        if (quadripedal)
                        {
                            rb.AddForce(Vector3.up * 500f);
                        }
                    }
                    if (updateTime_footLeft >= 1f)
                    {
                        CycleFootPlantPosition(targetFootLeft, basePositionFootLeft, ref plantPosFootLeft, ref updateTime_footLeft, IN_WATER);
                    }

                    // if quadripedal, check for hand placement update as well
                    if(quadripedal){
                        if (updateTime_handRight >= 1f)
                        {
                            CycleFootPlantPosition(targetHandRight, basePositionHandRight, ref plantPosHandRight, ref updateTime_handRight, IN_WATER);
                        }
                        if (updateTime_handLeft >= 1f)
                        {
                            CycleFootPlantPosition(targetHandLeft, basePositionHandLeft, ref plantPosHandLeft, ref updateTime_handLeft, IN_WATER);
                        }
                    }

                }
                else
                {
                    // not moving
                    SetPlantPosition(targetFootLeft, basePositionFootLeft, Vector3.zero, ref plantPosFootLeft);
                    SetPlantPosition(targetFootRight, basePositionFootRight, velHoriz_this.normalized * -.2f, ref plantPosFootRight);
                    if(quadripedal){
                        SetPlantPosition(targetHandLeft, basePositionHandLeft, Vector3.zero, ref plantPosHandLeft);
                        SetPlantPosition(targetHandRight, basePositionHandRight, velHoriz_this.normalized * -.2f, ref plantPosHandRight);
                    }
                }
            }

            // in the air
            else
            {
                if (!quadripedal)
                {
                    // if jumping, set foot plant point on jump point
                    if (jumpOffRight && groundIsClose)
                    {
                        SetPlantPosition(targetFootRight, basePositionFootRight, jumpPoint - basePositionFootRight.position, ref plantPosFootRight);
                    }
                    else
                    {
                        SetPlantPosition(targetFootRight, basePositionFootRight, Vector3.up * .3f + entityOrientation.body.forward * .5f + entityOrientation.body.right * .1f, ref plantPosFootRight);
                    }
                    if (jumpOffLeft && groundIsClose)
                    {
                        SetPlantPosition(targetFootLeft, basePositionFootLeft, jumpPoint - basePositionFootLeft.position, ref plantPosFootLeft);
                    }
                    else
                    {
                        SetPlantPosition(targetFootLeft, basePositionFootLeft, Vector3.up * .1f + entityOrientation.body.right * 0f, ref plantPosFootLeft);
                    }

                    updateTime_footRight = .2f;
                    updateTime_footLeft = .7f;
                }
                
                // if(quadripedal){
                //     SetPlantPosition(targetHandLeft, basePositionHandLeft, Vector3.up * .1f + entityOrientation.body.right * 0f, ref plantPosHandLeft);
                //     SetPlantPosition(targetHandRight, basePositionHandRight, Vector3.up * .3f + entityOrientation.body.forward * .5f + entityOrientation.body.right * .1f, ref plantPosHandRight);
                //     updateTime_handRight = .2f;
                //     updateTime_handLeft = .7f;
                // }
            }

            // update plant positions for accuracy
            UpdateFootPlantPosition(targetFootRight, basePositionFootRight, ref plantPosFootRight, groundIsClose);
            UpdateFootPlantPosition(targetFootLeft, basePositionFootLeft, ref plantPosFootLeft, groundIsClose);
            if(quadripedal){
                UpdateFootPlantPosition(targetHandRight, basePositionHandRight, ref plantPosHandRight, groundIsClose);
                UpdateFootPlantPosition(targetHandLeft, basePositionHandLeft, ref plantPosHandLeft, groundIsClose);
            }
            ApplyFootPositionConstraints();

        }
        else{
            if(!IN_WATER){
                ToggleIK(true);
                IKUpdate();
                if (!quadripedal)
                {
                    UpdateWeaponPolePosition();
                }
            }
        }

        if(!quadripedal)
        {
            UpdateWeaponPolePosition();
        }


        float footPlantTimeUpdateSpeed = runCycle_strideFrequency * Time.deltaTime;
        float handPlantTimeUpdateSpeed = quadripedal ? footPlantTimeUpdateSpeed : footPlantTimeUpdateSpeed * .25f;
        float hipsUpdateSpeed = footPlantTimeUpdateSpeed / 6f;
        float updateTimeModifier = 1f + changeInVelocityFactor;
        if(IsMoving())
        {
            updateTime_footRight += footPlantTimeUpdateSpeed * updateTimeModifier;
            updateTime_footLeft += footPlantTimeUpdateSpeed * updateTimeModifier;
            updateTime_handRight += handPlantTimeUpdateSpeed * updateTimeModifier;
            updateTime_handLeft += handPlantTimeUpdateSpeed * updateTimeModifier;
        }
        
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



    public void CycleFootPlantPosition(Transform targetIk, Transform baseTransform, ref Vector3 plantPositionPointer, ref float updateTime, bool water){

        float forwardReachDistance = water ? 0f : runCycle_limbForwardReachDistance * ((velHoriz_this.magnitude / maxSpeed_sprint) + (entityOrientation.bodyLean * .3f)) * Mathf.Max(.5f, (1f - changeInVelocityFactor));
        plantPositionPointer = baseTransform.position + velHoriz_this.normalized * 2.2f * forwardReachDistance;
        updateTime = Mathf.Max(updateTime, 1f) - 1f;
    }
    public void UpdateFootPlantPosition(Transform targetIk, Transform baseTransform, ref Vector3 plantPositionPointer, bool onGround){
        // move plantPos down until hits terrain
        if (onGround)
        {
            RaycastHit hit;
            if (Physics.Raycast(plantPositionPointer, Vector3.down, out hit, 100f, layerMask_walkable))
            {
                plantPositionPointer.y = hit.point.y + distanceFromGround;
            }
        }
        Vector3 pos = targetIk.position;
        pos.y = Mathf.Max(pos.y, baseTransform.position.y);
        targetIk.position = pos;
    }

    public void ApplyFootPositionConstraints(){
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


    public bool HandleHandGrab(Transform hand, Transform targetIk, Transform baseTransform, ref Vector3 plantPositionPointer, FastIKFabric ikScript, Vector3 directionToLook, ref float updateTime, bool handFree, bool water)
    {

        //Debug.Log("Handling hand plant pos");

        if(!handFree){ return false; }

        Vector3 referencePosition = obstacleHeightSense.position;
        float handSpeed = 150000f * Time.deltaTime;


        // if plant pos out of range, set hand plant position to the nearest nearby obstacle
        bool needsUpdate =
            Vector3.Distance(plantPositionPointer, referencePosition) > 1f
            || plantPositionPointer.y < referencePosition.y
            //|| body.transform.InverseTransformDirection(plantPositionPointer - referencePosition).z < 0f
            ;

        if (needsUpdate)
        {

            //Debug.Log("needs update");

            RaycastHit[] hits = Physics.SphereCastAll(referencePosition + Vector3.up * .25f, .5f, directionToLook, .75f, layerMask_feature, QueryTriggerInteraction.Ignore).Where(hit => hit.point.y >= referencePosition.y).ToArray();
            //Debug.Log("hits: " + hits.Length);
            
            if (hits.Length > 0f)
            {
                ikScript.enabled = true;

                float min = float.MaxValue;
                Vector3 targetHandPos = Vector3.zero;
                foreach (RaycastHit hit in hits)
                {
                    //Debug.Log("..." + hit.collider.gameObject.name);
                    float distance = Vector3.Distance(referencePosition, hit.point);
                    if (distance < min && hit.point.y >= referencePosition.y)
                    {
                        min = distance;
                        targetHandPos = hit.point;
                    }
                }

                //Debug.Log("DISTANCE: " + Vector3.Distance(referencePosition, targetHandPos));
                plantPositionPointer = targetHandPos;
                targetIk.position = Vector3.Lerp(targetIk.position, plantPositionPointer, handSpeed);

                if(isLocalPlayer && entityUserInput.pressJump && CanJump())
                {
                    //rb.AddForce((targetHandPos - referencePosition) * 500f);
                    rb.AddForce(Vector3.up * (targetHandPos - referencePosition).y * 500f);
                }


                
            }
            else
            {
                ikScript.enabled = false;
                return false;
            }   
        }
        else{
            targetIk.position = Vector3.Lerp(targetIk.position, plantPositionPointer, handSpeed);
        }

        return true;
        

    
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

    public void UpdateWeaponPoleTarget()
    {

        //Debug.Log("updating weapon pole target");

        Item weapon = entityItems.weaponEquipped_item;
        if (weapon.holdStyle.Equals(Item.ItemHoldStyle.Spear))
        {
            if (entityItems.rangedMode)
            {
                polePositionTarget_handRight = polePosition_handRight_underhandGrip;
            }
            else
            {
                polePositionTarget_handRight = polePosition_handRight_overhandGrip;
            }
        }
        else if (weapon.holdStyle.Equals(Item.ItemHoldStyle.Axe))
        {
            polePositionTarget_handRight = polePosition_handRight_underhandGrip;
        }
    }

    public void UpdateWeaponPolePosition()
    {
        float speed = 30f * Time.deltaTime;

        poleHandRight.position = Vector3.Lerp(poleHandRight.position, polePositionTarget_handRight.position, speed);
    }

    public void UpdateIKForCarryingItems(){

        if(quadripedal){
            return;
        }

        GameObject weaponObject = entityItems.weaponEquipped_object;
        GameObject holdingObject = entityItems.holding_object;


        // right hand
        if(weaponObject != null)
        {
            ikScript_handRight.enabled = true;
            handFree_right = false;
            ikScript_handRight.Target = weaponObject.transform.Find("IKTargetT_Right");
            UpdateWeaponPoleTarget();
        }
        else
        {
            ikScript_handRight.enabled = false;
            ikScript_handRight.Target = ikParent.Find("TargetHandRight");
            handFree_right = true;
        }
        
        // left hand
        if(holdingObject != null)
        {
            ikScript_handLeft.enabled = true;
            ikScript_handLeft.Target = holdingObject.transform.Find("IKTargetT_Left");
            handFree_left = false;
        }
        else
        {

            // if hand is free, support right hand with holding the weapon, if equipped
            if(!handFree_right && entityItems.weaponEquipped_item.holdStyle.Equals(Item.ItemHoldStyle.Axe))
            {
                ikScript_handLeft.enabled = true;
                ikScript_handLeft.Target = weaponObject.transform.Find("IKTargetT_Left");
                handFree_left = false;
            }
            else
            {
                ikScript_handLeft.enabled = false;
                ikScript_handLeft.Target = ikParent.transform.Find("TargetHandLeft");
                handFree_left = true;
            }
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
        Vector3 horVelDir = velHoriz_this.normalized;
        Vector3 rightFootForwardNess = Vector3.Project(footRight.position - transform.position, horVelDir);
        Vector3 leftFootForwardness = Vector3.Project(footRight.position - transform.position, horVelDir);
        if(rightFootForwardNess.magnitude > leftFootForwardness.magnitude)
        {
            jumpOffRight = true;
            jumpOffLeft = false;
            jumpPoint = footRight.position;
        }
        else
        {
            jumpOffRight = true;
            jumpOffLeft = false;
            jumpPoint = footLeft.position;
        }

        if(groundTime <= BASE_COOLDOWN_JUMP){
            float t = BASE_COOLDOWN_JUMP - groundTime;
            yield return new WaitForSecondsRealtime(t);
        }
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;
        Vector3 direction = Vector3.up;
        //rb.AddForce(direction * power, ForceMode.Force);
        jumpTime = 0;
        groundTime = 0;
        bool moving = IsMoving();
        float tFinal = 20f;
        for (float t = Mathf.Round(tFinal * .4f), f = 0; t <= tFinal; ++t)
        {
            if (f < 10f)
            {
                rb.AddForce(direction * power, ForceMode.Force);
            }
            if (handGrab && moving)
            {
                speedLimitModifier_launch = 1f + (Mathf.Sin(t / tFinal * Mathf.PI) * 20f);
            }
            ++f;
            yield return new WaitForSecondsRealtime(.01f);
            

        }
        speedLimitModifier_launch = 1f;
        
        //yield return new WaitForSecondsRealtime(.2f);
        jumpOffRight = jumpOffLeft = false;
        jumping = false;
        yield return null;
    }
    public void Vault(){
        Jump(BASE_FORCE_JUMP*.7f);
    }

    public bool CanJump(){

        if(!entityInfo.speciesInfo.behaviorProfile.canJump)
        {
            return false;
        }

        if(Physics.Raycast(groundSense.position, Vector3.down, groundCastDistance + .3f, layerMask_walkable) || (handGrab && rb.velocity.y < 0f)){
            return true;
        }

        return false;
    }

    public void SetHeadTarget(Vector3 position){
        head.rotation = Quaternion.LookRotation(position, Vector3.up);
    }


    // -------------

    // attacking

    public void ThrowWeapon(bool pointTowardsDirection){

        StartCoroutine(_LaunchProjectile());

        IEnumerator _LaunchProjectile()
        {
            Projectile projectile = Projectile.InstantiateProjectile(entityItems.weaponEquipped_item, entityItems.weaponEquipped_object, entityStats.statsCombined);
            AttackCollisionDetector acd =  projectile.worldObject.transform.Find("HitZone").GetComponent<AttackCollisionDetector>();
            acd.SetOwner(entityHandle);
            acd.SetProjectile(projectile);
            Utility.ToggleObjectPhysics(projectile.worldObject, true, true, true, true);
            Utility.IgnorePhysicsCollisions(projectile.worldObject.transform, gameObject.GetComponentInChildren<Collider>());
            entityItems.SetUpdateWeaponOrientation(false);

            Vector3 throwDir = entityOrientation.body.forward + (Vector3.up * .25f);
            Rigidbody projectileRb = projectile.worldObject.GetComponent<Rigidbody>();
            projectileRb.centerOfMass = Vector3.up * .622f;
            projectileRb.angularDrag = 5f;
            float throwTime = .5f;
            float addforceTime = .2f;
            float force = BASE_FORCE_THROW * Mathf.Lerp(1f, 1.5f, Mathf.InverseLerp(0f, BASE_MAX_SPEED, velHoriz_this.magnitude));
            projectileRb.velocity = rb.velocity;

            for(int i = 0; i < throwTime * 100f; ++i)
            {
                if(i < addforceTime * 100f)
                {
                    projectileRb.AddForce(throwDir * force);
                }
                yield return new WaitForFixedUpdate();
            }

            entityItems.weaponEquipped_item = null;
            entityItems.weaponEquipped_object = null;
            entityItems.SetUpdateWeaponOrientation(true);
            entityItems.OnItemsChange();


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
            ThrowWeapon(true);
        }
    }

    void StartAttackAnimation(bool charging, Item weapon, bool ranged)
    {
        if(weapon == null){ return; }

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
        meleeAttackCanHit = true;
    }
    void StopAttackHitTime(){
        meleeAttackCanHit = false;
        if(attackHit){
            attackHit = false;
        }
    }
    public void OnAttackHit(Collider collider, Vector3 hitPoint, Projectile projectile){
        GameObject hitObject = collider.gameObject;
        //Debug.Log(hitObject.layer);

        if(hitObject.layer == LayerMask.NameToLayer("Creature") || hitObject.layer == LayerMask.NameToLayer("Feature"))
        {
            EntityHitDetection ehd = collider.gameObject.GetComponentInParent<EntityHitDetection>();
            //Log("HIT!!!! " + collider.gameObject.name);
            ehd.OnHit(this.entityHandle, hitPoint, projectile, false);

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
                ihd.OnHit(this.entityHandle, hitPoint, projectile);
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
        while(timer.ElapsedMilliseconds / 1000f < time && Vector3.Distance(handRight.position, handleT.position) < .5f){
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
            Vector3 lungeDirection = target != null ? target.position - transform.position : velHoriz_this;
            Lunge(Utility.GetHorizontalVector(lungeDirection));
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
            Vector3 dir = ((direction).normalized + Vector3.up * .5f) * 200f * entityBehavior.behaviorProfile.lungePower;
            for (int i = 0; i < 25f; ++i)
            {
                rb.AddForce(dir);
                yield return new WaitForSecondsRealtime(.01f);
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
        //rb.drag = GroundIsClose() ? 10f : 0f;
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
                //entityOrientation.SetBodyRotationMode(EntityOrientation.BodyRotationMode.Target, null);
            }
            ApplyFlotationForce(waterY - y);
        }
        else if(y > waterY){
            if(IN_WATER){
                IN_WATER = false;
                offWaterTime = 0f;
                //animator.SetTrigger("Land");
                //entityOrientation.SetBodyRotationMode(EntityOrientation.BodyRotationMode.Target, entityOrientation.bodyRotationTarget);
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
            //worldCollider.sharedMaterial = noFrictionMat;
            worldCollider.sharedMaterial = highFrictionMat;
        }
    }

    void LimitSpeed(){


        Vector3 horvel = rb.velocity;
        float ySpeed = horvel.y;
        horvel.y = 0f;

        float max;

        if (IN_WATER)
        {
            max = maxSpeed_swim;
        }
        else
        {
            max = sprinting ? maxSpeed_sprint : maxSpeed_run;
        }

        if (!jumping)
        {
            max = Mathf.Lerp(max, max *= 1.5f - Mathf.InverseLerp(-3f, 3f, rb.velocity.y), 30f * Time.deltaTime);
        }

        max *= speedLimitModifier_launch;

        if (horvel.magnitude > max)
        {
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
            rb.useGravity = !GroundIsClose();
        }
        else{ rb.useGravity = true; }
    }


    public Vector3 GetHorizVelocity(){
        Vector3 horvel = rb.velocity;
        float ySpeed = horvel.y;
        horvel.y = 0f;
        return horvel;
    }

    public bool IsMoving(){
        return velHoriz_this.magnitude > .2f;
    }

    bool GroundIsClose(){

        return Physics.OverlapSphere(groundSense.position, .25f, layerMask_walkable).Length > 0;

    }


    void FixedUpdate()
    {

        velHoriz_this = GetHorizVelocity();
        velHoriz_delta = velHoriz_this - velHoriz_last;
        changeInVelocityFactor = Mathf.InverseLerp(0f, .35f, velHoriz_delta.magnitude);
        
        CheckPhysicMaterial();
        Move(moveDir, acceleration);
        CheckGround();
        //CheckWall();
        CheckWater();
        //CheckScrunch();
        CheckCrouch();
        LimitSpeed();
        SetGravity();

        velHoriz_last = velHoriz_this;
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
        if(meleeAttackCanHit){
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
                if(System.Object.ReferenceEquals(objectReference.GetObjectReference(), entityInfo.faction.camp))
                //if(objectReference.GetObjectReference() == entityInfo.faction.camp)
                {
                    //Debug.Log("Border Enter");
                    entityItems.OnCampBorderEnter();
                    if(isLocalPlayer)
                    {
                        PlayerCommand.current.SendCommand("Go Home");
                    }
                }
            }
        }
        
    }
    void OnTriggerExit(Collider col){
        int layer = col.gameObject.layer;
        if(layer == LayerMask.NameToLayer("Feature")){
            worldCollider.sharedMaterial = highFrictionMat;
        }
        else if(layer == LayerMask.NameToLayer("CampBorder"))
        {
            ObjectReference objectReference = col.gameObject.GetComponent<ObjectReference>();
            if(objectReference != null)
            {
                if(System.Object.ReferenceEquals(objectReference.GetObjectReference(), entityInfo.faction.camp))
                //if(objectReference.GetObjectReference() == entityInfo.faction.camp)
                {
                    //Debug.Log("Border Exit");
                    entityItems.OnCampBorderExit();
                    if(isLocalPlayer)
                    {
                        PlayerCommand.current.SendCommand("Follow Player");
                    }
                }
            }
        }
    }
}

