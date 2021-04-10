using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityUserInputMovement : EntityComponent
{
    bool jumpInput;
    float jumpInput_time;
    static float jumpInput_time_threshhold = .2f;


    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityUserInputMovement = this;
    }

    void Start(){

    }

    void ApplyMouseInput(){
        float y = Input.GetAxis("Mouse X") * 700f * Time.deltaTime;
        float x = 0f;
        float z = 0f;
        Quaternion targetRot = transform.rotation * Quaternion.Euler(new Vector3(x, y, z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f);
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
            jumpInput = true;
        }

        if(jumpInput){
            if(handle.entityPhysics.CanJump()){
                handle.entityPhysics.Jump();
                jumpInput_time = 0f;
            }
        }

        ApplyMouseInput();
        


        moveDir = moveDir.normalized;
        handle.entityPhysics.Move(moveDir, handle.entityPhysics.acceleration);
        
       


        jumpInput_time += Time.deltaTime;
        if(jumpInput_time > jumpInput_time_threshhold){
            jumpInput = false;
        }

    }



}
