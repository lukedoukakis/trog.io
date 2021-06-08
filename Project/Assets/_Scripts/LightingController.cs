using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingController : MonoBehaviour
{

    public static LightingController current;
    public GameObject sun, moon;
    public Light sunLight, moonLight;
    public float time;
    public float darkness;
    public Color fogColor_day, fogColor_night;

    // time in seconds for a full day
    public static float period = 8f;
    public static float sunIntensity = .8f;
    public static float moonIntensity = .1f;


    private void Awake()
    {
        current = this;

        sun = GameObject.Find("Sun");
        moon = GameObject.Find("Moon");
        sunLight = sun.GetComponent<Light>();
        moonLight = moon.GetComponent<Light>();
        

        time = 0f;
    }


    // Update is called once per frame
    void Update()
    {
       
        SetCelestialBodies(time);
        SetSkyboxColor(time);

        time += Time.deltaTime;
    }

    void SetCelestialBodies(float time){

        float sunX = Mathf.Sin(time / period);
        float sunY = Mathf.Cos(time / period);

        Vector3 sunRot = new Vector3(sunX, sunY, 0f);
        Vector3 moonRot = new Vector3(sunX * -1f, sunY * -1f, 0f);

        sun.transform.rotation = Quaternion.LookRotation(sunRot, Vector3.up);
        moon.transform.rotation = Quaternion.LookRotation(moonRot, Vector3.up);

        sunLight.intensity = (((sunY * -1f) + 1f) / 2f) * sunIntensity;
        moonLight.intensity = (((sunY) + 1f) / 2f) * moonIntensity;

    }

    void SetSkyboxColor(float time){

        darkness = (Mathf.Sin(time / period) + 1f) / 2f;

        // if (RenderSettings.skybox.HasProperty("_Tint")){
        //     RenderSettings.skybox.SetColor("_Tint", Color.HSVToRGB(h, s, v));
        // }
        // else if (RenderSettings.skybox.HasProperty("_SkyTint")){
        //     RenderSettings.skybox.SetColor("_SkyTint", Color.HSVToRGB(h, s, v));
        // }

        PolyverseSkies.timeOfDay = darkness;
        RenderSettings.fogColor = Color.Lerp(fogColor_day, fogColor_night, darkness);

    }


}
