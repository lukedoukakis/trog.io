using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCommand : MonoBehaviour
{


    public static MainCommand current;


    public int command;


    void Awake(){
        current = this;
    }

    public void SendCommand(int c){
        //Debug.Log("Sending command to selected");
        EntityBehavior behavior;
        foreach(EntityHandle handle in GlobalSelectionController.current.SelectedHandles.ToArray()){
            behavior = handle.entityBehavior;
            behavior.ProcessCommand(c, (int)EntityBehavior.Priority.FrontImmediate);
        }
        //Debug.Log("Commands sent!");
    }



    // Update is called once per frame
    void Update()
    {

        
    }
}
