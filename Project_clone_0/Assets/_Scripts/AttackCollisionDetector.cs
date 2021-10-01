using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollisionDetector : MonoBehaviour
{

    EntityHandle owner;
    Collider thisCollider;
    bool isProjectile;

    void Awake(){
        thisCollider = GetComponent<Collider>();
        isProjectile = false;
    }
    

    public void SetOwner(EntityHandle handle){
        owner = handle;
        thisCollider.enabled = true;
    }
    public void RemoveOwner(){
        SetOwner(null);
        thisCollider.enabled = false;
    }

    bool CanCollide()
    {
        if(owner != null)
        {
            return isProjectile || owner.entityPhysics.attackCanHit;
        }
        else{
            return false;
        }
    }

    public void SetIsProjectile(bool value)
    {
        isProjectile = value;
    }

    void OnTriggerEnter(Collider otherCollider){
        //Debug.Log("TRIGGER ENTER");
        if(CanCollide()){
            owner.entityPhysics.OnAttackHit(otherCollider);
            if(isProjectile)
            {
                FixedJoint joint = gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = otherCollider.gameObject.GetComponentInParent<Rigidbody>();
                if(joint.connectedBody == null)
                {
                    joint.connectedBody = otherCollider.gameObject.GetComponentInChildren<Rigidbody>();
                }
            }
        }
    }


}
