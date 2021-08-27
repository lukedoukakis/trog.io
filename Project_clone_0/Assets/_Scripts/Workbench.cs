using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workbench : ScriptableObject
{
    
    public Camp camp;
    public GameObject worldObject;



    public void SetWorkbench(Camp camp){
        this.camp = camp;
        this.worldObject = Utility.InstantiatePrefabSameName(CampResources.Prefab_Workbench);
    }

}
