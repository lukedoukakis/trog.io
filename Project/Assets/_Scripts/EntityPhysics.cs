using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPhysics : EntityComponent
{

    public Transform groundSense;
    RaycastHit groundInfo;
    public static float castDistance_ground = 100f;
    public Transform obstacleHeightSense;

    public Rigidbody rb;

    public static float JumpForce = 500f;
    public static float AccelerationScale = 20f;
    public static float MaxSpeedScale = 20f;
    public static float JumpCoolDown = .25f;


    public float jumpTime;
    public float acceleration;
    public float maxSpeed;



    public float ROTATION_Y_THIS;
    public float ROTATION_Y_LAST;
    public bool GROUNDTOUCH;






    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityPhysics = this;
        rb = GetComponent<Rigidbody>();
        groundSense = transform.Find("GroundSense");
        obstacleHeightSense = transform.Find("ObstacleHeightSense");
    }

    void Start(){
        acceleration = handle.entityStats.GetStat("speed") * AccelerationScale;
        maxSpeed = handle.entityStats.GetStat("speed") * MaxSpeedScale;
    }


    public void Move(Vector3 direction, float speed){
        float spd = speed * handle.entityStats.GetStat("speed");
        Vector3 move = transform.TransformDirection(direction) * spd;
        rb.AddForce(move * spd * (100f*Time.deltaTime), ForceMode.Force);
        //transform.position += move;
    }

    public void RotateTowards(Quaternion targetRot, float rotSpeed){
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed);
    }

    public void Jump(){
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;
        rb.AddForce(Vector3.up * JumpForce, ForceMode.Force);
        jumpTime = 0;
    }

    public bool CanJump(){
        return GROUNDTOUCH && jumpTime > JumpCoolDown;
    }





    void CheckGround(){
        // directly underneath
        GROUNDTOUCH = false;
        if(Physics.Raycast(groundSense.position, Vector3.down, out groundInfo, castDistance_ground)){
            if(Vector3.Distance(groundInfo.point, transform.position) < .2f){
                GROUNDTOUCH = true;
            }
        }
    }

    void LimitSpeed(){
        Vector3 vel = rb.velocity;
        float ySpeed = vel.y;
        vel.y = 0f;
        if(vel.magnitude > maxSpeed){
            vel = vel.normalized * maxSpeed;
            vel.y = ySpeed;
            rb.velocity = vel;
        }
    }


    void FixedUpdate()
    {
       ROTATION_Y_LAST = transform.rotation.y;
    }

    void Update(){

        jumpTime += Time.deltaTime;

        CheckGround();
        LimitSpeed();


        //ROTATION_Y_LAST = transform.rotation.y;
    }
}
