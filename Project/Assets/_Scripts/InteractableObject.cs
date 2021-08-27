using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    

    public GameObject attachedObject;

    public static void SetAttachedObject(GameObject o, GameObject attachedObject){
        o.GetComponent<InteractableObject>().attachedObject = attachedObject;
    }


}
