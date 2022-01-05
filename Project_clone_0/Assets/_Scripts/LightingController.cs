using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;

public class LightingController : MonoBehaviour
{

    public static LightingController current;
    
    // post-processing
    //public PostProcessVolume volume;
    //public ColorGrading colorGrading;

    public static float FOG_DISTANCE_START_BASE = 40f;

    public GameObject fog;
    public GameObject sun, moon;
    public Light sunLight, moonLight;
    public Color sunLightColor_base, sunLightColor_sunrise, sunLightColor_sunset;
    public Color ambientColorDay, ambientColorNight;
    public float time;
    public float darkness;
    public float timeOfDay;
    public Color fogColor_day, fogColor_night;

    // time in seconds for a full day
    public static float SECONDS_PER_DAY = 60f;


    private void Awake()
    {
        current = this;

        fog = GameObject.Find("Fog");
        sun = GameObject.Find("Sun");
        moon = GameObject.Find("Moon");
        sunLight = sun.GetComponent<Light>();
        moonLight = moon.GetComponent<Light>();

        InitFog();

        // initialize pp
        //volume = GetComponent<PostProcessVolume>();
        //volume.profile.TryGetSettings(out colorGrading);
        

        time = 200f;
        //time = 0f;
    }


    // Update is called once per frame
    void Update()
    {
       
        SetFog(time);
        SetCelestialBodies(time);
        SetColors(time);

        // pause time of day: comment out this line
        //time += Time.deltaTime;
    }

    void InitFog()
    {
        if(fog != null)
        {
            List<GameObject> fogLayers = new List<GameObject>();

            int i = 0;
            foreach(Transform t in fog.transform)
            {
                
                //Debug.Log("Initializing fog " + i);

                fogLayers.Add(t.gameObject);

                // set scale
                t.localScale = Vector3.one * (FOG_DISTANCE_START_BASE + (i * 2f));

                // set material and material properties
                Material newFogMat = Material.Instantiate(MaterialController.instance.baseFogMaterial);
                newFogMat.SetFloat("_FogThickness", (i + 1) + .002f);
                t.GetComponent<MeshRenderer>().sharedMaterial = newFogMat;

                ++i;
            }
        }
    }

    void SetFog(float time)
    {
        Vector3 playerPos = GameManager.instance.localPlayer.transform.position;
        Vector3 cameraPos = Camera.main.transform.position;
        if(fog != null)
        {
            fog.transform.position = playerPos;
        }
    }

    void SetCelestialBodies(float time)
    {
        float sunX = Mathf.Sin(time / SECONDS_PER_DAY);
        float sunY = Mathf.Cos(time / SECONDS_PER_DAY);
        Vector3 sunRot = new Vector3(sunX, sunY, 0f);
        Vector3 moonRot = new Vector3(sunX * -1f, sunY * -1f, 0f);
        sun.transform.rotation = Quaternion.LookRotation(sunRot, Vector3.up);
        moon.transform.rotation = Quaternion.LookRotation(moonRot, Vector3.up);
    }

    void SetColors(float time){

        darkness = (Mathf.Cos(time / SECONDS_PER_DAY) + 1f) / 2f;
        timeOfDay = (Mathf.Sin(time / SECONDS_PER_DAY) + 1f) / 2f;
        PolyverseSkies.timeOfDay = darkness;
        RenderSettings.fogColor = Color.Lerp(fogColor_day, fogColor_night, darkness);

        float sunset = Mathf.Max(0f, Mathf.Pow((((1f - timeOfDay) - .5f) * 2f), 15f));
        float sunrise = Mathf.Max(0f, Mathf.Pow((((timeOfDay) - .5f) * 2f), 5f));
        //Debug.Log(sunrise);

        Color color = Color.white;
        color *= Color.Lerp(sunLightColor_base, sunLightColor_sunrise, sunrise);
        color *= Color.Lerp(sunLightColor_base, sunLightColor_sunset, sunset);
        sunLight.color = color;

        //colorGrading.colorFilter.value = Color.Lerp(ambientColorDay, ambientColorNight, darkness);

    }


}
