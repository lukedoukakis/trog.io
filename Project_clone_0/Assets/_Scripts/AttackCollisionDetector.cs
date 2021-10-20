using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollisionDetector : MonoBehaviour
{

    EntityHandle owner;
    Collider thisCollider;
    LayerMask mask;
    Projectile projectile;
    FixedJoint joint;

    void Awake()
    {
        thisCollider = GetComponent<Collider>();
        mask = LayerMaskController.HITTABLE;
        projectile = null;
    }
    

    public void SetOwner(EntityHandle handle){
        owner = handle;

        if(thisCollider != null)
        {
            thisCollider.enabled = true;
        }
    }
    public void RemoveOwner(){
        SetOwner(null);
        thisCollider.enabled = false;
    }

    bool CanCollide()
    {
        if(owner != null)
        {
            return projectile != null || owner.entityPhysics.meleeAttackCanHit;
        }
        else{
            return false;
        }
    }

    public void SetProjectile(Projectile projectile)
    {
        this.projectile = projectile;
    }
    public Projectile GetProjectile()
    {
        return projectile;
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

    void OnTriggerEnter(Collider otherCollider)
    {
        //Debug.Log("TRIGGER ENTER");
        if ((mask.value & (1 << otherCollider.gameObject.layer)) > 0)
        {
            if (CanCollide())
            {
                owner.entityPhysics.OnAttackHit(otherCollider, thisCollider.bounds.center, projectile);
                if (projectile != null)
                {
                    AddFixedJoint(otherCollider.gameObject);
                    SetProjectile(null);
                }
            }
        }

    }


}
