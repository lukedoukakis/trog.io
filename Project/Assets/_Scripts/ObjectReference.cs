using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectReference : MonoBehaviour
{


    object objectReference;

    public object GetObjectReference(){
        return objectReference;
    }

    public void SetObjectReference(object o){
        this.objectReference = o;
    }

}
