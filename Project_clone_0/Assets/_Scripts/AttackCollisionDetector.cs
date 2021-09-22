using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollisionDetector : MonoBehaviour
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
        return owner != null && owner.entityPhysics.attackCanHit;
    }

    void OnTriggerEnter(Collider otherCollider){
        Debug.Log("TRIGGER ENTER");
        if(CanCollide()){
            Debug.Log("COLLISION");
            owner.entityPhysics.OnAttackHit(otherCollider);
        }
    }


}
