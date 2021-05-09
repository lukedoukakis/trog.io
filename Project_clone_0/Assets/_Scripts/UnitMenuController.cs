using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMenuController : MonoBehaviour
{

    public static UnitMenuController current;

    public bool open;
    public GameObject buttonPrefab;
    List<GameObject> buttons;

    



    void Awake(){
        current = this;
        buttons = new List<GameObject>();
    }

    public void AddButton(EntityHandle handle){
        GameObject newButton = Instantiate(buttonPrefab, this.gameObject.transform);
        UnitButtonController ubc = newButton.GetComponent<UnitButtonController>();
        ubc.SetFromObject(handle);
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
