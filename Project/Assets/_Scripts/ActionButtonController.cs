using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionButtonController : MonoBehaviour
{

    public UnitButtonController ubc;
    public string command;




    public void SendCommand(){
        //Debug.Log("ActionButtonController.SendCommand(): Sending command to unit!");
        ubc.handle.entityBehavior.InsertActionAndExecuteAsap(ActionParameters.GenerateActionParameters(command, ubc.handle), true);
    }

}
