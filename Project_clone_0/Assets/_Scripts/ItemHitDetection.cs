using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHitDetection : MonoBehaviour
{


    [SerializeField] string itemNme;
    public Item item;
    public EntityStats stats;



    void Awake(){
        item = Item.GetItemByName(itemNme);
    }

    public void OnHit(EntityHandle attackerHandle, Vector3 hitPoint, Projectile projectile){

        // add and set up info and stats if they don't exist
        if(stats == null){
            stats = gameObject.AddComponent<EntityStats>();
            stats.SetStatsSlot(StatsSlot.Base, item.baseStats);
            stats.SetBaseHpAndStamina();
            stats.drops = item.drops;
            //Debug.Log("Item: " + item.nme);
            //Debug.Log("drops null?: " + item.drops.items == null);
        }


        // take damage from the hit
        stats.TakeDamage(attackerHandle, projectile, false);

        // play particles
        GameObject particlesPrefab = item.hitParticlesPrefab;
        if (particlesPrefab != null)
        {
            //Debug.Log("Playing particles");
            GameObject particleObj = GameObject.Instantiate(particlesPrefab);
            particleObj.transform.position = hitPoint;
            particleObj.transform.LookAt(hitPoint + attackerHandle.GetComponent<Rigidbody>().velocity);
            particleObj.GetComponent<ParticleSystem>().Play();
            Utility.DestroyInSeconds(particleObj, 1f);
        }

        // shake
        Shake();


    }

    void Shake()
    {

        StartCoroutine(_Shake());

        IEnumerator _Shake()
        {
            Vector3 originalLocation = transform.position;
            float displacement = .25f;
            int steps = 10;
            float shakeSpeed = 30f;

            for(int i = 0; i < steps; ++i)
            {
                Vector3 targetPos = originalLocation + Utility.GetRandomVector(displacement);
                while(Vector3.Distance(transform.position, targetPos) > .1f)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPos, shakeSpeed * Time.deltaTime);
                    yield return null;
                }
                displacement *= .85f;
            }
            while(Vector3.Distance(transform.position, originalLocation) > .02f)
            {
                transform.position = Vector3.Lerp(transform.position, originalLocation, shakeSpeed * Time.deltaTime);
            }
            transform.position = originalLocation;
        }
    }
}
