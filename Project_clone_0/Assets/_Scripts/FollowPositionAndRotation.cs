using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPositionAndRotation : MonoBehaviour
{


    public Transform reference;
    public float followTightness;
    Vector3 baseLocalPosition;


    // Start is called before the first frame update
    void Start()
    {
        baseLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;
        Vector3 targetPos = reference.position - baseLocalPosition;
        Quaternion targetRot = reference.rotation;


        transform.position = Vector3.Lerp(currentPos, targetPos, (followTightness * Time.fixedDeltaTime));
        transform.rotation = Quaternion.Slerp(currentRot, targetRot, followTightness * Time.fixedDeltaTime);
    }
}
