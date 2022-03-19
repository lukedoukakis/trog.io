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
    public EntityHealthDisplayController entityHealthDisplayController;



    protected override void Awake()
    {
        this.fieldName = "entityHandle";
        base.Awake();
        entityHealthDisplayController = GetComponentInChildren<EntityHealthDisplayController>();

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
        entityHealthDisplayController.UpdateDisplay(this);
    }
    
}
