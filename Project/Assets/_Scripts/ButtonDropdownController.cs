using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonDropdownController : MonoBehaviour
{

    public bool open;
    public GameObject buttonPrefab;
    List<GameObject> buttons;



    void Awake(){
        buttons = new List<GameObject>();
    }

    public void AddButton(EntityHandle handle){
        GameObject newButton = Instantiate(buttonPrefab, this.gameObject.transform);
        DropdownButtonController dbc = newButton.GetComponent<DropdownButtonController>();
        dbc.SetFromObject(handle);
        dbc.bdc = this;
        buttons.Add(newButton);
    }

    public void RemoveButton(GameObject button){
        buttons.Remove(button);
        Destroy(button);
    }

    public void ClearButtons(){
        foreach(GameObject button in buttons.ToArray()){
            buttons.Remove(button);
            Destroy(button);
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
