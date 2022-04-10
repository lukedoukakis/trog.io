using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHitDetection : EntityComponent
{
    
    [SerializeField] public Species species;


    protected override void Awake()
    {
        this.fieldName = "entityHitDetection";

        base.Awake();
    }

    public void OnHit(EntityHandle attackerHandle, Item weapon, Vector3 hitPoint, Projectile projectile, bool instantKill)
    {


        if (ReferenceEquals(entityHandle, ClientCommand.instance.clientPlayerCharacterHandle) && Testing.instance.godMode)
        {
            return;
        }

        //Debug.Log("EntityHitDetection: OnHit()");

        
        // take damage from the hit
        if(entityStats != null)
        {
            entityStats.TakeDamage(attackerHandle, weapon, projectile, instantKill);
        }
        else
        {
            Debug.Log("entityStats is null");
        }

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
