using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityUserInput : EntityComponent
{

    //public enum InteractionType{ TakeItem, PlaceItem, }

    public static float DISTANCE_INTERACTABLE = 2f;

    public bool pressForward, pressBack, pressLeft, pressRight, pressSprint, pressJump, pressCrouch, pressDodge;
    public bool pressToggleAttackRanged;
    public float mouseX, mouseY, mouseZ;

    Transform selectionOrigin;
    Quaternion targetRot;
    public GameObject hoveredInteractableObject;
    public List<GameObject> interactableObjects;
    bool newInteract;




    public Vector3 move;

    protected override void Awake()
    {

        this.fieldName = "entityUserInput";

        base.Awake();

        selectionOrigin = Utility.FindDeepChild(transform, "SelectionOrigin");
    }


    void Start()
    {
        pressToggleAttackRanged = false;
    }

    

    void ApplyMouseInput(){

        float sensitivity = 5f;
        float smoothing = 100f * Time.deltaTime;

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

        Vector3 oldMove = move;
        move = Vector3.zero;

        pressForward = Input.GetKey(KeyCode.W);
        pressBack = Input.GetKey(KeyCode.S);
        pressLeft = Input.GetKey(KeyCode.A);
        pressRight = Input.GetKey(KeyCode.D);
        pressSprint = Input.GetKey(KeyCode.LeftShift);
        pressJump = Input.GetKey(KeyCode.Space);
        //pressDodge = Input.GetKey(KeyCode.Space);
        pressCrouch = Input.GetKey(KeyCode.LeftControl);
        pressToggleAttackRanged = Input.GetKeyDown(KeyCode.LeftControl);


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
        if(pressToggleAttackRanged){
            if(!entityPhysics.weaponCharging)
            {
                entityItems.ToggleWeaponRanged();
            }
        }
        if(pressDodge)
        {
            //Debug.Log("DODGE!!");
            entityPhysics.TryDodge();
        }

        if(move == Vector3.zero && entityPhysics.isDodging)
        {
            move = oldMove;
        }




        move = move.normalized;

    }

    void HandleRotation(){
        ApplyMouseInput();
    }

    void HandleAttack(){
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            if(entityItems.weaponEquipped_item != null || true)
            {
                entityPhysics.Attack(AttackType.Weapon, null);
            }
            else
            {
                List<AttackType> nonWeaponAttacks = entityInfo.speciesInfo.behaviorProfile.attackTypes.Where(attackType => !attackType.Equals(AttackType.Weapon)).ToList();
                entityPhysics.Attack(nonWeaponAttacks[UnityEngine.Random.Range(0, nonWeaponAttacks.Count)], null);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Mouse0)){
            if(entityItems.weaponEquipped_item != null || true)
            {
                entityPhysics.Attack(AttackType.Weapon, null);
            }
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
   
    void OnInteractInput()
    {

        //Log("Hovered object: " + hoveredInteractableObject.name);

        // if hovering over something, interact with it
        if(hoveredInteractableObject == null || Vector3.Distance(hoveredInteractableObject.transform.position, selectionOrigin.position) > DISTANCE_INTERACTABLE){
            entityItems.OnEmptyInteract();
        }

        else{
            string t = hoveredInteractableObject.tag;
            if(t == "Item"){
                entityItems.OnObjectTake(hoveredInteractableObject, hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference());
            }
            else if(t == "Npc")
            {
                EntityHandle hoveredEntityHandle = hoveredInteractableObject.GetComponentInParent<EntityHandle>();
                if(hoveredEntityHandle.entityInfo.faction.Equals(entityInfo.faction))
                {
                    entityItems.ExchangeItemsWithEntity(hoveredEntityHandle.entityItems);
                }
            }
            else if(t == "Bonfire"){
                entityInfo.faction.camp.CastFoodIntoBonfire(entityHandle);
            }
            else if (t.StartsWith("ObjectRack"))
            {
                if (t == "ObjectRack_Food")
                {
                    //entityItems.DropHolding((ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference());
                }
                else if (t == "ObjectRack_Clothing")
                {
                    //entityItems.DropHolding((ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference());
                }
                else if (t == "ObjectRack_Weapons")
                {
                    //entityItems.DropEquippedWeapon((ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference());
                }
                else if (t == "ObjectRack_Wood")
                {
                    entityInfo.faction.RemoveItemOwned(Item.WoodPiece, 1, (ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference(), true, entityItems);
                    //entityItems.DropHolding((ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference());
                }
                else if (t == "ObjectRack_Bone")
                {
                    entityInfo.faction.RemoveItemOwned(Item.BonePiece, 1, (ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference(), true, entityItems);
                    //entityItems.DropHolding((ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference());
                }
                else if (t == "ObjectRack_Stone")
                {
                    entityInfo.faction.RemoveItemOwned(Item.StoneSmall, 1, (ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference(), true, entityItems);
                    //entityItems.DropHolding((ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference());
                }
                else{

                }    
            }
            else if(t == "Workbench"){
                entityItems.DropHolding((Workbench)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference());
            }
            else if(t == "WorkbenchHammer"){
                Workbench wb = (Workbench)(Utility.FindScriptableObjectReference(hoveredInteractableObject.transform).GetObjectReference());
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

        if(Physics.Raycast(cameraT.position, cameraT.forward, out hit, 100f, LayerMaskController.INTERACTABLE, QueryTriggerInteraction.Collide)){
        //if(Physics.SphereCast(selectionOrigin.position, .3f, entityOrientation.body.forward, out hit, 1f, LAYERMASK_INTERACTABLE, QueryTriggerInteraction.Collide)){

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
        if(hoveredInteractableObject == null || Vector3.Distance(hoveredInteractableObject.transform.position, selectionOrigin.position) > DISTANCE_INTERACTABLE){
            InteractionPopupController.current.Hide();
        }
        else
        {

            //Debug.Log(hoveredInteractableObject.tag);

            // get the correct text based on the interactable object we are dealing with
            string txt = "";
            switch (hoveredInteractableObject.tag){
                case "Item" : 
                    txt += hoveredInteractableObject.name;
                    break;
                case "Npc" :
                    EntityHandle hoveredEntityHandle = hoveredInteractableObject.GetComponentInParent<EntityHandle>();
                    if(hoveredEntityHandle.entityInfo.faction.Equals(entityInfo.faction))
                    {
                        bool hasWeaponEquipped_thisEntity = entityItems.weaponEquipped_item != null;
                        bool hasWeaponUnequipped_thisEntity = entityItems.weaponUnequipped_item != null;
                        bool hasWeaponEquipped_hoveredEntity =  hoveredEntityHandle.entityItems.weaponEquipped_item != null;
                        bool hasWeaponUnequipped_hoveredEntity =  hoveredEntityHandle.entityItems.weaponUnequipped_item != null;

                        if(hasWeaponEquipped_thisEntity)
                        {
                            if(hasWeaponEquipped_hoveredEntity)
                            {
                                txt += "Trade weapons";
                            }
                            else
                            {
                                if(hasWeaponUnequipped_hoveredEntity)
                                {
                                    txt += "Trade weapons";
                                }
                                else
                                {
                                    txt += "Give weapon";
                                }
                            }
                        }
                        else
                        {
                            if(hasWeaponEquipped_hoveredEntity)
                            {
                                txt += "Take Weapon";
                            }
                        }
                    }
                    break;

                case "Bonfire" : 
                    txt += "Fire"; 
                    break;
                case "ObjectRack_Food" :
                    txt += "";
                    break;
                case "ObjectRack_Clothing" :
                    txt += "";
                    break;
                case "ObjectRack_Weapons" :
                    txt += "";
                    break;
                case "ObjectRack_Wood" :
                    txt += "Wood";
                    break;
                case "ObjectRack_Bone" :
                    txt += "Animal bones";
                    break;
                case "ObjectRack_Stone" :
                    txt += "Stones";
                    break;

                case "Workbench" :
                    txt += "Worktable";
                    break;
                case "WorkbenchHammer" :
                    Workbench wb = (Workbench)(Utility.FindScriptableObjectReference(hoveredInteractableObject.transform).GetObjectReference());
                    txt += !wb.IsEmpty() && wb.currentCraftableItem != null ? "Craft: " + wb.currentCraftableItem.nme : "";
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
