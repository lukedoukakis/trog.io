using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollisionDetector : MonoBehaviour
{

    EntityHandle owner;
    Collider thisCollider;
    bool isProjectile;
    FixedJoint joint;

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
            return (isProjectile && joint == null) || owner.entityPhysics.attackCanHit;
        }
        else{
            return false;
        }
    }

    public void SetIsProjectile(bool value)
    {
        isProjectile = value;
    }
    public bool GetIsProjectile()
    {
        return isProjectile;
    }


    public void AddFixedJoint(GameObject attachedObject)
    {
        RemoveFixedJoint();
        joint = transform.root.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = attachedObject.GetComponentInParent<Rigidbody>();
        if (joint.connectedBody == null)
        {
            joint.connectedBody = attachedObject.gameObject.GetComponentInChildren<Rigidbody>();
        }
    }

    public void RemoveFixedJoint()
    {
        if(joint != null)
        {
            Destroy(joint);
        }
    }

    void OnTriggerEnter(Collider otherCollider){
        //Debug.Log("TRIGGER ENTER");
        if(CanCollide()){
            owner.entityPhysics.OnAttackHit(otherCollider);
            if(isProjectile)
            {
                AddFixedJoint(otherCollider.gameObject);
                SetIsProjectile(false);
            }
        }
    }


}
