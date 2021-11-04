using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonfire : CampComponent
{

    public bool lit;
    public float intensity;
    public float scale;


    public void SetBonfire(Camp camp, bool lit, float intensity, float scale){
        this.lit = lit;
        SetWorldObject(lit ? Utility.InstantiateSameName(CampResources.PREFAB_BONFIRE_LIT) : Utility.InstantiateSameName(CampResources.PREFAB_BONFIRE_UNLIT));
        this.intensity = intensity;
        this.scale = scale;
    }
}
