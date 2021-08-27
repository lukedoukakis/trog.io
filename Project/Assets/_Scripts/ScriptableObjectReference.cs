using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectReference : MonoBehaviour
{


    ScriptableObject scriptableObject;

    public ScriptableObject GetScriptableObject(){
        return scriptableObject;
    }

    public void SetScriptableObject(ScriptableObject s){
        this.scriptableObject = s;
    }

}
