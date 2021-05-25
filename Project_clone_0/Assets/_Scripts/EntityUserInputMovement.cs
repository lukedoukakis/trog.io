using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityUserInputMovement : EntityComponent
{


    public bool pressForward, pressBack, pressLeft, pressRight;
    public List<GameObject> interactableObjects;
    public Vector3 move;

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

    void HandleMovement(){

        pressForward = Input.GetKey(KeyCode.W);
        pressBack = Input.GetKey(KeyCode.S);
        pressLeft = Input.GetKey(KeyCode.A);
        pressRight = Input.GetKey(KeyCode.D);

        if (pressForward)
        {
            move.z += 1;
        }
        if (pressBack)
        {
            move.z -= 1;
        }
        if (pressLeft)
        {
            move.x -= 1;
        }
        if (pressRight)
        {
            move.x += 1;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            if(handle.entityPhysics.CanJump()){
                handle.entityPhysics.Jump();
            }
        }

        move = move.normalized;

    }

    void HandleRotation(){
        ApplyMouseInput();
    }

    void HandleAttack(){
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            handle.entityAnimation.UseWeapon();
        }
    }

    void CheckInteractableItems(){
        interactableObjects = handle.entityBehavior.SenseSurroundingItems(-1, null, EntityBehavior.senseDistance_immediate, handle.entityInfo.faction.warringFactions);
    }

    void Interact(){
        
        if(interactableObjects.Count != 0){
            interactableObjects = interactableObjects.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).ToList();
            GameObject obj = interactableObjects[0];
            handle.entityBehavior.TakeFromGround(obj);            
        }
        
    }

    void Update(){

        move = Vector3.zero;
        if(isLocalPlayer){
            HandleMovement();
            HandleRotation();
            HandleAttack();
            if(Input.GetKeyUp(KeyCode.E)){
                CheckInteractableItems();
                Interact();
            }
        }

        

    }

    void FixedUpdate()
    {
        handle.entityPhysics.moveDir = move;
    }



}
