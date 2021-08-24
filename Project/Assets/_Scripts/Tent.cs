using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tent : MonoBehaviour
{
    public static GameObject Prefab_Tent = Resources.Load<GameObject>("Camp/Tent");

    // --

    public Camp camp;
    public GameObject worldObject;



    public void SetTent(Camp camp){
        this.camp = camp;
        this.worldObject = Instantiate(Prefab_Tent);
    }

}
