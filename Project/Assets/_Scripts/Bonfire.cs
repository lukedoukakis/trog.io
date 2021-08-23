using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonfire : MonoBehaviour
{

    public static GameObject Prefab_bonfireUnlit = Resources.Load<GameObject>("Camp/Bonfire");
    public static GameObject Prefab_bonfireLit = Resources.Load<GameObject>("Camp/Bonfire");


    public Camp camp;
    public GameObject worldObject;
    public bool lit;
    public float intensity;
    public float scale;


    public void SetBonfire(Camp camp, bool lit, float intensity, float scale){
        this.camp = camp;
        this.lit = lit;
        this.worldObject = lit ? Prefab_bonfireLit : Prefab_bonfireUnlit;
        this.intensity = intensity;
        this.scale = scale;
    }
}
