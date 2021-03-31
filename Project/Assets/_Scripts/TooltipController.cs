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
	
	public void Show(float time){
		text.enabled = true;
		backgroundImage.enabled = true;
		if(time > -1f){
			StartCoroutine(hideAfterDelay(time));
		}
	}
	public void Hide(){
		text.enabled = false;
		backgroundImage.enabled = false;
	}
	IEnumerator hideAfterDelay(float time){
		yield return new WaitForSeconds(time);
		Hide();
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
