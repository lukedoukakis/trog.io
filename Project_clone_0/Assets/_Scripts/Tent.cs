using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tent : CampComponent
{
    
    public Camp camp;



    public void SetTent(Camp camp){
        this.camp = camp;
        SetWorldObject(Utility.InstantiateSameName(CampResources.PREFAB_TENT));
    }

    public void DeleteSelf(){
        GameObject.Destroy(worldObject);
        ScriptableObject.Destroy(this);
    }

}
