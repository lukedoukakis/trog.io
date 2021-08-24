using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tent : ScriptableObject
{
    
    public Camp camp;
    public GameObject worldObject;



    public void SetTent(Camp camp){
        this.camp = camp;
        this.worldObject = Instantiate(CampResources.Prefab_Tent);
    }

}
