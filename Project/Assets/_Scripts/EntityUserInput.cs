using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityUserInput : EntityComponent
{

    //public enum InteractionType{ TakeItem, PlaceItem, }


    public bool pressForward, pressBack, pressLeft, pressRight, pressSprint, pressJump, pressCrouch;
    public float mouseX, mouseY, mouseZ;

    Quaternion targetRot;
    public GameObject hoveredInteractableObject;
    public List<GameObject> interactableObjects;

    // todo: use all interactable layers
    public LayerMask LAYERMASK_INTERACTABLE;
    bool newInteract;




    public Vector3 move;

    protected override void Awake(){

        base.Awake();

        LAYERMASK_INTERACTABLE = LayerMask.GetMask("HoverTrigger");
    }


    void Start(){

    }

    

    void ApplyMouseInput(){

        float sensitivity = 5f;
        float smoothing = 300f * Time.deltaTime;

        if(GameManager.cameraMode == 0){
            
        }
        else if(GameManager.cameraMode == 1){

            float deltaY = Input.GetAxis("Mouse X") * sensitivity * smoothing;
            mouseY = Mathf.Clamp(Mathf.Lerp(mouseY, deltaY, 1f / smoothing), mouseY - 2f, mouseY + 2f);

            mouseX = 0f;
            mouseZ = 0f;

            //Log(mouseY.ToString());

            targetRot = Quaternion.Slerp(transform.rotation, transform.rotation * Quaternion.Euler(new Vector3(mouseX, mouseY, mouseZ)), 1f / smoothing);
            transform.rotation = targetRot;
           
        }
        
    }

    void HandleMovement(){

        move = Vector3.zero;

        pressForward = Input.GetKey(KeyCode.W);
        pressBack = Input.GetKey(KeyCode.S);
        pressLeft = Input.GetKey(KeyCode.A);
        pressRight = Input.GetKey(KeyCode.D);
        pressSprint = Input.GetKey(KeyCode.LeftShift);
        pressJump = Input.GetKey(KeyCode.Space);
        pressCrouch = Input.GetKey(KeyCode.LeftControl);

        if (pressForward)
        {
            move.z += 1;
        }
        if (pressBack)
        {
            move.z -= 1;
        }
        if (pressLeft)
        {
            move.x -= 1;
        }
        if (pressRight)
        {
            move.x += 1;
        }
        if(pressSprint){
            //entityPhysics.sprinting = true;
        }
        if (pressJump)
        {
            if(entityPhysics.CanJump()){
                entityPhysics.Jump();
            }
        }
        if(pressCrouch){
            entityPhysics.OnCrouchInput();
        }

        move = move.normalized;

    }

    void HandleRotation(){
        ApplyMouseInput();
    }

    void HandleAttack(){
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            entityPhysics.Attack(AttackType.Weapon, null);
        }
        else if(Input.GetKeyUp(KeyCode.Mouse0)){
            entityPhysics.Attack(AttackType.Weapon, null);
        }
    }


    void CheckInteraction(){

        if(Input.GetKeyUp(KeyCode.E)){
            OnInteractInput();
        }
        if(Input.GetKeyUp(KeyCode.F)){
            OnDropInput();
        }

        if(Input.GetKeyUp(KeyCode.Alpha1)){
            entityItems.ToggleWeaponEquipped();
        }
    }
   
    void OnInteractInput(){

        //Log("Hovered object: " + hoveredInteractableObject.name);

        // if hovering over something, interact with it
        if(hoveredInteractableObject == null){
            entityItems.OnEmptyInteract();
        }

        else{
            string t = hoveredInteractableObject.tag;
            if(t == "Item"){
                entityItems.OnObjectInteract(hoveredInteractableObject, hoveredInteractableObject.GetComponent<ScriptableObjectReference>().GetScriptableObject());
            }
            else if (t.StartsWith("ObjectRack"))
            {
                if (t == "ObjectRack_Food")
                {
                    entityItems.DropHolding((ObjectRack)hoveredInteractableObject.GetComponent<ScriptableObjectReference>().GetScriptableObject());
                }
                else if (t == "ObjectRack_Clothing")
                {
                    entityItems.DropHolding((ObjectRack)hoveredInteractableObject.GetComponent<ScriptableObjectReference>().GetScriptableObject());
                }
                else if (t == "ObjectRack_Weapons")
                {
                    entityItems.DropEquippedWeapon((ObjectRack)hoveredInteractableObject.GetComponent<ScriptableObjectReference>().GetScriptableObject());
                }
                else if (t == "ObjectRack_Wood")
                {
                    entityItems.DropHolding((ObjectRack)hoveredInteractableObject.GetComponent<ScriptableObjectReference>().GetScriptableObject());
                }
                else if (t == "ObjectRack_Bone")
                {
                    entityItems.DropHolding((ObjectRack)hoveredInteractableObject.GetComponent<ScriptableObjectReference>().GetScriptableObject());
                }
                else{

                }    
            }
            else if(t == "Workbench"){
                entityItems.DropHolding((Workbench)hoveredInteractableObject.GetComponent<ScriptableObjectReference>().GetScriptableObject());
            }
            else if(t == "WorkbenchHammer"){
                Workbench wb = (Workbench)(Utility.FindScriptableObjectReference(hoveredInteractableObject.transform).GetScriptableObject());
                wb.OnCraft();
            }
        }
        
    }

    void OnDropInput(){
        entityItems.OnDropInput();
    }



    public void UpdateHoveredInteractable(){
        Transform cameraT = Camera.main.transform;
        RaycastHit hit;

        if(Physics.Raycast(cameraT.position, cameraT.forward, out hit, Vector3.Distance(transform.position, cameraT.position) + 2f, LAYERMASK_INTERACTABLE, QueryTriggerInteraction.Collide)){
            
            // set hoveredInteractableObject to parent of collider hit
            GameObject hovered = hit.collider.transform.parent.gameObject;
            newInteract = hovered != hoveredInteractableObject;
            hoveredInteractableObject = hovered;
            //Log("hovered: " + hoveredInteractableObject.name);
        }
        else{
            newInteract = true;
            hoveredInteractableObject = null;
            //Log("NO HOVERED INTERACTABLE GAMEOBJECT");
        }

        if(newInteract || true){
            HandleInteractionPopup();
        }

        // todo: interact ui popup
    }

    void HandleInteractionPopup(){
        if(hoveredInteractableObject == null){
            InteractionPopupController.current.Hide();
        }
        else
        {

            Item item;

            // get the correct text based on the interactable object we are dealing with
            string txt = "";
            switch (hoveredInteractableObject.tag){
                case "Item" : 
                    txt += "E: Pick up " + hoveredInteractableObject.name;
                    break;
                case "ObjectRack_Food" :
                    item = entityItems.holding_item;
                    if(item == null || !item.type.Equals(Item.ItemType.Food)){
                        txt = "";
                    }
                    else{
                        txt += "E: Place " + item.nme;
                    }
                    break;
                case "ObjectRack_Clothing" :
                    item = entityItems.holding_item;
                    if(item == null || !item.type.Equals(Item.ItemType.Clothing)){
                        txt = "";
                    }
                    else{
                        txt += "E: Place " + item.nme;
                    }
                    break;

                case "ObjectRack_Weapons" :
                    item = entityItems.weaponEquipped_item;
                    if(item == null){
                        txt = "";
                    }
                    else{
                        txt += "E: Place " + item.nme;
                    }
                    break;
                case "ObjectRack_Wood" :
                    item = entityItems.holding_item;
                    if(item == null || !item.type.Equals(Item.ItemType.Wood)){
                        txt = "";
                    }
                    else{
                        txt += "E: Place " + item.nme;
                    }
                    break;
                case "ObjectRack_Bone" :
                    item = entityItems.holding_item;
                    if(item == null || !item.type.Equals(Item.ItemType.Bone)){
                        txt = "";
                    }
                    else{
                        txt += "E: Place " + item.nme;
                    }
                    break;

                case "Workbench" :
                    item = entityItems.holding_item;
                    if(item == null){
                        txt = "";
                    }
                    else{
                        txt += "E: Place " + item.nme;
                    }
                    break;
                case "WorkbenchHammer" :

                    Workbench wb = (Workbench)(Utility.FindScriptableObjectReference(hoveredInteractableObject.transform).GetScriptableObject());
                    txt += !wb.IsEmpty() && wb.currentCraftableItem != null ? "E: Craft " + wb.currentCraftableItem.nme : "Drop resources onto the table to craft cool new things!";
                    break;
                // todo: handle other types of objects
                default:
                    txt += "<nomsg>";
                    break;
            }
            InteractionPopupController.current.SetText(txt);
            InteractionPopupController.current.Show();

        }
    }


    void Update(){

        if(isLocalPlayer){
            HandleMovement();
            if(!UIController.UImode){
                HandleRotation();
            }
            HandleAttack();
            CheckInteraction();
        
        }

    }

    void FixedUpdate()
    {
        entityPhysics.moveDir = move;

        UpdateHoveredInteractable();
    }



}
