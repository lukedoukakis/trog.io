using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tent : CampComponent
{
    
    public override void SetCampComponent(Camp camp)
    {
        base.SetCampComponent(camp);

        SetWorldObject(Utility.InstantiateSameName(CampResources.PREFAB_TENT));
    }

}
