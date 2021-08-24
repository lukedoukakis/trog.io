﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anvil : ScriptableObject
{
    
    public Camp camp;
    public GameObject worldObject;



    public void SetAnvil(Camp camp){
        this.camp = camp;
        this.worldObject = Instantiate(CampResources.Prefab_Anvil);
    }

}
