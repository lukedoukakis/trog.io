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

   
    public void AddToSelecting(EntityHandle osm){
        if(!osm.selecting && !osm.selected){
            osm.SetSelecting(true);
            SelectingHandles.Add(osm);
        }
    }

    public void Select(){
        foreach(EntityHandle osm in SelectingHandles){
            osm.SetSelecting(false);
            osm.SetSelected(true);
            SelectedHandles.Add(osm);
        }
        SelectingHandles.Clear();
    }

    public void RemoveFromSelected(EntityHandle osm){
        SelectedHandles.Remove(osm);
        osm.SetSelected(false);
    }

    public void ClearSelected(){
        foreach(EntityHandle osm in SelectedHandles){
            osm.SetSelected(false);
        }
        SelectedHandles.Clear();
    }

    public bool SelectionIsEmpty(){
        return SelectedHandles.Count == 0;
    }
}
