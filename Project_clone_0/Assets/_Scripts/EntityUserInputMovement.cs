using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityUserInputMovement : EntityComponent
{


    public bool pressForward, pressBack, pressLeft, pressRight, pressSprint, pressJump;
    public float mouseX, mouseY, mouseZ;

    Quaternion targetRot;
    public List<GameObject> interactableObjects;
    public Vector3 move;

    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityUserInputMovement = this;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        GameManager.current.localPlayer = this.gameObject;
    }

    void Start(){

    }

    

    void ApplyMouseInput(){

        float sensitivity = 5f;
        float smoothing = 300f * Time.deltaTime;

        if(GameManager.current.cameraMode == 0){
            
        }
        else if(GameManager.current.cameraMode == 1){

            float deltaY = Input.GetAxis("Mouse X") * sensitivity * smoothing;
            mouseY = Mathf.Clamp(Mathf.Lerp(mouseY, deltaY, 1f / smoothing), mouseY - 2f, mouseY + 2f);

            mouseX = 0f;
            mouseZ = 0f;

            //Log(mouseY.ToString());

            targetRot = Quaternion.Slerp(transform.rotation, transform.rotation * Quaternion.Euler(new Vector3(mouseX, mouseY, mouseZ)), 1f / smoothing);
            transform.rotation = targetRot;
           
        }
        
    }

    void HandleMovement(){

        pressForward = Input.GetKey(KeyCode.W);
        pressBack = Input.GetKey(KeyCode.S);
        pressLeft = Input.GetKey(KeyCode.A);
        pressRight = Input.GetKey(KeyCode.D);
        pressSprint = Input.GetKey(KeyCode.LeftShift);
        pressJump = Input.GetKey(KeyCode.Space);

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
        if(pressSprint){
            //handle.entityPhysics.sprinting = true;
        }
        if (pressJump)
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


    void CheckInteraction(){
        if(Input.GetKeyUp(KeyCode.E)){
            CheckInteractableItems();
            Interact();
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
            CheckInteraction();
            
        }

    }

    void FixedUpdate()
    {
        handle.entityPhysics.moveDir = move;
    }



}
