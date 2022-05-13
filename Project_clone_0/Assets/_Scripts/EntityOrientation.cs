using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyRotationMode { Normal, Target }

public class EntityOrientation : EntityComponent
{

    public Transform body;
    public Transform itemOrientationTarget;
    public Transform model;
    public Transform head;

    Animator animator;


    // rotation
    public Enum bodyRotationMode;
    public Transform bodyRotationTarget;
    public static float bodyRotationSpeed_player = .35f; //.04
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



    protected override void Awake()
    {

        this.fieldName = "entityOrientation";

        base.Awake();

      
        body = Utility.FindDeepChildWithTag(this.transform, "Body");
        itemOrientationTarget = Utility.FindDeepChild(this.transform, "ItemOrientationTarget");
        model = Utility.FindDeepChildWithTag(this.transform, "Model");
        head = Utility.FindDeepChild(body, "B-head");
        if(tag == "Player"){
            bodyRotationSpeed = bodyRotationSpeed_player;
        }
        else{
            bodyRotationSpeed = bodyRotationSpeed_ai;
        }
        posture_squat = .1f;


        animator = body.GetComponent<Animator>();

    }

    void Start(){
        SetBodyRotationMode(BodyRotationMode.Normal, null);
    }

    
    public void SetBodyRotationMode(Enum mode, Transform t)
    {
        //Log("Setting body rotation mode");
        bodyRotationMode = mode;
        if (t != null)
        {
            bodyRotationTarget = t;
        }
           
    }


    void UpdateBodyRotation()
    {

        if (IsClientPlayerCharacter() && !entityPhysics.isInWater)
        {
            bodyLean = Mathf.InverseLerp(leanBoundMin, leanBoundMax, Mathf.Sin(Camera.main.transform.rotation.eulerAngles.x * Mathf.Deg2Rad)) * 2f - 1f + .2f;
        }
        else
        {
            bodyLean = 0f;
        }
        //bodyLean = 0f;

        if(bodyRotationMode.Equals(BodyRotationMode.Normal))
        {

            if(!entityPhysics.isHandGrab || true)
            {
                Vector3 dirForwardTransform = transform.forward;
                Vector3 dirForwardBody = body.forward;
                Vector3 dirMovement = entityPhysics.isMoving ? entityPhysics.velHoriz_this : dirForwardBody;
                dirMovement.y = 0f;
                dirMovement = dirMovement.normalized;
                Vector3 dirCombined = Vector3.Lerp(dirForwardBody, dirMovement, 1f);

                dirCombined += (Vector3.up * bodyLean * -1f * (1f - (Vector3.Distance(dirForwardTransform, dirMovement) / 2f)));

                if(!entityPhysics.isDodging)
                {
                    if(dirCombined.magnitude > 0f)
                    {
                        Quaternion rot = Quaternion.LookRotation(dirCombined, Vector3.up);
                        body.rotation = Quaternion.Slerp(body.rotation, rot, .035f);
                        itemOrientationTarget.rotation = rot;
                    }
                }
                

                body.position = transform.position;
            }
            
        }
        else if(bodyRotationMode.Equals(BodyRotationMode.Target))
        {


            Vector3 dirForward = bodyRotationTarget == null ? transform.forward : Utility.GetHorizontalVector((bodyRotationTarget.position - body.position)).normalized;
            Vector3 dirMovement = entityPhysics.isMoving ? entityPhysics.velHoriz_this.normalized : dirForward;
            dirMovement.y = 0f;
            dirMovement = dirMovement.normalized;
            Vector3 dirCombined = Vector3.Lerp(dirForward, dirMovement, .35f);

            dirCombined += (Vector3.up * bodyLean * -1f * (1f - (Vector3.Distance(dirForward, dirMovement) / 2f)));

            if(!entityPhysics.isDodging)
            {
                if(dirCombined.magnitude > 0f)
                {
                    Quaternion rot = Quaternion.LookRotation(dirCombined, Vector3.up);
                    body.rotation = Quaternion.Slerp(body.rotation, rot, .1f);
                    itemOrientationTarget.rotation = rot;
                }
            }

            body.position = transform.position;
        
        };

        // if (entityItems != null)
        // {
        //     entityItems.orientationParent.rotation = itemOrientationTarget.rotation;
        //     entityItems.orientationParent.position = itemOrientationTarget.position;
        // }

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
