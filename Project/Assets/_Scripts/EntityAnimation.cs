using System;
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





    // should match 'parameters' in Animator
    public Dictionary<string, bool> movements = new Dictionary<string, bool>{
        {"Stand",   false},
        {"Run",     false},
        {"Limp",    false},
        {"Jump",    false},
        {"Land",    false},
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


    public void SetMovement(string movement, bool value){
        if(movements[movement] != value){
            movements[movement] = value;
            animator.SetBool(movement, value);
        }
    }
    public void SetPositionWeight(string position, float value){
        animator.SetLayerWeight(animator.GetLayerIndex(position), value);
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
                SetMovement("Run", true);
                SetMovement("Stand", false);
            }
            else{
                SetMovement("Run", false);
                SetMovement("Stand", true);
            }
            SetMovement("Jump", false);
        }
        else{
            
            if(handle.entityPhysics.jumpTime < .1f){
                SetMovement("Jump", true);
            }
            else{
                SetMovement("Jump", false);
            }
            SetMovement("Run", false);
            SetMovement("Stand", false);
        }





        float bodySkew = Mathf.InverseLerp(0f, 180f, Vector3.Angle(velHoriz, bodyT.forward));
        float slowness = 1f - Mathf.InverseLerp(0f, handle.entityPhysics.maxSpeed, velHoriz.magnitude);
        float runMagnitude = 1f - Mathf.Max(bodySkew, slowness);
        SetPositionWeight("Legs_full", runMagnitude);

        float backMagnitude = (1f - runMagnitude) * Mathf.Lerp(0f, .5f, bodySkew);
        SetPositionWeight("Legs_backpedal", backMagnitude);
        SetPositionWeight("Legs_shuffle", 1f - backMagnitude - runMagnitude);
        SetPositionWeight("Squat", handle.entityPhysics.landScrunch);

        
        

    }


    void FixedUpdate(){
        UpdateBodyRotation();
    }

    void Update(){
        //UpdateBodyRotation();
        UpdateMovement();

        bodyRotationLast = bodyT.rotation;
        angularVelocityY_last = angularVelocityY;
    }


}
