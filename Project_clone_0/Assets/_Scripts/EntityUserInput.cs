using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class EntityUserInput : EntityComponent
{

    //public enum InteractionType{ TakeItem, PlaceItem, }

    public static float DISTANCE_INTERACTABLE = 2f;
    public static float DODGE_INPUT_TIMESTEP = .2f;


    static float AUTO_SENSE_DISTANCE_FEATURE = 1f;
    static float AUTO_SENSE_DISTANCE_CREATURE = 15f;
    static float AUTO_ATTACK_DISTANCE_FEATURE = 1f;
    static float AUTO_ATTACK_DISTANCE_CREATURE = 2f;


    public bool pressForward, pressBack, pressLeft, pressRight, pressSprint, pressJump, pressWalk, pressDodge;
    public bool pressForwardDown, pressBackDown, pressLeftDown, pressRightDown;
    public float timeOffForward, timeOffBack, timeOffLeft, timeOffRight;
    public bool pressToggleAttackRanged;
    public float rotationY;
    float leftSpeedFromKey, rightSpeedFromKey;

    Transform selectionOrigin;
    Quaternion targetRot;
    public GameObject hoveredInteractableObject;
    public GameObject lastHoveredInteractableObject;
    public List<GameObject> interactableObjects;
    bool newInteract;




    List<GameObject> nearbyFeatures, attackDistanceFeatures;
    List<EntityHandle> nearbyCreatures, nearbyDangerousCreatures, attackDistanceCreatures;


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
        timeOffForward = timeOffBack = timeOffLeft = timeOffRight = 0f;
    }


    void ApplyRotationalInput()
    {
        if(GameManager.GAME_SETINGS_ROTATIONALINPUTMODE == RotationalInputMode.Mouse)
        {
            float sensitivity = 3f;
            float smoothing = 100f * Time.deltaTime;
            float deltaY = Input.GetAxis("Mouse X") * sensitivity * smoothing;
            rotationY = Mathf.Clamp(Mathf.Lerp(rotationY, deltaY, 1f / smoothing), rotationY - 2f, rotationY + 2f);
            targetRot = Quaternion.Slerp(transform.rotation, transform.rotation * Quaternion.Euler(new Vector3(0, rotationY, 0)), 1f / smoothing);
        }
        else if(GameManager.GAME_SETINGS_ROTATIONALINPUTMODE == RotationalInputMode.ArrowKeys)
        {
            float sensitivity = 130f;
            float acceleration = .02f;
            bool left = Input.GetKey(KeyCode.LeftArrow);
            bool right = Input.GetKey(KeyCode.RightArrow);
            if(left)
            {
                leftSpeedFromKey += acceleration;
            }
            else
            {
                leftSpeedFromKey -= acceleration;
            }
            if(right)
            {
                rightSpeedFromKey += acceleration;
            }
            else
            {
                rightSpeedFromKey -= acceleration;
            }
            leftSpeedFromKey = Mathf.Clamp01(leftSpeedFromKey);
            rightSpeedFromKey = Mathf.Clamp01(rightSpeedFromKey);
            float deltaY = ((leftSpeedFromKey * -1f) + rightSpeedFromKey) * sensitivity;
            rotationY = deltaY;
            targetRot = Quaternion.Slerp(transform.rotation, transform.rotation * Quaternion.Euler(new Vector3(0, rotationY, 0)), Time.deltaTime);
        }
        
        transform.rotation = targetRot;
    }

    void HandleMovement()
    {

        Vector3 oldMove = move;
        move = Vector3.zero;

        pressForwardDown = Input.GetKeyDown(KeyCode.W);
        pressBackDown = Input.GetKeyDown(KeyCode.S);
        pressLeftDown = Input.GetKeyDown(KeyCode.A);
        pressRightDown = Input.GetKeyDown(KeyCode.D);

        pressForward = Input.GetKey(KeyCode.W);
        pressBack = Input.GetKey(KeyCode.S);
        pressLeft = Input.GetKey(KeyCode.A);
        pressRight = Input.GetKey(KeyCode.D);
        pressSprint = Input.GetKey(KeyCode.LeftShift) && false;
        pressJump = Input.GetKey(KeyCode.Space);
        pressWalk = Input.GetKey(KeyCode.LeftShift);
        pressToggleAttackRanged = Input.GetKeyDown(KeyCode.LeftControl);

        pressDodge = false;
        if(pressForwardDown)
        {
            if(timeOffForward < DODGE_INPUT_TIMESTEP && pressSprint)
            {
                pressDodge = true;
            }
            timeOffForward = 0f;
        }
        if(pressBackDown)
        {
            if(timeOffBack < DODGE_INPUT_TIMESTEP && pressSprint)
            {
                pressDodge = true;
            }
            timeOffBack = 0f;
        }
        if(pressLeftDown)
        {
            if(timeOffLeft < DODGE_INPUT_TIMESTEP && pressSprint)
            {
                pressDodge = true;
            }
            timeOffLeft = 0f;
        }
        if(pressRightDown)
        {
            if(timeOffRight < DODGE_INPUT_TIMESTEP && pressSprint)
            {
                pressDodge = true;
            }
            timeOffRight = 0f;
        }


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

    void HandleRotation()
    {
        ApplyRotationalInput();
    }

    void HandleManualAttack(){
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            if(entityItems.weaponEquipped_item != null)
            {
                entityPhysics.TryAttack(AttackType.Weapon, null, 0f);
            }
            else
            {
                List<AttackType> nonWeaponAttacks = entityInfo.speciesInfo.behaviorProfile.attackTypes.Where(attackType => !attackType.Equals(AttackType.Weapon)).ToList();
                entityPhysics.TryAttack(nonWeaponAttacks[UnityEngine.Random.Range(0, nonWeaponAttacks.Count)], null, 0f);
            }
        }
    }

    void HandleAutoAttack()
    {

        if(entityItems.weaponEquipped_item == null)
        {
            return;
        }

        Transform targetAutoAttackTransform = GetAutoAttackTransform();
        if(targetAutoAttackTransform != null)
        {
            entityOrientation.SetBodyRotationMode(BodyRotationMode.Target, targetAutoAttackTransform);
            if (entityItems.weaponEquipped_item != null)
            {
                entityPhysics.TryAttack(AttackType.Weapon, targetAutoAttackTransform, .25f);
                //entityPhysics.TryAttack(AttackType.Weapon, null);
            }
            //Debug.Log(targetAutoAttackTransform.name);
        }
        
        else
        {
            entityOrientation.SetBodyRotationMode(BodyRotationMode.Normal, null);
        }
    }


    Transform GetAutoAttackTransform()
    {

        if(nearbyCreatures.Count > 0)
        {
            if(nearbyDangerousCreatures.Count > 0)
            {
                return nearbyDangerousCreatures.OrderBy(entityHandle => Vector3.Distance(transform.position, entityHandle.transform.position)).ToList()[0].transform;
            }

            if(attackDistanceCreatures.Count > 0)
            {
                return attackDistanceCreatures.OrderBy(entityHandle => Vector3.Distance(transform.position, entityHandle.transform.position)).ToList()[0].transform;
            }
        }

        if(attackDistanceFeatures.Count > 0)
        {
            return attackDistanceFeatures.OrderBy(worldObject => Vector3.Distance(transform.position, worldObject.transform.position)).ToList()[0].transform;
        }

        return null;
    }

    void UpdateAutoSense()
    {

        if(entityItems != null && entityItems.weaponEquipped_item != null)
        {
            // get nearby features
            nearbyFeatures = Utility.SenseSurroundingFeatures(transform.position, null, AUTO_SENSE_DISTANCE_FEATURE);

            // remove features where calculated damage from currently equipped weapon would be nearly zero
            foreach(GameObject feature in nearbyFeatures.ToArray())
            {
                ItemHitDetection ihd = feature.GetComponentInParent<ItemHitDetection>();
                if(ihd != null)
                {
                    if(!ihd.isInitialized)
                    {
                        ihd.Init();
                    }
                    if(ihd.stats == null)
                    {
                        Debug.Log("STATS NULL");
                    }
                    if(Stats.CalculateDamage(ihd.stats.combinedStats, entityStats, entityItems.weaponEquipped_item) < .01f)
                    {
                        nearbyFeatures.Remove(feature);
                    }
                }
                else
                {
                    nearbyFeatures.Remove(feature);
                }
            }

            // set attack distance features from nearby features within range
            attackDistanceFeatures = nearbyFeatures.Where(gameObject => Vector3.Distance(transform.position, gameObject.transform.position) < AUTO_ATTACK_DISTANCE_FEATURE).ToList(); 
        }
        else
        {
            nearbyFeatures = new List<GameObject>();
            attackDistanceFeatures = new List<GameObject>();
        }


        // get nearby creatures
        nearbyCreatures = Utility.SenseSurroundingCreatures(transform.position, Species.Any, AUTO_SENSE_DISTANCE_FEATURE).Where(entityHandle => !ReferenceEquals(entityHandle.entityInfo.faction, entityInfo.faction)).ToList();
        
        // set nearby dangerous creatures from nearby creatures who have certain behavior type
        nearbyDangerousCreatures = nearbyCreatures.Where(entityHandle => entityHandle.entityInfo.speciesInfo.behaviorProfile.behaviorType.Equals(BehaviorType.Aggressive) || entityHandle.entityInfo.speciesInfo.behaviorProfile.behaviorType.Equals(BehaviorType.Steadfast)).ToList();
        
        // set attack distance creatures from nearby creatures within range
        attackDistanceCreatures = nearbyCreatures.Where(entityHandle => Vector3.Distance(transform.position, entityHandle.transform.position) < AUTO_ATTACK_DISTANCE_CREATURE).ToList();

    }


    void CheckInteraction(){

        if(Input.GetKeyUp(KeyCode.E)){
            OnInteractInput();
        }
        if(Input.GetKeyUp(KeyCode.F)){
            OnDropInput();
        }
        if(Input.GetKeyUp(KeyCode.Mouse1)){
            OnUseInput();
        }

        if(Input.GetKeyUp(KeyCode.Alpha1)){
            entityItems.ToggleWeaponEquipped();
        }
    }
   
    void OnInteractInput()
    {
        
        if(hoveredInteractableObject == null){ return; }

        InteractionCountLimiter icl = hoveredInteractableObject.GetComponent<InteractionCountLimiter>();
        if(icl != null)
        {
            icl.OnInteract();
        }

        Transform hoveredInteractableObjectTransform = hoveredInteractableObject.transform;

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
                    entityItems.ExchangeWeaponsWithEntity(hoveredEntityHandle.entityItems);
                }
            }
            else if(t == "Bonfire"){
                entityInfo.faction.camp.CastFoodIntoBonfire(entityHandle);
            }
            else if (t.StartsWith("ObjectRack"))
            {
                ObjectRack rack = (ObjectRack)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference();
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
                    // GameObject[] rackObjectsThatAreItem = rack.GetObjectsOnRackThatAreItem(Item.WoodPiece);
                    // if(rackObjectsThatAreItem.Length > 0)
                    // {
                    //     GameObject foundObject = rackObjectsThatAreItem[rackObjectsThatAreItem.Length - 1];
                    //     entityItems.OnObjectTake(foundObject, rack);
                    // }        
                    entityInfo.faction.RemoveItemOwned(Item.WoodPiece, 1, rack, true, entityItems);
                }
                else if (t == "ObjectRack_Bone")
                {
                    // GameObject[] rackObjectsThatAreItem = rack.GetObjectsOnRackThatAreItem(Item.BonePiece);
                    // if(rackObjectsThatAreItem.Length > 0)
                    // {
                    //     GameObject foundObject = rackObjectsThatAreItem[rackObjectsThatAreItem.Length - 1];
                    //     entityItems.OnObjectTake(foundObject, rack);
                    // }    
                    entityInfo.faction.RemoveItemOwned(Item.BonePiece, 1, rack, true, entityItems);
                }
                else if (t == "ObjectRack_Stone")
                {
                    // GameObject[] rackObjectsThatAreItem = rack.GetObjectsOnRackThatAreItem(Item.StoneSmall);
                    // if(rackObjectsThatAreItem.Length > 0)
                    // {
                    //     GameObject foundObject = rackObjectsThatAreItem[rackObjectsThatAreItem.Length - 1];
                    //     entityItems.OnObjectTake(foundObject, rack);
                    // }    
                    entityInfo.faction.RemoveItemOwned(Item.StoneSmall, 1, rack, true, entityItems);
                }
                else if (t == "ObjectRack_Fruit")
                {
                    // GameObject[] rackObjectsThatAreItem = rack.GetObjectsOnRackThatAreItem(Item.StoneSmall);
                    // if(rackObjectsThatAreItem.Length > 0)
                    // {
                    //     GameObject foundObject = rackObjectsThatAreItem[rackObjectsThatAreItem.Length - 1];
                    //     entityItems.OnObjectTake(foundObject, rack);
                    // }    
                    entityInfo.faction.RemoveItemOwnedOfType(ItemType.Fruit, 1, rack, true, entityItems);
                }
                else{

                }    
            }
            else if(t == "Feature")
            {
                Item featureItem = Item.GetItemByName(hoveredInteractableObject.name.Split(' ')[0]);
                GameObject dropWorldObject;
                foreach(Item item in featureItem.drops.GetFlattenedItemList())
                {
                    dropWorldObject = Utility.InstantiateSameName(item.worldObjectPrefab, hoveredInteractableObjectTransform.position, Quaternion.identity);
                    entityItems.OnObjectTake(dropWorldObject, dropWorldObject.GetComponent<ObjectReference>().GetObjectReference());
                }
                if(featureItem == Item.BerryBush)
                {
                    foreach(Transform childT in hoveredInteractableObjectTransform.GetComponentsInChildren<Transform>())
                    {
                        if(childT.name.Split(" ")[0] == "Berries")
                        {
                            GameObject.Destroy(childT.gameObject);
                            break;
                        }
                    }
                }
            }
            else if(t == "Workbench"){
                entityItems.DropHolding((Workbench)hoveredInteractableObject.GetComponent<ObjectReference>().GetObjectReference());
            }
            else if(t == "WorkbenchHammer"){
                Workbench wb = (Workbench)(Utility.FindScriptableObjectReference(hoveredInteractableObject.transform).GetObjectReference());
                wb.OnCraft();
            }
            else if(t == "Creature")
            {
                EntityHandle hoveredEntityHandle = hoveredInteractableObject.GetComponentInParent<EntityHandle>();
                if(hoveredEntityHandle.entityBehavior.IsHalfDomesticated())
                {
                    entityInfo.faction.AddDomesticatedCreature(hoveredEntityHandle, true);
                    hoveredEntityHandle.entityBehavior.SetDomesticated(true);
                }
            }
        }
        
    }

    void OnDropInput(){
        entityItems.OnDropInput();
    }

    void OnUseInput()
    {
        entityItems.OnHoldingUse();
    }


    public static void SetObjectHighlighted(GameObject obj, bool highlight)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        // Debug.Log(obj.name);
        // Debug.Log(renderers.Length);

        if (highlight)
        {
            foreach (Renderer mr in renderers)
            {
                List<Material> mats = new List<Material>(mr.sharedMaterials);
                mats.Add(MaterialController.instance.selectedMaterial);
                mr.sharedMaterials = mats.ToArray();
            }
        }

        if (!highlight)
        {
            foreach (Renderer mr in renderers)
            {
                List<Material> mats = new List<Material>(mr.sharedMaterials);
                mats.Remove(MaterialController.instance.selectedMaterial);
                mr.sharedMaterials = mats.ToArray();
            }
        }
        
    }




    public static Collider GetBestColliderCandidate(Collider[] contendingColliders, Transform referenceT)
    {

        Vector3 referencePos = referenceT.position;
        float minAngle = float.MaxValue;
        float minDistance = float.MaxValue;
        Collider bestMatch = null;
        
        foreach(Collider col in contendingColliders)
        {
            Vector3 closestPoint = col.ClosestPoint(referenceT.position);
            float horizAngle = Vector3.Angle(referenceT.forward, closestPoint - referencePos);

            // by angle
            // if((horizAngle < minAngle && horizAngle < 45f))
            // {
            //     minAngle = horizAngle;
            //     bestMatch = col;
            // }

            // by distance
            float distance = Vector3.Distance(referenceT.position, closestPoint);
            if(distance < minDistance)
            {
                minDistance = distance;
                bestMatch = col;
            }
        }

        return bestMatch;
    }

    public void UpdateHoveredInteractable()
    {

        Transform cameraT = Camera.main.transform;

        bool hoveredOverSomething;
        Collider c;
        if(entityPhysics.isInsideCamp && false)
        {
            RaycastHit hit;
            hoveredOverSomething = Physics.Raycast(selectionOrigin.position, cameraT.forward, out hit, 100f, LayerMaskController.INTERACTABLE, QueryTriggerInteraction.Collide);
            c = hoveredOverSomething ? hit.collider : null;
        }
        else
        {
            Collider[] hitCols = Physics.OverlapSphere(selectionOrigin.position, 1f, LayerMaskController.INTERACTABLE, QueryTriggerInteraction.Collide);
            c = GetBestColliderCandidate(hitCols, selectionOrigin.transform);
            hoveredOverSomething = c != null;
        }

        if(hoveredOverSomething)
        {

            // set hoveredInteractableObject to parent of collider hit
            GameObject hovered = c.transform.parent.gameObject;
            newInteract = hovered != hoveredInteractableObject;


            SetObjectHighlighted(hovered, true);
            if(lastHoveredInteractableObject != null)
            {
                SetObjectHighlighted(lastHoveredInteractableObject, false);
            }

            hoveredInteractableObject = lastHoveredInteractableObject = hovered;
            //Log("hovered: " + hoveredInteractableObject.name);
        }
        else{
            newInteract = true;
            hoveredInteractableObject = null;
            if(lastHoveredInteractableObject != null)
            {
                SetObjectHighlighted(lastHoveredInteractableObject, false);
                lastHoveredInteractableObject = null;
            }
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
                                if(hasWeaponUnequipped_hoveredEntity)
                                {
                                    txt += "Trade weapons";
                                }
                                else
                                {
                                    txt += "Give weapon";
                                }
                                
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
                            else if(hasWeaponUnequipped_hoveredEntity)
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
                    txt += "Bones";
                    break;
                case "ObjectRack_Stone" :
                    txt += "Stones";
                    break;
                 case "ObjectRack_Fruit" :
                    txt += "Fruits";
                    break;
                case "Workbench" :
                    txt += "Worktable";
                    break;
                case "WorkbenchHammer" :
                    Workbench wb = (Workbench)(Utility.FindScriptableObjectReference(hoveredInteractableObject.transform).GetObjectReference());
                    txt += !wb.IsEmpty() && wb.currentCraftableItem != null ? "Craft: " + wb.currentCraftableItem.nme : "";
                    break;
                case "Feature" :
                    txt += hoveredInteractableObject.name;
                    break;
                case "Creature" :
                    EntityBehavior eb = hoveredInteractableObject.GetComponent<EntityBehavior>();
                    if(eb.IsHalfDomesticated())
                    {
                        txt += "Tame ";
                    }
                    txt += hoveredInteractableObject.name;
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

        if(IsClientPlayerCharacter())
        {

            HandleMovement();
            if(!UIController.UImode){
                HandleRotation();
            }
            if(GameManager.GAME_SETTINGS_AUTO_ATTACK)
            {
                HandleAutoAttack();
            }
            else
            {
                HandleManualAttack();
            }
        
            CheckInteraction();


            float dTime = Time.deltaTime;
            timeOffForward += dTime;
            timeOffBack += dTime;
            timeOffLeft += dTime;
            timeOffRight += dTime;
        
        }

    }



    void FixedUpdate()
    {
        entityPhysics.moveDir = move;

        UpdateHoveredInteractable();
        if(GameManager.GAME_SETTINGS_AUTO_ATTACK)
        {
            UpdateAutoSense();
        }
    }



}
