using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// acts as a handle to access every other component of type EntityComponent for this entity
// handles mouse input events
public class EntityHandle : EntityComponent
{
    public bool hovering;
    public bool selecting;
    public bool selected;
    public bool tooltip;
    public bool localP;


    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        if(isLocalPlayer){
            localP = true;
            InitAsLocalPlayer(false);
        }
    }

    public void InitAsLocalPlayer(bool fromMemory){

        // set global variables
        GameManager.current.SetLocalPlayer(this.gameObject);
        Testing.instance.playerHandle = this.gameObject.GetComponent<EntityHandle>();
        ChunkGenerator.current.playerT = transform;
        CameraController.current.enabled = true;
        CameraController.current.Init(this.transform);

        // init player specific entity settings
        transform.position = new Vector3(0f, 4720f, 0f);
        StartCoroutine(entityCommandServer.SetNewFactionWhenReady(this));

        // spawn initial tribe members
        for(int i = 0; i < GameManager.startingTribeMembers; i++){
            StartCoroutine(entityCommandServer.SpawnNpcWhenReady(this));
        }

        UIController.current.SetUIMode(false);









        // if loading from memory
        if(fromMemory){
            
        }

        // if new game
        else{

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
        if(!hovering){
            hovering = true;
            GlobalSelectionController.current.OnEntityMouseOver(this);
        }
    }

    void OnMouseExit(){
        hovering = false;
        GlobalSelectionController.current.OnEntityMouseExit(this);
    }





    // Update is called once per frame
    void Update()
    {
        
    }
    
}
