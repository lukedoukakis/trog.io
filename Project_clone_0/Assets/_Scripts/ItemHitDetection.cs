using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHitDetection : MonoBehaviour
{


    [SerializeField] string itemNme;
    Item item;
    EntityStats stats;



    void Awake(){
        item = Item.GetItemByName(itemNme);
    }

    public void OnHit(EntityHandle attackerHandle, Vector3 hitPoint, Projectile projectile){

        // add and set up info and stats if they don't exist
        if(stats == null){
            stats = gameObject.AddComponent<EntityStats>();
            stats.AddStatsModifier(item.baseStats);
            stats.SetBaseHpAndStamina();
            stats.drops = item.drops;
            //Debug.Log("Item: " + item.nme);
            //Debug.Log("drops null?: " + item.drops.items == null);
        }


        // take damage from the hit
        stats.TakeDamage(attackerHandle, projectile);

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


    }

    void Init(){
        
        


    }
}
