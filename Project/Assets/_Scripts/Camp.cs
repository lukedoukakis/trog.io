using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camp : ScriptableObject
{


    public static float BASE_CAMP_RADIUS;
    public enum ComponentType{
        Bonfire, Workbench, FoodRack, WeaponsRack, ClothingRack, Tent, Anvil
    }


    // --
    public Faction faction;
    public Vector3 origin;
    public float radius;
    public GameObject layout;

    public Bonfire bonfire;
    public Workbench workbench;
    public List<ItemRack> foodRacks;
    public List<ItemRack> weaponsRacks;
    public List<ItemRack> clothingRacks;
    public List<Tent> tents;
    public Anvil anvil;






    // client method to place a Camp
    public static void TryPlaceCamp(Faction faction, Vector3 position){
        if(CanPlaceCamp(position)){
            PlaceCamp(faction, position);
        }
        else{
            Debug.Log("Can't place camp - doesn't fit requirements");
        }
    }
    public static bool CanPlaceCamp(Vector3 position){
        // determine if flat enough
        return true;
    }
    public static Camp PlaceCamp(Faction faction, Vector3 position){
        Camp camp = ScriptableObject.CreateInstance<Camp>();
        faction.camp = camp;
        camp.faction = faction;
        camp.foodRacks = new List<ItemRack>();
        camp.weaponsRacks = new List<ItemRack>();
        camp.clothingRacks = new List<ItemRack>();
        camp.tents = new List<Tent>();
        camp.SetOrigin(position);
        camp.SetRadius(faction.members.Count);
        camp.SetCampLayout(position, Quaternion.identity);
        camp.PlaceCampComponents();
        return camp;
    }

    public void SetOrigin(Vector3 position){
        this.origin = position;
    }

    public void SetRadius(int population){
        this.radius = BASE_CAMP_RADIUS + (BASE_CAMP_RADIUS * population * 05f);
    }

    // place and adjust camp layout for component placement
    public void SetCampLayout(Vector3 position, Quaternion rotation){

        layout = Instantiate(CampResources.Prefab_CampLayout, position, Quaternion.identity);
        foreach(Transform orientation in layout.transform){
            Debug.Log("AdjustCampLayout(): adjusting orientation: " + orientation.name);
            Vector3 pos = orientation.position;
            pos.y = ChunkGenerator.ElevationAmplitude;
            RaycastHit hit;
            if(Physics.Raycast(pos, Vector3.down, out hit, ChunkGenerator.ElevationAmplitude, CampResources.LayerMask_Terrain)){
                orientation.position = hit.point;
            }
            else{
                orientation.position = Vector3.one * float.MaxValue;
            }
            //orientation.rotation = Quaternion.LookRotation(GetCampComponentOrientation(ComponentType.Bonfire).position - orientation.position, Vector3.up);
            Vector3 toCenterEulers = Quaternion.LookRotation(GetCampComponentOrientation(ComponentType.Bonfire).position - orientation.position, Vector3.up).eulerAngles;
            Vector3 normalEulers = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
            Vector3 orientationEulers = orientation.rotation.eulerAngles;
            //orientationEulers.z = normalEulers.z;
            orientationEulers.x = normalEulers.x;
            orientationEulers.y = toCenterEulers.y;
            orientation.rotation = Quaternion.Euler(orientationEulers);


        }
    }

    // place all relevant components (racks, tents, etc) based on faction items
    public void PlaceCampComponents(){

        ItemCollection factionItems = faction.ownedItems;

        // create lists for component placement for those that are based on faction's items
        List<GameObject> foodList = new List<GameObject>();
        List<GameObject> weaponsList = new List<GameObject>();
        List<GameObject> clothingList = new List<GameObject>();
        List<GameObject> miscLargeList = new List<GameObject>();
        List<GameObject> miscSmallList = new List<GameObject>();
        foreach(Item item in factionItems.items.Keys){
            Enum type = item.type;
            int itemCount = factionItems.GetItemCount(item);
            List<GameObject> list;
        
            if(type.Equals(Item.Type.Food)){
                list = foodList;
            }
            else if(type.Equals(Item.Type.Weapon)){
                list = weaponsList;
            }
            else if(type.Equals(Item.Type.Clothing)){
                list = clothingList;
            }
            else if(type.Equals(Item.Type.MiscLarge)){
                list = miscLargeList;
            }
            else if(type.Equals(Item.Type.MiscSmall)){
                list = miscSmallList;
            }
            else{
                list = miscLargeList;
                Debug.Log("Unrecognized item type");
            }

            for(int i = 0; i < itemCount; ++i){
                list.Add(Instantiate(item.gameobject));
            }

        }

        // place bonfire
        PlaceBonfire();

        // place workbench
        PlaceWorkbench();

        // place item racks
        PlaceItemRack(Item.Type.Food, foodList);
        PlaceItemRack(Item.Type.Weapon, weaponsList);
        PlaceItemRack(Item.Type.Clothing, clothingList);

        // place anvil
        PlaceAnvil();

        // place tents
        for(int i = 0; i < faction.members.Count / 2; ++i){
            PlaceTent();
        }

        // TODO: place items from miscLarge and miscSmall lists


    }


    public Transform GetCampComponentOrientation(Enum componentType){

        string search;

        switch (componentType) {
            case ComponentType.Bonfire :
                search = "OrientationBonfire";
                break;
            case ComponentType.Workbench :
                search = "OrientationWorkbench";
                break;
            case ComponentType.FoodRack :
                search = "OrientationFoodRack" + foodRacks.Count;
                break;
            case ComponentType.WeaponsRack :
                search = "OrientationWeaponsRack" + weaponsRacks.Count;
                break;
            case ComponentType.ClothingRack :
                search = "OrientationClothingRack" + clothingRacks.Count;
                break;
            case ComponentType.Tent :
                search = "OrientationTent" + tents.Count;
                break;
            case ComponentType.Anvil :
                search = "OrientationAnvil";
                break;
            default:
                search = "OrientationBonfire";
                Debug.Log("Getting component position for unsupported component type");
                break;
        }

        Debug.Log("GetCampComponentOrientation(): orientation name: " + search);
        return FindOrientationInCampLayout(search);

    }

    public Transform FindOrientationInCampLayout(string search){
        return layout.transform.Find(search);
    }


    public void PlaceBonfire(){
        Bonfire bonfire = ScriptableObject.CreateInstance<Bonfire>();
        bonfire.SetBonfire(this, faction.ownedItems.GetItemCount(Item.Wood) > 1f, 1f, 1f);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Bonfire);
        bonfire.worldObject.transform.position = targetOrientation.position;
        bonfire.worldObject.transform.rotation = targetOrientation.rotation;
        this.bonfire = bonfire;
    }

    public void PlaceWorkbench(){
        Workbench workbench = ScriptableObject.CreateInstance<Workbench>();
        workbench.SetWorkbench(this);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Workbench);
        workbench.worldObject.transform.position = targetOrientation.position;
        workbench.worldObject.transform.rotation = targetOrientation.rotation;
        this.workbench = workbench;
    }

    public void PlaceItemRack(Enum itemType, List<GameObject> objects){
        ItemRack itemRack =  ScriptableObject.CreateInstance<ItemRack>();
        itemRack.SetItemRack(this, itemType);
        Enum componentType;
        List<ItemRack> rackList;
        switch (itemType) {
            case Item.Type.Food :
                componentType = ComponentType.FoodRack;
                rackList = this.foodRacks;
                break;
            case Item.Type.Weapon :
                componentType = ComponentType.WeaponsRack;
                rackList = this.weaponsRacks;
                break;
            case Item.Type.Clothing :
                componentType = ComponentType.ClothingRack;
                rackList = this.clothingRacks;
                break;
            default :
                componentType = ComponentType.FoodRack;
                rackList = this.foodRacks;
                Debug.Log("Placing item rack for unsupported item type: " + itemType);
                break;
        }
        Transform targetOrientation = GetCampComponentOrientation(componentType);
        itemRack.worldObject.transform.position = targetOrientation.position;
        itemRack.worldObject.transform.rotation = targetOrientation.rotation;
        rackList.Add(itemRack);
        itemRack.AddItems(objects);
    }
    public void OnRackCapacityReached(Enum itemType, List<GameObject> objectsToAdd){

        // search existing racks for spots
        List<ItemRack> rackList;
        if (itemType.Equals(Item.Type.Food))
        {
            rackList = this.foodRacks;
        }
        else if (itemType.Equals(Item.Type.Weapon))
        {
            rackList = this.weaponsRacks;
        }
        else if (itemType.Equals(Item.Type.Clothing))
        {
            rackList = this.clothingRacks;
        }
        else{
            Debug.Log("Unrecognized item type");
            rackList = this.foodRacks;
        }

        // iterate through racks and add objects to first one with available slots (potentially recursive)
        foreach(ItemRack rack in rackList){
            if(rack.items.Count < rack.capacity){
                rack.AddItems(objectsToAdd);
                break;
            }
        }

        // if there are still objects to add, place new rack
        if(objectsToAdd.Count > 0){
            PlaceItemRack(itemType, objectsToAdd);
        }
        
    }

    public void PlaceAnvil(){
        Anvil anvil = ScriptableObject.CreateInstance<Anvil>();
        anvil.SetAnvil(this);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Anvil);
        anvil.worldObject.transform.position = targetOrientation.position;
        anvil.worldObject.transform.rotation = targetOrientation.rotation;
        this.anvil = anvil;
    }

    public void PlaceTent(){
        Tent tent = ScriptableObject.CreateInstance<Tent>();
        tent.SetTent(this);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Tent);
        tent.worldObject.transform.position = targetOrientation.position;
        tent.worldObject.transform.rotation = targetOrientation.rotation;
        this.tents.Add(tent);
    }





}
