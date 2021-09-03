using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHitDetection : EntityComponent
{
    


    public void OnHit(EntityStats attackerStats, Item attackerWeapon){

        // add stats if doesn't exist
        if(entityStats == null){
            entityStats = gameObject.AddComponent<EntityStats>();
        }

        entityStats.TakeDamage(attackerStats, attackerWeapon);


    }


}
