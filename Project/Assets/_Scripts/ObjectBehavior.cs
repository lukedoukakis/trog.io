using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBehavior : MonoBehaviour
{

    ObjectPhysics physics;


    public Action activeAction;
    public float senseDistance;
    public float maxJumpFromDistance;
    public float maxJumpableObstacleHeight;
    public float movementSpeed; //[0, 1]
    public float rotationSpeed; //[0, 1]
    public bool running;


    public List<Action> actions;


    public void Awake(){
        physics = GetComponent<ObjectPhysics>();
    }





    public void QueueAction(Action a){
        actions.Add(a);
    }
    public void InsertAction(Action a){
        actions.Insert(0, a);
    }
    public void InsertActionImmediate(Action a, bool clear){
        if(clear){
            actions.Clear();
        }
        InsertAction(a);
        OnActionInterrupt();
        NextAction();
    }
    public Action NextAction(){
        Action next = actions[0];
        actions.RemoveAt(0);
        activeAction = next;
        ExecuteAction(activeAction);
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
        NextAction();
    }



    void NavigateTowards(Transform t){
		
        bool playerSensed = false;
        float leftDistance, centerDistance, rightDistance;
        RaycastHit leftHitInfo, centerHitInfo, rightHitInfo;
		Vector3 path = transform.position - t.position;
		path.y = 0;

			
		// if obstacle in front and it's not the player object
		if(SenseObstacle() && !playerSensed){
				
			// if obstacle can't be jumped over, navigate around it
			if(!CanClearObstacle()){
				TurnTowardsMostOpenPath();
				RotateToward(t.position, .05f);
			}
			else{	
				
				// if close enough to obstacle and on the ground, jump
				if(Mathf.Min(Mathf.Min(leftDistance, centerDistance), rightDistance) < maxJumpFromDistance){
					if(physics.CanJump()){
						physics.Jump();
					}
				}
				RotateToward(t.position, .5f);
			}
		}
		
		// move forward
		running = true;
		physics.Move(Vector3.forward, 1f);




        bool SenseObstacle(){

            float castDistance = senseDistance;

            // set raycasts to reach castDistance units away
            bool leftCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left)).normalized, out leftHitInfo, castDistance);
            bool centerCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), transform.TransformDirection(Vector3.forward).normalized * 1f, out centerHitInfo, castDistance);
            bool rightCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right)).normalized, out rightHitInfo, castDistance);

            // set leftDistance, centerDistance, rightDistance, and playerSensed;
            if (leftCast)
            {
                leftDistance = (leftHitInfo.point - transform.position).magnitude;
                if (leftHitInfo.collider.gameObject.tag == "Player")
                {
                    playerSensed = true;
                }
            }
            else
            {
                leftDistance = int.MaxValue;
            }
            if (centerCast)
            {
                centerDistance = (centerHitInfo.point - transform.position).magnitude;
                if (centerHitInfo.collider.gameObject.tag == "Player")
                {
                    playerSensed = true;
                }
            }
            else
            {
                centerDistance = int.MaxValue;
            }
            if (rightCast)
            {
                rightDistance = (rightHitInfo.point - transform.position).magnitude;
                if (rightHitInfo.collider.gameObject.tag == "Player")
                {
                    playerSensed = true;
                }
            }
            else
            {
                rightDistance = int.MaxValue;
            }

            if ((leftCast || centerCast || rightCast))
            {
                return true;
            }
            return false;

        }

        bool CanClearObstacle(){
            if (
                !Physics.Raycast(transform.position + new Vector3(0, .1f, 0) + Vector3.up * maxJumpableObstacleHeight, (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left)).normalized * 1f, out leftHitInfo, senseDistance / 2)
                && !Physics.Raycast(transform.position + new Vector3(0, .1f, 0) + Vector3.up * maxJumpableObstacleHeight, transform.TransformDirection(Vector3.forward).normalized * 1f, out leftHitInfo, senseDistance * 2)
                && !Physics.Raycast(transform.position + new Vector3(0, .1f, 0) + Vector3.up * maxJumpableObstacleHeight, (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right)).normalized * 1f, out rightHitInfo, senseDistance / 2)
            )
            {
                return true;
            }
            return false;
        }

        void TurnTowardsMostOpenPath(){

            Quaternion leftRot = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left) * 1f, Vector3.up);
            Quaternion rightRot = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right) * 1f, Vector3.up);
            if (leftDistance < rightDistance){ transform.rotation = Quaternion.Slerp(transform.rotation, rightRot, rotationSpeed); }
            else{ transform.rotation = Quaternion.Slerp(transform.rotation, leftRot, rotationSpeed); }
        }

        void RotateToward(Vector3 targetPos, float magnitude){
            Vector3 p = transform.position - targetPos;
            p.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(path * -1, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, magnitude);
        }
	}



    public void Idle(Action a){

    }

    public void GoTo(Action a){
        StartCoroutine(_GoTo(a));
        IEnumerator _GoTo(Action _a){
            Transform targetT = _a.obj.transform;
            Transform t = gameObject.transform;
            while(Vector3.Distance(t.position, targetT.position) > 1f){

                yield return null;
            }
        }
    }

    public void Follow(Action a){

    }

    public void Collect(Action a){
        
    }

    public void Attack(Action a){

    }

    public void Build(Action a){

    }

    public void Hunt(Action a){

    }









    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
