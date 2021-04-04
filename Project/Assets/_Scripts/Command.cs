using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command : MonoBehaviour
{

    public static Command current;


    public Action command;


    void Awake(){
        current = this;
    }


    public void SetCommand(Action action){
        command = action;
    }

    public void SendCommand(){
        ObjectBehavior behavior;
        foreach(ObjectSelectionManager osm in GlobalSelectionController.current.SelectedOSMs){
            behavior = osm.behavior;
            behavior.QueueAction(command, (int)ObjectBehavior.Priority.FrontImmediate);
        }
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
