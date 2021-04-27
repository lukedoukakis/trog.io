﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAnimation : EntityComponent
{

    public Rigidbody rb;
    public Transform bodyT;
    public Animator animator;

    public static float bodyRotationSpeed_player = 10f;
    public static float bodyRotationSpeed_ai = 10f;

    float bodyRotationSpeed;
    Quaternion bodyRotation;
    Quaternion bodyRotationLast;
    Vector3 bodyAngularVelocity;
    float angularVelocityY;
    float angularVelocityY_last;
    public static float angularVelocityY_maxDelta = .1f;

    float squat;




    // should match 'parameters' in Animator
    public Dictionary<string, bool> animBools = new Dictionary<string, bool>{
        {"Stand",   false},
        {"Run",     false},
        {"Limp",    false},
        {"Jump",    false},
        {"Land",    false},
        {"RightArm_weapon",    false},
    };

    public Dictionary<string, int> animInts = new Dictionary<string, int>{
        {"LeftArm_holdStyle",   -1},
    };


    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityAnimation = this;
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
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
    }


    public void SetAnimationBool(string movement, bool value){
        if(animBools[movement] != value){
            animBools[movement] = value;
            animator.SetBool(movement, value);
        }
    }
    public void SetAnimationInt(string movement, int value){
        if(animInts[movement] != value){
            animInts[movement] = value;
            animator.SetInteger(movement, value);
        }
    }

    public void SetAnimationTrigger(string movement){
        animator.SetTrigger(movement);
    }





    public void SetAnimationLayerWeight(string position, float value){
        animator.SetLayerWeight(animator.GetLayerIndex(position), value);
    }
    public void DisableLayer(string layer, float time){
        StartCoroutine(_DisableLayer());
        IEnumerator _DisableLayer(){
            SetAnimationLayerWeight(layer, 0f);
            yield return new WaitForSecondsRealtime(time);
            SetAnimationLayerWeight(layer, 1f);
        }
    }



    void UpdateBodyRotation(){
        Vector3 velRaw = rb.velocity;
        Vector3 velHoriz = velRaw; velHoriz.y = 0;
        Vector3 direction = Vector3.RotateTowards(bodyT.transform.forward, velHoriz, bodyRotationSpeed * Time.deltaTime, 0f);    
        
        Quaternion rot = Quaternion.LookRotation(direction);
        //bodyT.rotation = Quaternion.LookRotation(direction);

        Vector3 v = (rot.eulerAngles) - (bodyRotationLast.eulerAngles);
        angularVelocityY = Mathf.Lerp(bodyAngularVelocity.y, v.y, .5f);
        angularVelocityY = Mathf.Clamp(angularVelocityY, -10, 10);

        float dif = angularVelocityY - angularVelocityY_last;
        if(Math.Abs(dif) > angularVelocityY_maxDelta){
            angularVelocityY = angularVelocityY_last + (angularVelocityY_maxDelta * Mathf.Sign(dif));
        }
        bodyT.rotation = rot * Quaternion.Euler(Vector3.forward*angularVelocityY*-5f);
        
        
        
        // if(tag == "Player"){
        //     Debug.Log(angularVelocityY);
        // }



    }
    void UpdateMovement(){
        Vector3 velRaw = rb.velocity;
        Vector3 velHoriz = velRaw; velHoriz.y = 0;

        if(handle.entityPhysics.GROUNDTOUCH){
            if(velHoriz.magnitude > .001f){
                SetAnimationBool("Run", true);
                SetAnimationBool("Stand", false);
            }
            else{
                SetAnimationBool("Run", false);
                SetAnimationBool("Stand", true);
            }
            SetAnimationBool("Jump", false);
        }
        else{
            
            if(handle.entityPhysics.jumpTime < .1f){
                SetAnimationBool("Jump", true);
            }
            else{
                SetAnimationBool("Jump", false);
            }
            SetAnimationBool("Run", false);
            SetAnimationBool("Stand", false);
        }


        // calculate run
        float bodySkew = Mathf.InverseLerp(0f, 180f, Vector3.Angle(velHoriz, bodyT.forward));
        float slowness = 1f - Mathf.InverseLerp(0f, handle.entityPhysics.maxSpeed, velHoriz.magnitude);
        float runMagnitude = 1f - Mathf.Max(bodySkew, slowness);
        SetAnimationLayerWeight("Legs_full", runMagnitude);

        // calculate backpedal
        float backMagnitude = (1f - runMagnitude) * Mathf.Lerp(0f, .5f, bodySkew);
        SetAnimationLayerWeight("Legs_backpedal", backMagnitude);
        SetAnimationLayerWeight("Legs_shuffle", 1f - backMagnitude - runMagnitude);


        // calculate squat
        SetAnimationLayerWeight("Squat", squat + handle.entityPhysics.landScrunch);

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
        DisableLayer("LeftArm", .5f);
    }


    IEnumerator SquatAndStand(){
        while(squat < .7f){
            squat = Mathf.Lerp(squat, .75f, 10f * Time.deltaTime);
            yield return null;
        }
        while(squat > .05f){
            squat = Mathf.Lerp(squat, 0f, 10f * Time.deltaTime);
            yield return null;
        }
    }


    void FixedUpdate(){
        UpdateBodyRotation();
    }

    void Update(){
        UpdateMovement();
        bodyRotationLast = bodyT.rotation;
        angularVelocityY_last = angularVelocityY;
    }


}
