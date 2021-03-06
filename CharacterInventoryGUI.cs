﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class CharacterInventoryGUI : Photon.MonoBehaviour, IPointerDownHandler
{
    TerrainScript terrain;
    WeaponsDatabase EquipWeapon;
    ArmorDatabase EquipArmor;
    WeaponSwitch SwitchWeapons;
    ArmorSwitch SwitchArmor;
    CharacterStats Stats;
    MiscellaneousItemsDatabase MiscItems;
    ToolDatabase ToolItems;
    PotionDatabase Potions;
    CharacterMovement movement;
    CharacterSkillBarGUI skillbarGUI;

    MineRocks RockOre;
    ChopTrees TreeLog;
    Herbloring Herbs;

    PickupObjects ItemsPickup;

    public int InventoryCount = 0;

    int InventorySelected;
    bool ShowItemInfo;

    bool[] ViewStatWindow;
    bool EquipWeaponorArmor;

    public int CurrentInventoryItemSlot; // for equiping stuff

    bool ShowInv;

    public Button[] InventoryButtonRects;
    public List<Image> InventoryButtonsIcons;
    public List<InventoryManager> InventoryManage;
    public List<Text> InventoryButtonsStackText;
    public GameObject ParentButton;

    public Image DropAmountImage;
    public List<InventoryManager> DroppedItemList; 
    public Sprite DefaultSprite;

    public void OnPointerDown(PointerEventData data)
    {
        terrain.canvas.GetComponent<MainGUI>().inventory.transform.SetAsLastSibling();
    }

    public void FindNextSlot()
    {
        bool ActiveInventory = terrain.canvas.GetComponent<MainGUI>().inventory.GetActive();
        if (ActiveInventory == false)
            terrain.canvas.GetComponent<MainGUI>().inventory.SetActive(true);
        for (int i = 0; i < 25; i++)
        {
            if (InventoryButtonsIcons[i].GetComponentInChildren<Mask>().showMaskGraphic == false)
            {
                InventoryCount = i;
                if (ActiveInventory == false)
                    terrain.canvas.GetComponent<MainGUI>().inventory.SetActive(false);
                return;
            }
        }
    }

    public void CombineStacks()
    {
        for (int i = 0; i < InventoryManage.Count; i++)
        {
            if (InventoryManage[i].isASecondary == 1)
            {
                for (int j = 0; j < InventoryManage.Count; j++)
                {
                    if (InventoryManage[i].SlotName == InventoryManage[j].SlotName && InventoryManage[i].CurrentInventorySlot < InventoryManage[j].CurrentInventorySlot && InventoryManage[i].StackAmounts + InventoryManage[j].StackAmounts < 50)
                    {
                        InventoryManage[i].StackAmounts += InventoryManage[j].StackAmounts;
                        InventoryButtonsStackText[InventoryManage[i].CurrentInventorySlot].GetComponentInChildren<Text>().text = InventoryManage[i].StackAmounts.ToString();

                        InventoryButtonsIcons[InventoryManage[j].CurrentInventorySlot].GetComponent<Mask>().showMaskGraphic = false;
                        InventoryButtonsIcons[InventoryManage[j].CurrentInventorySlot].sprite = DefaultSprite;
                        InventoryButtonsStackText[InventoryManage[j].CurrentInventorySlot].text = string.Empty;
                        InventoryManage.RemoveAt(j);
                    }
                }
            }
        }
    }

    public void RecalculateStackAmounts(int InvSlot, int Quantity) // typeofstacks = misc or potions, maybe weapons
    {
        if (terrain.canvas.GetComponent<MainGUI>().inventory != null)
        {
            if (InventoryManage[InvSlot].StackAmounts >= Quantity)
            {
                for (int i = 0; i < MiscItems.Miscellaneousitems.Count; i++)
                {
                    if (MiscItems.Miscellaneousitems[i].MiscellaneousItemName == InventoryManage[InvSlot].SlotName)
                    {
                        MiscItems.TotalMiscellaneousStacks[i] -= Quantity;
                        skillbarGUI.CheckAllUpdateStacksFromCraftingWindow(InventoryManage[InvSlot].SlotName, MiscItems.TotalMiscellaneousStacks[i]);
                    }
                }
                for (int i = 0; i < Potions.PotionList.Count; i++)
                {
                    if (Potions.PotionList[i].PotionName == InventoryManage[InvSlot].SlotName)
                        Potions.TotalPotionStacks[i] -= Quantity;
                }

                InventoryManage[InvSlot].StackAmounts = (int.Parse(InventoryButtonsStackText[InventoryManage[InvSlot].CurrentInventorySlot].GetComponentInChildren<Text>().text) - Quantity);
                InventoryButtonsStackText[InventoryManage[InvSlot].CurrentInventorySlot].GetComponentInChildren<Text>().text = InventoryManage[InvSlot].StackAmounts.ToString();   
            }           
        }        
        if (InventoryManage[InvSlot].StackAmounts <= 0)
            RemoveStackAmounts();
    }

    public void RemoveStackAmounts()
    {
        for (int i = 0; i < InventoryManage.Count; i++)
        {
            if (InventoryManage[i].isASecondary == 1)
            {
                if (InventoryManage[i].StackAmounts <= 0)
                {
                    if (InventoryManage[i].CurrentInventorySlot == SwitchWeapons.CurrentLoadedProjectile)
                    {
                        SwitchWeapons.CurrentLoadedProjectile = -1;
                        SwitchWeapons.DestroyObject(SwitchWeapons.CurrentProjectileObject, 1);
                    }

                    InventoryButtonsIcons[InventoryManage[i].CurrentInventorySlot].GetComponent<Mask>().showMaskGraphic = false;
                    InventoryButtonsIcons[InventoryManage[i].CurrentInventorySlot].sprite = DefaultSprite;
                    InventoryButtonsStackText[InventoryManage[i].CurrentInventorySlot].text = string.Empty;
                    InventoryManage.RemoveAt(i);    
                }
            }
        }
    }

    public void AddtoInventoryDropped(InventoryManager InvManage)
    {
        for (int i = 0; i < InventoryManage.Count; i++)
        {
            if (InvManage.isASecondary == 1 &&
                InventoryManage[i].SlotName == InvManage.SlotName &&
                InventoryManage[i].Rarity == InvManage.Rarity && 
                InventoryManage[i].StackAmounts + InvManage.StackAmounts < 51)
            {
                InventoryManage[i].StackAmounts += InvManage.StackAmounts;
                RecalculateStackAmounts(i, -InvManage.StackAmounts);

                //InventoryButtonsStackText[InventoryManage[i].CurrentInventorySlot].text = InventoryManage[i].StackAmounts.ToString();
                InventoryButtonsIcons[InventoryManage[i].CurrentInventorySlot].sprite = InvManage.tSprite;
                InventoryButtonsIcons[InventoryManage[i].CurrentInventorySlot].GetComponent<Mask>().showMaskGraphic = true;
                return;
            }
            else if (i >= InventoryManage.Count - 1)
            {
                FindNextSlot();
                InventoryManager NewStack = new InventoryManager(InvManage.SlotName, InvManage.Rarity, InvManage.tSprite, null, InvManage.DamageOrValue, InvManage.WeaponAttackSpeed, InvManage.CritRate, InvManage.ArmorPenetration, InvManage.Defense, InvManage.Health, InvManage.Stamina, InventoryCount, InvManage.StackAmounts, InvManage.isASecondary);
                AddToInventory(NewStack);
                return;
            }          
        }
    }

    public void AddToInventory(InventoryManager InvManage) 
    {
        FindNextSlot();

        InventoryManage.Add(InvManage);
        InventoryManage[InventoryManage.Count - 1].CurrentInventorySlot = InventoryCount;

        if (InvManage.isASecondary == 1) 
        {
            //if (InventoryManage[InventoryManage.Count - 1].CurrentInventorySlot != InvManage.CurrentInventorySlot)
            InventoryButtonsStackText[InventoryCount].text = "0"; // needed for recalculate to work at start
            RecalculateStackAmounts(InventoryManage.Count - 1, -InvManage.StackAmounts);
        }

        InventoryButtonsIcons[InventoryCount].sprite = InvManage.tSprite;
        InventoryButtonsIcons[InventoryCount].GetComponent<Mask>().showMaskGraphic = true;
                
        return;
                               
    }

    public void SetArmorValues() // armor switch
    {
        for (int i = 0; i < EquipArmor.ArmorList.Count; i++)
        {
            if (InventoryButtonsIcons[CurrentInventoryItemSlot].sprite.name == EquipArmor.ArmorList[i].ArmorName &&
                Stats.PlayerLevel >= EquipArmor.ArmorList[i].LevelRank)
            {
                SwitchArmor.StartCoroutine(SwitchArmor.InstantiateArmors(CurrentInventoryItemSlot));
            }
        }
    }

    public void SetWeaponValues()
    {
        for (int i = 0; i < EquipWeapon.WeaponList.Count; i++)
        {
            if (InventoryButtonsIcons[CurrentInventoryItemSlot].sprite.name == EquipWeapon.WeaponList[i].WeaponName && 
                Stats.PlayerLevel >= EquipWeapon.WeaponList[i].LevelRank)
            {
                if (EquipWeapon.WeaponList[i].IsASecondaryWeapon == 0 && movement.Attacking == false)
                {
                    InventoryButtonRects[CurrentInventoryItemSlot].transform.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.66f);
                    if (SwitchWeapons.CurrentWeaponItemSlot != -1)
                        InventoryButtonRects[SwitchWeapons.CurrentWeaponItemSlot].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                }
                SwitchWeapons.StartCoroutine(SwitchWeapons.WepSwitch(CurrentInventoryItemSlot));
            }
        }
    }

    public void SetToolValues()
    {
        for (int i = 0; i < ToolItems.ToolList.Count; i++)
        {
            if (InventoryButtonsIcons[CurrentInventoryItemSlot].sprite.name == ToolItems.ToolList[i].ToolName)
            {
                if (movement.Attacking == false && ToolItems.ToolList[i].ToolId != 1 && ToolItems.ToolList[i].ToolId != 2)
                {
                    InventoryButtonRects[CurrentInventoryItemSlot].transform.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.66f);
                    if (SwitchWeapons.CurrentWeaponItemSlot != -1)
                        InventoryButtonRects[SwitchWeapons.CurrentWeaponItemSlot].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                }
                SwitchWeapons.StartCoroutine(SwitchWeapons.WepSwitch(CurrentInventoryItemSlot));
            }
        }
    }

    [PunRPC]
    public IEnumerator AddandRemoveDropList(string name, string rare, int view, float val, float attackspeed, float crit, float arpen, float def, float health, float stam, int slot, int stack, int secondary, int AddorRemove, int Dropindex) 
    {
        Sprite sp = Resources.Load<Sprite>(name);
        GameObject obj = PhotonView.Find(view).gameObject;
        InventoryManager items = new InventoryManager(name, rare, sp, obj, val, attackspeed, crit, arpen, def, health, stam,  slot, stack, secondary);

        if (AddorRemove == 1)
        {
            DroppedItemList.Add(items);

            yield return new WaitForSeconds(120);
            if (obj != null)
            {
                for (int i = 0; i < DroppedItemList.Count; i++)
                {
                    if (DroppedItemList[i].tObject == obj)
                    {
                        Destroy(DroppedItemList[i].tObject);
                        DroppedItemList.RemoveAt(i);
                    }
                }
            }
        }
        else
        {
            Destroy(DroppedItemList[Dropindex].tObject);
            DroppedItemList.RemoveAt(Dropindex);
        }
    }

    public void DropStackAmounts(Image StackAmountImageREF, GameObject PreviousButtonLocation) // i == inventoryIndex
    {
        if (StackAmountImageREF.GetComponentInChildren<InputField>().GetComponentInChildren<Text>().text != string.Empty)
        {
            if (int.Parse(StackAmountImageREF.GetComponentInChildren<InputField>().GetComponentInChildren<Text>().text) <= int.Parse(PreviousButtonLocation.GetComponentInChildren<Text>().text))
            {
                for (int i = 0; i < InventoryManage.Count; i++) // misc and pots
                {
                    if (InventoryManage[i].CurrentInventorySlot == int.Parse(PreviousButtonLocation.name))
                    {
                        GameObject Items = PhotonNetwork.Instantiate(StackAmountImageREF.transform.Find("Image").Find("ImageSprite").GetComponent<Image>().sprite.name, terrain.Player.transform.position + new Vector3(0, 2, 0) + terrain.Player.transform.forward, Quaternion.Euler(0, 0, 0), 0)
                                as GameObject;
                        InventoryManager ItemsDrop = new InventoryManager(InventoryManage[i].SlotName, InventoryManage[i].Rarity, InventoryManage[i].tSprite, Items, InventoryManage[i].DamageOrValue, InventoryManage[i].WeaponAttackSpeed, InventoryManage[i].CritRate, InventoryManage[i].ArmorPenetration, InventoryManage[i].Defense, InventoryManage[i].Health, InventoryManage[i].Stamina, InventoryManage[i].CurrentInventorySlot, int.Parse(StackAmountImageREF.GetComponentInChildren<InputField>().GetComponentInChildren<Text>().text), 1);
                        terrain.canvas.GetPhotonView().RPC("AddandRemoveDropList", PhotonTargets.AllBufferedViaServer, InventoryManage[i].SlotName, InventoryManage[i].Rarity, Items.GetPhotonView().viewID, InventoryManage[i].DamageOrValue, InventoryManage[i].WeaponAttackSpeed, InventoryManage[i].CritRate, InventoryManage[i].ArmorPenetration, InventoryManage[i].Defense, InventoryManage[i].Health, InventoryManage[i].Stamina, InventoryManage[i].CurrentInventorySlot, int.Parse(StackAmountImageREF.GetComponentInChildren<InputField>().GetComponentInChildren<Text>().text), 1, 1, DroppedItemList.Count);

                        //if (StackAmountImageREF.GetComponentInChildren<Text>().text == EquipWeapon.WeaponList[i].WeaponName
                        //    && EquipWeapon.WeaponList[i].IsASecondaryWeapon == 1 && PreviousButtonLocation.GetComponentInChildren<Text>().text == "0") // weapon that is stackable and wearable and stacks = 0
                        //{
                        //    SwitchWeapons.DropObject(InventoryManage[i].CurrentInventorySlot);
                        //}

                        RecalculateStackAmounts(i, int.Parse(StackAmountImageREF.GetComponentInChildren<InputField>().GetComponentInChildren<Text>().text));

                        Destroy(StackAmountImageREF.gameObject);
                    }
                }
            }
        }
    }

    public void dropitem(GameObject PreviousButtonLocation)
    {
        for (int i = 0; i < InventoryManage.Count; i++) // misc and pots
        {
            if (InventoryManage[i].CurrentInventorySlot == int.Parse(PreviousButtonLocation.name) &&
                InventoryManage[i].isASecondary == 1)
            {
                Image DropAmountImageRef = Instantiate(DropAmountImage, new Vector2(0,0), transform.rotation) as Image;
                DropAmountImageRef.transform.SetParent(terrain.canvas.transform);
                DropAmountImageRef.transform.localScale = new Vector3(1, 1, 1);
                DropAmountImageRef.GetComponent<RectTransform>().localPosition = new Vector2(0,0);
                DropAmountImageRef.transform.Find("Image").Find("ImageSprite").GetComponent<Image>().sprite = InventoryManage[i].tSprite;
                DropAmountImageRef.GetComponentInChildren<Button>().onClick.AddListener(() => DropStackAmounts(DropAmountImageRef, PreviousButtonLocation));
            }

            else if (InventoryManage[i].CurrentInventorySlot == int.Parse(PreviousButtonLocation.name)
                && InventoryManage[i].isASecondary == 0) // for weapons/armors that are primary 
            {
                if (InventoryManage[i].CurrentInventorySlot == SwitchWeapons.CurrentWeaponItemSlot)
                {
                    InventoryButtonRects[SwitchWeapons.CurrentWeaponItemSlot].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                    SwitchWeapons.DropObject(InventoryManage[i].CurrentInventorySlot);
                    SwitchWeapons.DestroyObject(SwitchWeapons.CurrentProjectileObject, 1);
                }
                else if (InventoryManage[i].CurrentInventorySlot == SwitchArmor.CurrentChestplateIteration ||
                    InventoryManage[i].CurrentInventorySlot == SwitchArmor.CurrentHelmetIteration || InventoryManage[i].CurrentInventorySlot == SwitchArmor.CurrentLegsIteration) // armor 
                {
                    if (InventoryManage[i].CurrentInventorySlot == SwitchArmor.CurrentChestplateIteration)
                    InventoryButtonRects[SwitchArmor.CurrentChestplateIteration].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                    else if (InventoryManage[i].CurrentInventorySlot == SwitchArmor.CurrentHelmetIteration)
                        InventoryButtonRects[SwitchArmor.CurrentHelmetIteration].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                    else if (InventoryManage[i].CurrentInventorySlot == SwitchArmor.CurrentLegsIteration)
                        InventoryButtonRects[SwitchArmor.CurrentLegsIteration].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);

                    SwitchArmor.DropObject(InventoryManage[i].CurrentInventorySlot);
                }

                GameObject Item = PhotonNetwork.Instantiate(InventoryManage[i].SlotName, terrain.Player.transform.position + new Vector3(0, 2, 0) + terrain.Player.transform.forward, terrain.Player.transform.rotation, 0)
                    as GameObject;
                terrain.canvas.GetPhotonView().RPC("AddandRemoveDropList", PhotonTargets.AllBufferedViaServer, InventoryManage[i].SlotName, InventoryManage[i].Rarity, Item.GetPhotonView().viewID, InventoryManage[i].DamageOrValue, InventoryManage[i].WeaponAttackSpeed, InventoryManage[i].CritRate, InventoryManage[i].ArmorPenetration, InventoryManage[i].Defense, InventoryManage[i].Health, InventoryManage[i].Stamina, -1, 1, 0, 1, DroppedItemList.Count);

                if (Item.GetComponentInChildren<SkinnedMeshRenderer>())
                    Item.GetComponentInChildren<SkinnedMeshRenderer>().material = new Material(Item.GetComponentInChildren<SkinnedMeshRenderer>().material); // create new instance for material seperate from rocks/trees
                else
                    Item.GetComponentInChildren<MeshRenderer>().material = new Material(Item.GetComponentInChildren<MeshRenderer>().material); // create new instance for material seperate from rocks/trees

                Color color = Color.white;

                if (InventoryManage[i].Rarity == "Rare")
                    color = Color.blue;
                if (InventoryManage[i].Rarity == "Epic")
                    color = Color.magenta;
                if (InventoryManage[i].Rarity == "Unique")
                    color = Color.red;
                if (InventoryManage[i].Rarity == "Legendary")
                    color = Color.green;
                if (InventoryManage[i].Rarity == "Mythic")
                    color = Color.cyan;

                SwitchArmor.gameObject.GetPhotonView().RPC("MultiplayerGlow", PhotonTargets.AllBufferedViaServer, Item.GetPhotonView().viewID, color.r, color.g, color.b);

                InventoryManage.RemoveAt(i); // remove from inventorymanage and add to droplist
                PreviousButtonLocation.transform.Find("ImageScript").GetComponent<Image>().sprite = DefaultSprite;
                PreviousButtonLocation.transform.GetComponentInChildren<Mask>().showMaskGraphic = false;
            }
        }
    }

    void TestingPurposes()
    {
        //if (terrain.IsThisALoadedCharacterGame == false)
        //{
            InventoryManager pick = new InventoryManager(ToolItems.ToolList[0].ToolName, "Common", ToolItems.ToolSprites[0], null,
     0, 0, 0, 0, 0, 0, 0, 0, -1, 0);

            InventoryManager axe = new InventoryManager(ToolItems.ToolList[1].ToolName, "Common", ToolItems.ToolSprites[1], null,
                 0, 0, 0, 0, 0, 0, 0, 1, -1, 0);
            InventoryManager staff = new InventoryManager(EquipWeapon.WeaponList[0].WeaponName, "Common", EquipWeapon.WeaponSprites[0], null,
                EquipWeapon.WeaponList[0].WeaponDamage, EquipWeapon.WeaponList[0].WeaponAttackSpeed,
                EquipWeapon.WeaponList[0].CritRate, EquipWeapon.WeaponList[0].ArmorPenetration, 0, 0, 0, 5, -1, 0);
            InventoryManager misc4 = new InventoryManager(MiscItems.Miscellaneousitems[1].MiscellaneousItemName, "Common", MiscItems.MiscellaneousSprites[1], null,
                0, 0, 0, 0, 0, 0, 0, 6, 50, 1);
            InventoryManager misc1 = new InventoryManager(MiscItems.Miscellaneousitems[19].MiscellaneousItemName, "Common", MiscItems.MiscellaneousSprites[19], null,
                0, 0, 0, 0, 0, 0, 0, 6, 50, 1);
            InventoryManager misc2 = new InventoryManager(MiscItems.Miscellaneousitems[11].MiscellaneousItemName, "Common", MiscItems.MiscellaneousSprites[11], null,
                0, 0, 0, 0, 0, 0, 0, 7, 50, 1);
            InventoryManager misc3 = new InventoryManager(MiscItems.Miscellaneousitems[28].MiscellaneousItemName, "Common", MiscItems.MiscellaneousSprites[28], null,
                0, 0, 0, 0, 0, 0, 0, 8, 50, 1);
            InventoryManager misc5 = new InventoryManager(MiscItems.Miscellaneousitems[20].MiscellaneousItemName, "Common", MiscItems.MiscellaneousSprites[20], null,
                0, 0, 0, 0, 0, 0, 0, 6, 50, 1);
            InventoryManager misc6 = new InventoryManager(MiscItems.Miscellaneousitems[35].MiscellaneousItemName, "Common", MiscItems.MiscellaneousSprites[35], null,
                0, 0, 0, 0, 0, 0, 0, 6, 50, 1);
            InventoryManager misc7 = new InventoryManager(MiscItems.Miscellaneousitems[0].MiscellaneousItemName, "Common", MiscItems.MiscellaneousSprites[0], null,
                0, 0, 0, 0, 0, 0, 0, 6, 50, 1);
            InventoryManager pot = new InventoryManager(Potions.PotionList[0].PotionName, "Common", Potions.PotionSprites[0], null,
        Potions.PotionList[0].PotionValue, 0, 0, 0, 0, 0, 0, 1, 200, 1);
            InventoryManager helm2 = new InventoryManager(EquipArmor.ArmorList[0].ArmorName, "Common", EquipArmor.ArmorSprites[0], null,
    0, 0, 0, 0, EquipArmor.ArmorList[0].DefenseValues, EquipArmor.ArmorList[0].BonusHealth, EquipArmor.ArmorList[0].BonusStamina, 1, -1, 0);
            InventoryManager robe = new InventoryManager(EquipArmor.ArmorList[9].ArmorName, "Common", EquipArmor.ArmorSprites[9], null,
0, 0, 0, 0, EquipArmor.ArmorList[9].DefenseValues, EquipArmor.ArmorList[9].BonusHealth, EquipArmor.ArmorList[9].BonusStamina, 1, -1, 0);

            AddToInventory(pick);
            AddToInventory(axe);
            AddToInventory(staff);
            AddToInventory(misc1);
            AddToInventory(misc2);
            AddToInventory(misc3);
            AddToInventory(misc4);
            AddToInventory(misc5);
            AddToInventory(misc6);
            AddToInventory(misc7);
            AddToInventory(robe);
            AddToInventory(helm2);
            AddToInventory(pot);
            InventoryManager craftingbox = new InventoryManager(ToolItems.ToolList[18].ToolName, "Common", ToolItems.ToolSprites[18], null,
0, 0, 0, 0, 0, 0, 0, 0, -1, 0);
            AddToInventory(craftingbox);

            InventoryManager misc15 = new InventoryManager(MiscItems.Miscellaneousitems[37].MiscellaneousItemName, "Rare", MiscItems.MiscellaneousSprites[37], null,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        AddToInventory(misc15);
        InventoryManager misc18 = new InventoryManager(MiscItems.Miscellaneousitems[37].MiscellaneousItemName, "Rare", MiscItems.MiscellaneousSprites[37], null,
0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        AddToInventory(misc18);
            InventoryManager misc16 = new InventoryManager(MiscItems.Miscellaneousitems[38].MiscellaneousItemName, "Epic", MiscItems.MiscellaneousSprites[38], null,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            AddToInventory(misc16);
            InventoryManager misc17 = new InventoryManager(MiscItems.Miscellaneousitems[39].MiscellaneousItemName, "Unique", MiscItems.MiscellaneousSprites[39], null,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            AddToInventory(misc17);
            InventoryManager staff1 = new InventoryManager(EquipWeapon.WeaponList[0].WeaponName, "Common", EquipWeapon.WeaponSprites[0], null,
        EquipWeapon.WeaponList[0].WeaponDamage, EquipWeapon.WeaponList[0].WeaponAttackSpeed,
        EquipWeapon.WeaponList[0].CritRate, EquipWeapon.WeaponList[0].ArmorPenetration, 0, 0, 0, 5, -1, 0);
            AddToInventory(staff1);
            InventoryManager staff2 = new InventoryManager(EquipWeapon.WeaponList[0].WeaponName, "Common", EquipWeapon.WeaponSprites[0], null,
    EquipWeapon.WeaponList[0].WeaponDamage, EquipWeapon.WeaponList[0].WeaponAttackSpeed,
    EquipWeapon.WeaponList[0].CritRate, EquipWeapon.WeaponList[0].ArmorPenetration, 0, 0, 0, 5, -1, 0);
            AddToInventory(staff2);
        //}
    }

    void Start()
    {
        terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
        InventoryManage = new List<InventoryManager>();
        Debug.Log("inv spawned");
        DroppedItemList = new List<InventoryManager>();

        EquipWeapon = terrain.Player.GetComponentInChildren<WeaponsDatabase>();
        EquipArmor = terrain.Player.GetComponentInChildren<ArmorDatabase>();
        SwitchWeapons = terrain.Player.GetComponentInChildren<WeaponSwitch>();
        SwitchArmor = terrain.Player.GetComponentInChildren<ArmorSwitch>();
        Stats = terrain.Player.GetComponentInChildren<CharacterStats>();
        MiscItems = terrain.Player.GetComponentInChildren<MiscellaneousItemsDatabase>();
        ToolItems = terrain.Player.GetComponentInChildren<ToolDatabase>();
        RockOre = terrain.Player.GetComponentInChildren<MineRocks>();
        TreeLog = terrain.Player.GetComponentInChildren<ChopTrees>();
        Herbs = terrain.Player.GetComponentInChildren<Herbloring>();
        ItemsPickup = terrain.Player.GetComponentInChildren<PickupObjects>();
        Potions = terrain.Player.GetComponentInChildren<PotionDatabase>();
        movement = terrain.Player.GetComponentInChildren<CharacterMovement>();
        skillbarGUI = terrain.canvas.GetComponent<MainGUI>().characterSkillsBarGUI;

        Invoke("TestingPurposes", 4);
        InvokeRepeating("CombineStacks", 4, 3);
    }

    void Update()
    {
        //if (InventoryButtonRects.Length >= 24 &&
        //    terrain.canvas.GetComponent<MainGUI>().inventory.GetActive() == true) // only update inv when it is open
        //{
        //    RemoveStackAmounts(); // put in recalculatestackamounts
        //}
    }

}

public class InventoryManager
{
    public string SlotName;
    public string Rarity;
    public Sprite tSprite;
    public GameObject tObject;
    public float DamageOrValue;
    public float WeaponAttackSpeed;
    public float CritRate;
    public float ArmorPenetration;
    public float Defense;
    public float Health;
    public float Stamina;
    public int CurrentInventorySlot;
    public int StackAmounts;
    public int isASecondary; //stackable
    public string Description; // still need to do

    public InventoryManager(string name, string rare, Sprite sp, GameObject obj, float val, float attackspeed, float crit, float armorPen, float def, float health, float stam, int slot, int stack, int secondary)
    {
        SlotName = name;
        Rarity = rare;
        tSprite = sp;
        tObject = obj;
        DamageOrValue = val;
        WeaponAttackSpeed = attackspeed;
        CritRate = crit;
        ArmorPenetration = armorPen;
        Defense = def;
        Health = health;
        Stamina = stam;
        CurrentInventorySlot = slot; 
        StackAmounts = stack;
        isASecondary = secondary;
    }
}