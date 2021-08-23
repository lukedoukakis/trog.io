using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camp : MonoBehaviour
{


    public static GameObject CAMP_LAYOUT_PREFAB;
    public static LayerMask LAYERMASK_TERRAIN = LayerMask.GetMask("Terrain");


    public static float BASE_CAMP_RADIUS;
    public enum ComponentType{
        Bonfire, FoodRack, WeaponsRack, ClothingRack, Tent, Anvil
    }


    // --
    public Faction faction;
    public Vector3 origin;
    public float radius;
    public GameObject layout;

    public Bonfire bonfire;
    public List<ItemRack> foodRacks;
    public List<ItemRack> weaponsRacks;
    public List<ItemRack> clothingRacks;
    public List<GameObject> tents;
    public GameObject anvil;







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
        Camp camp = new Camp();
        camp.SetOrigin(position);
        camp.SetRadius(faction.population);
        camp.SetCampLayout(position, Quaternion.identity);
        camp.PlaceCampComponents(faction.ownedItems);
        camp.faction = faction;
        faction.camp = camp;
        return camp;
    }

    public void SetOrigin(Vector3 position){
        this.origin = position;
    }

    public void SetRadius(int population){
        this.radius = BASE_CAMP_RADIUS + (BASE_CAMP_RADIUS * population * 05f);
    }

    public void SetCampLayout(Vector3 position, Quaternion rotation){

        layout = Instantiate(CAMP_LAYOUT_PREFAB, position, Quaternion.identity);
        foreach(Transform orientation in layout.transform){
            Debug.Log("AdjustCampLayout(): adjusting orientation: " + orientation.name);
            Vector3 pos = orientation.position;
            RaycastHit hit;
            if((Physics.Raycast(pos, Vector3.down, out hit, 10f, LAYERMASK_TERRAIN)) || Physics.Raycast(pos, Vector3.up, out hit, 10f, LAYERMASK_TERRAIN)){
                orientation.position = hit.point;
                orientation.rotation = Quaternion.FromToRotation(transform.up, hit.normal);
            }
        }
    }

    public void PlaceCampComponents(ItemCollection factionItems){

        // bonfire
        PlaceBonfire();

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

        // place item racks
        PlaceItemRack(Item.Type.Food, foodList);
        PlaceItemRack(Item.Type.Weapon, weaponsList);
        PlaceItemRack(Item.Type.Clothing, clothingList);

        // place anvil
        PlaceAnvil();

        // TODO: place items from miscLarge and miscSmall lists


    }


    public Transform GetCampComponentOrientation(Enum componentType){

        string search;

        switch (componentType) {
            case ComponentType.Bonfire :
                search = "Bonfire";
                break;
            case ComponentType.FoodRack :
                search = "FoodRack" + foodRacks.Count;
                break;
            case ComponentType.WeaponsRack :
                search = "WeaponsRack" + weaponsRacks.Count;
                break;
            case ComponentType.ClothingRack :
                search = "ClothingRack" + clothingRacks.Count;
                break;
            case ComponentType.Tent :
                search = "Tent" + tents.Count;
                break;
            case ComponentType.Anvil :
                search = "Anvil";
                break;
            default:
                search = "Bonfire";
                Debug.Log("Getting component position for unsupported component type");
                break;
        }

        return FindOrientationInCampLayout(search);

    }

    public Transform FindOrientationInCampLayout(string search){
        return layout.transform.Find(search);
    }


    public void PlaceBonfire(){
        Bonfire bonfire = new Bonfire();
        bonfire.SetBonfire(this, faction.ownedItems.GetItemCount(Item.Wood) > 1f, 1f, 1f);
        Transform targetOrientation = GetCampComponentOrientation(ComponentType.Bonfire);
        bonfire.worldObject.transform.position = targetOrientation.position;
        bonfire.worldObject.transform.rotation = targetOrientation.rotation;
    }

    public void PlaceItemRack(Enum itemType, List<GameObject> objects){
        ItemRack itemRack = new ItemRack();
        itemRack.SetItemRack(this, itemType, objects);
        Enum componentType;
        switch (itemType) {
            case Item.Type.Food :
                componentType = ComponentType.FoodRack;
                foodRacks.Add(itemRack);
                break;
            case Item.Type.Weapon :
                componentType = ComponentType.WeaponsRack;
                weaponsRacks.Add(itemRack);
                break;
            case Item.Type.Clothing :
                componentType = ComponentType.ClothingRack;
                clothingRacks.Add(itemRack);
                break;
            default :
                componentType = ComponentType.FoodRack;
                Debug.Log("Placing item rack for unsupported item type: " + itemType);
                break;
        }
        Transform targetOrientation = GetCampComponentOrientation(componentType);
        itemRack.worldObject.transform.position = targetOrientation.position;
        itemRack.worldObject.transform.rotation = targetOrientation.rotation;
    }
    public void OnRackCapacityReached(Enum itemType, List<GameObject> objects){
        PlaceItemRack(itemType, objects);
    }

    public void PlaceAnvil(){
        // TODO
    }





}
