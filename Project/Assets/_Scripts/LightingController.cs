using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingController : MonoBehaviour
{

    public static LightingController current;

    public static float RENDER_SETTINGS_FOG_DISTANCE_START = 250f;
    public static float RENDER_SETTINGS_FOG_DISTANCE_END = 500f;

    public static float FOG_DISTANCE_START_BASE = 10f;
    public static float PI = Mathf.PI;

    public GameObject fog;
    public GameObject sun, moon;
    public Light sunLight, moonLight;
    public Color atmosphereColor;
    [SerializeField] Gradient  atmosphereColorGradient_timeOfDay, atmosphereColorGradient_temperature;
    [SerializeField] Gradient fogAmountGradient;
    public float time, timeOfDay;


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

        time = 264f;
        //time = 0f;

    }


    // Update is called once per frame
    void Update()
    {
        
        UpdateTime();
        UpdateAtmosphereColor();
        UpdateCelestialBodies();
        UpdateCamera();
        UpdateRenderFog();
        //UpdateFog(time);

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
                t.localScale = Vector3.one * (FOG_DISTANCE_START_BASE + (i * 1f));

                // set material and material properties
                Material newFogMat = Material.Instantiate(MaterialController.instance.baseFogMaterial);
                //newFogMat.SetFloat("_FogThickness", .01f);
                t.GetComponent<MeshRenderer>().sharedMaterial = newFogMat;

                ++i;
            }
        }
    }


    void UpdateTime()
    {
        time += Time.deltaTime;
        //time = SECONDS_PER_DAY * .75f;
        timeOfDay = (time % SECONDS_PER_DAY) / SECONDS_PER_DAY; // 0 is midnight, 1 is 11:59pm
        //Debug.Log("timeOfDay: " + timeOfDay);
    }

    void UpdateAtmosphereColor()
    {
        atmosphereColor = atmosphereColorGradient_timeOfDay.Evaluate(timeOfDay);
        //atmosphereColor = Color.Lerp(atmosphereColor, atmosphereColorGradient_temperature.Evaluate(timeOfDay), .25f);
    }

    void UpdateCelestialBodies()
    {
        float sunX = Mathf.Sin(time / SECONDS_PER_DAY * 2f * PI);
        float sunY = Mathf.Cos(time / SECONDS_PER_DAY * 2f * PI);
        Vector3 sunRot = new Vector3(sunX, sunY, 0);
        Vector3 moonRot = new Vector3(-sunX, -sunY, 0);
        sun.transform.rotation = Quaternion.LookRotation(sunRot, Vector3.up);
        moon.transform.rotation = Quaternion.LookRotation(moonRot, Vector3.up);
        sunLight.color = atmosphereColor;
    }

    void UpdateCamera()
    {
        Camera.main.backgroundColor = atmosphereColor;
    }

    void UpdateRenderFog()
    {
        float environmentModifier = Mathf.Lerp(1f, 2f, 1f - fogAmountGradient.Evaluate(timeOfDay).r);
        RenderSettings.fogStartDistance = RENDER_SETTINGS_FOG_DISTANCE_START * environmentModifier;
        RenderSettings.fogEndDistance = RENDER_SETTINGS_FOG_DISTANCE_END * environmentModifier;
        RenderSettings.fogColor = atmosphereColor;
    }

    void UpdateFog(float time)
    {
        Vector3 playerPos = ClientCommand.instance.clientPlayerCharacter.transform.position;
        Vector3 cameraPos = Camera.main.transform.position;
        if(fog != null)
        {
            // float scale = Mathf.Lerp(1f, 3f, Mathf.InverseLerp(20f, 60f, Vector3.Distance(cameraPos, playerPos)));
            // fog.transform.localScale = Vector3.one * scale;
            fog.transform.position = playerPos;
            Shader.SetGlobalColor("_FogColor", atmosphereColor);
        }
    }



}
