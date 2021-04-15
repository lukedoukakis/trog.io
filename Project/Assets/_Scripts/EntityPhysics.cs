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
    public static float JumpCoolDown = .15f;


    public float jumpTime; bool jumping;
    public float airTime;
    public float groundTime;
    public float acceleration;
    public float maxSpeed;


    public static float landScrunch_recoverySpeed = .8f;
    public static float landScrunch_airTimeThreshhold = 2.4f;
    public float landScrunch;


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
        if(!jumping){
            StartCoroutine(_Jump());
        }
        
        // Vector3 vel = rb.velocity;
        // vel.y = 0f;
        // rb.velocity = vel;
        // rb.AddForce(Vector3.up * JumpForce, ForceMode.Force);
        // jumpTime = 0;
        // groundTime = 0;
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
        if(Physics.Raycast(groundSense.position, Vector3.down, out groundInfo, castDistance_ground)){
            if(Vector3.Distance(groundInfo.point, transform.position) < .2f){
                if(!GROUNDTOUCH){
                    GROUNDTOUCH = true;
                    vel.y = 0f;
                    rb.velocity = vel;
                }
                groundTime += Time.deltaTime;
            }
            else{
                if(GROUNDTOUCH){
                    GROUNDTOUCH = false;
                    groundTime = 0f;
                    airTime = 0f;
                }
                airTime += Time.deltaTime;
            }
        }
    }

    void CheckScrunch(){
        if(groundTime < 1f - landScrunch_recoverySpeed){
            landScrunch = Mathf.Sin(Mathf.InverseLerp(0f, 1f - landScrunch_recoverySpeed, groundTime) * Mathf.PI);
            float at = Mathf.Lerp(0f, 1f, airTime / landScrunch_airTimeThreshhold);
            landScrunch = Mathf.Lerp(0f, at, landScrunch);
        }else{
            landScrunch = 0f;
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


    void FixedUpdate()
    {
       ROTATION_Y_LAST = transform.rotation.y;
    }

    void Update(){

        jumpTime += Time.deltaTime;

        CheckGround();
        CheckScrunch();
        LimitSpeed();


        //ROTATION_Y_LAST = transform.rotation.y;
    }
}
