using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command : MonoBehaviour
{

    public static Command current;


    public int command;


    void Awake(){
        current = this;
    }


    public void SetCommand(int c){
        command = c;
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
