using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class UIEvents : MonoBehaviour
{

    public static UIEvents current;


    bool mouseDown;
    bool hoveringUI;

    void Awake()
    {
        current = this;
    }

    public void OnUnitMouseOver(ObjectSelectionManager osm){
        if(mouseDown){
            GlobalSelectionController.current.AddToSelecting(osm);
        }
        else{
            // show tooltip for pcb
        }
    }

    public void OnUIPointerEnter(){
        hoveringUI = true;
        //Debug.Log("UI Pointer Enter");
    }
    public void OnUIPointerExit(){
        hoveringUI = false;
        //Debug.Log("UI Pointer Exit");
    }

    void OnMouseUp(){

    }


    void Update()
    {
        if(!hoveringUI){
            if(Input.GetKey(KeyCode.Mouse0)){
                if(!mouseDown){
                    mouseDown = true;
                    if(!Input.GetKey(KeyCode.LeftControl)){
                        GlobalSelectionController.current.ClearSelected();

                        SelectionMenuController.current.ClearSelectionMenu();
                        SelectionMenuController.current.SetVisible(false);
                    }
                }
            }
            else{
                if(mouseDown){
                    mouseDown = false;
                    GlobalSelectionController.current.Select();

                    SelectionMenuController.current.UpdateSelectionMenu();
                    if(!GlobalSelectionController.current.SelectionIsEmpty()){
                        SelectionMenuController.current.SetVisible(true);
                    }
                }
            }
        }
        
    }

}
