using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonfire : ScriptableObject
{

    public Camp camp;
    public GameObject worldObject;
    public bool lit;
    public float intensity;
    public float scale;


    public void SetBonfire(Camp camp, bool lit, float intensity, float scale){
        this.camp = camp;
        this.lit = lit;
        this.worldObject = lit ? Utility.InstantiatePrefabSameName(CampResources.Prefab_BonfireLit) : Utility.InstantiatePrefabSameName(CampResources.Prefab_BonfireUnlit);
        this.intensity = intensity;
        this.scale = scale;
    }
}
