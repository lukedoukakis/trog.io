using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceRenderTexture : MonoBehaviour
{

    [SerializeField] RenderTexture renderTexture;
    RectTransform rectTransform;


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        renderTexture.width = Screen.width / 5;
        renderTexture.height = Screen.height / 5;
        rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
    }

}
