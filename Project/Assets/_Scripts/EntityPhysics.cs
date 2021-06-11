using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPhysics : EntityComponent
{

    public Collider hitbox;
    public PhysicMaterial highFrictionMat;
    public PhysicMaterial noFrictionMat;
    public LayerMask layerMask_water;
    public LayerMask layerMask_noWater;

    public Rigidbody rb;
    public Transform gyro;
    public Transform rightHand;
    public Transform groundSense, wallSense, waterSense, obstacleHeightSense;
    public RaycastHit groundInfo, wallInfo, waterInfo;
    public static float groundCastDistance_player = .1f;
    public static float groundCastDistance_npc = .1f;
    public static float groundCastDistance_far = 100f;
    public static float wallCastDistance = 1f;
    float groundCastDistance;
    public static float JumpForce = 2800f;
    public static float ThrowForce = 200f;
    public static float AccelerationScale = 35f;
    public static float MaxSpeedScale = 20f;
    public static float JumpCoolDown = .15f;


    public Vector3 moveDir;
    public bool jumping, jumpOpposite, sprinting;
    public float offWallTime, offWaterTime, jumpTime, airTime, groundTime;
    public float acceleration;
    public float maxSpeed_run, maxSpeed_sprint, maxSpeed_climb, maxSpeed_swim;


    public static float landScrunch_recoverySpeed = .75f;
    public static float landScrunch_airTimeThreshhold = 1.2f;
    public float landScrunch;


    public float ROTATION_Y_THIS;
    public float ROTATION_Y_LAST;
    public bool GROUNDTOUCH, WALLTOUCH, IN_WATER;





    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityPhysics = this;
        hitbox = GetComponentInChildren<CapsuleCollider>();
        highFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/HighFriction");
        noFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/NoFriction");
        layerMask_water = LayerMask.GetMask("Water");
        layerMask_noWater = LayerMask.GetMask("Terrain");
        rb = GetComponent<Rigidbody>();
        gyro = Utility.FindDeepChild(this.transform, "Gyro");
        rightHand = Utility.FindDeepChild(this.transform, "B-hand_R");
        groundSense = Utility.FindDeepChild(transform, "GroundSense");
        obstacleHeightSense = Utility.FindDeepChild(transform, "ObstacleHeightSense");
        wallSense = Utility.FindDeepChild(transform, "WallSense");
        waterSense = Utility.FindDeepChild(transform, "WaterSense");
        if(tag == "Player"){
            groundCastDistance = groundCastDistance_player;
        }else if(tag == "Npc"){
            groundCastDistance = groundCastDistance_npc;
        }
    }

    void Start(){
        acceleration = handle.entityStats.GetStat("speed") * AccelerationScale;
        maxSpeed_run = handle.entityStats.GetStat("speed") * MaxSpeedScale;
        maxSpeed_sprint = maxSpeed_run * 1.5f;
        maxSpeed_climb = maxSpeed_run * .25f;
        maxSpeed_swim = maxSpeed_run * .75f;
    }


    public void Move(Vector3 direction, float speed){
        float speedStat = speed * handle.entityStats.GetStat("speed");
        sprinting = handle.entityBehavior.urgent || (isLocalPlayer && handle.entityUserInputMovement.pressSprint);
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
        if(Physics.Raycast(groundSense.position, Vector3.down, groundCastDistance + .3f, layerMask_noWater)){
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
            bool notTargeting = handle.entityAnimation.bodyRotationMode != (int)EntityAnimation.BodyRotationMode.Target;
            if(notTargeting){
                handle.entityAnimation.SetBodyRotationMode((int)EntityAnimation.BodyRotationMode.Target, temp.transform);
            }
    
            yield return new WaitForSeconds(.2f);

            GameObject projectile = GameObject.Instantiate(projectilePrefab, rightHand.position, Quaternion.identity);
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
                handle.entityAnimation.SetBodyRotationMode((int)EntityAnimation.BodyRotationMode.Normal, null);
            }
            GameObject.Destroy(temp);

        }

        
    }





    void CheckGround(){
        Vector3 vel = rb.velocity;
        if(Physics.Raycast(groundSense.position, Vector3.down, out groundInfo, groundCastDistance_far, layerMask_noWater)){
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
                if(Physics.Raycast(wallSense.position, transform.forward, out wallInfo, wallCastDistance) || Physics.Raycast(wallSense.position, handle.entityAnimation.bodyT.forward, out wallInfo, wallCastDistance)){
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
        }
        else{ rb.useGravity = true; }
    }


    Vector3 GetHorizVelocity(){
        Vector3 horvel = rb.velocity;
        float ySpeed = horvel.y;
        horvel.y = 0f;
        return horvel;
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
        
        
    }
}
