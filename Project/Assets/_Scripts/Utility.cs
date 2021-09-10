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

    public static IEnumerator DespawnObject(GameObject gameObject, float delay){
        yield return new WaitForSeconds(delay);
        GameObject.Destroy(gameObject);
    }


    public static void ToggleObjectPhysics(GameObject o, bool value){
        if (o != null)
        {
            Collider col = o.GetComponent<BoxCollider>();
            Rigidbody rb = o.GetComponent<Rigidbody>();
            if (col != null)
            {
                col.enabled = value;
            }
            if (rb != null)
            {
                rb.isKinematic = !value;
            }
        }   
    }

}
