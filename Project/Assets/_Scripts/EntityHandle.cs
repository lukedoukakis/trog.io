﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// acts as a handle to access every other component of type EntityComponent for this entity
// handles mouse input events
public class EntityHandle : EntityComponent
{
    public GameObject npcPrefab;
    public bool hovering;
    public bool selecting;
    public bool selected;
    public bool tooltip;


    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Log("Starting local player");
    }

    protected override void Awake(){

        base.Awake();

        //Log(isLocalPlayer.ToString());

        if(tag == "Player"){
            Debug.Log("INIT PLAYER");
            InitPlayer(false);
            UIController.current.SetUIMode(false);
        }
    }

    public void InitPlayer(bool fromMemory){

        // set global variables
        GameManager.current.localPlayer = this.gameObject;
        ChunkGenerator.current.playerT = transform;
        CameraController.current.enabled = true;
        CameraController.current.Init(this.transform);
        npcPrefab = Resources.Load<GameObject>("Entities/Npc");

        // init player specific entity settings
        transform.position = new Vector3(0f, 1600f * 3f, 0f);
        Faction faction = Faction.GenerateFaction("Faction " + (Random.Range(0, 10000f)).ToString(), true);
        faction.AddMember(this);
        entityInfo.faction = faction;


        // spawn initial tribe members
        for(int i = 0; i < GameManager.startingTribeMembers; i++){
            GameManager.SpawnNpc(npcPrefab, this.gameObject);
        }









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
        if(Input.GetKeyUp(KeyCode.R)){
            transform.position = new Vector3(Random.Range(-50000f, 50000f), 1650f * 3f, Random.Range(-5000f, 5000f));
        }
        if(isLocalPlayer){
            if(Input.GetKeyUp(KeyCode.LeftControl)){
                GameObject.Find("Torch").transform.position = transform.position + Vector3.up * 3f;
            }
        }
    }
    
}
