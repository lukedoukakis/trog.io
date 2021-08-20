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
    public int bodyRotationMode;
    Transform bodyRotationTarget;
    public static float bodyRotationSpeed_player = .04f; //.0625
    public static float bodyRotationSpeed_ai = .0625f;
    public static float leanBoundMin = -10f;
    public static float leanBoundMax = 11f;
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




    // should match 'parameters' in Animator
    public Dictionary<string, bool> animBools = new Dictionary<string, bool>{
        {"Stand",   false},
        {"Rotate",   false},
        {"Rotate Opposite",   false},
        {"Run",     false},
        {"Sprint",     false},
        {"Climb",     false},
        {"Swim",     false},
        {"Tread",     false},
        {"Jump",    false},
        {"Jump Opposite",    false},
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

    public List<string> keysList;

    public enum BodyRotationMode{
        Normal, Target
    }


    protected override void Awake(){

        base.Awake();

      
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        headT = Utility.FindDeepChild(transform, "B-head");
        bodyT = Utility.FindDeepChild(transform, "Human Model 2");
        if(tag == "Player"){
            bodyRotationSpeed = bodyRotationSpeed_player;
        }
        else{
            bodyRotationSpeed = bodyRotationSpeed_ai;
        }
        posture_squat = .1f;

        keysList = new List<string>(animBools.Keys);
    }

    public void ToggleAnimation(bool value){
        animator.enabled = value;
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

        switch (bodyRotationMode)
        {

            // normal rotation
            case (int)BodyRotationMode.Normal:

                if(isLocalPlayer){
                    bodyLean = Mathf.InverseLerp(leanBoundMin, leanBoundMax, Mathf.Sin(Camera.main.transform.rotation.eulerAngles.x * Mathf.Deg2Rad)) * 2f - 1f;
                }
                else{
                    bodyLean = 0f;
                }


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
            case (int)BodyRotationMode.Target:
                Vector3 dir = bodyRotationTarget.position - bodyT.position;
                Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
                bodyT.rotation = Quaternion.Slerp(bodyT.rotation, rot, .1f);
                break;

            default:
                Log("UpdateBodyRotation: bodyRotationMode not recognized: " + bodyRotationMode);
                    break;
        };

        

        


        
        


    }
    void UpdateMovement(){
        Vector3 velRaw = rb.velocity;
        Vector3 velHoriz = velRaw; velHoriz.y = 0;

        bool ground = entityPhysics.GROUNDTOUCH;
        bool wall = entityPhysics.WALLTOUCH;
        bool water = entityPhysics.IN_WATER;

        foreach(string mvmt in keysList){
            SetAnimationBool(mvmt, false);
        }

        if(ground && !wall && !water){
            if(velHoriz.magnitude > .05f){
                if(entityPhysics.sprinting){
                    SetAnimationBool("Sprint", true);
                    SetAnimationFloat("LegSpeed", 1.22f);
                }
                else{
                    SetAnimationBool("Run", true);
                    SetAnimationFloat("LegSpeed", 1f);
                }
            }
            else{
                if(isLocalPlayer){
                    if(Mathf.Abs(entityUserInputMovement.mouseY) > .5f){
                        SetAnimationBool("Rotate", true);
                    }
                    else{
                        SetAnimationBool("Stand", true);
                    }
                }
                else{
                    SetAnimationBool("Stand", true);
                }
            }
        }
        else{
            if(wall){
                SetAnimationBool("Climb", true);
                if(entityPhysics.moveDir.magnitude > 0f){
                    SetAnimationFloat("ClimbSpeed", 1f);
                }
                else{
                    SetAnimationFloat("ClimbSpeed", 0f);
                }
            
            }
            else{
                if(water){
                    if(entityPhysics.moveDir.magnitude > 0f){
                        SetAnimationBool("Swim", true);
                    }
                    else if(isLocalPlayer && entityUserInputMovement.move.magnitude > 0){
                        SetAnimationBool("Swim", true);
                    }
                    else{
                        SetAnimationBool("Tread", true);
                    }
                }
                else{
                    if(entityPhysics.jumpTime < .3f || entityPhysics.offWallTime < .3f){
                        SetAnimationBool("Jump", true);
                    }
                }
                
            }
        }
        SetAnimationBool("Jump Opposite", entityPhysics.jumpOpposite);
        if(isLocalPlayer){
            SetAnimationBool("Rotate Opposite", entityUserInputMovement.mouseY > .5f);
        }

        // calculate run
        //Log(entityPhysics.moveDir.magnitude.ToString());
        if(entityPhysics.moveDir.magnitude > 0){

            if(entityPhysics.GROUNDTOUCH){
                bodySkew = Mathf.Lerp(bodySkew, Mathf.InverseLerp(0f, 180f, Vector3.Angle(velHoriz, bodyT.forward)), .05f);
                //Log(bodySkew.ToString());
            }
            else{
                bodySkew = Mathf.Lerp(bodySkew, 0f, .01f);
            }
        }
        else{
            bodySkew = Mathf.Lerp(bodySkew, 0f, .01f);
        }

        slowness = 1f - Mathf.InverseLerp(0f, entityPhysics.maxSpeed_run, velHoriz.magnitude);
        runMagnitude = 1f - Mathf.Max(bodySkew, slowness);
        SetAnimationLayerWeight("Legs_full", runMagnitude);
        if (bodyRotationMode == (int)BodyRotationMode.Normal)
        {
            if (angularVelocityY < 0)
            {
                SetAnimationLayerWeight("Turn Left Position", bodySkew / 10f);
                SetAnimationLayerWeight("Turn Right Position", 0f);
            }
            else
            {
                SetAnimationLayerWeight("Turn Left Position", 0f);
                SetAnimationLayerWeight("Turn Right Position", bodySkew / 10f);
            }
            SetAnimationLayerWeight("Hips Right", 0f);
            SetAnimationLayerWeight("Hips Left", 0f);
        }
        else if (bodyRotationMode == (int)BodyRotationMode.Target)
        {
            if (Vector3.SignedAngle(velHoriz, bodyT.forward, Vector3.up) < -90f){
                SetAnimationLayerWeight("Hips Right", bodySkew);
                SetAnimationLayerWeight("Hips Left", 0f);
            }
            else
            {
                SetAnimationLayerWeight("Hips Right", 0f);
                SetAnimationLayerWeight("Hips Left", bodySkew);
            }
            SetAnimationLayerWeight("Turn Left Position", 0f);
            SetAnimationLayerWeight("Turn Right Position", 0f);
        }
        
        
        
    


        // calculate backpedal
        float backMagnitude = (1f - runMagnitude) * Mathf.Lerp(0f, .2f, bodySkew);
        SetAnimationLayerWeight("Legs_backpedal", backMagnitude);
        SetAnimationLayerWeight("Legs_shuffle", 1f - backMagnitude - runMagnitude);

        // calculate posture
        SetAnimationLayerWeight("Squat Position", posture_squat + squat_activity + entityPhysics.landScrunch);


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
        if(animator.enabled){
            UpdateMovement();
        }
        UpdateBodyRotation();
        bodyRotationLast = bodyT.rotation;
        angularVelocityY_last = angularVelocityY;
    }

    void Update(){
        
    }


}
