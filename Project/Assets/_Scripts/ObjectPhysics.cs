using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPhysics : MonoBehaviour
{

    ObjectStats stats;

    BoxCollider groundSenseCollider;
    Rigidbody rigidbody;

    public static float jumpForce = 100f;

    public bool GROUNDTOUCH;






    void Awake(){
        stats = GetComponent<ObjectStats>();
        groundSenseCollider = GetComponent<BoxCollider>();
        groundSenseCollider.isTrigger = true;
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate(){

    }


    public void Move(Vector3 direction, float speed){
        float movementSpeed = speed * stats.GetStat("speed");
        Vector3 move = transform.TransformDirection(direction) * movementSpeed;
		rigidbody.AddForce(move, ForceMode.VelocityChange);
    }

    public void Jump(){
        rigidbody.AddForce(Vector3.up*jumpForce, ForceMode.VelocityChange);
    }

    public bool CanJump(){
        return GROUNDTOUCH;
    }


    void OnTriggerEnter(Collider other){
		GROUNDTOUCH = true;
	}
	void OnTriggerExit(Collider other){
		GROUNDTOUCH = false;
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
