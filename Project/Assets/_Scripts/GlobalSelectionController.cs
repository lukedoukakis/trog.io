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

    public List<ObjectSelectionManager> SelectingOSMs;
    public List<ObjectSelectionManager> SelectedOSMs;

   
    public void AddToSelecting(ObjectSelectionManager osm){
        if(!osm.selecting && !osm.selected){
            osm.SetSelecting(true);
            SelectingOSMs.Add(osm);
        }
    }

    public void Select(){
        foreach(ObjectSelectionManager osm in SelectingOSMs){
            osm.SetSelecting(false);
            osm.SetSelected(true);
            SelectedOSMs.Add(osm);
        }
        SelectingOSMs.Clear();
    }

    public void RemoveFromSelected(ObjectSelectionManager osm){
        SelectedOSMs.Remove(osm);
        osm.SetSelected(false);
    }

    public void ClearSelected(){
        foreach(ObjectSelectionManager osm in SelectedOSMs){
            osm.SetSelected(false);
        }
        SelectedOSMs.Clear();
    }

    public bool SelectionIsEmpty(){
        return SelectedOSMs.Count == 0;
    }
}
