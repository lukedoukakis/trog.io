using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPopupController : MonoBehaviour
{

    public static InteractionPopupController current;

    [SerializeField] Transform parent;
    [SerializeField] TextMeshProUGUI text;
    bool showing;


    void Awake(){
        current = this;
    }

    void Start(){
        Show();
        Hide();
    }   

    public void SetText(string txt){
        this.text.SetText(txt);
    }

    public void Show(){
        if(!showing){
            parent.localScale = Vector3.one;
            showing = true;
        }
    }
    public void Hide(){
        if(showing){
            parent.localScale = Vector3.zero;
            showing = false;
        }
    }

}
