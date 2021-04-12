using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHandle : EntityComponent
{

    public EntityInfo entityInfo;
    public EntityStats entityStats;
    public EntityBehavior entityBehavior;
    public EntityAnimation entityAnimation;
    public EntityPhysics entityPhysics;
    public EntityStatus entityStatus;
    public EntityInventory entityInventory;
    public EntityUserInputMovement entityUserInputMovement;




    public SkinnedMeshRenderer rdr;
    public Material mat_none, mat_selecting, mat_selected;

    public bool selecting;
    public bool selected;
    public bool tooltip;

    void Awake(){
        handle = this;
        rdr = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    void Start(){

    }


    public void SetSelecting(bool b){
        selecting = b;
        if(b){
            rdr.sharedMaterial = mat_selecting;
        }
    }
    public void SetSelected(bool b){
        selected = b;
        if(b){
            rdr.sharedMaterial = mat_selected;
        }
        else{
            rdr.sharedMaterial = mat_none;
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
