using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonfire : CampComponent
{

    public Camp camp;
    public bool lit;
    public float intensity;
    public float scale;


    public void SetBonfire(Camp camp, bool lit, float intensity, float scale){
        this.camp = camp;
        this.lit = lit;
        SetWorldObject(lit ? Utility.InstantiatePrefabSameName(CampResources.PREFAB_BONFIRE_LIT) : Utility.InstantiatePrefabSameName(CampResources.PREFAB_BONFIRE_UNLIT));
        this.intensity = intensity;
        this.scale = scale;
    }
}
