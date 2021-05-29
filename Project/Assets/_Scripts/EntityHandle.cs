using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// acts as a handle to access every other component of type EntityComponent for this entity
// handles mouse input events
public class EntityHandle : EntityComponent
{

    public EntityInfo entityInfo;
    public EntityStats entityStats;
    public EntityBehavior entityBehavior;
    public EntityAnimation entityAnimation;
    public EntityPhysics entityPhysics;
    public EntityStatus entityStatus;
    public EntityItems entityItems;
    public EntityUserInputMovement entityUserInputMovement;


    public bool selecting;
    public bool selected;
    public bool tooltip;

    void Awake(){
        handle = this;
        
    }


    void Start(){
        InitEntity(false);
    }

    void InitEntity(bool fromMemory){


        // set camera
        if(isLocalPlayer){
            //Debug.Log("Setting player");
            ChunkGenerator.current.playerT = transform;
            CameraController.current.enabled = true;
            CameraController.current.Init(this.transform);
            transform.position = new Vector3(0f, 1650f, 0f);

        }

        // if loading from memory
        if(fromMemory){
            // TODO: initialize every EntityComponent for this handle with values from memory

            // EntityInfo


            // EntityStats


            // EntityPhysics


            // EntityBehavior


            // EntityAnimation


            // EntityItems


            // EntityStatus


            // EntityUserInputMovement


            // EntityInfo
        }

        // if creating a new entity
        else{

            // EntityInfo
    

            // EntityStats


            // EntityPhysics


            // EntityBehavior


            // EntityAnimation


            // EntityItems


            // EntityStatus


            // EntityUserInputMovement


            // EntityInfo



        }
    }




    public void SetSelecting(bool b){
        selecting = b;
        if(b){
            //rdr.sharedMaterial = mat_selecting;
        }
    }
    public void SetSelected(bool b){
        selected = b;
        if(b){
            //rdr.sharedMaterial = mat_selected;
        }
        else{
            //rdr.sharedMaterial = mat_none;
        }
    }

    public void ShowTooltip(){
        tooltip = true;
        TooltipController.current.SetText(entityStats.CreateStatsList());
        TooltipController.current.Show(TooltipController.DefaultDelay);
    }
    public void HideTooltip(){
        tooltip = false;
        TooltipController.current.Hide();
    }


    void OnMouseOver(){
        UIEvents.current.OnUnitMouseOver(this);
    }

    void OnMouseExit(){
        UIEvents.current.OnUnitMouseExit(this);
    }





    // Update is called once per frame
    void Update()
    {
        
    }
    
}
