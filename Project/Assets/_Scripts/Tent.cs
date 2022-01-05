using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tent : CampComponent
{


    public static int MAXIMUM_OCCUPANT_COUNT = 1;

    public List<EntityHandle> occupants;

    void Awake()
    {
        occupants = new List<EntityHandle>();
    }
    
    public override void SetCampComponent(Camp camp)
    {
        base.SetCampComponent(camp);

        SetWorldObject(Utility.InstantiateSameName(CampResources.PREFAB_TENT));
    }

    public void AddOccupant(EntityHandle entityHandle)
    {
        occupants.Add(entityHandle);
    }

    public void RemoveOccupant(EntityHandle entityHandle)
    {
        occupants.Remove(entityHandle);
    }

    public bool IsOpen()
    {
        return GetOccupantCount() < MAXIMUM_OCCUPANT_COUNT;
    }

    public int GetOccupantCount()
    {
        return occupants.Count;
    }

    

}
