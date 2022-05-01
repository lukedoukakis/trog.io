using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{

    public static Utility instance;

    System.Random rand = new System.Random();

    void Awake(){
        instance = this;
        rand = new System.Random();
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

    public static GameObject InstantiateSameName(GameObject prefab){
        //Debug.Log("Prefab name: " + prefab.name);
        GameObject instance = Instantiate(prefab);
        instance.name = prefab.name;
        return instance;
    }
    public static GameObject InstantiateSameName(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject instance = InstantiateSameName(prefab);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
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

    public static void IgnorePhysicsCollisions(Transform t, Collider colliderToIgnore)
    {
        Collider[] cols = t.GetComponentsInChildren<Collider>();
        if (cols.Length > 0)
        {
            foreach (Collider col in cols)
            {
                //Debug.Log("Ignoring collision with col: " + t.gameObject.name);
                Physics.IgnoreCollision(col, colliderToIgnore, true);
            }
        }
    }

    public static void IgnorePhysicsCollisions(Transform t0, Transform t1)
    {
        Collider[] collidersToIgnore = t1.GetComponentsInChildren<Collider>();
        foreach(Collider colliderToIgnore in collidersToIgnore)
        {
            IgnorePhysicsCollisions(t0, colliderToIgnore);
        }
    }

    public static void IgnorePhysicsCollisions(Transform t0, Transform[] t1)
    {
        foreach(Transform _t in t1)
        {
            IgnorePhysicsCollisions(t0, _t);
        }
    }


    public static bool IsBetween(float testValue, float bound1, float bound2)
    {
        if (bound1 > bound2)
        {
            return testValue >= bound2 && testValue <= bound1;
        }
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
                if(hit.point.y > ChunkGenerator.SeaLevel * ChunkGenerator.Amplitude){
                    v.y = hit.point.y;
                }
                else{
                    v.y = ChunkGenerator.SeaLevel * ChunkGenerator.Amplitude;
                }
            }
        }
        
        
        return v;
    }

    public static Vector3 GetRandomVector(float magnitude){
        return new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f)).normalized * magnitude;
    }

    public static Vector3 GetRandomVectorHorizontal(float magnitude)
    {
        return new Vector3(UnityEngine.Random.Range(-100f, 100f), 0f, UnityEngine.Random.Range(-100f, 100f)).normalized * magnitude;
    }

    public static Quaternion GetRandomRotation(float maxDegrees){
        return Quaternion.Euler(UnityEngine.Random.Range(0f, maxDegrees), UnityEngine.Random.Range(0f, maxDegrees), UnityEngine.Random.Range(0f, maxDegrees));
    }

    public static bool GetRandomBoolean()
    {
        return instance.rand.NextDouble() >= 0.5;
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

    public static bool IsInHierarchy(Transform baseChild, Transform target)
    {
        Transform compare = baseChild;
        while(!ReferenceEquals(compare, target))
        {
            compare = compare.parent;
            if(compare == null)
            {
                return false;
            }
        }
        return true;
    }


    public static IEnumerator FlipForTime(GameObject worldObject, float upwardTranslation, float flipForce, float flipTime)
    {

        if(worldObject == null){ yield break; }

        Vector3 targetPos = worldObject.transform.position + Vector3.up * upwardTranslation;
        float spinForce = flipForce;
        for (int i = 0; i * .01f < flipTime; ++i)
        {
            worldObject.transform.Rotate((Vector3.up + Vector3.right) * spinForce);
            worldObject.transform.position = Vector3.Lerp(worldObject.transform.position, targetPos, 10f * Time.deltaTime);
            spinForce *= .8f;
            yield return new WaitForSecondsRealtime(.01f);
            if(worldObject == null){ yield break; }
        }
        Quaternion targetRot = Quaternion.identity;
    }

    public static void SetGlobalScale(Transform transform, Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
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

    public IEnumerator FlyObjectToPosition(GameObject worldObject, Transform targetT, bool doFlip, bool destroyWhenDone, float delay)
    {

        yield return new WaitForSecondsRealtime(delay);

        if(worldObject == null)
        {   
            yield break;
        }
        if(targetT == null)
        {
            if(destroyWhenDone)
            {
                GameObject.Destroy(worldObject);
            }
            yield break;
        }


        Utility.ToggleObjectPhysics(worldObject, false, false, false, false);

        if (doFlip)
        {

            yield return StartCoroutine(FlipForTime(worldObject, 3f, 1000f, .25f));

            if (worldObject == null)
            {
                yield break;
            }
            if (targetT == null)
            {
                if (destroyWhenDone)
                {
                    GameObject.Destroy(worldObject);
                }
                yield break;
            }
        }

        // move object to location before destroying
        Vector3 worldObjectPosition = worldObject.transform.position;
        Vector3 targetPosition = targetT.position;
        while (Vector3.Distance(worldObjectPosition, targetPosition) > .5f)
        {
            worldObject.transform.position = Vector3.Lerp(worldObject.transform.position, targetT.position, ObjectRack.OBJECT_MOVEMENT_ANIMATION_SPEED * Time.deltaTime);
            worldObject.transform.Rotate(Vector3.right * 10f);
            yield return null;

            if (worldObject == null)
            {
                yield break;
            }
            if (targetT == null)
            {
                if (destroyWhenDone)
                {
                    GameObject.Destroy(worldObject);
                }
                yield break;
            }

            worldObjectPosition = worldObject.transform.position;
            targetPosition = targetT.position;
        }

        if(destroyWhenDone)
        {
            GameObject.Destroy(worldObject);
        }
    }

    public static List<EntityHandle> SenseSurroundingCreatures(Vector3 position, Species targetSpecies, float distance){

        Collider[] colliders = Physics.OverlapSphere(position, distance, LayerMaskController.CREATURE);
        //Debug.Log("sense distance: " + distance + "... creatures found: " + colliders.Length);

        List<EntityHandle> foundHandles = new List<EntityHandle>();
        GameObject o;
        EntityHandle foundHandle;
        foreach(Collider col in colliders)
        {
            o = col.gameObject;
            foundHandle = o.GetComponentInParent<EntityHandle>();
            if(foundHandle != null)
            {
                if(!foundHandles.Contains(foundHandle))
                {
                    if ((targetSpecies.Equals(Species.Any) || targetSpecies.Equals(foundHandle.entityInfo.species)))
                    {
                        foundHandles.Add(foundHandle);
                    }
                }
            }
        }
        
        return foundHandles;
    }


    public static List<GameObject> SenseSurroundingFeatures(Vector3 position, string featureName, float distance)
    {
        Collider[] colliders = Physics.OverlapSphere(position, distance, LayerMaskController.FEATURE);
        //Debug.Log("sense distance: " + distance + "... creatures found: " + colliders.Length);

        List<GameObject> foundGameObjects = new List<GameObject>();
        GameObject o;
        foreach(Collider col in colliders)
        {
            o = col.gameObject;
            if (!foundGameObjects.Contains(o))
            {
                if (featureName == null || featureName == o.name)
                {
                    foundGameObjects.Add(o);
                }
            }  
        }
        return foundGameObjects;
    }



    public static List<T> Shuffle<T>(List<T> list)  
    {
        System.Random rng = new System.Random();
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }
        return list;
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
