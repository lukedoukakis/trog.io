using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityComponent : MonoBehaviour
{

    public EntityHandle handle;





    public void Log(string msg){
        Debug.Log("Entity " + handle.entityInfo.ID + ": " + this.GetType().Name + ": " + msg);
    }
}
