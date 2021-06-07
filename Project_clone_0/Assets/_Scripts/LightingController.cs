using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingController : MonoBehaviour
{

    public static LightingController current;
    public GameObject sun, moon;
    public Light sunLight, moonLight;
    public Material skyboxMat;
    public Color skyboxColor_base;

    float time;

    // time in seconds for a full day
    public static float period = 4f;
    public static float sunIntensity = .8f;
    public static float moonIntensity = .1f;


    private void Awake()
    {
        current = this;

        sun = GameObject.Find("Sun");
        moon = GameObject.Find("Moon");
        sunLight = sun.GetComponent<Light>();
        moonLight = moon.GetComponent<Light>();
        skyboxMat = Resources.Load<Material>("Materials/skybox");
        skyboxColor_base = Color.HSVToRGB(.53f, .22f, 1f);

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

        float sunUp = Mathf.Sin(time / period);
        float sunRight = Mathf.Cos(time / period);

        Vector3 sunRot = new Vector3(sunUp, sunRight, 0f);
        Vector3 moonRot = new Vector3(sunUp * -1f, sunRight * -1f, 0f);

        sun.transform.rotation = Quaternion.LookRotation(sunRot, Vector3.up);
        moon.transform.rotation = Quaternion.LookRotation(moonRot, Vector3.up);

        sunLight.intensity = ((sunUp + 1f) / 2f) * sunIntensity;
        moonLight.intensity = (((sunUp * -1f) + 1f) / 2f) * moonIntensity;

    }

    void SetSkyboxColor(float time){

        float darkness = (Mathf.Cos(time / period) + 1f) / 2f;

        float h, s, v;
        Color.RGBToHSV(skyboxColor_base, out h, out s, out v);

        // make modifications to color values based on conditions
        h = h;
        s = s;
        v = v * (1f - darkness);

        if (RenderSettings.skybox.HasProperty("_Tint")){
            RenderSettings.skybox.SetColor("_Tint", Color.HSVToRGB(h, s, v));
        }
        else if (RenderSettings.skybox.HasProperty("_SkyTint")){
            RenderSettings.skybox.SetColor("_SkyTint", Color.HSVToRGB(h, s, v));
        }




    }


}
