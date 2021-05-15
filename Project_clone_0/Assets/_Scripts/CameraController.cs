using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraController : MonoBehaviour
{
    public Transform playerT;
    public Transform focusT;
    public static float cameraDistanceScale = .05f;
    
    public static CameraController current;

    [SerializeField] float sensitivity;
    [SerializeField] float featureCullDistance;
    [SerializeField] float smallFeatureCullDistance;
    float acceleration;


    void Awake(){
        current = this;
    }
    // Start is called before the first frame update
    void Start()
    {
    
    }

    public void Init(Transform t){
        playerT = t;
        //Cursor.visible = false;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;
        float[] cullDistances = new float[32];
        cullDistances[10] = featureCullDistance;
        cullDistances[11] = smallFeatureCullDistance;
        Camera.main.layerCullDistances = cullDistances;

        //RandomSpawn();
    }


    void AdjustCamera(int mode){
        // static camera
        if (mode == 0)
        {
            Vector3 targetPos = playerT.position + (Vector3.forward * .07f) + (Vector3.up * .09f);
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, 50f * Time.deltaTime);
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(45f, 0f, 0f));
        }

        // dynamic camera
        else if (mode == 1)
        {
            Vector3 targetPos = playerT.position + playerT.TransformDirection((Vector3.forward * -.7f) + (Vector3.up * .35f));
            Quaternion targetRot = playerT.rotation * Quaternion.Euler(new Vector3(25f, 0f, 0f));
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, 5000f * Time.deltaTime);
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, targetRot, 5000f * Time.deltaTime);
        }
    }


    void Update()
    {

        if(playerT != null){
            AdjustCamera(GameManager.current.cameraMode);
        }
    }

    void FixedUpdate()
    {
        

    }









    // void RandomSpawn(){
    //     bool landHit = false;
    //     Vector3 randomPos = Vector3.zero;
    //     int i = 0;
    //     while(!landHit){
    //         randomPos = new Vector3(Random.Range(-1000f, 1000f), 0f, Random.Range(-1000f, 1000f)) + Vector3.up * (ChunkGenerator.ElevationAmplitude*.82f);
    //         landHit = Mathf.PerlinNoise((randomPos.x - ChunkGenerator.Seed + .01f) / ChunkGenerator.ElevationMapScale, (randomPos.z - ChunkGenerator.Seed + .01f) / ChunkGenerator.ElevationMapScale) >= .5f;
    //         i++;

    //         if(i > 1000){
    //             Debug.Log(":(");
    //             break;
    //         }
    //     } 
    //     MainCamera.transform.position = randomPos;
    //     MainCamera.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
    // }
}
