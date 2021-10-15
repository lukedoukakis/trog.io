using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSelectionController : MonoBehaviour
{

    public static GlobalSelectionController current;

    public List<EntityHandle> SelectingHandles;
    public List<EntityHandle> SelectedHandles;

    bool mouseDown;
    int hoveredEntities;


    void Awake()
    {
        current = this;
        hoveredEntities = 0;
    }
   
    public void AddToSelecting(EntityHandle handle){
        if(!handle.selecting && !handle.selected){
            handle.SetSelecting(true);
            SelectingHandles.Add(handle);
        }
    }

    public void ApplySelection(){
        foreach(EntityHandle handle in SelectingHandles){
            handle.SetSelecting(false);
            handle.SetSelected(true);
            SelectedHandles.Add(handle);
        }
        UIController.current.UpdateSelectionMenu();
        SelectingHandles.Clear();
    }

    public void RemoveFromSelected(EntityHandle handle){
        SelectedHandles.Remove(handle);
        handle.SetSelected(false);
    }

    public void ClearSelected(){
        foreach(EntityHandle handle in SelectedHandles){
            handle.SetSelected(false);
        }
        SelectedHandles.Clear();
        UIController.current.UpdateSelectionMenu();
    }

    public bool SelectionIsEmpty(){
        return SelectedHandles.Count == 0;
    }



    public void SelectAllPlayerFactionMembers(){
        ClearSelected();
        Faction f = GameManager.current.localPlayer.GetComponent<EntityHandle>().entityInfo.faction;
        foreach(EntityHandle handle in f.memberHandles){
            if(handle != GameManager.current.localPlayer.GetComponent<EntityHandle>()){
                AddToSelecting(handle);
            }
        }
        ApplySelection();
    }


    public void OnEntityMouseOver(EntityHandle handle)
    {

        if(handle.tag == "Player"){ return; }

        // handle tooltip
        if (!handle.tooltip)
        {
            //handle.ShowTooltip();
        }

        // handle selecting
        if (mouseDown)
        {
            if (!handle.selected || !handle.selecting)
            {
                GlobalSelectionController.current.AddToSelecting(handle);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (handle.selected)
            {
                GlobalSelectionController.current.RemoveFromSelected(handle);
            }
        }

        hoveredEntities++;
        
    }


    public void OnEntityMouseExit(EntityHandle handle)
    {

        if(handle.tag == "Player"){ return; }

        handle.HideTooltip();
        hoveredEntities--;
    }


    void Update(){

        //Debug.Log(hoveredEntities);

        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (!mouseDown)
            {
                mouseDown = true;
                if (!Input.GetKey(KeyCode.LeftControl))
                {
                    ClearSelected();
                    UIController.current.ClearSelectionMenu();
                    UIController.current.ToggleUIMode();
                }
            }
        }
        else
        {
            if (mouseDown)
            {
                mouseDown = false;
                ApplySelection();
                if (!SelectionIsEmpty())
                {
                    //SelectionMenuController.current.SetVisible(true);
                }
            }
        }
    }

}
