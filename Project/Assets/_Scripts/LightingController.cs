using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class LightingController : MonoBehaviour
{

    public static LightingController current;
    
    // post-processing
    public PostProcessVolume volume;
    public ColorGrading colorGrading;

    public GameObject sun, moon;
    public Light sunLight, moonLight;
    public Color sunLightColor_base, sunLightColor_sunrise, sunLightColor_sunset;
    public float time;
    public float timeOfDay;
    public float darkness;
    public Color fogColor_day, fogColor_night;

    // time in seconds for a full day
    public static float period = 5f;
    public static float sunIntensity = .8f;
    public static float moonIntensity = .03f;


    private void Awake()
    {
        current = this;

        sun = GameObject.Find("Sun");
        moon = GameObject.Find("Moon");
        sunLight = sun.GetComponent<Light>();
        moonLight = moon.GetComponent<Light>();

        // initialize pp
        volume = GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out colorGrading);
        

        time = 200f;
        //time = 0f;
    }


    // Update is called once per frame
    void Update()
    {
       
        SetCelestialBodies(time);
        SetColors(time);

        // pause time of day: comment out this line
        //time += Time.deltaTime;
    }

    void SetCelestialBodies(float time){

        float sunX = Mathf.Sin(time / period);
        float sunY = Mathf.Cos(time / period);

        Vector3 sunRot = new Vector3(sunX, sunY, 0f);
        Vector3 moonRot = new Vector3(sunX * -1f, sunY * -1f, 0f);

        sun.transform.rotation = Quaternion.LookRotation(sunRot, Vector3.up);
        moon.transform.rotation = Quaternion.LookRotation(moonRot, Vector3.up);

        float s = (((sunY * -1f) + 1f) / 2f) * sunIntensity;
        sunLight.intensity = Mathf.InverseLerp(.25f, 1f, s);
        moonLight.intensity = (1f - sunLight.intensity) * moonIntensity;

    }

    void SetColors(float time){

        timeOfDay = (Mathf.Cos(time / period) + 1f) / 2f;
        darkness = (Mathf.Sin(time / period) + 1f) / 2f;
        PolyverseSkies.timeOfDay = timeOfDay;
        //RenderSettings.fogColor = Color.Lerp(fogColor_day, fogColor_night, timeOfDay);

        float sunset = Mathf.Max(0f, Mathf.Pow((((1f - darkness) - .5f) * 2f), 15f));
        float sunrise = Mathf.Max(0f, Mathf.Pow((((darkness) - .5f) * 2f), 5f));
        //Debug.Log(sunrise);

        Color color = Color.white;
        color *= Color.Lerp(sunLightColor_base, sunLightColor_sunrise, sunrise);
        color *= Color.Lerp(sunLightColor_base, sunLightColor_sunset, sunset);
        sunLight.color = color;

    }


}
