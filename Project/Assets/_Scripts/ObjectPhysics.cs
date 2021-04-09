using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPhysics : MonoBehaviour
{

    ObjectStats stats;
    ObjectAnimation anim;

    public Transform groundSense;
    RaycastHit groundInfo;
    public static float castDistance_ground = .01f;
    public Transform obstacleHeightSense;

    Rigidbody rb;

    public static float jumpForce = 500f;
    public float jumpTime;
    public static float jumpCoolDown = .2f;
    float fallSpeed;
    public static float maxSpeed = 10f;

    public static float TimeFactor;

    public bool GROUNDTOUCH;






    void Awake(){
        stats = GetComponent<ObjectStats>();
        anim = GetComponent<ObjectAnimation>();
        rb = GetComponent<Rigidbody>();
        groundSense = transform.Find("GroundSense");
        obstacleHeightSense = transform.Find("ObstacleHeightSense");
    }


    public void Move(Vector3 direction, float speed){
        float spd = speed * stats.GetStat("speed");
        Vector3 move = transform.TransformDirection(direction) * spd;
        rb.AddForce(move * spd, ForceMode.Force);
    }

    public void RotateTowards(Quaternion targetRot, float rotSpeed){
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed);
    }

    public void Jump(){
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Force);
        jumpTime = 0;
    }

    public bool CanJump(){
        return GROUNDTOUCH && jumpTime > jumpCoolDown;
    }





    void CheckGround(){
        // directly underneath
        if(Physics.Raycast(groundSense.position, Vector3.down, out groundInfo, castDistance_ground)){
            GROUNDTOUCH = true;
        }else{
            GROUNDTOUCH = false;
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
       
    }

    void Update(){
        TimeFactor = 100f * Time.deltaTime;
        jumpTime += Time.deltaTime;

        CheckGround();
        LimitSpeed();
    }
}
