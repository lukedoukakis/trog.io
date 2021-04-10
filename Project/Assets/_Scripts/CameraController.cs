using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Camera MainCamera;
    public Transform playerT;
    public Transform focusT;

    public static CameraController current;

    [SerializeField] float sensitivity;
    [SerializeField] float featureCullDistance;
    [SerializeField] float smallFeatureCullDistance;
    float acceleration;


    void Awake(){
        current = this;
        playerT = GameObject.FindGameObjectWithTag("Player").transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        //Cursor.visible = false;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;
        float[] cullDistances = new float[32];
        cullDistances[10] = featureCullDistance;
        cullDistances[11] = smallFeatureCullDistance;
        MainCamera.layerCullDistances = cullDistances;

        //RandomSpawn();
    }
    

    void RandomSpawn(){
        bool landHit = false;
        Vector3 randomPos = Vector3.zero;
        int i = 0;
        while(!landHit){
            randomPos = new Vector3(Random.Range(-1000f, 1000f), 0f, Random.Range(-1000f, 1000f)) + Vector3.up * (ChunkGenerator.ElevationAmplitude*.82f);
            landHit = Mathf.PerlinNoise((randomPos.x - ChunkGenerator.Seed + .01f) / ChunkGenerator.ElevationMapScale, (randomPos.z - ChunkGenerator.Seed + .01f) / ChunkGenerator.ElevationMapScale) >= .5f;
            i++;

            if(i > 1000){
                Debug.Log(":(");
                break;
            }
        } 
        MainCamera.transform.position = randomPos;
        MainCamera.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
    }

    void Update(){
        Vector3 targetPos = playerT.position + playerT.TransformDirection((Vector3.forward*-6.75f) + (Vector3.up*4.75f));
        Quaternion targetRot = playerT.rotation * Quaternion.Euler(new Vector3(25f, 0f, 0f));


        transform.position = Vector3.Lerp(transform.position, targetPos, 100f * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 100f * Time.deltaTime);

    }

    void FixedUpdate()
    {


    }
}
