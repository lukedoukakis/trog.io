using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightFlicker : MonoBehaviour
{


    [SerializeField] new Light light;
    [SerializeField] float intensityMin, intensityMax;
    [SerializeField] float flickerSpeed;
    [SerializeField] float exponent;


    // Update is called once per frame
    void Update()
    {
        light.intensity = Mathf.Lerp(intensityMin, intensityMax, Mathf.Pow(Mathf.PerlinNoise(Time.deltaTime * flickerSpeed, 0), exponent));
    }
}
