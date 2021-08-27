using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectReference : MonoBehaviour
{


    ScriptableObject scriptableObjectReference;

    public ScriptableObject GetScriptableObject(){
        return scriptableObjectReference;
    }

    public void SetScriptableObjectReference(ScriptableObject s){
        this.scriptableObjectReference = s;
    }

}
