using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSelectionController : MonoBehaviour
{

    public static GlobalSelectionController current;
    void Awake()
    {
        current = this;
    }

    public List<EntityHandle> SelectingHandles;
    public List<EntityHandle> SelectedHandles;

   
    public void AddToSelecting(EntityHandle handle){
        if(!handle.selecting && !handle.selected){
            handle.SetSelecting(true);
            SelectingHandles.Add(handle);
        }
    }

    public void Select(){
        foreach(EntityHandle handle in SelectingHandles){
            handle.SetSelecting(false);
            handle.SetSelected(true);
            SelectedHandles.Add(handle);
        }
        SelectionMenuController.current.UpdateSelectionMenu();
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
        SelectionMenuController.current.UpdateSelectionMenu();
    }

    public bool SelectionIsEmpty(){
        return SelectedHandles.Count == 0;
    }



    public void SelectAllPlayerFactionMembers(){
        ClearSelected();
        Faction f = GameManager.current.localPlayer.GetComponent<EntityHandle>().entityInfo.faction;
        foreach(EntityHandle handle in f.members){
            AddToSelecting(handle);
        }
        Select();
    }


    // selects all entities with given tag
    public void SelectAllEntitiesWithTag(string tag){
        ClearSelected();
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag(tag)){
            AddToSelecting(obj.GetComponent<EntityHandle>());
        }
        Select();
    }

}
