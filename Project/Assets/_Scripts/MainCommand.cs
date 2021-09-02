﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCommand : MonoBehaviour
{


    public static MainCommand current;


    public string command;


    void Awake(){
        current = this;
    }

    public void SendCommand(string c){
        //Debug.Log("Sending command to selected");
        EntityBehavior behavior;
        foreach(EntityHandle handle in GlobalSelectionController.current.SelectedHandles.ToArray()){
            behavior = handle.entityBehavior;
            behavior.InsertActionImmediate(Action.GenerateAction(c, behavior.entityHandle), true);
        }
        //Debug.Log("Commands sent!");
    }



    // Update is called once per frame
    void Update()
    {

        
    }
}
