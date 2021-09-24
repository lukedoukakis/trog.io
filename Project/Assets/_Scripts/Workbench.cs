using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workbench : ObjectRack
{


    public void SetWorkbench(Camp camp){
        base.SetObjectRack(camp, Item.Type.Any);
    }


    // workbench override for add objects
    public override void AddObjects(Item item, ref int countToAdd){
        
        // do the regular thing
        base.AddObjects(item, ref countToAdd);

        // todo: workbench magic: look at recipes and highlight other items that can be added


    }

    public Item CraftItem(Item item){
        // todo: craft item

        // consume items on table and generate new one



        return Item.None;
    }


}
