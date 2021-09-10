using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHitDetection : EntityComponent
{
    
    [SerializeField] Species species;

    public void OnHit(EntityStats attackerStats, Item attackerWeapon){

        // add and set up info and stats if they don't exist
        if(entityInfo == null){
            entityInfo = gameObject.AddComponent<EntityInfo>();
            entityInfo.species = species;
        }
        if(entityStats == null){
            entityStats = gameObject.AddComponent<EntityStats>();
        }
        entityInfo.FindAndSetEntityReferences();
        entityStats.FindAndSetEntityReferences();


        // take damage from the hit
        entityStats.TakeDamage(attackerStats, attackerWeapon);


    }


}
