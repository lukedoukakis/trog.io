using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAnimation : EntityComponent
{

    public Rigidbody rb;
    public Transform bodyT;
    public Animator animator;

    public static float bodyRotationSpeed_player = .2f;
    public static float bodyRotationSpeed_ai = .01f;
    float bodyRotationSpeed;



    // should match 'parameters' in Animator
    public Dictionary<string, bool> movements = new Dictionary<string, bool>{
        {"Stand",   false},
        {"Run",     false},
        {"Limp",    false},
    };

    // stay set according to 'layers' in Animator
    public static Dictionary<string, int> AnimationLayers = new Dictionary<string, int>{
        {"Legs_full", 1},
        {"Legs_shuffle", 2},
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


    public void SetAnimation(string movement, bool value){
        if(movements[movement] != value){
            movements[movement] = value;
            animator.SetBool(movement, value);
        }
    }

    void UpdateBodyRotation(){
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        Vector3 direction = Vector3.RotateTowards(bodyT.transform.forward, vel, 5f * Time.deltaTime, 0f);
        bodyT.rotation = Quaternion.LookRotation(direction);
    }
    void UpdateMovementAnimation(){
        Vector3 velRaw = rb.velocity;
        Vector3 velHoriz = velRaw; velHoriz.y = 0;

        if(velHoriz.magnitude > .5f){
            if(handle.entityPhysics.GROUNDTOUCH){
                SetAnimation("Run", true);
                SetAnimation("Stand", false);
            }
        }
        else{
            if(handle.entityPhysics.GROUNDTOUCH){
                SetAnimation("Stand", true);
                SetAnimation("Run", false);
            }
        }

        float bodySkew = Mathf.InverseLerp(0f, 180f, Vector3.Angle(velHoriz, bodyT.forward));
        float slowness = 1f - Mathf.InverseLerp(0f, handle.entityPhysics.maxSpeed, velHoriz.magnitude);
        float shuffleMagnitude = Mathf.Max(bodySkew, slowness);
        animator.SetLayerWeight(AnimationLayers["Legs_full"], 1f - shuffleMagnitude);
        animator.SetLayerWeight(AnimationLayers["Legs_shuffle"], shuffleMagnitude);
    }

    void FixedUpdate(){

    }

    void Update(){
        UpdateBodyRotation();
        UpdateMovementAnimation();
    }


}
