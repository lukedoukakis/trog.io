using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollisionDetector : MonoBehaviour
{

    EntityHandle owner;
    Collider thisCollider;

    void Awake(){
        thisCollider = GetComponent<Collider>();
    }
    

    public void SetOwner(EntityHandle handle){
        owner = handle;
        thisCollider.enabled = true;
    }
    public void RemoveOwner(){
        SetOwner(null);
        thisCollider.enabled = false;
    }

    bool CanCollide(){
        return owner != null && owner.entityPhysics.weaponCanHit;
    }

    void OnTriggerEnter(Collider otherCollider){
        if(CanCollide()){
            owner.entityPhysics.OnWeaponHit(otherCollider);
        }
    }


}
