using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tent : ScriptableObject
{
    
    public Camp camp;
    public GameObject worldObject;



    public void SetTent(Camp camp){
        this.camp = camp;
        this.worldObject = Utility.InstantiatePrefabSameName(CampResources.PREFAB_TENT);
    }

    public void DeleteSelf(){
        GameObject.Destroy(worldObject);
        ScriptableObject.Destroy(this);
    }

}
