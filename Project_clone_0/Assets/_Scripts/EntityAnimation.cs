using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAnimation : EntityComponent
{

    public Rigidbody rb;
    public Transform bodyT;
    public Transform headT;
    public Animator animator;


    // rotation
    int bodyRotationMode;
    Transform bodyRotationTarget;
    public static float bodyRotationSpeed_player = .25f;
    public static float bodyRotationSpeed_ai = .25f;
    float bodyRotationSpeed;
    Quaternion bodyRotation;
    Quaternion bodyRotationLast;
    Vector3 bodyAngularVelocity;
    float angularVelocityY;
    float angularVelocityY_last;
    public static float angularVelocityY_maxDelta = .1f;

    float posture_squat, squat_activity;

    float runMagnitude;
    float bodySkew, slowness;




    // should match 'parameters' in Animator
    public Dictionary<string, bool> animBools = new Dictionary<string, bool>{
        {"Stand",   false},
        {"Run",     false},
        {"Sprint",     false},
        {"Climb",     false},
        {"Swim",     false},
        {"Tread",     false},
        {"Jump",    false},
        {"Land",    false},
        {"RightArm_weapon",    false},
    };

    public Dictionary<string, int> animInts = new Dictionary<string, int>{
        {"LeftArm_holdStyle",   -1},
    };

    public Dictionary<string, float> animFloats = new Dictionary<string, float>{
        {"LegSpeed",   -1f},
        {"ClimbSpeed",   -1f},
        {"SwimSpeed",   -1f},
        {"TreadSpeed",   -1f},
    };

    public enum BodyRotationMode{
        Normal, Target
    }


    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityAnimation = this;
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        headT = Utility.FindDeepChild(transform, "B-head");
        foreach(Transform tr in transform)
        {
            if(tr.tag == "Body")
            {
                bodyT =  tr;
                break;
            }
        }
        if(tag == "Player"){
            bodyRotationSpeed = bodyRotationSpeed_player;
        }
        else{
            bodyRotationSpeed = bodyRotationSpeed_ai;
        }
        posture_squat = .1f;
    }


    public void SetAnimationBool(string movement, bool value){
        if(animBools[movement] != value){
            animBools[movement] = value;
            animator.SetBool(movement, value);
        }
    }
    public bool GetAnimationBool(string movement){
        return animBools[movement];
    }

    public void SetAnimationInt(string movement, int value){
        if(animInts[movement] != value){
            animInts[movement] = value;
            animator.SetInteger(movement, value);
        }
    }
    public int GetAnimationInt(string movement){
        return animInts[movement];
    }

    public void SetAnimationFloat(string movement, float value){
        if(animFloats[movement] != value){
            animFloats[movement] = value;
            animator.SetFloat(movement, value);
        }
    }
    public float GetAnimationFloat(string movement){
        return animFloats[movement];
    }

    public void SetAnimationTrigger(string movement){
        animator.SetTrigger(movement);
    }
    public void SetAnimationLayerWeight(string position, float value){
        animator.SetLayerWeight(animator.GetLayerIndex(position), value);
    }
    public void DisableAnimationLayer(string layer, float time){
        StartCoroutine(_DisableAnimationLayer());
        IEnumerator _DisableAnimationLayer(){
            float w = 0f;
            while( w < 1f){
                SetAnimationLayerWeight(layer, w);
                w += .2f;
                yield return null;
            }
            SetAnimationLayerWeight(layer, 1f);
        }
    }

    public void MaximizeAnimationLayer(string layer, float time){
        float originalWeight = animator.GetLayerWeight(animator.GetLayerIndex(layer));
        StartCoroutine(_MaximizeAnimationLayer());
        IEnumerator _MaximizeAnimationLayer(){
            float w = 1f;
            while( w > originalWeight){
                SetAnimationLayerWeight(layer, w);
                w -= .2f;
                yield return null;
            }
            SetAnimationLayerWeight(layer, originalWeight);
        }
    }

    public void SetBodyRotationMode(int mode, Transform t){
        bodyRotationMode = mode;
        if(t != null){
            bodyRotationTarget = t;
        }
    }


    void UpdateBodyRotation(){

        if(handle.entityPhysics.moveDir.magnitude > 0){
            switch (bodyRotationMode){

            // normal rotation
            case (int)BodyRotationMode.Normal:


                Vector3 velRaw = rb.velocity;
                Vector3 velHoriz = velRaw; velHoriz.y = 0;
                Vector3 direction = Vector3.RotateTowards(bodyT.forward, velHoriz, bodyRotationSpeed, 0f);
                Quaternion rotation = Quaternion.LookRotation(direction);
                if (handle.entityPhysics.GROUNDTOUCH)
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
                bodyT.rotation = rotation * Quaternion.Euler(Vector3.forward * angularVelocityY * -1f);

            break;

            // targeting rotation
            case (int)BodyRotationMode.Target:
                Vector3 dir = bodyRotationTarget.position - bodyT.position;
                Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
                bodyT.rotation = Quaternion.Slerp(bodyT.rotation, rot, .5f);
                break;

            default:
                Log("UpdateBodyRotation: bodyRotationMode not recognized: " + bodyRotationMode);
                    break;
        };

        }

        


        
        


    }
    void UpdateMovement(){
        Vector3 velRaw = rb.velocity;
        Vector3 velHoriz = velRaw; velHoriz.y = 0;

        bool ground = handle.entityPhysics.GROUNDTOUCH;
        bool wall = handle.entityPhysics.WALLTOUCH;
        bool water = handle.entityPhysics.IN_WATER;

        foreach(string mvmt in animBools.Keys){
            SetAnimationBool(mvmt, false);
        }

        if(ground && !wall && !water){
            if(velHoriz.magnitude > .05f){
                if(handle.entityPhysics.sprinting){
                    SetAnimationBool("Sprint", true);
                    SetAnimationFloat("LegSpeed", 1.22f);
                }
                else{
                    SetAnimationBool("Run", true);
                    SetAnimationFloat("LegSpeed", 1f);
                }
            }
            else{
                SetAnimationBool("Stand", true);
            }
        }
        else{
            if(wall){
                SetAnimationBool("Climb", true);
                if(handle.entityPhysics.moveDir.magnitude > 0f){
                    SetAnimationFloat("ClimbSpeed", 1f);
                }
                else{
                    SetAnimationFloat("ClimbSpeed", 0f);
                }
            
            }
            else{
                if(water){
                    if(handle.entityPhysics.moveDir.magnitude > 0f){
                        SetAnimationBool("Swim", true);
                    }
                    else{
                        SetAnimationBool("Tread", true);
                    }
                }
                else{
                    if(handle.entityPhysics.jumpTime < .3f || handle.entityPhysics.offWallTime < .3f){
                        SetAnimationBool("Jump", true);
                    }
                }
                
            }
        }

        // calculate run
        if(handle.entityPhysics.moveDir.magnitude > 0){
            if(handle.entityPhysics.GROUNDTOUCH){
                bodySkew = Mathf.Lerp(bodySkew, Mathf.InverseLerp(0f, 230f, Vector3.Angle(velHoriz, bodyT.forward)), .03f);
            }
            else{
                bodySkew = 0f;
            }
        }
        else{
            bodySkew = Mathf.Lerp(bodySkew, 0f, .5f);
        }
        slowness = 1f - Mathf.InverseLerp(0f, handle.entityPhysics.maxSpeed_run, velHoriz.magnitude);
        runMagnitude = 1f - Mathf.Max(bodySkew, slowness);
        SetAnimationLayerWeight("Legs_full", runMagnitude);
        if(angularVelocityY < 0){
            SetAnimationLayerWeight("Turn Left Position", bodySkew/8f);
            SetAnimationLayerWeight("Turn Right Position", 0f);
        }
        else{
            SetAnimationLayerWeight("Turn Left Position", 0f);
            SetAnimationLayerWeight("Turn Right Position", bodySkew/8f);
        }
    


        // calculate backpedal
        float backMagnitude = (1f - runMagnitude) * Mathf.Lerp(0f, .2f, bodySkew);
        SetAnimationLayerWeight("Legs_backpedal", backMagnitude);
        SetAnimationLayerWeight("Legs_shuffle", 1f - backMagnitude - runMagnitude);


        // calculate posture
        SetAnimationLayerWeight("Squat Position", posture_squat + squat_activity + handle.entityPhysics.landScrunch);


    }


    // play pickup animation for an item
    public void Pickup(Item i){

        StartCoroutine(SquatAndStand());

        if(i.type == (int)Item.Type.Weapon){
            SetAnimationBool("RightArm_weapon", true);
        }
        else if(i.type != (int)Item.Type.Pocket){
            SetAnimationInt("LeftArm_holdStyle", i.holdStyle);
        }
    }

    public void UseWeapon(){

        string trigger;
        Item weap;

        if(handle.entityItems.weapon_equipped == null){
            trigger = "Thrust";
        }
        else{
            weap = handle.entityItems.weapon_equipped.Item1;
            if (weap == null)
            {
                trigger = "Thrust";
            }
            else
            {
                switch (weap.holdStyle)
                {
                    case (int)Item.HoldStyle.Spear:
                        trigger = "Thrust";
                        break;
                    case (int)Item.HoldStyle.Axe:
                        trigger = "Swing";
                        break;
                    default:
                        trigger = "Thrust";
                        Log("Trying to swing a weapon with no specified hold style");
                        break;
                }
            }
        }
        
        SetAnimationTrigger(trigger);
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
        UpdateMovement();
        bodyRotationLast = bodyT.rotation;
        angularVelocityY_last = angularVelocityY;
    }

    void Update(){
       
    }


}
