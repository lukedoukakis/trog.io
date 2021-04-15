﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBehavior : EntityComponent
{

    public Transform home;
    static float distanceThreshhold_home = 10f;
    static float distanceThreshhold_spot = 4f;
    public static float randomOffsetRange = 1f;
    Vector3 randomOffset;


    // sensing and movement parameters
    public static float senseDistance = 1f;
    public static float maxJumpFromDistance = 3f;
    public static float rotationSpeed = 1f;
    public bool running;


    public Action activeAction;
    public List<Action> actions;
    public enum Priority{
        Back, Front, FrontImmediate
    }

    public enum Command{
        Idle, Go_home, Follow_player
    }
    IEnumerator coroutine_movement;
    IEnumerator coroutine_hands;


    // temp
    GameObject tempObject;




    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityBehavior = this;
        home = GameObject.FindGameObjectWithTag("Home").transform;
        randomOffset = new Vector3(Random.Range(randomOffsetRange*-1f, randomOffsetRange), 0f, Random.Range(randomOffsetRange*-1f, 0));
    }

    void Start(){

    }



    // primary method to be used for queueing actions
    public void ProcessCommand(int command, int priority){
        Action a = CreateAction(command);
        switch(priority){
            case (int)Priority.Back :
                AddAction(a);
                break;
            case (int)Priority.Front :
                InsertAction(a);
                break;
            case (int)Priority.FrontImmediate :
                InsertActionImmediate(a, true);
                break;
        }
        //Debug.Log("ProcessCommand() done");
    }

    public Action CreateAction(int command){
        Action a = new Action(-1, null, -1, -1, -1);
        switch(command){
            case (int)Command.Idle :
                a.type = (int)Action.ActionTypes.Idle;
                break;
            case (int)Command.Go_home :
                a.type = (int)Action.ActionTypes.GoTo;
                a.obj = home.gameObject;
                break;
            case (int)Command.Follow_player :
                a.type = (int)Action.ActionTypes.Follow;
                a.obj = Player.current.gameObject;
                break;
            default :
            //Debug.Log("ObjectBehavior: no action for command specified");
                break;
        }
        //Debug.Log("CreateAction() done");
        return a;    
    }

    // add an action to the end of the queue
    void AddAction(Action a){
        actions.Add(a);
    }

    // insert an action to the front of the queue, to be executed when the current action is finished
    void InsertAction(Action a){
        if(actions.Count > 0){
            actions.Insert(0, a);
        }
        else{
            actions.Add(a);
        }
    }

    // insert an action to the front of the queue and immediately execute
    void InsertActionImmediate(Action a, bool clear){
        if(clear){
            if(actions.Count > 0){
                actions.Clear();
            }
        }
        InsertAction(a);
        OnActionInterrupt();
        NextAction();
        //Debug.Log("InsertActionImmediate() done");
    }



    // select and execute the next action in the queue... if list is empty, insert "go home" or "idle" action
    public Action NextAction(){
        if(actions.Count == 0){
            if(IsAtPosition(home.position, distanceThreshhold_spot)){
                ProcessCommand((int)Command.Idle, (int)Priority.Front);
            }
            else{
                ProcessCommand((int)Command.Go_home, (int)Priority.Front);
            }
        }
        Action next = actions[0];
        actions.RemoveAt(0);
        activeAction = next;
        ExecuteAction(activeAction);
        //Debug.Log("NextAction() done");
        return next;
    }
    public void OnActionInterrupt(){

    }

    public void ExecuteAction(Action a){
        switch(a.type){
            case (int)Action.ActionTypes.Idle :
                Idle(a);
                break;
            case (int)Action.ActionTypes.GoTo :
                GoTo(a);
                break;
            case (int)Action.ActionTypes.Follow :
                Follow(a);
                break;
            case (int)Action.ActionTypes.Collect :
                Collect(a);
                break;
            case (int)Action.ActionTypes.Attack :
                Attack(a);
                break;
            case (int)Action.ActionTypes.Build :
                Build(a);
                break;
            case (int)Action.ActionTypes.Hunt :
                Hunt(a);
                break;
            default:
                Debug.Log("ObjectBehavior: called action not a defined action (" + a.type + ")... idling.");
                break;
        }
        //NextAction();
    }



    void NavigateTowards(Transform targetT){
		
        float leftDistance, centerDistance, rightDistance;
        RaycastHit leftHitInfo, centerHitInfo, rightHitInfo;
		Vector3 path = transform.position - targetT.position;
		path.y = 0;

			
		// if obstacle in front and it's not the player object
		if(SenseObstacle()){
				
			// if obstacle can't be jumped over, navigate around it
			if(!CanClearObstacle()){
				TurnTowardsMostOpenPath();
			}
			else{	
				
				// if close enough to obstacle and on the ground, jump
				if(Mathf.Min(Mathf.Min(leftDistance, centerDistance), rightDistance) < maxJumpFromDistance){
					if(handle.entityPhysics.CanJump()){
						handle.entityPhysics.Jump();
					}
				}
			}
		}

        float offsetMultipler;
        Rigidbody targetRb = targetT.GetComponent<Rigidbody>();
        if(targetRb == null){
            offsetMultipler = 1f;
        }
        else{
            offsetMultipler = (Mathf.InverseLerp(0f, handle.entityPhysics.maxSpeed, targetT.GetComponent<Rigidbody>().velocity.magnitude) + 1f) * 5f;
        }
        Vector3 tp = targetT.position;
        offsetMultipler = 1f;
        tp += targetT.TransformDirection(randomOffset * offsetMultipler);
        RotateToward(tp, .05f);
		
		// move forward
		handle.entityPhysics.Move(Vector3.forward, handle.entityPhysics.acceleration);



        bool SenseObstacle(){

            // set raycasts to reach castDistance units away
            Transform gs = handle.entityPhysics.groundSense;
            bool leftCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left)).normalized, out leftHitInfo, senseDistance);
            bool centerCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), transform.TransformDirection(Vector3.forward).normalized * 1f, out centerHitInfo, senseDistance);
            bool rightCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right)).normalized, out rightHitInfo, senseDistance);

            List<RaycastHit> hitInfos = new List<RaycastHit>();

            // set leftDistance, centerDistance, rightDistance
            if (leftCast){ leftDistance = (leftHitInfo.point - transform.position).magnitude; hitInfos.Add(leftHitInfo); }
            else{ leftDistance = int.MaxValue; }
            if (centerCast){ centerDistance = (centerHitInfo.point - transform.position).magnitude; hitInfos.Add(centerHitInfo); }
            else{ centerDistance = int.MaxValue; }
            if (rightCast){ rightDistance = (rightHitInfo.point - transform.position).magnitude; hitInfos.Add(rightHitInfo); }
            else{ rightDistance = int.MaxValue; }

            // return true if any of the raycasts hit something besides a tribe member
            int hits = 0;
            foreach(RaycastHit hitInfo in hitInfos){
                string tag = hitInfo.collider.gameObject.tag;
                if(tag != "TribeMember" && tag != "Player"){
                    hits++;
                }
            }
            return hits >= 1;

        }

        bool CanClearObstacle(){
            Transform ohs = handle.entityPhysics.obstacleHeightSense;
            if
            (
                // !Physics.Raycast(ohs.position, (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left)).normalized * 1f, out leftHitInfo, senseDistance/2)
                // && !Physics.Raycast(ohs.position, transform.TransformDirection(Vector3.forward).normalized * 1f, out centerHitInfo, senseDistance*2)
                // && !Physics.Raycast(ohs.position, (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right)).normalized * 1f, out rightHitInfo, senseDistance/2)
                !Physics.BoxCast(ohs.position, new Vector3(.5f, .01f, .5f), transform.forward, transform.rotation, Mathf.Max(leftDistance, centerDistance, rightDistance))
            )
            {
                return true;
            }
            return false;
        }

        void TurnTowardsMostOpenPath(){

            Quaternion leftRot = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left) * 1f, Vector3.up);
            Quaternion rightRot = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right) * 1f, Vector3.up);
            if (leftDistance < rightDistance){
                handle.entityPhysics.RotateTowards(rightRot, rotationSpeed);
            }
            else{
                handle.entityPhysics.RotateTowards(leftRot, rotationSpeed);
            }
        }

        void RotateToward(Vector3 targetPos, float magnitude){
            Vector3 p = transform.position - targetPos;
            p.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(p * -1, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, magnitude);
        }
	}



    public void Idle(Action a){

    }

    public void GoTo(Action a){
        TerminateMovement();
        coroutine_movement = _GoTo(a, false);
        StartCoroutine(coroutine_movement);
    }

    public void Follow(Action a){
        TerminateMovement();
        coroutine_movement = _GoTo(a, true);
        StartCoroutine(coroutine_movement);
    }

    public void Collect(Action a){
        
    }

    public void Attack(Action a){

    }

    public void Build(Action a){

    }

    public void Hunt(Action a){

    }


    void TerminateMovement(){
        if(coroutine_movement != null){
            StopCoroutine(coroutine_movement);
            coroutine_movement = null;
        }
        if(tempObject != null){
            GameObject.Destroy(GameObject.Find("temp_" + handle.entityInfo.ID));
        }
    }



    public bool IsAtPosition(Vector3 position, float distanceThreshhold){
        return Vector3.Distance(transform.position, position) < distanceThreshhold;
    }





    IEnumerator _GoTo(Action a, bool follow){
        Transform t = gameObject.transform;
        Transform targetT;
        if (follow) { targetT = a.obj.transform; }
        else
        {
            tempObject = new GameObject("temp_" + handle.entityInfo.ID);
            tempObject.transform.position = a.obj.transform.position;
            targetT = tempObject.transform;
        }
        while(true)
        {
            if(!IsAtPosition(targetT.position, distanceThreshhold_spot)){
                NavigateTowards(targetT);
            }
            else{
                if(!follow){
                    if(handle.entityPhysics.GROUNDTOUCH){
                        //Debug.Log("Destination reached");
                        TerminateMovement();
                    }
                    
                }
            }
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
