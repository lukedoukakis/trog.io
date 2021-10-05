using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Projectile : ScriptableObject
{
    


    public Item item;
    public GameObject worldObject;
    public Stats stats;


    public static Projectile InstantiateProjectile(Item item, GameObject worldObject, Stats stats)
    {
        Projectile projectile = ScriptableObject.CreateInstance<Projectile>();
        projectile.item = item;
        projectile.worldObject = worldObject;
        projectile.stats = Instantiate(stats);
        return projectile;


    }

}
