using NHSE.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum ItemFilter
{
    Items,
    MsgBottle,
    Recipes,
    Fossils
}

public class UI_SearchWindow : MonoBehaviour
{
    public readonly int MAXITEMS = 50;
    public readonly int MESSAGEBOTTLEITEM = 5793;
    public readonly int RECIPEITEM = 5794;
    public readonly int FOSSILITEM = 2579;

    //editor vars
    public RectTransform SelectionOverlay;
    public Text ItemSelectedName;
    public Toggle KeepItemLoadedToggle;
    public InputField SearchField;
    public Dropdown DropdownFilter;
    public UI_SearchItem SearchItemPrefab;
    public Text MappableText;
    public UI_SetControl SetController;
    public UI_WrappingControl WrapController;
    public UI_FlowerEditor FlowerController;

    public GameObject FlowerButtonRoot;

    public GameObject SearchBlocker;

    [HideInInspector]
    public int CurrentSelectedIndex = -1;
    [HideInInspector]
    public int CurrentItemID = -1;
    [HideInInspector]
    public ItemFilter CurrentFilter;
    [HideInInspector]
    public bool IsNoItemMode = true;

    private string currentSearchString = "";

    private List<ComboItem> cachedItemList;
    private List<ComboItem> cachedRecipeList;
    private List<ComboItem> cachedFossilList;

    private bool stopSearch = false;

    private List<UI_SearchItem> spawnedObjects;

    private Coroutine currentAnimationFuction;

    private void Start()
    {
        SearchField.onValueChanged.AddListener(delegate
        {
            UpdateSearchString(SearchField.text);
        });
        DropdownFilter.onValueChanged.AddListener(delegate
        {
            UpdateFilter(DropdownFilter);
        });
        KeepItemLoadedToggle.onValueChanged.AddListener(delegate
        {
            stopSearch = KeepItemLoadedToggle.isOn;
            SearchBlocker.gameObject.SetActive(stopSearch);
        });
        SearchItemPrefab.gameObject.SetActive(false);
        UpdateSearchString(SearchField.text);
        FlowerButtonRoot.SetActive(false);
    }

    private void Update()
    {
    }

    public void UpdateFilter(Dropdown filt)
    {
        CurrentFilter = (ItemFilter)filt.value;
        UpdateSearchString(SearchField.text);
    }

    public void UpdateSearchString(string val)
    {
        if (stopSearch)
            return;
        if (val != "")
        {
            currentSearchString = val.ToLower();
            List<ComboItem> list;
            if (UI_Settings.GetSearchMode() == StringSearchMode.Contains)
                list = allItems(CurrentFilter).FindAll((ComboItem x) => x.Text.ToLower().Contains(currentSearchString));
            else
                list = allItems(CurrentFilter).FindAll((ComboItem x) => x.Text.ToLower().StartsWith(currentSearchString));
            ComboItem fullMatch = list.Find((ComboItem x) => x.Text.ToLower() == currentSearchString);
            List<ComboItem> range = list.GetRange(0, Mathf.Min(MAXITEMS, list.Count));
            if (fullMatch.Text != null)
            {
                ComboItem item = range.Find((ComboItem x) => x.Text == fullMatch.Text);
                if (item.Text != null)
                {
                    range.Remove(item);
                }
                range.Insert(0, fullMatch);
            }
            initFor(range, list.Count > MAXITEMS, currentSearchString);
            IsNoItemMode = false;
        }
        else
        {
            initFor(new List<ComboItem>(), greaterThanMaxVal: false, currentSearchString);
            IsNoItemMode = true;
        }
    }

    private void initFor(List<ComboItem> vals, bool greaterThanMaxVal, string searchedString)
    {
        //IL_0166: Unknown result type (might be due to invalid IL or missing references)
        CurrentItemID = -1;
        CurrentSelectedIndex = -1;
        SetController.InitNumbers();
        UI_SearchItem uI_SearchItem = null;
        if (spawnedObjects != null && spawnedObjects.Count > 0)
        {
            foreach (UI_SearchItem spawnedObject in spawnedObjects)
            {
                Destroy(spawnedObject.gameObject);
            }
            spawnedObjects.Clear();
        }
        if (vals.Count < 1)
        {
            MappableText.text = "No items";
            ItemSelectedName.text = "No item selected";
            MappableText.gameObject.SetActive(true);
            SelectionOverlay.gameObject.SetActive(false);
        }
        else
        {
            SelectionOverlay.gameObject.SetActive(true);
            if (greaterThanMaxVal)
            {
                MappableText.text = "...more entries exist. Refine your search.";
                MappableText.gameObject.SetActive(true);
            }
            else
            {
                MappableText.gameObject.SetActive(false);
            }
            spawnedObjects = new List<UI_SearchItem>();
            for (int i = 0; i < vals.Count; i++)
            {
                GameObject obj = Instantiate(SearchItemPrefab.gameObject);
                obj.gameObject.SetActive(true);
                obj.transform.SetParent(SearchItemPrefab.transform.parent);
                obj.transform.localScale = SearchItemPrefab.transform.localScale;
                UI_SearchItem component = obj.GetComponent<UI_SearchItem>();
                component.InitialiseFor(vals[i].Text, searchedString, vals[i].Value, CurrentFilter, this);
                spawnedObjects.Add(component);
            }
            uI_SearchItem = spawnedObjects[0];
        }
        if (CurrentFilter == ItemFilter.MsgBottle)
        {
            WrapController.WrapToggle.isOn = false;
            WrapController.WrapToggle.gameObject.SetActive(false);
            SetController.FFlagOne.gameObject.SetActive(true);
        }
        else
        {
            WrapController.WrapToggle.gameObject.SetActive(true);
            SetController.FFlagOne.gameObject.SetActive(false);
        }
        if (MappableText.gameObject.activeSelf)
        {
            MappableText.transform.SetAsLastSibling();
        }
        if (uI_SearchItem != null)
        {
            uI_SearchItem.SelectionButton.onClick.Invoke();
        }
    }

    public void SelectItem(ItemFilter itemF, int id, UI_SearchItem sItem)
    {
        if (id == Item.NONE)
        {
            return;
        }
        SetController.FCount.text = id.ToString();
        ItemSelectedName.text = sItem.RawValue;
        switch (itemF)
        {
            case ItemFilter.Recipes:
                CurrentItemID = RECIPEITEM;
                break;
            case ItemFilter.Fossils:
                CurrentItemID = FOSSILITEM;
                break;
            case ItemFilter.MsgBottle:
                CurrentItemID = MESSAGEBOTTLEITEM;
                break;
            default:
                CurrentItemID = id;
                SetController.FCount.text = 0.ToString();
                break;
        }
        CurrentSelectedIndex = spawnedObjects.IndexOf(sItem);
        if (ItemInfo.GetItemKind(Convert.ToUInt16(CurrentItemID)).IsFlower())
        {
            FlowerButtonRoot.SetActive(true);
        }
        else
        {
            FlowerController.ResetToZero();
            FlowerButtonRoot.SetActive(false);
        }
        short remakeIndex = ItemRemakeUtil.GetRemakeIndex(Convert.ToUInt16(CurrentItemID));
        if (remakeIndex < 0)
        {
            SetController.CreateBody(new string[0]);
            SetController.CreateFabric(new string[0]);
        }
        else
        {
            ItemRemakeInfo itemRemakeInfo = ItemRemakeInfoData.List[remakeIndex];
            string bodySummary = itemRemakeInfo.GetBodySummary(GameInfo.Strings, tryGetDescriptor: false);
            if (bodySummary.Length != 0)
            {
                string[] values = bodySummary.Split(new string[3]
                {
                    "\r\n",
                    "\r",
                    "\n"
                }, StringSplitOptions.None);
                SetController.CreateBody(values);
            }
            else
            {
                SetController.CreateBody(new string[0]);
            }
            string fabricSummary = itemRemakeInfo.GetFabricSummary(GameInfo.Strings, tryGetDescriptor: false);
            if (fabricSummary.Length != 0)
            {
                string[] values2 = fabricSummary.Split(new string[3]
                {
                    "\r\n",
                    "\r",
                    "\n"
                }, StringSplitOptions.None);
                SetController.CreateFabric(values2);
            }
            else
            {
                SetController.CreateFabric(new string[0]);
            }
        }
        if (currentAnimationFuction != null)
        {
            StopCoroutine(currentAnimationFuction);
        }
        currentAnimationFuction = StartCoroutine(sendSelectorToSelected());
    }

    private IEnumerator sendSelectorToSelected(float time = 0.1f)
    {
        float i = 0f;
        float rate = 1f / time;
        Vector3 startPos = SelectionOverlay.transform.position;
        Vector3 endPos = spawnedObjects[CurrentSelectedIndex].transform.position;
        while (i < 1f)
        {
            i += Time.deltaTime * rate;
            Vector3 position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, i));
            SelectionOverlay.transform.position = position;
            yield return null;
        }
        while (CurrentSelectedIndex < spawnedObjects.Count && CurrentSelectedIndex >= 0)
        {
            SelectionOverlay.transform.position = spawnedObjects[CurrentSelectedIndex].transform.position;
            yield return null;
        }
    }

    public void LoadItem(Item item)
    {
        if (stopSearch)
            return;
        CurrentItemID = item.ItemId;
        if (CurrentItemID == MESSAGEBOTTLEITEM)
        {
            CurrentFilter = ItemFilter.MsgBottle;
        }
        else if (CurrentItemID == RECIPEITEM)
        {
            CurrentFilter = ItemFilter.Recipes;
        }
        else if (CurrentItemID == FOSSILITEM)
        {
            CurrentFilter = ItemFilter.Fossils;
        }
        else
        {
            CurrentFilter = ItemFilter.Items;
        }
        List<ComboItem> list = allItems(CurrentFilter);
        ComboItem comboItem = (CurrentFilter == ItemFilter.Items) ? list.Find((ComboItem x) => x.Value == item.ItemId) : list.Find((ComboItem x) => x.Value == item.Count);
        SearchField.text = comboItem.Text;
        CurrentItemID = item.ItemId;
        ItemKind itemKind = ItemInfo.GetItemKind(Convert.ToUInt16(CurrentItemID));
        if (itemKind.IsFlower())
        {
            loadGenes(item.Genes);
            FlowerController.DaysWatered.text = item.DaysWatered.ToString();
            FlowerController.GoldCanWatered.isOn = item.IsWateredGold;
            FlowerController.Watered.isOn = item.IsWatered;
            bool[] array = new bool[10];
            for (int i = 0; i < 10; i++)
            {
                array[i] = item.GetIsWateredByVisitor(i);
            }
            FlowerController.SetVisitorWatered(array);
            return;
        }
        SetController.FCount.text = item.Count.ToString();
        SetController.CompileBodyFabricFromCount();
        SetController.FUses.text = item.UseCount.ToString();
        SetController.FFlagZero.text = item.SystemParam.ToString();
        if (itemKind == ItemKind.Kind_MessageBottle)
        {
            SetController.FFlagOne.text = item.AdditionalParam.ToString();
            return;
        }
        WrapController.WrapToggle.isOn = item.WrappingType != ItemWrapping.Nothing;
        WrapController.WrapType.value = (int)item.WrappingType;
        WrapController.WrapColor.value = (int)item.WrappingPaper;
        WrapController.ShowItemToggle.isOn = item.WrappingShowItem;
        WrapController.Flag80 = item.Wrapping80;

        DropdownFilter.SetValueWithoutNotify((int)CurrentFilter);
        DropdownFilter.RefreshShownValue();
    }

    public Item GetAsItem(Item referenceItem)
    {
        if (CurrentItemID == -1)
            return referenceItem;
        Item item = (referenceItem == null) ? new Item() : referenceItem;
        ushort num = Convert.ToUInt16(CurrentItemID);
        ItemKind itemKind = ItemInfo.GetItemKind(num);
        item.ItemId = num;
        if (itemKind.IsFlower())
        {
            item.Genes = saveGenes();
            item.DaysWatered = int.Parse(FlowerController.DaysWatered.text);
            item.IsWateredGold = FlowerController.GoldCanWatered.isOn;
            item.IsWatered = FlowerController.Watered.isOn;
            bool[] visitorWatered = FlowerController.GetVisitorWatered();
            for (int i = 0; i < visitorWatered.Length; i++)
            {
                item.SetIsWateredByVisitor(i, visitorWatered[i]);
            }
            item.SystemParam = 0;
            item.AdditionalParam = 0;
        }
        else
        {
            int value = int.Parse(SetController.FCount.text);
            int value2 = int.Parse(SetController.FUses.text);
            int value3 = int.Parse(SetController.FFlagZero.text);
            int value4 = int.Parse(SetController.FFlagOne.text);
            item.Count = Convert.ToUInt16(value);
            item.UseCount = Convert.ToUInt16(value2);
            item.SystemParam = Convert.ToByte(value3);
            if (itemKind == ItemKind.Kind_MessageBottle)
            {
                item.AdditionalParam = Convert.ToByte(value4);
            }
            else if (!WrapController.WrapToggle.isOn)
            {
                item.SetWrapping(ItemWrapping.Nothing, ItemWrappingPaper.Yellow);
            }
            else
            {
                ItemWrapping wrap = (ItemWrapping)WrapController.ItemWrap;
                ItemWrappingPaper color = (ItemWrappingPaper)WrapController.ItemColor;
                bool isOn = WrapController.ShowItemToggle.isOn;
                bool flag = WrapController.Flag80;
                item.SetWrapping(wrap, color, isOn, flag);
            }
        }
        return item;
    }

    private List<ComboItem> allItems(ItemFilter filter)
    {
        switch (filter)
        {
            case ItemFilter.Items:
                return GameInfo.Strings.ItemDataSource.ToList();
            case ItemFilter.MsgBottle:
                return GameInfo.Strings.CreateItemDataSource(RecipeList.Recipes, false);
            case ItemFilter.Recipes:
                return GameInfo.Strings.CreateItemDataSource(RecipeList.Recipes, false);
            case ItemFilter.Fossils:
                return GameInfo.Strings.CreateItemDataSource(GameLists.Fossils, false);
            default:
                return GameInfo.Strings.ItemDataSource.ToList(); 
        }
    }

    private FlowerGene saveGenes() // bad il rebuild but surprisingly readable
    {
        FlowerGene flowerGene = FlowerGene.None;
        if (FlowerController.R1.isOn)
        {
            flowerGene |= FlowerGene.R1;
        }
        if (FlowerController.R2.isOn)
        {
            flowerGene |= FlowerGene.R2;
        }
        if (FlowerController.Y1.isOn)
        {
            flowerGene |= FlowerGene.Y1;
        }
        if (FlowerController.Y2.isOn)
        {
            flowerGene |= FlowerGene.Y2;
        }
        if (!FlowerController.W1.isOn)
        {
            flowerGene |= FlowerGene.w1;
        }
        if (!FlowerController.W2.isOn)
        {
            flowerGene |= FlowerGene.w2;
        }
        if (FlowerController.S1.isOn)
        {
            flowerGene |= FlowerGene.S1;
        }
        if (FlowerController.S2.isOn)
        {
            flowerGene |= FlowerGene.S2;
        }
        return flowerGene;
    }

    private void loadGenes(FlowerGene genes)
    {
        FlowerController.R1.isOn = (genes & FlowerGene.R1) != 0;
        FlowerController.R2.isOn = (genes & FlowerGene.R2) != 0;
        FlowerController.Y1.isOn = (genes & FlowerGene.Y1) != 0;
        FlowerController.Y2.isOn = (genes & FlowerGene.Y2) != 0;
        FlowerController.W1.isOn = (genes & FlowerGene.w1) == 0;
        FlowerController.W2.isOn = (genes & FlowerGene.w2) == 0;
        FlowerController.S1.isOn = (genes & FlowerGene.S1) != 0;
        FlowerController.S2.isOn = (genes & FlowerGene.S2) != 0;
    }
}
