using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{

    public static Utility instance;

    void Awake(){
        instance = this;
    }

    public static Vector3 GetHorizontalVector(Vector3 vector)
    {
        float yComponent = vector.y;
        vector.y = 0f;
        return vector;
    }

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


    public static void ToggleObjectPhysics(GameObject o, bool nonTriggers, bool triggers, bool rigidbodies, bool gravity){
        if (o != null)
        {
            Collider[] cols = o.GetComponentsInChildren<Collider>();
            if (cols.Length > 0)
            {
                foreach(Collider col in cols){
                    if(col.isTrigger){
                        col.enabled = triggers;
                    }
                    else{
                        col.enabled = nonTriggers;
                    }
                }
            }

            Rigidbody[] rbs = o.GetComponentsInChildren<Rigidbody>();
            if (rbs.Length > 0)
            {
                foreach(Rigidbody rb in rbs){
                    rb.isKinematic = !rigidbodies;
                    rb.useGravity = gravity;
                }
            }
        }   
    }

    public static void IgnorePhysicsCollisions(GameObject o, Collider colliderToIgnore)
    {
        Collider[] cols = o.GetComponentsInChildren<Collider>();
        if (cols.Length > 0)
        {
            foreach (Collider col in cols)
            {
                Physics.IgnoreCollision(col, colliderToIgnore, true);
            }
        }
    }

    public static void IgnorePhysicsCollisions(GameObject o, Collider[] collidersToIgnore)
    {
        foreach(Collider colliderToIgnore in collidersToIgnore)
        {
            IgnorePhysicsCollisions(o, colliderToIgnore);
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

    public static Vector3 GetRandomVector(float magnitude){
        return new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f)).normalized * magnitude;
    }

    public static Quaternion GetRandomRotation(float maxDegrees){
        return Quaternion.Euler(UnityEngine.Random.Range(0f, maxDegrees), UnityEngine.Random.Range(0f, maxDegrees), UnityEngine.Random.Range(0f, maxDegrees));
    }


    // return the first scriptable object reference found in a transform or any of its parents
    public static ObjectReference FindScriptableObjectReference(Transform t){
        Transform parent = t;
        ObjectReference sor = parent.GetComponent<ObjectReference>();
        while(sor == null){
            parent = parent.parent;
            if(parent == null){ break; }
            sor = parent.GetComponent<ObjectReference>();
        }
        return sor;
    }

    public static void DestroyInSeconds(GameObject o, float seconds){
        instance.StartCoroutine(instance._DestroyInSeconds(o, seconds));
    }
    IEnumerator _DestroyInSeconds(GameObject o, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        GameObject.Destroy(o);
    }

    public static bool RandomBoolean()
    {
        return UnityEngine.Random.value >= 0.5f;
    }

}


public class Ref<T>
{
    private T backing;
    public T Value { get { return backing; } set { backing = value; } }
    public Ref(T reference)
    {
        backing = reference;
    }
}
