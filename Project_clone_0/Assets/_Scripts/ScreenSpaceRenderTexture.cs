using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceRenderTexture : MonoBehaviour
{

    [SerializeField] RenderTexture renderTexture;
    RectTransform rectTransform;
    [SerializeField] int resolutionDivisor;


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        renderTexture.width = Screen.width / resolutionDivisor;
        renderTexture.height = Screen.height / resolutionDivisor;
        rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
    }

}
