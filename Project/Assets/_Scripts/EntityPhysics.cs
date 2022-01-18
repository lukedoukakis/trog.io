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

    public Rigidbody rb;
    public Animator mainAnimator;
    public Animator helperAnimator;
    public Animator iKTargetAnimator;
    public Transform gyro;
    public Transform body;
    public Transform model;
    public Transform[] bodyPartTs, bodyPartTs_legs, bodyPartTs_upperBody;
    public Transform hips, head, handRight, handLeft, fingerRight, fingerLeft, footRight, footLeft, toeRight, toeLeft;
    public Transform groundSense, wallSense, waterSense, obstacleHeightSense, kneeHeightT;
    public RaycastHit groundInfo, wallInfo, waterInfo;
    public static float BASE_GROUND_DISTANCE_TO_JUMP_PLAYER = 2f;
    public static float BASE_GROUND_DISTANCE_TO_JUMP_NPC = 3f;
    public static float BASE_CASTDISTANCE_WALL = 1f;
    float groundDistanceToJump;
    float distanceFromGround;

    public static float BASE_FORCE_JUMP = 600f;
    public static float BASE_FORCE_THROW = 250f;
    public static float BASE_ACCELERATION = 40f;
    public static float BASE_MAX_SPEED = 25f;
    public static float BASE_COOLDOWN_JUMP = .15f;
    public static float BASE_TIMESTEP_ATTACK = 1f;
    public static float WEAPON_CHARGETIME_MAX = .25f;
    public static float WEAPON_HITTIME_MAX = .25f;
    public static float BASE_COOLDOWN_DODGE = .5f;
    public static float DODGE_LASTING_TIME = .5f;


    public Vector3 moveDir;
    public bool isJumping, jumpOffLeft, jumpOffRight, isSprinting, isWalking, isSquatting;
    public bool animFlag_jump, animFlag_jumpMirror;
    public bool isDodging;
    Vector3 jumpPoint;
    public float offWallTime, offWaterTime, jumpTime, airTime, groundTime;
    public float acceleration;
    public float maxSpeed_run, maxSpeed_sprint, maxSpeed_climb, maxSpeed_swim, maxSpeed_dodge, maxSpeed_walk;



    public static float landScrunch_recoverySpeed = .75f;
    public static float landScrunch_airTimeThreshhold = 1.2f;
    public float landScrunch;
    public float squatMagnitude;
    public bool handFree_right, handFree_left;
    public bool isMoving, isMoving_last;
    public bool isGrounded, isGrounded_last;
    public bool isGroundedStrict, isGroundedStrict_last;
    public bool isInsideCamp;
    public bool isHandGrab;
    public float speedLimitModifier_launch;
    public float differenceInDegreesBetweenMovementAndFacing;

    public Vector3 velHoriz_this, velHoriz_last, velHoriz_delta;
    public float changeInVelocityFactor;
    public bool GROUNDTOUCH, WALLTOUCH, isInWater;

    IEnumerator squattingCoroutine;


    // ik
    public bool ikEnabled;
    public FastIKFabric ikScript_hips, ikScript_footLeft, ikScript_footRight, ikScript_toeRight, ikScript_toeLeft, ikScript_handRight, ikScript_handLeft, ikScript_fingerRight, ikScript_fingerLeft;
    public FastIKFabric[] ikScripts_all, ikScripts_legs, ikScripts_arms, ikScripts_torso;
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
    public bool isQuadripedal;
    public bool hasFingersAndToes;
    public float runCycle_strideFrequency;
    public float runCycle_lerpTightness;
    public float runCycle_limbVerticalDisplacement;
    public float runCycle_limbForwardReachDistance;


    // grab
    public Transform grabOriginTs;
    public Transform grabOrigin_handRight;
    public Transform grabOrigin_handLeft;


    // other settings
    public float weaponChargeTime;
    public bool weaponCharging;
    public float weaponChargeAmount;
    float attackHitTime;
    public bool attackCanHit;
    public float timeSince_attack;
    public bool attackHit;
    public float dodgeTime;





    protected override void Awake()
    {

        this.fieldName = "entityPhysics";

        base.Awake();

        body = Utility.FindDeepChildWithTag(this.transform, "Body");
        model = Utility.FindDeepChildWithTag(this.transform, "Model");
        worldCollider = body.GetComponent<CapsuleCollider>();
        highFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/HighFriction");
        noFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/NoFriction");
        rb = GetComponent<Rigidbody>();
        mainAnimator = body.GetComponent<Animator>();
        helperAnimator = model.GetComponent<Animator>();
        gyro = Utility.FindDeepChild(this.transform, "Gyro");
        speedLimitModifier_launch = 1f;
        squatMagnitude = 0f;

        ikProfile = entityInfo.speciesInfo.ikProfile;
        isQuadripedal = ikProfile.quadripedal;
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
        if (hasFingersAndToes)
        {
            toeRight = Utility.FindDeepChild(body, ikProfile.name_toeRight);
            toeLeft = Utility.FindDeepChild(body, ikProfile.name_toeLeft);
            fingerRight = Utility.FindDeepChild(body, ikProfile.name_fingerRight);
            fingerLeft = Utility.FindDeepChild(body, ikProfile.name_fingerLeft);
        }

        bodyPartTs = new Transform[] { handRight, handLeft, fingerRight, fingerLeft, footRight, footLeft, toeRight, toeLeft };
        bodyPartTs_legs = new Transform[] { footRight, footLeft, toeRight, toeLeft };
        bodyPartTs_upperBody = new Transform[] { handRight, handLeft, fingerRight, fingerLeft };
        groundSense = Utility.FindDeepChild(transform, "GroundSense");
        obstacleHeightSense = Utility.FindDeepChild(transform, "ObstacleHeightSense");
        wallSense = Utility.FindDeepChild(transform, "WallSense");
        waterSense = Utility.FindDeepChild(transform, "WaterSense");
        kneeHeightT = Utility.FindDeepChild(transform, "KneeHeight");
        if (tag == "Player")
        {
            groundDistanceToJump = BASE_GROUND_DISTANCE_TO_JUMP_PLAYER;
        }
        else
        {
            groundDistanceToJump = BASE_GROUND_DISTANCE_TO_JUMP_NPC;
        }

        ikEnabled = isQuadripedal;
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
        if (hasFingersAndToes)
        {
            targetToeRight = ikParent.Find("TargetToeRight");
            targetToeLeft = ikParent.Find("TargetToeLeft");
            targetFingerRight = ikParent.Find("TargetFingerRight");
            targetFingerLeft = ikParent.Find("TargetFingerLeft");
            ikScript_toeRight = toeRight.GetComponent<FastIKFabric>();
            ikScript_toeLeft = toeLeft.GetComponent<FastIKFabric>();
            ikScript_fingerRight = fingerRight.GetComponent<FastIKFabric>(); ;
            ikScript_fingerLeft = fingerLeft.GetComponent<FastIKFabric>();
        }
        ikScripts_all = new FastIKFabric[] { ikScript_hips, ikScript_footRight, ikScript_footLeft, ikScript_toeRight, ikScript_toeLeft, ikScript_handRight, ikScript_handLeft, ikScript_fingerRight, ikScript_fingerLeft };
        ikScripts_legs = new FastIKFabric[] { ikScript_footRight, ikScript_footLeft, ikScript_toeRight, ikScript_toeLeft };
        ikScripts_arms = new FastIKFabric[] { ikScript_handRight, ikScript_handLeft, ikScript_fingerRight, ikScript_fingerLeft };
        ikScripts_torso = new FastIKFabric[] { ikScript_hips };
        iKTargetAnimator = ikParent.GetComponent<Animator>();

        acceleration = Stats.GetStatValue(entityStats.combinedStats, Stats.StatType.Agility) * BASE_ACCELERATION;
        maxSpeed_run = Stats.GetStatValue(entityStats.combinedStats, Stats.StatType.Speed) * BASE_MAX_SPEED;
        maxSpeed_sprint = maxSpeed_run * 3f;
        maxSpeed_climb = maxSpeed_run * .25f;
        maxSpeed_swim = maxSpeed_run * 1f;
        maxSpeed_dodge = maxSpeed_run * 4f;
        maxSpeed_walk = maxSpeed_run * .25f;

        plantPosFootRight = targetFootRight.position;
        plantPosFootLeft = targetFootLeft.position;
        plantPosHandRight = targetHandRight.position;
        plantPosHandLeft = targetHandLeft.position;
        if (isQuadripedal)
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

        if (!isQuadripedal)
        {
            poleHandRight = ikParent.Find("PoleHandRight");
            polePosition_handRight_underhandGrip = ikParent.Find("PolePositionHandRightUnderhand");
            polePosition_handRight_overhandGrip = ikParent.Find("PolePositionHandRightOverhand");
            polePositionTarget_handRight = polePosition_handRight_underhandGrip;

            handFree_right = handFree_left = true;
        }
        else
        {
            handFree_right = handFree_left = false;
        }

        // set grab refs
        if(!isQuadripedal)
        {
            grabOriginTs = Utility.FindDeepChild(transform, "GrabOriginPositions");
            grabOrigin_handRight = grabOriginTs.Find("HandRight");
            grabOrigin_handLeft = grabOriginTs.Find("HandLeft");
        }

        // find attackCollisionDetectors
        foreach (AttackCollisionDetector acd in GetComponentsInChildren<AttackCollisionDetector>())
        {
            acd.SetOwner(entityHandle);
        }

        // disable all self collision
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col0 in allColliders)
        {
            foreach (Collider col1 in allColliders)
            {
                Physics.IgnoreCollision(col0, col1, true);
            }
        }

        ToggleIkAll(!ikProfile.useAnimationMovement);

        if (!isQuadripedal)
        {
            UpdateIKForCarryingItems();
        }

        // if(!isQuadripedal)
        // {
        //     targetHandRight.SetParent(null);
        //     targetHandLeft.SetParent(null);
        // }

        velHoriz_this = velHoriz_last = velHoriz_delta = Vector3.zero;

        dodgeTime = 0f;

    }

    void Start()
    {

        
    }

    public void ToggleIkAll(bool value)
    {
        ToggleIk(ikScripts_all, value);
        ikEnabled = value;
        SetAnimatorEnabled(mainAnimator, !value);
    }

    public void ToggleIk(FastIKFabric[] scripts, bool value)
    {
        foreach (FastIKFabric scr in scripts)
        {
            if(scr != null)
            {
                if(scr.enabled != value)
                {
                    scr.enabled = value;
                }
            }
        }
    }

    void SetAnimatorEnabled(Animator anim, bool value)
    {
        if(anim != null)
        {
            anim.enabled = value;
        }
    }



    // check if foot is behind threshhold, if so set new plant point
    public void UpdateIK()
    {

        float dTime = Time.fixedDeltaTime;

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


        
        UpdateLimbPositions(isInWater);

        // if (!isQuadripedal)
        // {

        //     Transform t = body;

        //     isHandGrab = false;

        //     // reach up right
        //     isHandGrab = HandleHandGrab(handRight, targetHandRight, ref plantPosHandRight, ikScript_handRight, grabOrigin_handRight, (t.up).normalized, ref updateTime_handRight, handFree_right, isInWater);

        //     // reach up left
        //     isHandGrab = isHandGrab || HandleHandGrab(handLeft, targetHandLeft, ref plantPosHandLeft, ikScript_handLeft, grabOrigin_handLeft, (t.up).normalized, ref updateTime_handLeft, handFree_left, isInWater);



        //     // vault right
        //     isHandGrab = isHandGrab || HandleHandGrab(handRight, targetHandRight, ref plantPosHandRight, ikScript_handRight, grabOrigin_handRight, (t.forward).normalized, ref updateTime_handRight, handFree_right, isInWater);

        //     // vault left
        //     isHandGrab = isHandGrab || HandleHandGrab(handLeft, targetHandLeft, ref plantPosHandLeft, ikScript_handLeft, grabOrigin_handLeft, (t.forward).normalized, ref updateTime_handLeft, handFree_left, isInWater);


        // }


        if (!ikProfile.useAnimationMovement)
        {

            if(isGrounded)
            {
                if(!ikScript_footRight.enabled)
                {
                    targetFootRight.position = footRight.position;
                    targetFootLeft.position = footLeft.position;
                }
            }


            ToggleIk(ikScripts_legs, isGrounded);
            //ToggleIk(ikScripts_torso, isGrounded);


            // if on the ground or in water
            if (isGrounded || isInWater)
            {
                
                // if moving or in water
                if (isMoving || isInWater)
                {
                    
                    // check if plant points need update
                    if (updateTime_footRight >= 1f)
                    {
                        CycleFootPlantPosition(targetFootRight, basePositionFootRight, ref plantPosFootRight, ref updateTime_footRight, isInWater);
                        if (isQuadripedal && isSprinting)
                        {
                            rb.AddForce(Vector3.up * 500f);
                        }
                    }
                    if (updateTime_footLeft >= 1f)
                    {
                        CycleFootPlantPosition(targetFootLeft, basePositionFootLeft, ref plantPosFootLeft, ref updateTime_footLeft, isInWater);
                    }

                    // if quadripedal, check for hand placement update as well
                    if (isQuadripedal)
                    {
                        if (updateTime_handRight >= 1f)
                        {
                            CycleFootPlantPosition(targetHandRight, basePositionHandRight, ref plantPosHandRight, ref updateTime_handRight, isInWater);
                        }
                        if (updateTime_handLeft >= 1f)
                        {
                            CycleFootPlantPosition(targetHandLeft, basePositionHandLeft, ref plantPosHandLeft, ref updateTime_handLeft, isInWater);
                        }
                    }

                }
                else
                {

                    Vector3 footPositionOffsetToAccountForBodyLean = body.forward * (entityOrientation.bodyLean * .45f);

                    // not moving
                    SetPlantPosition(targetFootLeft, basePositionFootLeft, footPositionOffsetToAccountForBodyLean, ref plantPosFootLeft);
                    SetPlantPosition(targetFootRight, basePositionFootRight, footPositionOffsetToAccountForBodyLean, ref plantPosFootRight);
                    if (isQuadripedal)
                    {
                        SetPlantPosition(targetHandLeft, basePositionHandLeft, Vector3.zero, ref plantPosHandLeft);
                        SetPlantPosition(targetHandRight, basePositionHandRight, Vector3.zero, ref plantPosHandRight);
                    }
                }
            }

            // if in the air
            else
            {
                if (!isQuadripedal)
                {

                    // if just jumped, set foot plant point on jump point
                    if (jumpOffRight)
                    {
                        SetPlantPosition(targetFootRight, basePositionFootRight, jumpPoint - basePositionFootRight.position, ref plantPosFootRight);
                    }
                    else
                    {
                        SetPlantPosition(targetFootRight, basePositionFootRight, Vector3.up * .3f + entityOrientation.body.forward * .5f + entityOrientation.body.right * .1f, ref plantPosFootRight);
                    }
                    if (jumpOffLeft)
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
            }

            // update plant positions for accuracy
            UpdateFootPlantPosition(targetFootRight, basePositionFootRight, ref plantPosFootRight, isGrounded);
            UpdateFootPlantPosition(targetFootLeft, basePositionFootLeft, ref plantPosFootLeft, isGrounded);
            if (isQuadripedal)
            {
                UpdateFootPlantPosition(targetHandRight, basePositionHandRight, ref plantPosHandRight, isGrounded);
                UpdateFootPlantPosition(targetHandLeft, basePositionHandLeft, ref plantPosHandLeft, isGrounded);
            }
            ApplyFootPositionConstraints();

        }
        else
        {

        }


        if (!isQuadripedal)
        {
            UpdateWeaponPolePosition();
        }


        float footPlantTimeUpdateSpeed = runCycle_strideFrequency * dTime;
        float handPlantTimeUpdateSpeed = isQuadripedal ? footPlantTimeUpdateSpeed : footPlantTimeUpdateSpeed * .25f;
        float hipsUpdateSpeed = footPlantTimeUpdateSpeed / 6f;
        float updateTimeModifier = 1f + changeInVelocityFactor;
        if (isMoving)
        {
            updateTime_footRight += footPlantTimeUpdateSpeed * updateTimeModifier;
            updateTime_footLeft += footPlantTimeUpdateSpeed * updateTimeModifier;
            updateTime_handRight += handPlantTimeUpdateSpeed * updateTimeModifier;
            updateTime_handLeft += handPlantTimeUpdateSpeed * updateTimeModifier;
        }

        updateTime_hips += hipsUpdateSpeed;


    }

    public void UpdateLimbPositions(bool water)
    {

        float dTime = Time.fixedDeltaTime;

        // hips
        if (updateTime_hips > 1f)
        {
            updateTime_hips = updateTime_hips - 1f;
        }
        targetHips.position = basePositionHips.position + Vector3.up * GetRunCyclePhase(updateTime_hips, 0f) * .2f;


        // feet and toes
        float changePositionSpeed = runCycle_lerpTightness * dTime;
        Vector3 vertFootLeft, vertFootRight;
        if (isMoving)
        {
            // moving
            vertFootLeft = Vector3.up * GetRunCycleVerticality(updateTime_footLeft, water);
            vertFootRight = Vector3.up * GetRunCycleVerticality(updateTime_footRight, water);
            Vector3 toeForward = (entityOrientation.body.forward).normalized;
            if (hasFingersAndToes)
            {
                targetToeRight.position = targetFootRight.position + toeForward + Vector3.down * (GetRunCyclePhase(updateTime_footRight, 0f) + .2f);
                targetToeLeft.position = targetFootLeft.position + toeForward + Vector3.down * (GetRunCyclePhase(updateTime_footLeft, 0f) + .2f);
                if (isQuadripedal)
                {
                    targetFingerRight.position = targetFingerRight.position + toeForward + Vector3.down * (GetRunCyclePhase(updateTime_footLeft, 0f) + .2f);
                    targetFingerLeft.position = targetFingerLeft.position + toeForward + Vector3.down * (GetRunCyclePhase(updateTime_footRight, 0f) + .2f);
                }
            }


        }
        else
        {
            // not moving
            vertFootLeft = vertFootRight = Vector3.up * GetRunCycleVerticality(.65f, water);
            if (hasFingersAndToes)
            {
                targetToeRight.position = targetFootRight.position + entityOrientation.body.forward.normalized + Vector3.down;
                targetToeLeft.position = targetFootLeft.position + entityOrientation.body.forward.normalized + Vector3.down;
                if (isQuadripedal)
                {
                    targetFingerRight.position = targetHandRight.position + entityOrientation.body.forward.normalized + Vector3.down;
                    targetFingerLeft.position = targetHandLeft.position + entityOrientation.body.forward.normalized + Vector3.down;
                }
            }


        }
        targetFootRight.position = Vector3.Lerp(targetFootRight.position, plantPosFootRight, changePositionSpeed) + vertFootRight;
        targetFootLeft.position = Vector3.Lerp(targetFootLeft.position, plantPosFootLeft, changePositionSpeed) + vertFootLeft;
        if (isQuadripedal)
        {
            targetHandRight.position = Vector3.Lerp(targetHandRight.position, plantPosHandRight, changePositionSpeed) + vertFootLeft;
            targetHandLeft.position = Vector3.Lerp(targetHandLeft.position, plantPosHandLeft, changePositionSpeed) + vertFootRight;
        }


    }

    float GetRunCycleVerticality(float updateTime, bool water)
    {
        float verticalityBase = water ? .006f : .015f;
        if(isDodging)
        {
            verticalityBase *= 3f;
        }
        verticalityBase *= runCycle_limbVerticalDisplacement;
        return (verticalityBase + .025f * Mathf.InverseLerp(0f, 2f, rb.velocity.y)) * Mathf.Pow(GetRunCyclePhase(updateTime, 0f), 1f);
    }

    // .5 is stance phase, -.5 is swing phase
    float GetRunCyclePhase(float updateTime, float offset)
    {
        return Mathf.Cos(updateTime * 2f * Mathf.PI + offset) + 1;
    }



    public void CycleFootPlantPosition(Transform targetIk, Transform baseTransform, ref Vector3 plantPositionPointer, ref float updateTime, bool water)
    {

        float forwardReachDistance = water ? 0f : runCycle_limbForwardReachDistance * ((velHoriz_this.magnitude / 8f) + (entityOrientation.bodyLean * .3f)) * Mathf.Max(.5f, (1f - changeInVelocityFactor));
        plantPositionPointer = baseTransform.position + velHoriz_this.normalized * 2.2f * forwardReachDistance;
        updateTime = Mathf.Max(updateTime, 1f) - 1f;
    }
    public void UpdateFootPlantPosition(Transform targetIk, Transform baseTransform, ref Vector3 plantPositionPointer, bool onGround)
    {
        // move plantPos down until hits walkable surface
        if (onGround)
        {
            RaycastHit hit;
            if (Physics.Raycast(plantPositionPointer, Vector3.down, out hit, 100f, LayerMaskController.WALKABLE))
            {
                plantPositionPointer.y = hit.point.y + distanceFromGround;
            }
        }
        Vector3 pos = targetIk.position;
        pos.y = Mathf.Max(pos.y, baseTransform.position.y);
        targetIk.position = pos;
    }

    public void ApplyFootPositionConstraints()
    {
        Vector3 pos;

        pos = targetFootRight.position;
        pos.y = Mathf.Min(pos.y, kneeHeightT.position.y);
        targetFootRight.position = pos;


        pos = targetFootLeft.position;
        pos.y = Mathf.Min(pos.y, kneeHeightT.position.y);
        targetFootLeft.position = pos;



        if (isQuadripedal)
        {
            pos = targetHandRight.position;
            pos.y = Mathf.Min(pos.y, kneeHeightT.position.y);
            targetHandRight.position = pos;

            pos = targetHandLeft.position;
            pos.y = Mathf.Min(pos.y, kneeHeightT.position.y);
            targetHandLeft.position = pos;
        }
    }


    public bool HandleHandGrab(Transform hand, Transform targetIk, ref Vector3 plantPositionPointer, FastIKFabric ikScript, Transform lookOriginT, Vector3 lookDirection, ref float updateTime, bool handFree, bool water)
    {

        

        if (!handFree || !isJumping)
        {
            return false;
        }


        // if plant pos out of range, set hand plant position to the nearest nearby obstacle
        float dTime = Time.fixedDeltaTime;
        float handSpeed = 20f * dTime;
        float armsReachDistance = 1f;
        bool needsUpdate =
            Vector3.Distance(plantPositionPointer, lookOriginT.position) > armsReachDistance
            //isJumping
            //|| body.transform.InverseTransformDirection(plantPositionPointer - referencePosition).z < 0f
            ;
   
    
        if (needsUpdate)
        {
            
            RaycastHit[] hits = Physics.SphereCastAll(lookOriginT.position, .5f, lookDirection, armsReachDistance, LayerMaskController.SWINGABLE, QueryTriggerInteraction.Ignore).Where(hit => hit.point.y > 0f).ToArray();
            if (hits.Length > 0f)
            {
                
                ikScript.enabled = true;
    
                float min = float.MaxValue;
                Vector3 targetHandPos = Vector3.zero;
                Collider foundCollider = hits[0].collider;
                foreach (RaycastHit hit in hits)
                {
                    float distance = Vector3.Distance(lookOriginT.position, hit.point);
                    if (distance < min)
                    {
                        min = distance;
                        targetHandPos = hit.point;
                        foundCollider = hit.collider;
                    }
                }
                
                plantPositionPointer = targetHandPos;
                targetIk.position = hand.position;
                targetIk.position = Vector3.Lerp(targetIk.position, plantPositionPointer, handSpeed);




            }
            else
            {
                ikScript.enabled = false;
                return false;
            }
        }
        else
        {
            targetIk.position = Vector3.Lerp(targetIk.position, plantPositionPointer, handSpeed);
            Vector3 distanceVector = plantPositionPointer - lookOriginT.position;
            Vector3 swingDirection = distanceVector;
            swingDirection.y = Mathf.Max(0f, swingDirection.y);
            swingDirection.y *= 20f;
            swingDirection.y *= Mathf.Pow(Mathf.InverseLerp(.2f, armsReachDistance, distanceVector.magnitude), 5f);
            rb.AddForce(swingDirection * 50f, ForceMode.Acceleration);
            
        }

        return true;



    }



    public void SetPlantPosition(Transform targetIk, Transform targetTransform, Vector3 offset, ref Vector3 positionPointer)
    {
        Vector3 pos = targetTransform.position + offset;
        RaycastHit hit;
        if (Physics.Raycast(pos, Vector3.up, out hit, 1f, LayerMaskController.WALKABLE))
        {
            positionPointer = hit.point;
        }
        else
        {
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
        float dTime = Time.fixedDeltaTime;
        float speed = 30f * dTime;

        poleHandRight.position = Vector3.Lerp(poleHandRight.position, polePositionTarget_handRight.position, speed);
    }

    public void UpdateIKForCarryingItems()
    {

        if (isQuadripedal)
        {
            return;
        }

        GameObject weaponObject = entityItems.weaponEquipped_object;
        GameObject holdingObject = entityItems.holding_object;


        // right hand
        if (weaponObject != null && !isSquatting)
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
        if (holdingObject != null)
        {
            ikScript_handLeft.enabled = true;
            ikScript_handLeft.Target = holdingObject.transform.Find("IKTargetT_Left");
            handFree_left = false;
        }
        else
        {

            // if hand is free, support right hand with holding the weapon, if equipped
            if (!handFree_right && entityItems.weaponEquipped_item.holdStyle.Equals(Item.ItemHoldStyle.Axe) && !isSquatting)
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

    void UpdateAnimation()
    {

        // main animation
        if (ikProfile.useAnimationMovement)
        {
            if (mainAnimator != null)
            {

                mainAnimator.SetLayerWeight(1, 1f - differenceInDegreesBetweenMovementAndFacing);
                mainAnimator.SetLayerWeight(2, differenceInDegreesBetweenMovementAndFacing);
                mainAnimator.SetLayerWeight(6, Mathf.Max(squatMagnitude, landScrunch));
                // if(squatMagnitude > 0f)
                // {
                //     Debug.Log(squatMagnitude);
                // }
                

                if (isInWater)
                {
                    mainAnimator.SetBool("Stand", false);
                    mainAnimator.SetBool("Run", false);
                    mainAnimator.SetBool("Sprint", false);
                    mainAnimator.SetBool("Swim", true);
                }
                else
                {
                    mainAnimator.SetBool("Swim", false);
                    // grounded
                    if (isGroundedStrict && !isJumping)
                    {
                        if (!isGroundedStrict_last)
                        {
                            mainAnimator.SetTrigger("Land");
                        }

                        if (isMoving)
                        {
                            if (isSprinting)
                            {
                                mainAnimator.SetBool("Sprint", true);
                                mainAnimator.SetBool("Run", false);
                            }
                            else
                            {
                                mainAnimator.SetBool("Run", true);
                                mainAnimator.SetBool("Sprint", false);
                            }

                            mainAnimator.SetBool("Stand", false);
                        }
                        else
                        {
                            mainAnimator.SetBool("Stand", true);
                            mainAnimator.SetBool("Run", false);
                            mainAnimator.SetBool("Sprint", false);
                        }
                    }

                    if (isGroundedStrict && !isGroundedStrict_last)
                    {
                        mainAnimator.SetTrigger("Fall");
                    }

                    if (animFlag_jump)
                    {
                        mainAnimator.SetBool("Stand", false);
                        mainAnimator.SetBool("Run", false);
                        mainAnimator.SetBool("Sprint", false);
                        mainAnimator.SetBool("JumpMirror", animFlag_jumpMirror);
                        mainAnimator.SetTrigger("JumpTrigger");
                    }


                    mainAnimator.SetLayerWeight(8, Mathf.Min(1f, (weaponChargeTime / WEAPON_CHARGETIME_MAX)) * .25f);

                }
            }


        }

        // helper animation
        if (helperAnimator != null)
        {
            if (helperAnimator.enabled)
            {
                float twistRight = Mathf.Min(1f, (weaponChargeTime / WEAPON_CHARGETIME_MAX));
                helperAnimator.SetLayerWeight(1, Mathf.Min(1f, (weaponChargeTime / WEAPON_CHARGETIME_MAX)) * .25f);
            }

        }

        animFlag_jump = false;

    }


    public void Move(Vector3 direction, float speed)
    {
        float speedStat = speed * Stats.GetStatValue(entityStats.combinedStats, Stats.StatType.Speed);
        SetIsSprinting();
        SetIsWalking();
        Vector3 move = transform.TransformDirection(direction).normalized * speedStat;
        rb.AddForce(move * speedStat, ForceMode.Force);

    }

    void SetIsSprinting()
    {
        if(isLocalPlayer)
        {

            isSprinting = false;

            //isSprinting = entityUserInput.pressSprint;
        }
        else
        {

            isSprinting = false;

            // isSprinting = entityBehavior.urgent;
            // if(entityInfo.isFactionFollower)
            // {
            //     isSprinting = isSprinting || entityInfo.faction.leaderHandle.entityPhysics.isSprinting;
            // }
        }
    }

    public void SetIsWalking()
    {
        if(isLocalPlayer)
        {
            isWalking = entityUserInput.pressWalk;
        }
        else
        {
           isWalking = isInsideCamp && entityInfo.faction.leaderInCamp;
           if(entityBehavior.activeAction != null)
           {
               isWalking = isWalking && !entityBehavior.activeAction.urgent;
           }
        }
    }


    public void AssertStanding()
    {
        ToggleSquat(false);
    }

    public void AssertSquatting()
    {
        ToggleSquat(true);
    }

    public void ToggleSquat(bool targetValue)
    {
        if(isSquatting != targetValue)
        {
            ToggleSquat();
            UpdateIKForCarryingItems();
        }
    }

    public void ToggleSquat()
    {

        isSquatting = !isSquatting;
        //Debug.Log(" Setting isSquatting: " + isSquatting);

        if(squattingCoroutine != null)
        {
            StopCoroutine(squattingCoroutine);
        }
        squattingCoroutine = _ToggleSquat(isSquatting);
        StartCoroutine(squattingCoroutine);

    }

    IEnumerator _ToggleSquat(bool value)
    {

        float targetMagnitude = value ? 1f : 0f;

        while(Mathf.Abs(squatMagnitude - targetMagnitude) < .1f)
        {
            squatMagnitude = Mathf.Lerp(squatMagnitude, targetMagnitude, 5f * Time.deltaTime);
            yield return null;
        }
        squatMagnitude = targetMagnitude;
            
    }

    public void TryDodge()
    {
        if(dodgeTime >= BASE_COOLDOWN_DODGE)
        {
            Dodge();
        }
    }

    public void Dodge()
    {
        StartCoroutine(_Dodge());

        IEnumerator _Dodge()
        {
            dodgeTime = 0f;
            isDodging = true;
            Vector3 forceDirection = velHoriz_this.normalized;
            Vector3 bodyDirection;
            if(isMoving)
            {
                bodyDirection = Vector3.Cross(velHoriz_this, Vector3.up) * -1f;
            }
            else
            {
                bodyDirection = Vector3.Cross(transform.right, Vector3.up) * -1f;
            }
            for(int i = 0 ; i < DODGE_LASTING_TIME * 100f; ++i)
            {
                rb.AddForce(Vector3.down * 1f, ForceMode.VelocityChange);
                bodyDirection = Vector3.Cross(velHoriz_this, Vector3.up) * -1f;
                entityOrientation.body.RotateAround(obstacleHeightSense.position, bodyDirection, (360f / 100f) * 1.5f);
                yield return new WaitForSecondsRealtime(.01f);
            }
            isDodging = false;
        }
    }

    public void Jump(float power)
    {
        if (!isJumping)
        {
            StartCoroutine(_Jump(power));
        }
    }
    public void Jump()
    {
        Jump(BASE_FORCE_JUMP);
    }
    IEnumerator _Jump(float power)
    {
        animFlag_jump = true;
        animFlag_jumpMirror = !animFlag_jumpMirror;
        isJumping = true;
        Vector3 horVelDir = velHoriz_this.normalized;
        Vector3 rightFootForwardNess = Vector3.Project(footRight.position - transform.position, horVelDir);
        Vector3 leftFootForwardness = Vector3.Project(footRight.position - transform.position, horVelDir);
        if (rightFootForwardNess.magnitude > leftFootForwardness.magnitude)
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


        yield return new WaitForSecondsRealtime(BASE_COOLDOWN_JUMP);

        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;
        Vector3 direction = Vector3.up;
        jumpTime = 0;
        groundTime = 0;
        float tFinal = 20f;
        for (float t = Mathf.Round(tFinal * .4f), f = 0; t <= tFinal; ++t)
        {
            if (f < 10f)
            {
                rb.AddForce(direction * power, ForceMode.Force);
            }
            if (isHandGrab && isMoving)
            {
                speedLimitModifier_launch = 1f + (Mathf.Sin(t / tFinal * Mathf.PI) * 20f);
            }
            ++f;
            yield return new WaitForSecondsRealtime(.01f);


        }
        speedLimitModifier_launch = 1f;

        //yield return new WaitForSecondsRealtime(.2f);
        jumpOffRight = jumpOffLeft = false;
        isJumping = false;
        yield return null;
    }
    public void Vault()
    {
        Jump(BASE_FORCE_JUMP * .7f);
    }

    public bool CanJump()
    {

        return false;

        if (!entityInfo.speciesInfo.behaviorProfile.canJump)
        {
            return false;
        }


        if (Physics.Raycast(groundSense.position, Vector3.down, groundDistanceToJump, LayerMaskController.WALKABLE))
        {
            return true;
        }


        return false;
    }

    public void SetHeadTarget(Vector3 position)
    {
        head.rotation = Quaternion.LookRotation(position, Vector3.up);
    }


    // -------------

    // attacking

    public void ThrowWeapon(bool pointTowardsDirection)
    {

        StartCoroutine(_LaunchProjectile());

        IEnumerator _LaunchProjectile()
        {
            Projectile projectile = Projectile.InstantiateProjectile(entityItems.weaponEquipped_item, entityItems.weaponEquipped_object, entityStats.combinedStats);
            AttackCollisionDetector acd = projectile.worldObject.transform.Find("HitZone").GetComponent<AttackCollisionDetector>();
            acd.SetOwner(entityHandle);
            acd.SetProjectile(projectile);
            Utility.ToggleObjectPhysics(projectile.worldObject, true, true, true, true);
            Utility.IgnorePhysicsCollisions(projectile.worldObject.transform, gameObject.GetComponentInChildren<Collider>());
            entityItems.SetUpdateWeaponOrientation(false);

            Vector3 throwDir = entityOrientation.body.forward + (Vector3.up * .25f);
            Rigidbody projectileRb = projectile.worldObject.GetComponent<Rigidbody>();
            //projectileRb.centerOfMass = Vector3.up * .622f;
            //projectileRb.angularDrag = 5f;
            float throwTime = .5f;
            float addforceTime = .2f;
            float force = BASE_FORCE_THROW * Mathf.Lerp(1f, 1.5f, Mathf.InverseLerp(0f, BASE_MAX_SPEED, velHoriz_this.magnitude));
            projectileRb.velocity = rb.velocity;

            entityItems.weaponEquipped_item = null;
            entityItems.weaponEquipped_object = null;
            entityItems.SetUpdateWeaponOrientation(true);
            entityItems.OnItemsChange();

            for (int i = 0; i < throwTime * 100f; ++i)
            {
                if (i < addforceTime * 100f)
                {
                    projectileRb.AddForce(throwDir * force);
                }
                yield return new WaitForFixedUpdate();
            }

            


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
                    entityPhysics.Attack(AttackType.Weapon, null, 0f);
                    timeSince_attack = 0f;
                }
            }
        }
    }


    public void TryAttack(AttackType attackType, Transform guaranteedHitTarget, float guaranteedHitTargetDelay)
    {
        if(CalculateTimeUntilCanAttack() <= 0f)
        {
            Attack(attackType, guaranteedHitTarget, guaranteedHitTargetDelay);
        }
    }

    public void Attack(AttackType attackType, Transform guaranteedHitTarget, float guaranteedHitTargetDelay)
    {

        switch (attackType)
        {
            case (AttackType.Weapon):
                OnWeaponAttack(guaranteedHitTarget, guaranteedHitTargetDelay);
                break;
            case (AttackType.Bite):
                AttackBite(guaranteedHitTarget);
                break;
            case (AttackType.Swipe):
                AttackSwipe(guaranteedHitTarget);
                break;
            case (AttackType.HeadButt):
                AttackHeadButt(guaranteedHitTarget);
                break;
            case (AttackType.Stomp):
                AttackStomp(guaranteedHitTarget);
                break;
        }

    }

    void OnWeaponAttack(Transform guaranteedHitTarget, float guaranteedHitTargetDelay)
    {
        Item weapon = entityItems.weaponEquipped_item;

        if(weapon == null){ return; }

        bool ranged = entityItems.rangedMode;
        bool charge = (weaponChargeTime <= 0f);
        if (weaponChargeTime <= 0f)
        {
            OnWeaponChargeBegin(weapon, ranged);
        }
        else
        {
            OnWeaponRelease(weapon, ranged, guaranteedHitTarget, guaranteedHitTargetDelay);
            timeSince_attack = 0f;
            //AssertWeaponChargedStatus(false);
        }
        StartWeaponAttackAnimation(charge, weapon, ranged);



    }
    void OnWeaponChargeBegin(Item weapon, bool ranged)
    {
        weaponCharging = true;
        weaponChargeAmount = .001f;
        //entityOrientation.SetBodyRotationMode(BodyRotationMode.Target, null);
    }
    void OnWeaponRelease(Item weapon, bool ranged, Transform guaranteedHitTarget, float guaranteedHitTargetDelay)
    {
        weaponCharging = false;
        weaponChargeAmount = Mathf.InverseLerp(0f, WEAPON_CHARGETIME_MAX, weaponChargeTime);
        weaponChargeTime = 0f;
        if (!ranged)
        {
            if(guaranteedHitTarget != null)
            {
                Collider col = guaranteedHitTarget.GetComponentInChildren<Collider>();
                if (col == null){ col = guaranteedHitTarget.GetComponentInParent<Collider>(); }

                StartCoroutine(OnAttackHit(col, guaranteedHitTarget.position, null, .25f));
            }
            else
            {
                BeginMeleeAttackHitTime();
            }


            // Vector3 lungeDir = IsMoving() ? velHoriz_this : transform.forward;
            // Lunge(lungeDir);
        }
        else
        {
            if (weapon.holdStyle.Equals(Item.ItemHoldStyle.Spear))
            {
                // if ranged mode and using spear, launch the weapon
                ThrowWeapon(true);
            }
        }
    
    }

    void StartWeaponAttackAnimation(bool charging, Item weapon, bool ranged)
    {
        if (weapon == null) { return; }

        if (weapon.holdStyle.Equals(Item.ItemHoldStyle.Spear))
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
        else if (weapon.holdStyle.Equals(Item.ItemHoldStyle.Axe))
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

    void BeginMeleeAttackHitTime()
    {
        attackHitTime = 0f;
        attackCanHit = true;
    }
    void StopMeleeAttackHitTime()
    {
        attackCanHit = false;
        if (attackHit)
        {
            attackHit = false;
        }
        if (isLocalPlayer)
        {
            entityOrientation.SetBodyRotationMode(BodyRotationMode.Normal, null);
        }
        else
        {
            GameObject targetedWorldObject = entityBehavior.activeAction.targetedWorldObject;
            if(targetedWorldObject != null)
            {
                //entityBehavior.EquipOptimalWeaponForTarget(entityBehavior.activeAction);
                entityOrientation.SetBodyRotationMode(entityBehavior.activeAction.bodyRotationMode, entityBehavior.activeAction.targetedWorldObject.transform);
            }
            else
            {
                entityOrientation.SetBodyRotationMode(BodyRotationMode.Normal, null);
            }
        }
    }
    public IEnumerator OnAttackHit(Collider collider, Vector3 hitPoint, Projectile projectile, float delay)
    {
        GameObject hitObject = collider.gameObject;
        EntityStats hitObjectStats = null;
        //Debug.Log(hitObject.layer);
        
        if (LayerMaskController.HITTABLE == (LayerMaskController.HITTABLE | (1 << hitObject.layer)))
        {
            EntityHitDetection ehd = collider.gameObject.GetComponentInParent<EntityHitDetection>();
            if(ehd != null)
            {
                yield return new WaitForSecondsRealtime(delay);
                ehd.OnHit(this.entityHandle, hitPoint, projectile, false);
                hitObjectStats = ehd.entityStats;
            }
            else
            {
                ItemHitDetection ihd = collider.gameObject.GetComponentInParent<ItemHitDetection>();
                if (ihd != null)
                {
                    yield return new WaitForSecondsRealtime(delay);
                    ihd.OnHit(this.entityHandle, hitPoint, projectile);
                    hitObjectStats = ihd.stats;
                }
            }

            // apply fixed weapon position effect if applicable
            if(isLocalPlayer)
            {
                if (entityItems != null)
                {
                    //StartCoroutine(FixWeaponPosition(entityItems.weaponEquipped_object, collider.transform, .45f));
                }
            }
        

            StopMeleeAttackHitTime();
        }
       
        // if faction leader and hit object is still not destroyed, call members to attack the same object
        if(entityInfo.isFactionLeader)
        {
            if(hitObjectStats != null)
            {
                if(hitObjectStats.health > 0)
                {
                    ActionParameters ap = ActionParameters.GenerateActionParameters(null, ActionType.Chase, hitObject, Vector3.zero, -1, null, null, -1, EntityBehavior.DISTANCE_THRESHOLD_SAME_SPOT, BodyRotationMode.Target, InterruptionTier.Anything, true, null, entityBehavior.entityActionSequence_AssertStanding, null);
                    entityInfo.faction.SendPartyCommandToAll(ap);
                }
            }
        }

        yield return null;
    }


    IEnumerator FixWeaponPosition(GameObject weapon, Transform targetT, float time)
    {

        Rigidbody rbTarget = targetT.gameObject.GetComponentInParent<Rigidbody>();
        Rigidbody rbWeapon = weapon.GetComponent<Rigidbody>();
        if (rbTarget == null)
        {
            rbTarget = targetT.gameObject.AddComponent<Rigidbody>();
            rbTarget.isKinematic = true;
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

        Transform handleT_rightHand = weapon.transform.Find("IKTargetT_Right");


        float maxDistance;
        if(handFree_left)
        {
            Transform handleT_leftHand = weapon.transform.Find("IKTargetT_Left");
            maxDistance = Mathf.Min(Vector3.Distance(handRight.position, handleT_rightHand.position), Vector3.Distance(handLeft.position, handleT_leftHand.position));
        }
        else
        {
            maxDistance = Vector3.Distance(handRight.position, handleT_rightHand.position);
        }

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        while (timer.ElapsedMilliseconds / 1000f < time && maxDistance < .5f && targetT != null)
        {
            if (handFree_left)
            {
                Transform handleT_leftHand = weapon.transform.Find("IKTargetT_Left");
                maxDistance = Mathf.Min(Vector3.Distance(handRight.position, handleT_rightHand.position), Vector3.Distance(handLeft.position, handleT_leftHand.position));
            }
            else
            {
                maxDistance = Vector3.Distance(handRight.position, handleT_rightHand.position);
            }
            yield return null;
        }
        timer.Stop();
        Destroy(j);
        entityItems.ToggleItemOrientationUpdate(true);
    }


    public void OnWeaponHitRemove()
    {

    }

    void AttackBite(Transform target)
    {
        // todo: bite attack
        timeSince_attack = 0f;
    }

    void AttackSwipe(Transform target)
    {
        // todo: swipe attack
        StartCoroutine(_AttackSwipe());

        IEnumerator _AttackSwipe()
        {
            BeginMeleeAttackHitTime();
            timeSince_attack = 0f;
            Vector3 lungeDirection = target != null ? target.position - transform.position : velHoriz_this;
            Lunge(Utility.GetHorizontalVector(lungeDirection));
            iKTargetAnimator.enabled = true;
            iKTargetAnimator.SetTrigger("AttackSwipe");
            yield return new WaitForSeconds(.7f);
            iKTargetAnimator.enabled = false;
            StopMeleeAttackHitTime();
        }

    }

    void AttackHeadButt(Transform target)
    {
        // todo: head butt attack
        timeSince_attack = 0f;
    }

    void AttackStomp(Transform target)
    {
        // todo: stomp attack
        timeSince_attack = 0f;
    }


    public void Lunge(Vector3 direction)
    {

        StartCoroutine(_Lunge());

        IEnumerator _Lunge()
        {
            Vector3 dir = ((direction).normalized + Vector3.up * .5f) * 200f * entityBehavior.behaviorProfile.lungePower;
            for (int i = 0; i < 10f; ++i)
            {
                rb.AddForce(dir, ForceMode.Acceleration);
                yield return new WaitForSecondsRealtime(.01f);
            }
        }
    }



    // ----------------------------------------------------------


    void CheckWall()
    {
        bool w = false;
        if (offWallTime > .4f)
        {
            if (moveDir.magnitude > 0)
            {
                if (Physics.Raycast(wallSense.position, transform.forward, out wallInfo, BASE_CASTDISTANCE_WALL) || Physics.Raycast(wallSense.position, entityOrientation.body.forward, out wallInfo, BASE_CASTDISTANCE_WALL))
                {
                    string tag = wallInfo.collider.gameObject.tag;
                    if (tag != "Npc" && tag != "Player" && tag != "Body")
                    {
                        w = true;
                    }
                }
            }
        }

        if (w)
        {
            WALLTOUCH = true;
        }
        else
        {
            if (WALLTOUCH)
            {
                offWallTime = 0f;
                Vault();
            }
            WALLTOUCH = false;
        }
    }

    void CheckWater()
    {
        float y = transform.position.y;
        float waterY = ChunkGenerator.SeaLevel * ChunkGenerator.ElevationAmplitude;
        bool w = y <= waterY - .2f;
        if (w)
        {
            if (!isInWater)
            {
                isInWater = true;
                //animator.SetTrigger("Water");
                //entityOrientation.SetBodyRotationMode(EntityOrientation.BodyRotationMode.Target, null);
            }
            ApplyFlotationForce(waterY - y);
        }
        else
        {
            if (isInWater)
            {
                isInWater = false;
                offWaterTime = 0f;
                //animator.SetTrigger("Land");
                //entityOrientation.SetBodyRotationMode(EntityOrientation.BodyRotationMode.Target, entityOrientation.bodyRotationTarget);
            }
        }
    }

    public void SetAnimationLayerWeight(string position, float value)
    {
        mainAnimator.SetLayerWeight(mainAnimator.GetLayerIndex(position), value);
    }

    void CheckScrunch()
    {
        if (groundTime < 1f - landScrunch_recoverySpeed)
        {
            landScrunch = Mathf.Sin(Mathf.InverseLerp(0f, 1f - landScrunch_recoverySpeed, groundTime) * Mathf.PI * 1f) * .5f;
            //float fromAirTime = Mathf.Lerp(0f, 1f, this.airTime / landScrunch_airTimeThreshhold);
            //landScrunch = Mathf.Lerp(0f, Mathf.Max(fromAirTime), landScrunch);
        }
        else
        {
            landScrunch = 0f;
        }
    }




    void LimitSpeed()
    {


        Vector3 horvel = rb.velocity;
        float ySpeed = horvel.y;
        horvel.y = 0f;

        float max;

        if (isInWater)
        {
            max = maxSpeed_swim;
        }
        else
        {

            if(isSprinting)
            {
                max = maxSpeed_sprint;
            }
            else if(isWalking)
            {
                max = maxSpeed_walk;
            }
            else
            {
                max = maxSpeed_run;
            }

            if(isDodging)
            {
                max = Mathf.Lerp(max, maxSpeed_dodge, Mathf.Sin((dodgeTime / 60f * 100f) * Mathf.PI));
            }
            
        }

        if (!isJumping)
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

    void ApplyFlotationForce(float distanceFromSurface)
    {
        rb.AddForce(Physics.gravity * 2f * (Mathf.InverseLerp(0f, 20f, distanceFromSurface) + .5f) * -1f, ForceMode.Force);
    }

    void SetPhysicMaterial()
    {


        bool lowFrictionCondition =
            moveDir.magnitude > 0f
            ;


        worldCollider.sharedMaterial = lowFrictionCondition ? noFrictionMat : highFrictionMat;
        
    }

    void SetGravity()
    {

        bool gravityCondition =
            true
            ;

        rb.useGravity = gravityCondition;
    }


    public Vector3 GetHorizVelocity()
    {
        Vector3 horvel = rb.velocity;
        float ySpeed = horvel.y;
        horvel.y = 0f;
        return horvel;
    }

    public bool IsMoving()
    {
        return velHoriz_this.magnitude > .3f;
    }

    bool IsGrounded()
    {
        return Physics.OverlapSphere(groundSense.position, 1f, LayerMaskController.WALKABLE).Length > 0;
    }

    bool IsGroundedStrict()
    {
        return Physics.OverlapSphere(groundSense.position, .5f, LayerMaskController.WALKABLE).Length > 0;
    }

    public void OnCampBorderEnter()
    {
        //Debug.Log("OnCampBorderCross()");
        isInsideCamp = true;
        entityItems.EmptyInventory();
        if(entityInfo.isFactionLeader)
        {
            entityInfo.faction.UpdateLeaderCampStatus();
        }
        if(isLocalPlayer)
        {
            CameraController.instance.SetBakedCameraDistanceSmooth(CameraController.CAMERA_DISTANCE_INSIDECAMP, CameraController.CAMERA_ZOOM_SPEED_CAMPTRANSITION);
            CameraController.instance.SetLockVerticalCameraMovement(false, CameraController.CAMERA_LOCK_VERTICALITY_INSIDECAMP);
        }
        // todo: command tribe memebrs to line up to orientations
    }

    public void OnCampBorderExit()
    {
        //Debug.Log("OnCampBorderCross()");
        isInsideCamp = false;
        if(entityInfo.isFactionLeader)
        {
            entityInfo.faction.UpdateLeaderCampStatus();
        }
        if(isLocalPlayer)
        {
            CameraController.instance.SetBakedCameraDistanceSmooth(CameraController.CAMERA_DISTANCE_OUTSIDECAMP, CameraController.CAMERA_ZOOM_SPEED_CAMPTRANSITION * .25f);
            CameraController.instance.SetLockVerticalCameraMovement(false, CameraController.CAMERA_LOCK_VERTICALITY_OUTSIDECAMP);
        }
        // todo: command tribe memebrs to follow
    }

    public float CalculateAttackCooldownTime()
    {
        return BASE_TIMESTEP_ATTACK * entityStats.combinedStats.attackSpeed;
    }

    public float CalculateTimeUntilCanAttack()
    {
        return CalculateAttackCooldownTime() - timeSince_attack;
    }


    void FixedUpdate()
    {

        velHoriz_this = GetHorizVelocity();
        velHoriz_delta = velHoriz_this - velHoriz_last;
        changeInVelocityFactor = Mathf.InverseLerp(0f, .35f, velHoriz_delta.magnitude);
        isMoving = IsMoving();
        isGrounded = IsGrounded();
        isGroundedStrict = IsGroundedStrict();
        differenceInDegreesBetweenMovementAndFacing = Mathf.InverseLerp(0f, 180f, Vector3.Angle(Utility.GetHorizontalVector(body.forward), velHoriz_this));

        SetPhysicMaterial();
        Move(moveDir, acceleration);
        CheckWater();
        CheckScrunch();
        LimitSpeed();
        SetGravity();
        UpdateAnimation();
        UpdateIK();

        velHoriz_last = velHoriz_this;
        isMoving_last = isMoving;
        isGrounded_last = isGrounded;
        isGroundedStrict_last = isGroundedStrict;
    }

    void Update()
    {

        float dTime = Time.deltaTime;

        jumpTime += dTime;
        offWallTime += dTime;
        offWaterTime += dTime;
        dodgeTime += dTime;
        groundTime = isGrounded ? groundTime += dTime : 0;
        timeSince_attack += Time.deltaTime;

        if (Input.GetKeyUp(KeyCode.P))
        {
            acceleration *= 2f;
            maxSpeed_run *= 2f;
        }

        if (Input.GetKeyUp(KeyCode.O))
        {
            acceleration /= 2f;
            maxSpeed_run /= 2f;
        }

        if (weaponCharging)
        {
            weaponChargeTime += dTime;
        }
        if (attackCanHit)
        {
            //Log("Weapon can hit");
            attackHitTime += dTime;
            if (attackHitTime >= WEAPON_HITTIME_MAX)
            {
                StopMeleeAttackHitTime();
                //Log("Weapon cannot hit");
            }
        }

        // if (isLocalPlayer)
        // {
        //     SetHeadTarget((Camera.main.transform.position + Camera.main.transform.right * 1000f) + (transform.forward * 500f));
        // }


    }



    void OnTriggerEnter(Collider col)
    {
        int layer = col.gameObject.layer;
        if (layer == LayerMask.NameToLayer("Feature"))
        {
            worldCollider.sharedMaterial = noFrictionMat;
        }

        // check if crossing into own camp border
        else if (layer == LayerMask.NameToLayer("CampBorder"))
        {
            ObjectReference objectReference = col.gameObject.GetComponent<ObjectReference>();
            if (objectReference != null)
            {
                if (System.Object.ReferenceEquals(objectReference.GetObjectReference(), entityInfo.faction.camp))
                {
                    //Debug.Log("Border Enter");
                    OnCampBorderEnter();
                }
            }
        }

    }
    void OnTriggerExit(Collider col)
    {
        int layer = col.gameObject.layer;
        if (layer == LayerMask.NameToLayer("Feature"))
        {
            worldCollider.sharedMaterial = highFrictionMat;
        }
        else if (layer == LayerMask.NameToLayer("CampBorder"))
        {
            ObjectReference objectReference = col.gameObject.GetComponent<ObjectReference>();
            if (objectReference != null)
            {
                if (System.Object.ReferenceEquals(objectReference.GetObjectReference(), entityInfo.faction.camp))
                {
                    //Debug.Log("Border Exit");
                    OnCampBorderExit();
                }
            }
        }
    }
}

