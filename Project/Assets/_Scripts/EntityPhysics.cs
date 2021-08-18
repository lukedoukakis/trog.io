using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DitzelGames.FastIK;

public class EntityPhysics : EntityComponent
{

    public Collider hitbox;
    public Collider obstacleHitbox;
    public PhysicMaterial highFrictionMat;
    public PhysicMaterial noFrictionMat;
    public LayerMask layerMask_water;
    public LayerMask layerMask_walkable;

    public Rigidbody rb;
    public Transform gyro;
    public Transform[] bodyPartTs, bodyPartTs_legs, bodyPartTs_upperBody;
    public Transform hips, head, handRight, handLeft, footRight, footLeft, toeRight, toeLeft;
    public Transform groundSense, wallSense, waterSense, obstacleHeightSense, kneeHeightT;
    public RaycastHit groundInfo, wallInfo, waterInfo;
    public static float groundCastDistance_player = .1f;
    public static float groundCastDistance_npc = .1f;
    public static float groundCastDistance_far = 100f;
    public static float wallCastDistance = 1f;
    float groundCastDistance;
    public static float JumpForce = 2800f;
    public static float ThrowForce = 200f;
    public static float AccelerationScale = 30f;
    public static float MaxSpeedScale = 15f;
    public static float JumpCoolDown = .15f;


    public Vector3 moveDir;
    public bool jumping, jumpOpposite, sprinting;
    public float offWallTime, offWaterTime, jumpTime, airTime, groundTime;
    public float acceleration;
    public float maxSpeed_run, maxSpeed_sprint, maxSpeed_climb, maxSpeed_swim;


    public static float landScrunch_recoverySpeed = .75f;
    public static float landScrunch_airTimeThreshhold = 1.2f;
    public float landScrunch;
    public bool handFree_right, handFree_left;


    public float ROTATION_Y_THIS;
    public float ROTATION_Y_LAST;
    public bool GROUNDTOUCH, WALLTOUCH, IN_WATER;


    // ik
    public bool ikEnabled;
    public FastIKFabric ikScript_hips, ikScript_head, ikScript_footLeft, ikScript_footRight, ikScript_toeRight, ikScript_toeLeft, ikScript_handRight, ikScript_handLeft;
    public FastIKFabric[] ikScripts, ikScripts_legs, ikScripts_upperBody;
    public Transform ikParent;
    public Transform basePositionHips, basePositionHead, basePositionFootRight, basePositionFootLeft, basePositionHandRight, basePositionHandLeft;
    public Transform targetHips, targetHead, targetFootRight, targetFootLeft, targetToeRight, targetToeLeft, targetHandRight, targetHandLeft;
    public Vector3 plantPosFootLeft, plantPosFootRight;
    public float updateTime_footRight, updateTime_footLeft, updateTime_hips;




    protected override void Awake(){

        base.Awake();

        hitbox = transform.Find("HumanIK").GetComponent<CapsuleCollider>();
        obstacleHitbox = transform.Find("HumanIK").GetComponent<BoxCollider>();
        highFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/HighFriction");
        noFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/NoFriction");
        layerMask_water = LayerMask.GetMask("Water");
        layerMask_walkable = LayerMask.GetMask("Terrain", "Feature");
        rb = GetComponent<Rigidbody>();
        gyro = Utility.FindDeepChild(this.transform, "Gyro");

        hips = Utility.FindDeepChild(this.transform, "B-hips");
        head = Utility.FindDeepChild(this.transform, "B-head");
        handRight = Utility.FindDeepChild(this.transform, "B-palm_01_R");
        handLeft = Utility.FindDeepChild(this.transform, "B-palm_01_L");
        footRight = Utility.FindDeepChild(this.transform, "B-foot_R");
        footLeft = Utility.FindDeepChild(this.transform, "B-foot_L");
        toeRight = Utility.FindDeepChild(this.transform, "B-toe_R");
        toeLeft = Utility.FindDeepChild(this.transform, "B-toe_L");
        bodyPartTs = new Transform[]{ handRight, handLeft, footRight, footLeft, toeRight, toeLeft };
        bodyPartTs_legs = new Transform[]{ footRight, footLeft, toeRight, toeLeft };
        bodyPartTs_upperBody = new Transform[]{ handRight, handLeft };
        groundSense = Utility.FindDeepChild(transform, "GroundSense");
        obstacleHeightSense = Utility.FindDeepChild(transform, "ObstacleHeightSense");
        wallSense = Utility.FindDeepChild(transform, "WallSense");
        waterSense = Utility.FindDeepChild(transform, "WaterSense");
        kneeHeightT = Utility.FindDeepChild(transform, "KneeHeight");
        if(tag == "Player"){
            groundCastDistance = groundCastDistance_player;
        }else if(tag == "Npc"){
            groundCastDistance = groundCastDistance_npc;
        }

        ikEnabled = true;
        ikParent = Utility.FindDeepChild(transform, "IKTargets");
        basePositionHips = ikParent.Find("BasePositionHips");
        basePositionHead = ikParent.Find("BasePositionHead");
        basePositionFootRight = ikParent.Find("BasePositionFootRight");
        basePositionFootLeft = ikParent.Find("BasePositionFootLeft");
        targetHips = ikParent.Find("TargetHips");
        targetHead = ikParent.Find("TargetHead");
        targetFootRight = ikParent.Find("TargetFootRight");
        targetFootLeft = ikParent.Find("TargetFootLeft");
        targetToeRight = ikParent.Find("TargetToeRight");
        targetToeLeft = ikParent.Find("TargetToeLeft");
        targetHandRight = ikParent.Find("TargetHandRight");
        targetHandLeft = ikParent.Find("TargetHandLeft");
        basePositionHandRight = ikParent.Find("BasePositionHandRight");
        basePositionHandLeft = ikParent.Find("BasePositionHandLeft");
        ikScript_hips = hips.GetComponent<FastIKFabric>();
        ikScript_head = head.GetComponent<FastIKFabric>();
        ikScript_footRight = footRight.GetComponent<FastIKFabric>();
        ikScript_footLeft = footLeft.GetComponent<FastIKFabric>();
        ikScript_toeRight = toeRight.GetComponent<FastIKFabric>();
        ikScript_toeLeft = toeLeft.GetComponent<FastIKFabric>();
        ikScript_handRight = handRight.GetComponent<FastIKFabric>();
        ikScript_handLeft = handLeft.GetComponent<FastIKFabric>();
        ikScripts = new FastIKFabric[]{ ikScript_footRight, ikScript_footLeft, ikScript_toeRight, ikScript_toeLeft, ikScript_handRight, ikScript_handLeft };
        ikScripts_legs = new FastIKFabric[]{ ikScript_footRight, ikScript_footLeft, ikScript_toeRight, ikScript_toeLeft };
        ikScripts_upperBody = new FastIKFabric[]{ ikScript_handRight, ikScript_handLeft };

        //targetFootRight.SetParent(GameObject.Find("Global Object").transform);
        //targetFootLeft.SetParent(GameObject.Find("Global Object").transform);

    }

    void Start(){
        acceleration = entityStats.GetStat("speed") * AccelerationScale;
        maxSpeed_run = entityStats.GetStat("speed") * MaxSpeedScale;
        maxSpeed_sprint = maxSpeed_run * 1.5f;
        maxSpeed_climb = maxSpeed_run * .25f;
        maxSpeed_swim = maxSpeed_run * .75f;


        plantPosFootRight = targetFootRight.position;
        plantPosFootLeft = targetFootLeft.position;
        updateTime_footRight = 0f;
        updateTime_footLeft = .5f;
        updateTime_hips = 0f;

        handFree_right = handFree_left = true;
        UpdateIKForCarryingItems();
    }


    // check if foot is behind threshhold, if so set new plant point
    public void IKUpdate(){
        if (ikEnabled)
        {

            UpdateLimbPositions();


            bool moving = IsMoving();
            bool groundIsClose = GroundIsClose();

            if (groundIsClose)
            {
                // on ground
                if (moving)
                {
                    // check if plant points need update
                    if (updateTime_footRight >= 1f)
                    {
                        CycleFootPlantPosition(targetFootRight, basePositionFootRight, ref plantPosFootRight, ref updateTime_footRight);
                    }
                    if (updateTime_footLeft >= 1f)
                    {
                        CycleFootPlantPosition(targetFootLeft, basePositionFootLeft, ref plantPosFootLeft, ref updateTime_footLeft);
                    }
                }
                else
                {
                    SetPlantPosition(targetFootLeft, basePositionFootLeft, Vector3.zero, ref plantPosFootLeft);
                    SetPlantPosition(targetFootRight, basePositionFootRight, entityAnimation.bodyT.forward * -.2f, ref plantPosFootRight);
                }
            }
            else
            {
                // not on ground
                SetPlantPosition(targetFootLeft, basePositionFootLeft, Vector3.up * -.2f + entityAnimation.bodyT.forward * 0f + entityAnimation.bodyT.right * 0f, ref plantPosFootLeft);
                SetPlantPosition(targetFootRight, basePositionFootRight, Vector3.up * .6f + entityAnimation.bodyT.forward * .5f + entityAnimation.bodyT.right * .1f, ref plantPosFootRight);
                updateTime_footRight = .2f;
                updateTime_footLeft = .7f;
            }
            AdjustFootPlantPosition(targetFootRight, basePositionFootRight, ref plantPosFootRight, groundIsClose);
            AdjustFootPlantPosition(targetFootLeft, basePositionFootLeft, ref plantPosFootLeft, groundIsClose);
            ApplyFootPositionRestraints();

        }
        
        float footPlantTimeUpdateSpeed = 3f * Time.deltaTime;
        float hipsUpdateSpeed = footPlantTimeUpdateSpeed / 2f;
        updateTime_footRight += footPlantTimeUpdateSpeed;
        updateTime_footLeft += footPlantTimeUpdateSpeed;
        updateTime_hips += hipsUpdateSpeed;


    }

    public void UpdateLimbPositions(){

        // hips
        if(updateTime_hips > 1f){
            updateTime_hips = updateTime_hips - 1f;
        }
        targetHips.position = basePositionHips.position + Vector3.up * GetRunCyclePhase(updateTime_hips, 0f) * .1f;


        // feet and toes
        float changePositionSpeed = 5f  * Time.deltaTime;
        Vector3 vertLeft, vertRight;
        if(IsMoving()){
            // moving
            float vertMin = Mathf.Lerp(GetRunCycleVerticality(.5f), GetRunCycleVerticality(.75f), Mathf.InverseLerp(0f, 5f, rb.velocity.y));
            vertLeft = Vector3.up * Mathf.Max(vertMin, GetRunCycleVerticality(updateTime_footLeft));
            vertRight = Vector3.up * Mathf.Max(vertMin, GetRunCycleVerticality(updateTime_footRight));
            // targetToeRight.position = targetFootRight.position + entityAnimation.bodyT.forward * .2f + Vector3.down * ((GetRunCyclePhase(updateTime_footRight, -5f) + 1) / 2f + .2f);
            // targetToeLeft.position = targetFootLeft.position + entityAnimation.bodyT.forward * .2f + Vector3.down * ((GetRunCyclePhase(updateTime_footLeft, -5f) + 1) / 2f + .2f);
        }
        else{
            // not moving
            vertLeft = vertRight = Vector3.zero;
        }
        targetFootRight.position = Vector3.Lerp(targetFootRight.position, plantPosFootRight, changePositionSpeed) + vertRight;
        targetFootLeft.position = Vector3.Lerp(targetFootLeft.position, plantPosFootLeft, changePositionSpeed) + vertLeft;
        targetToeRight.position = targetFootRight.position + entityAnimation.bodyT.forward + Vector3.down;
        targetToeLeft.position = targetFootLeft.position + entityAnimation.bodyT.forward + Vector3.down;

    


        float GetRunCycleVerticality(float updateTime){
            return .018f * Mathf.Pow(GetRunCyclePhase(updateTime, 0f), 1f);
        }

        // .5 is stance phase, -.5 is swing phase
        float GetRunCyclePhase(float updateTime, float offset){
            return Mathf.Cos(updateTime * 2f * Mathf.PI + offset) + 1;
        }

        
    }



    public void CycleFootPlantPosition(Transform targetIk, Transform baseTransform, ref Vector3 plantPos, ref float updateTime){

        float forwardReachDistance = .8f * (GetHorizVelocity().magnitude / maxSpeed_sprint);

        plantPos = baseTransform.position + GetHorizVelocity().normalized * 2.2f * forwardReachDistance;
        // RaycastHit hit;
        // if(Physics.Raycast(plantPos, Vector3.up + GetHorizVelocity().normalized, out hit, 1f, layerMask_terrain)){
        //     Vector3 pt = hit.point;
        //     plantPos = new Vector3(pt.x, Mathf.Min(pt.y, kneeHeightT.position.y), pt.z);
        // }
        updateTime = 0f + (Mathf.Max(updateTime, 1f) - 1f);
    }
    public void AdjustFootPlantPosition(Transform targetIk, Transform baseTransform, ref Vector3 plantPos, bool onGround){
        // move plantPos down until hits terrain
        if (onGround)
        {
            RaycastHit hit;
            if (Physics.Raycast(plantPos, Vector3.down, out hit, 100f, layerMask_walkable))
            {
                plantPos.y = hit.point.y;
            }
        }
        
        // else{
        //     if(Physics.Raycast(plantPos, Vector3.up + GetHorizVelocity().normalized, out hit, 1f, layerMask_terrain)){
        //         plantPos = new Vector3(plantPos.x, Mathf.Min(hit.point.x, kneeHeightT.position.y), plantPos.z);
        //     }
        // }
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

        GameObject itemRight = entityItems.weaponEquipped_object;
        GameObject itemLeft = entityItems.holding_object;


        // right hand
        if(itemRight != null){
            ikScript_handRight.enabled = true;
            handFree_right = false;
            ikScript_handRight.Target = itemRight.transform.Find("IKTargetT_Right");
        }
        else{
            ikScript_handRight.enabled = false;
            ikScript_handRight.Target = ikParent.Find("TargetHandRight");
            handFree_right = true;
        }
        
        // left hand
        Log("Figuring out left hand.");
        if(itemLeft != null){
            ikScript_handLeft.enabled = true;
            Log("Left hand not free");
            handFree_left = false;
            targetHandLeft = itemRight.transform.Find("IKTargetT_Left");
        }
        else{
            
            Log("Left hand free");

            // if hand is free, support right hand with holding the weapon, if equipped
            if(!handFree_right){
                Log("Right hand not free- help right hand");
                ikScript_handLeft.enabled = true;
                ikScript_handLeft.Target = itemRight.transform.Find("IKTargetT_Left");
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

    public void OnItemSwitch(){
        UpdateIKForCarryingItems();
    }




    public void Move(Vector3 direction, float speed){
        float speedStat = speed * entityStats.GetStat("speed");
        sprinting = entityBehavior.urgent || (isLocalPlayer && entityUserInputMovement.pressSprint);
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
            StartCoroutine(_Jump(JumpForce));
        }
    }
    IEnumerator _Jump(float power){
        jumping = true;
        jumpOpposite = !jumpOpposite;
        if(groundTime <= JumpCoolDown){
            float t = JumpCoolDown - groundTime;
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
        Jump(JumpForce*.7f);
    }

    public bool CanJump(){
        if(Physics.Raycast(groundSense.position, Vector3.down, groundCastDistance + .3f, layerMask_walkable)){
            return true;
        }
        return false;
    }

    public void LaunchProjectile(GameObject projectilePrefab){

        StartCoroutine(_LaunchProjectile());

        IEnumerator _LaunchProjectile(){

            
            GameObject temp = new GameObject();
            temp.transform.position = transform.position + transform.forward;
            temp.transform.SetParent(transform);
            bool notTargeting = entityAnimation.bodyRotationMode != (int)EntityAnimation.BodyRotationMode.Target;
            if(notTargeting){
                entityAnimation.SetBodyRotationMode((int)EntityAnimation.BodyRotationMode.Target, temp.transform);
            }
    
            yield return new WaitForSeconds(.2f);

            GameObject projectile = GameObject.Instantiate(projectilePrefab, handRight.position, Quaternion.identity);
            Physics.IgnoreCollision(projectile.GetComponent<Collider>(), hitbox);
            Vector3 targetPos, throwDir;

            if (isLocalPlayer){
                targetPos = Camera.main.transform.position + (Camera.main.transform.up * 100f) + (Camera.main.transform.forward * 1000f);
            }
            else
            {
                targetPos = Vector3.zero;
            }

            throwDir = targetPos - transform.position;
            projectile.GetComponent<Rigidbody>().velocity = (throwDir.normalized * ThrowForce) + rb.velocity;
            StartCoroutine(Utility.DespawnObject(projectile, 5f));


            yield return new WaitForSeconds(.075f);
            if(notTargeting){
                entityAnimation.SetBodyRotationMode((int)EntityAnimation.BodyRotationMode.Normal, null);
            }
            GameObject.Destroy(temp);

        }

        
    }

    public void Attack(){
        if(entityItems.weaponEquipped_item == null){
            LaunchProjectile(Item.SmallStone.gameobject);
        }
        else{
            Item weapItem = entityItems.weaponEquipped_item;
            switch (weapItem.holdStyle)
            {
                case Item.HoldStyle.Spear:
                    StartCoroutine("SetWeaponTrajectoryThrust");
                    break;

                case Item.HoldStyle.Axe:
                    StartCoroutine("SetWeaponTrajectorySwing");
                    break;
                default:

                    Log("Trying to attack with a weapon with no specified hold style!!!");
                    break;
                }
            
        }
    }
    IEnumerator SetWeaponTrajectoryThrust(){
        yield return null;
    }
    IEnumerator SetWeaponTrajectorySwing(){
        yield return null;
    }





    void CheckGround(){
        Vector3 vel = rb.velocity;
        if(Physics.Raycast(groundSense.position, Vector3.down, out groundInfo, groundCastDistance_far, layerMask_walkable)){
            if(Vector3.Distance(groundInfo.point, transform.position) < groundCastDistance){
                if(!GROUNDTOUCH){
                    GROUNDTOUCH = true;
                    vel.y = 0f;
                    rb.velocity = vel;
                }
                groundTime += Time.fixedDeltaTime;
            }
            else{
                if(GROUNDTOUCH){
                    GROUNDTOUCH = false;
                    groundTime = 0f;
                    airTime = 0f;
                }
                airTime += Time.fixedDeltaTime;
            }
        }
    }

    void CheckWall(){
        bool w = false;
        if(offWallTime > .4f){
            if(moveDir.magnitude > 0){
                if(Physics.Raycast(wallSense.position, transform.forward, out wallInfo, wallCastDistance) || Physics.Raycast(wallSense.position, entityAnimation.bodyT.forward, out wallInfo, wallCastDistance)){
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
        bool w = false;
        float y = transform.position.y;
        float waterY = ChunkGenerator.SeaLevel*ChunkGenerator.ElevationAmplitude;
        if(y <= waterY){
        //if(Physics.Raycast(waterSense.position, Vector3.up, out waterInfo, layerMask_water)){
            w = true;
        }
        if(y <= waterY + .2f){
            ApplyFlotationForce(waterY - y);    
        }
        
        if(w){
            IN_WATER = true;
        }
        else{
            if(IN_WATER){
                offWaterTime = 0f;
            }
            IN_WATER = false;
        }
    }

    void CheckScrunch(){
        if(groundTime < 1f - landScrunch_recoverySpeed){
            landScrunch = Mathf.Sin(Mathf.InverseLerp(0f, 1f - landScrunch_recoverySpeed, groundTime) * Mathf.PI * 1.25f);
            float at = Mathf.Lerp(0f, 1f, airTime / landScrunch_airTimeThreshhold);
            landScrunch = Mathf.Lerp(0f, at, landScrunch);
        }else{
            landScrunch = 0f;
        }
    }

    void CheckPhysicMaterial(){
        if(moveDir.magnitude > 0f){
            hitbox.sharedMaterial = noFrictionMat;
        }
        else{
            hitbox.sharedMaterial = highFrictionMat;
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
            max *= 1.5f - Mathf.InverseLerp(-3f, 3f, rb.velocity.y);
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
            rb.useGravity = false;
            //rb.useGravity = true;
        }
        else{ rb.useGravity = true; }
    }


    Vector3 GetHorizVelocity(){
        Vector3 horvel = rb.velocity;
        float ySpeed = horvel.y;
        horvel.y = 0f;
        return horvel;
    }

    bool IsMoving(){
        return GetHorizVelocity().magnitude > .1f;
    }

    bool GroundIsClose(){

        return Physics.OverlapSphere(groundSense.position, .5f, layerMask_walkable).Length > 0;

    }

    void FixedUpdate()
    {
       ROTATION_Y_LAST = transform.rotation.y;

        CheckPhysicMaterial();
        Move(moveDir, acceleration);
        CheckGround();
        //CheckWall();
        CheckWater();
        CheckScrunch();
        LimitSpeed();
        SetGravity();
    }

    void Update(){



        jumpTime += Time.deltaTime;
        offWallTime += Time.deltaTime;
        offWaterTime += Time.deltaTime;

        if(Input.GetKeyUp(KeyCode.P)){
            acceleration *= 2f;
            maxSpeed_run *= 2f;
        }
        if(Input.GetKeyUp(KeyCode.O)){
            acceleration /= 2f;
            maxSpeed_run /= 2f;
        }
        
        IKUpdate();
    }


    void OnTriggerEnter(Collider col){
        if(col.gameObject.layer == LayerMask.NameToLayer("Feature")){
            hitbox.sharedMaterial = noFrictionMat;
        }
        
    }
    void OnTriggerExit(Collider col){
        if(col.gameObject.layer == LayerMask.NameToLayer("Feature")){
            hitbox.sharedMaterial = highFrictionMat;
        }
    }
}

