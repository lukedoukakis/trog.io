using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAnimation : MonoBehaviour
{

    Rigidbody rb;
    public Transform bodyT;
    public Animator animator;
    public ObjectPhysics physics;

    float weight_backpedal;



    // should match 'parameters' in Animator
    enum Movement{
        Stand, Run, Limp,
    }


    void Awake(){
        rb = GetComponent<Rigidbody>();
        foreach(Transform tr in transform)
        {
            if(tr.tag == "Body")
            {
                bodyT =  tr;
                break;
            }
        }
        animator = GetComponentInChildren<Animator>();
        physics = GetComponent<ObjectPhysics>();
        SetAnimation((int)Movement.Run, true);
    }

    public void SetAnimation(int movement, bool value){
        animator.SetBool(Enum.GetName(typeof(Movement), movement), value);
    }

    void UpdateBodyRotation(){
        Vector3 vel = rb.velocity;
        vel.y = 0f;

    }


    void Update(){
        UpdateBodyRotation();
    }


}
