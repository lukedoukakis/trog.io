using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollisionDetector : MonoBehaviour
{

    EntityHandle ownerHandle;
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
        ownerHandle = handle;

        if(thisCollider != null)
        {
            thisCollider.enabled = true;
        }
    }
    public void RemoveOwner(){
        SetOwner(null);
        thisCollider.enabled = false;
    }

    bool CanHit()
    {
        if(ownerHandle != null)
        {
            return projectile != null || ownerHandle.entityPhysics.meleeAttackCanHit;
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

        GameObject otherObject = otherCollider.gameObject;
        if (mask == (mask | (1 << otherObject.layer)))
        {
            //Debug.Log("Layer Approved");
            if (CanHit())
            {
                //Debug.Log("Can Hit");

                // if entity is targeting this object
                if(ownerHandle.entityBehavior.IsTargetedObject(otherObject))
                {
                    //Debug.Log("Call OnAttackHit()");

                    ownerHandle.entityPhysics.OnAttackHit(otherCollider, thisCollider.bounds.center, projectile);
                    if (projectile != null)
                    {
                        AddFixedJoint(otherCollider.gameObject);
                        SetProjectile(null);
                    }
                }
                
            }
        }

    }


}
