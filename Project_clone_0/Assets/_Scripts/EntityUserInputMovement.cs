﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityUserInputMovement : EntityComponent
{


    public bool pressForward, pressBack, pressLeft, pressRight, pressSprint, pressJump;
    public float mouseX, mouseY, mouseZ;

    Quaternion targetRot;
    public GameObject hoveredInteractableObject;
    public List<GameObject> interactableObjects;




    public Vector3 move;


    void Start(){

    }

    

    void ApplyMouseInput(){

        float sensitivity = 5f;
        float smoothing = 300f * Time.deltaTime;

        if(GameManager.cameraMode == 0){
            
        }
        else if(GameManager.cameraMode == 1){

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

        move = Vector3.zero;

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
            //entityPhysics.sprinting = true;
        }
        if (pressJump)
        {
            if(entityPhysics.CanJump()){
                entityPhysics.Jump();
            }
        }

        move = move.normalized;

    }

    void HandleRotation(){
        ApplyMouseInput();
    }

    void HandleAttack(){
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            entityPhysics.Attack();
        }
        else if(Input.GetKeyUp(KeyCode.Mouse0)){
            entityPhysics.Attack();
        }
    }


    void CheckInteraction(){

        if(Input.GetKeyUp(KeyCode.E)){
            OnInteract();
        }

        if(Input.GetKeyUp(KeyCode.Alpha1)){
            entityItems.ToggleWeaponEquipped();
        }
    }
   
    void OnInteract(){

        //Log("Hovered object: " + hoveredInteractableObject.name);

        // if hovering over something, interact with it
        if(hoveredInteractableObject == null){
            entityItems.OnEmptyInteract();
        }

        else{
            switch (hoveredInteractableObject.tag) {
                case "Item" :
                    entityItems.OnObjectInteract(hoveredInteractableObject, hoveredInteractableObject.GetComponent<ScriptableObjectReference>().GetScriptableObject());
                    break;
                case "Human" :
                    // todo: human interact
                    break;
            }   
        }
        
    }

    void CheckUse(){
        if(Input.GetKeyUp(KeyCode.F)){
            entityItems.OnHoldingUse();
        }
    }


    public void UpdateHoveredInteractable(){
        Transform cameraT = Camera.main.transform;
        RaycastHit hit;

        if(Physics.Raycast(cameraT.position, cameraT.forward, out hit, Vector3.Distance(transform.position, cameraT.position) + 2f, LayerMask.GetMask("Item"))){ //todo: add "Entity" layer
            hoveredInteractableObject = hit.collider.gameObject;
            //Log(hoveredInteractableObject.name);
        }
        else{
            hoveredInteractableObject = null;
            //Log("NO HOVERED INTERACTABLE GAMEOBJECT");
        }
    }


    void Update(){

        if(isLocalPlayer){
            HandleMovement();
            if(!UIController.UImode){
                HandleRotation();
            }
            HandleAttack();
            CheckInteraction();
            CheckUse();
        
        }

    }

    void FixedUpdate()
    {
        entityPhysics.moveDir = move;

        UpdateHoveredInteractable();
    }



}
