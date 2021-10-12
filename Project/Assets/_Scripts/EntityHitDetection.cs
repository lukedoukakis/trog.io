using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHitDetection : EntityComponent
{
    
    [SerializeField] public Species species;

    public void OnHit(EntityHandle attackerHandle, Vector3 hitPoint, Projectile projectile, bool instantKill){

        // add and set up info and stats if they don't exist
        if(entityInfo == null){
            entityInfo = gameObject.AddComponent<EntityInfo>();
            entityInfo.species = species;
            entityInfo.Init();
            entityStats = gameObject.AddComponent<EntityStats>();
            //entityStats.FindAndSetEntityReferences();
        }


        // take damage from the hit
        entityStats.TakeDamage(attackerHandle, projectile, instantKill);

        // play particles
        GameObject particlesPrefab = SpeciesInfo.GetSpeciesInfo(species).onHitParticlesPrefab;
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


}
