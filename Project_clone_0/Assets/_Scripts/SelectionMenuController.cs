﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionMenuController : MonoBehaviour
{
    public bool visible;


    public static SelectionMenuController current;
    void Awake()
    {
        current = this;
    }

    public void UpdateSelectionMenu(){
        UnitMenuController.current.ClearButtons();
        foreach(EntityHandle handle in GlobalSelectionController.current.SelectedHandles.ToArray()){
            UnitMenuController.current.AddButton(handle);
        }
    }

    public void ClearSelectionMenu(){
        UnitMenuController.current.ClearButtons();

    }

    public void SetVisible(bool b){
        if(b && !visible){
            GetComponent<RectTransform>().localScale = Vector2.one;
        }
        else if(!b && visible){
            GetComponent<RectTransform>().localScale = Vector2.zero;
        }
        visible = b;
    }
    public void ToggleVisible(){
        SetVisible(!visible);
    }

    




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
