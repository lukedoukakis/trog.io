using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    ObjectPhysics physics;
    Rigidbody rigidbody;


    void Awake(){
        physics = GetComponent<ObjectPhysics>();
        rigidbody = GetComponent<Rigidbody>();
    }
    void Update()
    {

        Vector3 moveDir = Vector3.zero;


        if (Input.GetKey(KeyCode.W))
        {
            moveDir.z += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir.z -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir.x += 1;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            if(physics.CanJump()){
                physics.Jump();
            }
        }
        moveDir = moveDir.normalized;
        physics.Move(moveDir, ObjectBehavior.acceleration);
        
       




    }



}
