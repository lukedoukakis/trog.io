using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPhysics : EntityComponent
{

    public Collider hitbox;
    public PhysicMaterial highFrictionMat;
    public PhysicMaterial noFrictionMat;

    public Rigidbody rb;
    public Transform groundSense;
    public Transform gyro;
    RaycastHit groundInfo;
    public static float groundCastDistance_player = .01f * 10f;
    public static float groundCastDistance_npc = .05f * 10f;
    public static float groundCastDistance_far = 100f;
    float groundCastDistance;
    public Transform obstacleHeightSense;

    public static float JumpForce = 1500f;
    public static float AccelerationScale = 50f;
    public static float MaxSpeedScale = 20f;
    public static float JumpCoolDown = .15f;


    public Vector3 moveDir;
    public float jumpTime; bool jumping;
    public float airTime;
    public float groundTime;
    public float acceleration;
    public float maxSpeed;


    public static float landScrunch_recoverySpeed = .75f;
    public static float landScrunch_airTimeThreshhold = 1.2f;
    public float landScrunch;


    public float ROTATION_Y_THIS;
    public float ROTATION_Y_LAST;
    public bool GROUNDTOUCH;






    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityPhysics = this;
        hitbox = GetComponentInChildren<CapsuleCollider>();
        highFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/HighFriction");
        noFrictionMat = (PhysicMaterial)Resources.Load("PhysicMaterials/NoFriction");
        rb = GetComponent<Rigidbody>();
        gyro = Utility.FindDeepChild(this.transform, "Gyro");
        groundSense = Utility.FindDeepChild(transform, "GroundSense");
        obstacleHeightSense = Utility.FindDeepChild(transform, "ObstacleHeightSense");
        if(tag == "Player"){
            groundCastDistance = groundCastDistance_player;
        }else if(tag == "Npc"){
            groundCastDistance = groundCastDistance_npc;
        }
    }

    void Start(){
        acceleration = handle.entityStats.GetStat("speed") * AccelerationScale;
        maxSpeed = handle.entityStats.GetStat("speed") * MaxSpeedScale;
    }


    public void Move(Vector3 direction, float speed){
        float speedStat = speed * handle.entityStats.GetStat("speed");
        Vector3 move = transform.TransformDirection(direction).normalized * speedStat;
        rb.AddForce(move * speedStat, ForceMode.Force);
    }

    public void Jump(){
        if(!jumping){
            StartCoroutine(_Jump());
        }
    }
    IEnumerator _Jump(){
        jumping = true;
        if(groundTime <= JumpCoolDown){
            float t = JumpCoolDown - groundTime;
            yield return new WaitForSecondsRealtime(t);
        }
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;
        rb.AddForce(Vector3.up * JumpForce, ForceMode.Force);
        jumpTime = 0;
        groundTime = 0;
        yield return new WaitForSecondsRealtime(.1f);
        jumping = false;
        yield return null;
    }

    public bool CanJump(){
        return GROUNDTOUCH;
    }





    void CheckGround(){
        Vector3 vel = rb.velocity;
        // directly underneath
        if(Physics.Raycast(groundSense.position, Vector3.down, out groundInfo, groundCastDistance_far)){
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
        if(GROUNDTOUCH && moveDir.magnitude > 0f){
            rb.useGravity = false;
        }else{ rb.useGravity = true; }
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
        if(horvel.magnitude > maxSpeed){
            horvel = horvel.normalized * maxSpeed;
            horvel.y = ySpeed;
            rb.velocity = horvel;
        }
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
        CheckScrunch();
        LimitSpeed();

        
    }

    void Update(){



        jumpTime += Time.deltaTime;

        if(Input.GetKeyUp(KeyCode.P)){
            acceleration *= 2f;
            maxSpeed *= 2f;
        }
        if(Input.GetKeyUp(KeyCode.O)){
            acceleration /= 2f;
            maxSpeed /= 2f;
        }
        
    
    }
}
