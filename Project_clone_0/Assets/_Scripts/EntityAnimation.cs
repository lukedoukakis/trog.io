﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAnimation : EntityComponent
{

    public Rigidbody rb;
    public Transform bodyT;
    public Transform headT;


    // rotation
    public Enum bodyRotationMode;
    public Transform bodyRotationTarget;
    public static float bodyRotationSpeed_player = .04f; //.0625
    public static float bodyRotationSpeed_ai = .0625f;
    public static float leanBoundMin = -.4f;
    public static float leanBoundMax = 1.05f;
    float bodyRotationSpeed;
    Quaternion bodyRotation;
    Quaternion bodyRotationLast;
    Vector3 bodyAngularVelocity;
    public float bodyLean;
    public float angularVelocityY;
    float angularVelocityY_last;
    public static float angularVelocityY_maxDelta = .1f;

    float posture_squat, squat_activity;

    float runMagnitude, climbMagnitude;
    public float bodySkew, slowness;




    public enum BodyRotationMode{
        Normal, Target
    }


    protected override void Awake(){

        base.Awake();

      
        rb = GetComponent<Rigidbody>();
        headT = Utility.FindDeepChild(transform, "B-head");
        bodyT = Utility.FindDeepChild(transform, "HumanIK");
        //bodyT = Utility.FindDeepChild(transform, "Human Model 2");
        if(tag == "Player"){
            bodyRotationSpeed = bodyRotationSpeed_player;
        }
        else{
            bodyRotationSpeed = bodyRotationSpeed_ai;
        }
        posture_squat = .1f;
    }

    void Start(){
        SetBodyRotationMode(BodyRotationMode.Target, null);
    }

    
    public void SetBodyRotationMode(Enum mode, Transform t){
        if (true)
        {
            Log("Setting body rotation mode");
            bodyRotationMode = mode;
            if (t != null)
            {
                bodyRotationTarget = t;
            }
        }
    }


    void UpdateBodyRotation(){

        if (isLocalPlayer && !entityPhysics.IN_WATER)
        {
            bodyLean = Mathf.InverseLerp(leanBoundMin, leanBoundMax, Mathf.Sin(Camera.main.transform.rotation.eulerAngles.x * Mathf.Deg2Rad)) * 2f - 1f + .2f;
        }
        else
        {
            bodyLean = 0f;
        }

        switch (bodyRotationMode)
        {

            // normal rotation
            case BodyRotationMode.Normal:


                bool moving = entityPhysics.IsMoving();
                Vector3 dirHoriz = moving ? rb.velocity : bodyT.forward;
                dirHoriz.y = 0;
                dirHoriz = dirHoriz.normalized;
                Vector3 direction = Vector3.RotateTowards(bodyT.forward, dirHoriz, bodyRotationSpeed, 0f).normalized;
                
                direction += Vector3.up * (bodyLean * -1f);

                Quaternion rotation = Quaternion.LookRotation(direction);
                if (entityPhysics.GROUNDTOUCH)
                {
                    Vector3 v = (rotation.eulerAngles) - (bodyRotationLast.eulerAngles);
                    angularVelocityY = Mathf.Lerp(bodyAngularVelocity.y, v.y, .5f);
                    angularVelocityY = Mathf.Clamp(angularVelocityY, -5f, 5f);
                    float dif = angularVelocityY - angularVelocityY_last;
                    if (Math.Abs(dif) > angularVelocityY_maxDelta)
                    {
                        angularVelocityY = angularVelocityY_last + (angularVelocityY_maxDelta * Mathf.Sign(dif));
                    }
                }
                else
                {
                    angularVelocityY = 0f;
                }
                rotation *= Quaternion.Euler(Vector3.forward * angularVelocityY * -1f);
                bodyT.rotation = rotation;
      
            

                break;

            // targeting rotation
            case BodyRotationMode.Target:
                Vector3 dir;
                if(bodyRotationTarget != null){
                    dir = (bodyRotationTarget.position - bodyT.position).normalized;
                }
                else{
                    dir = transform.forward;
                }
                dir += (Vector3.up * bodyLean * -1f);
                Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
                bodyT.rotation = Quaternion.Slerp(bodyT.rotation, rot, .1f);
                break;

            default:
                Log("UpdateBodyRotation: bodyRotationMode not recognized: " + bodyRotationMode);
                    break;
        };

        

        


        
        


    }
    
        


    // play pickup animation for an item
    public void Pickup(Item i){

        StartCoroutine(SquatAndStand());

        // if(i.type == (int)Item.Type.Weapon){
        //     SetAnimationBool("RightArm_weapon", true);
        // }
        // else if(i.type != (int)Item.Type.Pocket){
        //     SetAnimationInt("LeftArm_holdStyle", i.holdStyle);
        // }
    }


    public void OnAttack(){

        // string trigger;
        // Item weap;

        // if(entityItems.weaponEquipped == null){
        //     trigger = "Throw";
        //     entityPhysics.LaunchProjectile(Item.SmallStone.gameobject);
        // }
        // else{
        //     weap = entityItems.weaponEquipped.Item1;
        //     switch (weap.holdStyle)
        //     {
        //         case Item.HoldStyle.Spear:
        //             trigger = "Thrust";
        //             break;
        //         case Item.HoldStyle.Axe:
        //             trigger = "Swing";
        //             break;
        //         default:
        //             trigger = "Thrust";
        //             Log("Trying to swing a weapon with no specified hold style");
        //             break;
        //         }
            
        // }
        
        // SetAnimationTrigger(trigger);
        //DisableAnimationLayer("LeftArm", .5f);
        //MaximizeAnimationLayer("RightArm", .5f);
    }



    IEnumerator SquatAndStand(){
        while(squat_activity < .7f){
            squat_activity = Mathf.Lerp(squat_activity, .75f, 10f * Time.deltaTime);
            yield return null;
        }
        while(squat_activity > .05f){
            squat_activity = Mathf.Lerp(squat_activity, 0f, 10f * Time.deltaTime);
            yield return null;
        }
        squat_activity = 0f;
    }


    void FixedUpdate(){
        UpdateBodyRotation();
        bodyRotationLast = bodyT.rotation;
        angularVelocityY_last = angularVelocityY;
    }

    void Update(){
        
    }


}
