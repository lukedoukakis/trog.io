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


        Vector3 angularVelocity = rot.eulerAngles - bodyRotationLast.eulerAngles;
        if(tag == "Player"){
            Debug.Log(angularVelocity);
        }
        bodyT.rotation = rot * Quaternion.Euler(Vector3.forward*angularVelocity.y*-2f);
        //bodyT.Rotate(Vector3.forward*angularVelocity.y*10f);
        
        
        
        



    }
    void UpdateMovement(){
        Vector3 velRaw = rb.velocity;
        Vector3 velHoriz = velRaw; velHoriz.y = 0;

        if(velHoriz.magnitude > .001f){
            if(handle.entityPhysics.GROUNDTOUCH){
                SetMovement("Run", true);
                SetMovement("Stand", false);
            }
        }
        else{
            if(handle.entityPhysics.GROUNDTOUCH){
                SetMovement("Run", false);
                SetMovement("Stand", true);
            }
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

    }

    void Update(){
        UpdateBodyRotation();
        UpdateMovement();

        bodyRotationLast = bodyT.rotation;
    }


}
