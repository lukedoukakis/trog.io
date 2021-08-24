using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anvil : MonoBehaviour
{
    public static GameObject Prefab_Anvil = Resources.Load<GameObject>("Camp/Anvil");

    // --

    public Camp camp;
    public GameObject worldObject;



    public void SetAnvil(Camp camp){
        this.camp = camp;
        this.worldObject = Instantiate(Prefab_Anvil);
    }

}
