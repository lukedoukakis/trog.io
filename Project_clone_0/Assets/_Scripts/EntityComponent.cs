using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EntityComponent : NetworkBehaviour
{

    public EntityHandle handle;


    void Init(){
        
    }


    public void Log(string msg){
        Debug.Log("Entity " + handle.entityInfo.id + ": " + this.GetType().Name + ": " + msg);
    }
}
