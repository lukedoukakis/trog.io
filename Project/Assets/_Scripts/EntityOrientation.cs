using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityOrientation : EntityComponent
{

    public Rigidbody rb;
    public Transform body;
    public Transform head;


    // rotation
    public Enum bodyRotationMode;
    public Transform bodyRotationTarget;
    public static float bodyRotationSpeed_player = .1f; //.04
    public static float bodyRotationSpeed_ai = .01f;
    public static float leanBoundMin = -.4f;
    public static float leanBoundMax = 1.05f;
    float bodyRotationSpeed;
    Quaternion bodyRotation;
    Quaternion bodyRotationLast;
    Vector3 bodyAngularVelocity;
    public float bodyLean;
    public float angularVelocityY;
    float angularVelocityY_last;
    public static float angularVelocityY_maxDelta = .1f;

    float posture_squat, squat_activity;

    float runMagnitude, climbMagnitude;
    public float bodySkew, slowness;




    public enum BodyRotationMode{
        Normal, Target
    }


    protected override void Awake(){

        base.Awake();

      
        rb = GetComponent<Rigidbody>();
        body = Utility.FindDeepChildWithTag(this.transform, "Body");
        //body = Utility.FindDeepChildWithTag(this.transform, "Body2");
        head = Utility.FindDeepChild(body, "B-head");
        if(tag == "Player"){
            bodyRotationSpeed = bodyRotationSpeed_player;
        }
        else{
            bodyRotationSpeed = bodyRotationSpeed_ai;
        }
        posture_squat = .1f;
    }

    void Start(){
        SetBodyRotationMode(BodyRotationMode.Target, null);
    }

    
    public void SetBodyRotationMode(Enum mode, Transform t){
        if (true)
        {
            //Log("Setting body rotation mode");
            bodyRotationMode = mode;
            if (t != null)
            {
                bodyRotationTarget = t;
            }
        }
    }


    void UpdateBodyRotation(){

        if (isLocalPlayer && !entityPhysics.IN_WATER)
        {
            bodyLean = Mathf.InverseLerp(leanBoundMin, leanBoundMax, Mathf.Sin(Camera.main.transform.rotation.eulerAngles.x * Mathf.Deg2Rad)) * 2f - 1f + .2f;
        }
        else
        {
            bodyLean = 0f;
        }

        if(bodyRotationMode.Equals(BodyRotationMode.Normal))
        {
            Vector3 dirForward = transform.forward;
            bool moving = entityPhysics.IsMoving();
            Vector3 dirMovement = moving ? entityPhysics.GetHorizVelocity() : dirForward;
            dirMovement.y = 0f;
            dirMovement = dirMovement.normalized;
            Vector3 dirCombined = Vector3.Lerp(dirForward, dirMovement, 1f);

            dirCombined += (Vector3.up * bodyLean * -1f * (1f - (Vector3.Distance(dirForward, dirMovement) / 2f)));

            Quaternion rot = Quaternion.LookRotation(dirCombined, Vector3.up);
            body.rotation = Quaternion.Slerp(body.rotation, rot, .1f);

            body.position = transform.position;
        }
        else if(bodyRotationMode.Equals(BodyRotationMode.Target))
        {


            Vector3 dirForward = bodyRotationTarget == null ? transform.forward : (bodyRotationTarget.position - body.position).normalized;
            bool moving = entityPhysics.IsMoving();
            Vector3 dirMovement = moving ? entityPhysics.GetHorizVelocity().normalized : dirForward;
            dirMovement.y = 0f;
            dirMovement = dirMovement.normalized;
            Vector3 dirCombined = Vector3.Lerp(dirForward, dirMovement, .35f);

            dirCombined += (Vector3.up * bodyLean * -1f * (1f - (Vector3.Distance(dirForward, dirMovement) / 2f)));

            Quaternion rot = Quaternion.LookRotation(dirCombined, Vector3.up);
            body.rotation = Quaternion.Slerp(body.rotation, rot, .1f);

            body.position = transform.position;



            // Vector3 dir;
            // if (bodyRotationTarget != null)
            // {
            //     dir = (bodyRotationTarget.position - body.position).normalized;
            // }
            // else
            // {
            //     dir = transform.forward;
            // }
            // dir += (Vector3.up * bodyLean * -1f);
            // Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            // body.rotation = Quaternion.Slerp(body.rotation, rot, .1f);

            // body.position = transform.position;
        
        };

        

        


        
        


    }
    
        


    // play pickup animation for an item
    public void Pickup(Item i){

        StartCoroutine(SquatAndStand());

        // if(i.type == (int)Item.Type.Weapon){
        //     SetAnimationBool("RightArm_weapon", true);
        // }
        // else if(i.type != (int)Item.Type.Pocket){
        //     SetAnimationInt("LeftArm_holdStyle", i.holdStyle);
        // }
    }




    IEnumerator SquatAndStand(){
        while(squat_activity < .7f){
            squat_activity = Mathf.Lerp(squat_activity, .75f, 10f * Time.deltaTime);
            yield return null;
        }
        while(squat_activity > .05f){
            squat_activity = Mathf.Lerp(squat_activity, 0f, 10f * Time.deltaTime);
            yield return null;
        }
        squat_activity = 0f;
    }


    void FixedUpdate(){
        UpdateBodyRotation();
        bodyRotationLast = body.rotation;
        angularVelocityY_last = angularVelocityY;
    }

    void Update(){
        
    }


}
