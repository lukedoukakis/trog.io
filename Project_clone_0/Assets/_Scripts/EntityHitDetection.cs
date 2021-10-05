using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHitDetection : EntityComponent
{
    
    [SerializeField] Species species;

    public void OnHit(EntityHandle attackerHandle, Projectile projectile){

        // add and set up info and stats if they don't exist
        if(entityInfo == null){
            entityInfo = gameObject.AddComponent<EntityInfo>();
            entityInfo.species = species;
            entityInfo.Init();
            entityStats = gameObject.AddComponent<EntityStats>();
            //entityStats.FindAndSetEntityReferences();
        }


        // take damage from the hit
        entityStats.TakeDamage(attackerHandle, projectile);


    }


}
