﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingTable : Activatable
{
    /// <summary>
    /// CraftingTable
    /// Jeremy Casada 
    ///
    /// Last Updated: 5/28/22
    /// 
    /// - Crafting Table interactions
    /// </summary>
    public GameObject Menu;
    public GameObject optionParent;
    public GameObject craftingOptionPrefab;
    
    public List<CraftingRecipe> recipes;

    public static CraftingTable Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("You Have Two Crafting Tables in the Scene, Please remove one of them");
            Destroy(Instance.gameObject);
        }
        Instance = this;
        Menu = UI.Instance.transform.Find("Crafting Menu").gameObject;
        optionParent = Menu.transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;
    }

    private void Start()
    {
        //Menu = UI.Instance.transform.Find("Crafting Menu").gameObject;
        //optionParent = Menu.transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;
        CraftingOption tempOption;
        foreach (CraftingRecipe recipe in recipes)
        {
            tempOption = Instantiate(craftingOptionPrefab, optionParent.transform).GetComponent<CraftingOption>();
            tempOption.recipe = recipe;

            

            if (recipe.craftingResult.resultType == ResultType.ammo)
            {
                switch (recipe.craftingResult.ammoType)
                {
                    case AmmoType.LightAmmo:
                        tempOption.itemName.text = "Light Ammo";
                        break;
                    case AmmoType.HeavyAmmo:
                        tempOption.itemName.text = "Heavy Ammo";
                        break;
                    default:
                        break;
                }
                tempOption.description.text = "";
                InventoryUI.Instance.RefreshUI();
                tempOption.SetDropDown();
                if (recipe.craftingResult.displaySprite != null)
                {
                    tempOption.itemImage.sprite = recipe.craftingResult.displaySprite;
                }
            }
            else if(recipe.craftingResult.resultType == ResultType.attachment)
            {
                switch (recipe.craftingResult.attachType)
                {
                    case AttachType.laser:
                        tempOption.itemName.text = "Laser Sight";
                        break;
                    case AttachType.tunedBarrel:
                        break;
                    default:
                        break;
                }
                tempOption.description.text = "";
                InventoryUI.Instance.RefreshUI();
                
                if (recipe.craftingResult.displaySprite != null)
                {
                    tempOption.itemImage.sprite = recipe.craftingResult.displaySprite;
                }
            }
            else if(recipe.craftingResult.resultType == ResultType.armor)
            {
                tempOption.itemName.text = "Armor Plate";
                tempOption.description.text = recipe.craftingResult.nonItemDescription;
                InventoryUI.Instance.RefreshUI();
                
                if (recipe.craftingResult.displaySprite != null)
                {
                    tempOption.itemImage.sprite = recipe.craftingResult.displaySprite;
                }
            }
            else if(recipe.craftingResult.resultType == ResultType.fireBullets)
            {
                if(recipe.craftingResult.ammoType == AmmoType.HeavyAmmo)
                {
                    tempOption.itemName.text = "Heavy Fire Bullets";
                }
                else
                {
                    tempOption.itemName.text = "Light Fire Bullets";
                }
                
                tempOption.description.text = recipe.craftingResult.nonItemDescription;
                InventoryUI.Instance.RefreshUI();
                
                if (recipe.craftingResult.displaySprite != null)
                {
                    tempOption.itemImage.sprite = recipe.craftingResult.displaySprite;
                }
            }
            else
            {
                //Debug.Log(recipe.name);
                tempOption.itemName.text = recipe.craftingResult.itemResult.itemName;
                tempOption.description.text = recipe.craftingResult.itemResult.itemDescription;
                if (recipe.craftingResult.itemResult.itemSprite != null)
                {
                    tempOption.itemImage.sprite = recipe.craftingResult.itemResult.itemSprite;
                }
            }

            
            foreach (ResourceRequirement requirement in recipe.Requirements)
            {
                switch (requirement.Resource)
                {
                    case Resource.ResourceType.scrap:
                        tempOption.scrapText.text = requirement.Amount.ToString();
                        break;
                    case Resource.ResourceType.meds:
                        tempOption.medsText.text = requirement.Amount.ToString();
                        break;
                    case Resource.ResourceType.cloth:
                        tempOption.clothText.text = requirement.Amount.ToString();
                        break;
                    default:
                        break;
                }
            }

            
            tempOption.craftButton.onClick.AddListener(() => recipe.Craft(Player.Instance));
            tempOption.craftButton.onClick.AddListener(() => tempOption.SetDropDown());
            if (!recipe.IsCraftable(Player.Instance))
            {
                tempOption.craftButton.interactable = false;
            }


        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !UI.Instance.PausedStatus && !Upgrades.Instance.upgradeMenu.activeInHierarchy)
        {
            DeactivateMenu();
        }
    }

    public override void Activate()
    {
        if (Menu.activeInHierarchy)
        {
            DeactivateMenu();
        }
        else
        {
            ActivateMenu();
        }
    }

    public void ActivateMenu()
    {
        Menu.SetActive(true);
        foreach (Transform option in optionParent.transform)
        {
            CraftingOption craftOption = option.GetComponent<CraftingOption>();

            if (craftOption.recipe.craftingResult.resultType == ResultType.ammo)
            {
                craftOption.SetDropDown();
            }

        }
        UpdateCraftingTable();
    }

    public void DeactivateMenu()
    {
        Menu.SetActive(false);
    }

    public void UpdateCraftingTable()
    {

        foreach (Transform option in optionParent.transform)
        {
            CraftingOption craftOption = option.GetComponent<CraftingOption>();
            craftOption.craftButton.interactable = craftOption.recipe.IsCraftable(Player.Instance);

            if(craftOption.recipe.craftingResult.resultType == ResultType.ammo)
            {
                foreach (ResourceRequirement requirement in craftOption.recipe.Requirements)
                {
                    switch (requirement.Resource)
                    {
                        case Resource.ResourceType.scrap:
                            craftOption.scrapText.text = (requirement.Amount * craftOption.recipe.amountToCraft).ToString();
                            break;
                        case Resource.ResourceType.meds:
                            craftOption.medsText.text = (requirement.Amount * craftOption.recipe.amountToCraft).ToString();
                            break;
                        case Resource.ResourceType.cloth:
                            craftOption.clothText.text = (requirement.Amount * craftOption.recipe.amountToCraft).ToString();
                            break;
                        default:
                            break;
                    }
                }
            }
            else if(craftOption.recipe.craftingResult.resultType == ResultType.attachment)
            {
                
                if(Player.Instance.inventory.selectedItem == null || Player.Instance.inventory.selectedItem.itemData.itemType != ItemType.RangedWeapon)
                {
                   
                    craftOption.craftButton.GetComponentInChildren<Text>().text = "Select A Ranged Weapon to Craft an Attachment";
                }
                else
                {
                    craftOption.craftButton.GetComponentInChildren<Text>().text = "";
                    switch (craftOption.recipe.craftingResult.attachType)
                    {
                        case AttachType.laser:
                            if(Player.Instance.inventory.selectedItem != null && Player.Instance.inventory.selectedItem.itemData.hasLaserSight)
                            {
                                craftOption.craftButton.GetComponentInChildren<Text>().text = "Can't Craft Because Gun Already has Laser";
                            }
                            break;
                        case AttachType.tunedBarrel:
                            break;
                        default:
                            break;
                    }
                    
                }

            
            }
            else if (craftOption.recipe.craftingResult.resultType == ResultType.armor)
            {
                if(Player.Instance.hasArmorPlate)
                {
                    craftOption.craftButton.GetComponentInChildren<Text>().text = "Can't Craft Because You Already have an Armor Plate";
                }
                else
                {
                    craftOption.craftButton.GetComponentInChildren<Text>().text = "";
                }
            }
            else if(craftOption.recipe.craftingResult.resultType == ResultType.fireBullets)
            {
                if (Player.Instance.inventory.selectedItem == null || Player.Instance.inventory.selectedItem.itemData.itemType != ItemType.RangedWeapon)
                {

                    craftOption.craftButton.GetComponentInChildren<Text>().text = "Select A Ranged Weapon to Craft Fire Bullets";
                }
                else
                {
                    craftOption.craftButton.GetComponentInChildren<Text>().text = "";
                    
                    if(Player.Instance.inventory.selectedItem.itemData.usingFireBullets)
                    {
                        craftOption.craftButton.GetComponentInChildren<Text>().text = "Can't Craft, Weapon Already has Fire Bullets";
                    }
                    if(Player.Instance.inventory.selectedItem.itemData.ammoType != craftOption.recipe.craftingResult.ammoType)
                    {
                        craftOption.craftButton.GetComponentInChildren<Text>().text = "Can't Craft, Incompatible Ammo Size";
                    }

                }
            }
            else if(craftOption.recipe.craftingResult.resultType == ResultType.item)
            {
                craftOption.craftButton.GetComponentInChildren<Text>().text = "";
                if (Player.Instance.inventory.IsFull() && craftOption.recipe.craftingResult.itemResult.itemType != ItemType.BackPack)
                {
                    craftOption.craftButton.GetComponentInChildren<Text>().text = "Can't Craft, Inventory is Full";
                }
                else if(craftOption.recipe.craftingResult.itemResult.itemType == ItemType.BackPack && Player.Instance.hasBackPack)
                {
                    craftOption.craftButton.GetComponentInChildren<Text>().text = "Can't Craft, Already have Back Pack";
                }
            }

        }
    }



    


}
