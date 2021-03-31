using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownButtonController : MonoBehaviour
{

    public ButtonDropdownController bdc;
    public ObjectSelectionManager referencedOSM;
    public GameObject referencedObject;
    public ObjectStats referencedObjectStats;
    public Button button;
    public TextMeshProUGUI tmp;






    string label;






    void Awake(){
        button = GetComponent<Button>();
        tmp = button.GetComponentInChildren<TextMeshProUGUI>();
    }



    public void SetFromObject(ObjectSelectionManager refOSM){
        referencedOSM = refOSM;
        referencedObject = referencedOSM.gameObject;
        referencedObjectStats = referencedObject.GetComponent<ObjectStats>();
        if(referencedObjectStats.name == null){
            label = referencedObjectStats.type;
        }
        else{
            label = referencedObjectStats.name;
        }
        UpdateLook();
    }

    void UpdateLook(){
        tmp.text = label;
    }

    public void OnXButtonPress(){
        if(bdc != null){
            bdc.RemoveButton(this.gameObject);
            GlobalSelectionController.current.RemoveFromSelected(referencedOSM);
        }
        else{
            Debug.Log("DropdownButtonController: No referenced ButtonDropdownController!");
        }
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
