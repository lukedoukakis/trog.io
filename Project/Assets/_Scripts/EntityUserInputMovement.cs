using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityUserInputMovement : EntityComponent
{


    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityUserInputMovement = this;
    }

    void Start(){

    }

    void ApplyMouseInput(){

        if(GameManager.current.cameraMode == 0){
            
        }
        else if(GameManager.current.cameraMode == 1){
            float y = Input.GetAxis("Mouse X") * 700f * Time.deltaTime;
            float x = 0f;
            float z = 0f;
            Quaternion targetRot = transform.rotation * Quaternion.Euler(new Vector3(x, y, z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f);
        }
        
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
            if(handle.entityPhysics.CanJump()){
                handle.entityPhysics.Jump();
            }
        }

        ApplyMouseInput();
        


        moveDir = moveDir.normalized;
        handle.entityPhysics.Move(moveDir, handle.entityPhysics.acceleration);
        
    
    }



}
