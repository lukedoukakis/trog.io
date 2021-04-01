using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionMenuController : MonoBehaviour
{
    public bool visible;
    public ButtonDropdownController unitBDC;


    public static SelectionMenuController current;
    void Awake()
    {
        current = this;
    }

    public void UpdateSelectionMenu(){
        unitBDC.ClearButtons();
        foreach(ObjectSelectionManager osm in GlobalSelectionController.current.SelectedOSMs.ToArray()){
            unitBDC.AddButton(osm);
        }
    }

    public void ClearSelectionMenu(){
        unitBDC.ClearButtons();

    }

    public void SetVisible(bool b){
        if(b && !visible){
            GetComponent<RectTransform>().localScale = Vector2.one;
        }
        else if(!b && visible){
            GetComponent<RectTransform>().localScale = Vector2.zero;
        }
        visible = b;
    }
    public void ToggleVisible(){
        SetVisible(!visible);
    }

    




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
