using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CraftingOption : MonoBehaviour
{
    /// <summary>
    /// CraftingOption
    /// Jeremy Casada
    /// 
    /// Last Updated:
    /// 
    ///  - handles drop down results while crafting
    /// 
    /// </summary>
   // public int requiredScrap;
   // public int requiredMeds;
   // public int requiredCloth;

    public Text itemName, scrapText, medsText, clothText, description;
    public Image itemImage;
    public Button craftButton;
    public CraftingRecipe recipe;
    public Dropdown amountDropDown;

    public void AmountChanged(int amount)
    {

        recipe.amountToCraft = amount;
        CraftingTable.Instance.UpdateCraftingTable();
    }

    public void SetDropDown()
    {
        if(recipe.craftingResult.resultType == ResultType.ammo)
        {
            amountDropDown.gameObject.SetActive(true);
            amountDropDown.ClearOptions();
            List<string> optionNums = new List<string>();
            switch (recipe.craftingResult.ammoType)
            {
                case AmmoType.LightAmmo:
                    for (int index = 0; index < Player.Instance.maxLightAmmo - Player.Instance.currentLightAmmo + 1 ; index++)
                    {
                        optionNums.Add(index.ToString());
                    }
                    break;
                case AmmoType.HeavyAmmo:
                    for (int index = 0; index < Player.Instance.maxHeavyAmmo - Player.Instance.currentHeavyAmmo + 1; index++)
                    {
                        optionNums.Add(index.ToString());
                    }
                    break;
                default:
                    break;
            }
            amountDropDown.AddOptions(optionNums);
            amountDropDown.value = 1;
            AmountChanged(amountDropDown.value);
            
        }
        
    }
}
