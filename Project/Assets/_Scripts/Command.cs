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
        Debug.Log("Sending command to selected");
        ObjectBehavior behavior;
        foreach(ObjectSelectionManager osm in GlobalSelectionController.current.SelectedOSMs.ToArray()){
            behavior = osm.behavior;
            behavior.ProcessCommand(c, (int)ObjectBehavior.Priority.FrontImmediate);
        }
        Debug.Log("Commands sent!");
    }



    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKeyUp(KeyCode.Mouse1)){
        //     SendCommand((int)ObjectBehavior.Command.Go_home);
        // }

        
    }
}
