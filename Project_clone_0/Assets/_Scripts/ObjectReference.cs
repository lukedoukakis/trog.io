using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectReference : MonoBehaviour
{


    object objectReference;
    Faction owningFaction;

    public object GetObjectReference(){
        return objectReference;
    }

    public void SetObjectReference(object o){
        objectReference = o;
    }

    public Faction GetOwningFaction()
    {
        return owningFaction;
    }

    public void SetOwningFaction(Faction faction)
    {
        owningFaction = faction;
    }

}
