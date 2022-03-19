using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollisionDetector : MonoBehaviour
{

    EntityHandle ownerHandle;
    Collider thisCollider;
    LayerMask collisionMask;
    LayerMask dealDamageMask;
    Projectile projectile;
    FixedJoint joint;

    void Awake()
    {
        thisCollider = GetComponent<Collider>();
        collisionMask = LayerMaskController.COLLIDEABLE;
        dealDamageMask = LayerMaskController.HITTABLE;
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
            return projectile != null || ownerHandle.entityPhysics.attackCanHit;
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
        if (collisionMask == (collisionMask | (1 << otherObject.layer)) || dealDamageMask == (dealDamageMask | (1 << otherObject.layer)))
        {
            //Debug.Log("Layer Approved");


            // if entity is targeting this object
            if (CanHit())
            {
                //Debug.Log("Can Hit");
                if (ReferenceEquals(ClientCommand.instance.clientPlayerCharacterHandle, ownerHandle) || ownerHandle.entityBehavior.IsTargetedObject(otherObject))
                {
            
                    //Debug.Log("Call OnAttackHit()");

                    if (dealDamageMask == (dealDamageMask | (1 << otherObject.layer)))
                    {
                        ownerHandle.entityPhysics.OnAttackHit(otherCollider, thisCollider.bounds.center, projectile, 0f);
                    }
                    
                    if(projectile != null)
                    {
                        AddFixedJoint(otherCollider.gameObject);
                        //Debug.Log("PROJECTILE NULL");
                        SetProjectile(null);
                    }
                    
                }
            }


        }

    }

    void Update()
    {
        if(projectile != null)
        {
            //Debug.Log("rotating projectile...");
            GameObject worldObject = projectile.worldObject;
            Vector3 velocity = worldObject.GetComponent<Rigidbody>().velocity;
            float velY = velocity.y;
            Vector3 targetLookAt = worldObject.transform.position + (velocity * 10f) + Vector3.down * Mathf.Lerp(5000f, 20000f, Mathf.InverseLerp(-2f, 2f, velY) * 1f);
            worldObject.transform.LookAt(targetLookAt);
        }
    }


}
