using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipController : MonoBehaviour
{
	
	Image backgroundImage;
	RectTransform backgroundImageT;
	TextMeshProUGUI text;
	RectTransform rectTransform;
	RectTransform canvasRectTransform;
	[SerializeField] Vector2 padding;

	public static TooltipController current;
	public static float DefaultDelay = .3f;
	bool isEnabled;
	
	public void Awake(){
		current = this;
		backgroundImage = GetComponentInChildren<Image>();
		backgroundImageT = backgroundImage.gameObject.GetComponent<RectTransform>();
		text = GetComponentInChildren<TextMeshProUGUI>();
		rectTransform = GetComponent<RectTransform>();
		canvasRectTransform = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<RectTransform>();
	}
	
	public void SetText(string str){
		text.SetText(str);
		text.ForceMeshUpdate();
		Vector2 textSize = text.GetRenderedValues(false);
		backgroundImageT.sizeDelta = textSize + padding;
	}
	
	public void Show(float delay){
		isEnabled = true;
		StartCoroutine(ShowAfterDelay(delay));
	}
	IEnumerator ShowAfterDelay(float delay){
		yield return new WaitForSecondsRealtime(delay);
		if(isEnabled){
			text.enabled = true;
			backgroundImage.enabled = true;
		}
	}

	public void Hide(){
		isEnabled = false;
		text.enabled = false;
		backgroundImage.enabled = false;
	}
	
	
    // Start is called before the first frame update
    void Start()
    {
        SetText("dkvgfhjdkfbhkdmsigbsikdlvnisdkygheudfksgbvkufdhgbvdcfiuksgdbsdkuvhbnsdiugbyhvsdkugbvsodkiulhvbgskudiyfvbhgksumyvbseidukfyvbmfduksygmvshbkedufbg");
		Hide();
		
    }

    // Update is called once per frame
    void Update()
    {
		rectTransform.anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;
    }
}
