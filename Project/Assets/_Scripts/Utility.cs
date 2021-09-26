using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{

    public static Transform FindDeepChild(Transform parentT, string name)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(parentT);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == name)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }

    public static GameObject InstantiatePrefabSameName(GameObject prefab){
        GameObject instance = Instantiate(prefab);
        instance.name = prefab.name;
        return instance;
    }

    public static Transform FindDeepChildWithTag(Transform parentT, string _tag)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(parentT);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.tag == _tag)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }

    public static IEnumerator DespawnObject(GameObject gameObject, float delay){
        yield return new WaitForSeconds(delay);
        GameObject.Destroy(gameObject);
    }


    public static void ToggleObjectPhysics(GameObject o, bool value, bool toggleTriggers){
        if (o != null)
        {
            Collider[] cols = o.GetComponentsInChildren<BoxCollider>();
            Rigidbody[] rbs = o.GetComponentsInChildren<Rigidbody>();
            if (cols.Length > 0)
            {
                foreach(Collider col in cols){
                    if(!col.isTrigger || toggleTriggers)
                    {
                        col.enabled = value;
                    }
                }
            }
            if (rbs.Length > 0)
            {
                foreach(Rigidbody rb in rbs){
                    rb.isKinematic = !value;
                }
            }
        }   
    }

    public static bool IsBetween(float testValue, float bound1, float bound2)
    {
        if (bound1 > bound2)
            return testValue >= bound2 && testValue <= bound1;
        return testValue >= bound1 && testValue <= bound2;
    }

    public static Vector3 GetRandomVectorOffset(Vector3 position, float offsetMagnitude, bool mustBeOnLand)
    {
        Vector3 v = position + new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f)) * offsetMagnitude;
        
        RaycastHit hit;
        if (Physics.Raycast(v, Vector3.up, out hit, float.MaxValue, LayerMask.GetMask("Terrain")))
        {
            v.y = hit.point.y;
        }
        if(mustBeOnLand)
        {
            if(Physics.Raycast(v, Vector3.down, out hit, float.MaxValue, LayerMask.GetMask("Terrain"))){
                if(hit.point.y > ChunkGenerator.SeaLevel * ChunkGenerator.ElevationAmplitude){
                    v.y = hit.point.y;
                }
                else{
                    v.y = ChunkGenerator.SeaLevel * ChunkGenerator.ElevationAmplitude;
                }
            }
        }
        
        
        return v;
    }


    // return the first scriptable object reference found in a transform or any of its parents
    public static ScriptableObjectReference FindScriptableObjectReference(Transform t){
        Transform parent = t;
        ScriptableObjectReference sor = parent.GetComponent<ScriptableObjectReference>();
        while(sor == null){
            parent = parent.parent;
            if(parent == null){ break; }
            sor = parent.GetComponent<ScriptableObjectReference>();
        }
        return sor;
    }

}
