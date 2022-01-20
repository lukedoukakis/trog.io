using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHitDetection : EntityComponent
{
    
    [SerializeField] public Species species;
    public bool isInitialized;


    protected override void Awake()
    {
        this.fieldName = "entityHitDetection";

        base.Awake();
    }


    void Init()
    {
        // add and set up info and stats if they don't exist
        entityInfo = gameObject.AddComponent<EntityInfo>();
        entityInfo.species = species;
        entityInfo.Init();
        entityStats = gameObject.AddComponent<EntityStats>();
        //entityStats.FindAndSetEntityReferences();

        isInitialized = true;
    }

    public void OnHit(EntityHandle attackerHandle, Vector3 hitPoint, Projectile projectile, bool instantKill)
    {


        if (ReferenceEquals(entityHandle, GameManager.instance.localPlayerHandle) && Testing.instance.godMode)
        {
            return;
        }

        //Debug.Log("EntityHitDetection: OnHit()");

        if(!isInitialized)
        {
            Init();
        }

        
        // take damage from the hit
        if(entityStats != null)
        {
            entityStats.TakeDamage(attackerHandle, projectile, instantKill);
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
