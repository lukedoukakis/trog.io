using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitButtonController : MonoBehaviour
{

    public EntityHandle handle;
    public GameObject referencedObject;
    public EntityStats referencedObjectStats;
    public EntityInfo referencedObjectInfo;
    public Button button;
    public TextMeshProUGUI tmp;






    string label;






    void Awake(){
        button = GetComponent<Button>();
        tmp = button.GetComponentInChildren<TextMeshProUGUI>();
    }



    public void SetFromObject(EntityHandle handle){

        // set entity handle and info
        this.handle = handle;
        referencedObject = this.handle.gameObject;
        referencedObjectInfo = referencedObject.GetComponent<EntityInfo>();
        if(referencedObjectInfo.nickname == ""){
            label = referencedObjectInfo.species.ToString();
        }
        else{
            label = referencedObjectInfo.nickname;
        }

        // set buttons contained within
        foreach(ActionButtonController abc in transform.GetComponentsInChildren<ActionButtonController>()){
            //Debug.Log("found abc");
            abc.ubc = this;

        }


        UpdateAppearance();
    }

    void UpdateAppearance(){
        tmp.text = label;
    }

    public void OnXButtonPress(){
        UnitMenuController.current.RemoveButton(this.gameObject);
        GlobalSelectionController.current.RemoveFromSelected(handle);
    }

    public void OnButtonPointerEnter(){
        if(!handle.tooltip){
            handle.ShowTooltip();
        }
    }
    public void OnButtonPointerExit(){
        handle.HideTooltip();
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
