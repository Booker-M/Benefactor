using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;
using System;
using UnityEngine.SocialPlatforms;
using UnityEditor;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance = null;

    public GameObject playerInventory;
    public GameObject otherInventory;
    public InventorySlot[] playerSlots;
    public InventorySlot[] otherSlots;
    public GameObject playerStats;
    public GameObject portrait;
    public Text characterName;
    public Text characterLevel;
    public Text characterLevelLabel;
    public Text healthText;
    public Text movesText;
    public GameObject attackInfo;
    public Text attackDamage;
    public Text attackHit;
    public Text attackCrit;
    public GameObject experience;
    public GameObject expBar;
    public GameObject expBarBackground;
    public Text expText;
    public GameObject mouseIndicatorSprite;
    public GameObject tileIndicatorSprite;
    public GameObject levelUp;
    public GameObject levelUpPortrait;
    public Text levelUpCharacterName;
    public Text[] stats;
    public Image[] statIcons;
    public List<GameObject> indicators;
    private GameObject mouseIndicator;
    public float unhighlightedAlpha;
    public float highlightedAlpha;
    public Color defaultColor;
    public Color defaultPlayerColor;
    public Color defaultEnemyColor;
    public CanvasGroup actionMenu;
    private Dictionary<String, GameObject> actionButtons;
    public GameObject backButton;

    public float expUpdateTime;
    public float expHideDelay;
    // private SpriteRenderer spriteRenderer;
    private float inverseUpdateTime;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        // DontDestroyOnLoad(gameObject);

        actionButtons = new Dictionary<String, GameObject>();
        actionButtons.Add("Attack", GameObject.Find("AttackButton"));
        actionButtons.Add("Talk", GameObject.Find("TalkButton"));
        actionButtons.Add("Heal", GameObject.Find("HealButton"));
        actionButtons.Add("Door", GameObject.Find("DoorButton"));
        actionButtons.Add("Unlock", GameObject.Find("UnlockButton"));
        actionButtons.Add("Lever", GameObject.Find("LeverButton"));
        actionButtons.Add("Loot", GameObject.Find("LootButton"));
        actionButtons.Add("Steal", GameObject.Find("StealButton"));
        actionButtons.Add("Trade", GameObject.Find("TradeButton"));
        actionButtons.Add("Wait", GameObject.Find("WaitButton"));
        HideActionMenu();

        playerInventory.transform.position = new Vector2(Screen.width / 2, Screen.height / 4);
        otherInventory.transform.position = new Vector2(Screen.width / 2, Screen.height / 4);
        playerSlots = playerInventory.GetComponentsInChildren<InventorySlot>();
        otherSlots = otherInventory.GetComponentsInChildren<InventorySlot>();
        HideInventories();

        HidePlayerStats();
        
        HideAttackInfo();

        inverseUpdateTime = 1 / expUpdateTime;
        HideExperience();

        HideLevelUp();

        backButton.transform.position = new Vector2(Screen.width*0.9f, Screen.height*0.1f);
        HideBackButton();

        defaultColor.a = unhighlightedAlpha;
        defaultPlayerColor.a = unhighlightedAlpha;
        defaultEnemyColor.a = unhighlightedAlpha;
    }

    public void SetupActionMenu(SortedSet<String> actions)
    {
        Vector2 position = new Vector2(Screen.width / 2, Screen.height / 3);
        GameObject buttonForScale;
        actionButtons.TryGetValue("Attack", out buttonForScale);
        float buttonHeight = buttonForScale.GetComponent<RectTransform>().rect.height,
            buttonWidth = buttonForScale.GetComponent<RectTransform>().rect.width,
            spacing = buttonHeight*0.3f,
            width  = buttonWidth + spacing * 2,
            height = (buttonHeight + spacing) * actions.Count + spacing;
        RectTransform panelRectTransform = GameObject.Find("ActionPanel").transform.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = new Vector2(width, height);
        panelRectTransform.transform.position = position;
        int index = 0;
        foreach (string action in actions)
        {
            GameObject button;
            actionButtons.TryGetValue(action, out button);
            button.SetActive(true);
            button.GetComponent<RectTransform>().transform.localPosition = new Vector2(0, (0 + height/2 - spacing - buttonHeight/2 - (buttonHeight + spacing) * index));
            index++;
        }

        actionMenu.alpha = 1f;
        actionMenu.blocksRaycasts = true;
        Debug.Log("Player waiting for act input");
    }

    public void HideActionMenu()
    {
        actionMenu.alpha = 0f;
        actionMenu.blocksRaycasts = false;
        foreach (GameObject button in actionButtons.Values)
        {
            button.SetActive(false);
        }
    }

    public List<HoldableObject> SortedInventory(String type, List<HoldableObject> inventory)
    {
        return inventory.FindAll(e => e.type == type);
    }

    public void ShowPlayerInventory(String type, List<HoldableObject> inventory, int range = 0, List<HoldableObject> items = null, String name = null)
    {
        if (items == null)
            items = SortedInventory(type, inventory);
        int j = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items != null && (range == 0 || items[i].range >= range))
            {
                playerSlots[j].AddItem(items[i]);
                j++;
            }
        }
        while (j < playerSlots.Length)
        {
            playerSlots[j].ClearSlot();
            j++;
        }

        playerInventory.GetComponentsInChildren<Text>()[0].text = name != null ? name : type;
        playerInventory.SetActive(true);
        ShowBackButton();
        // instance.HidePlayerStats();
    }
    public void ShowOtherInventory(String type, List<HoldableObject> inventory, int range = 0, List<HoldableObject> items = null, String name = null)
    {
        if (items == null)
            items = SortedInventory(type, inventory);
        int j = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items != null && (range == 0 || items[i].range >= range))
            {
                otherSlots[j].AddItem(items[i]);
                j++;
            }
        }
        while (j < otherSlots.Length)
        {
            otherSlots[j].ClearSlot();
            j++;
        }

        otherInventory.GetComponentsInChildren<Text>()[0].text = name != null ? name : type;
        playerInventory.GetComponent<RectTransform>().transform.localPosition = new Vector2(-120, 0);
        otherInventory.GetComponent<RectTransform>().transform.localPosition = new Vector2(120, 0);
        otherInventory.SetActive(true);
        ShowBackButton();
        instance.HidePlayerStats();
    }

    public void HideInventories()
    {
        playerInventory.SetActive(false);
        otherInventory.SetActive(false);
        playerInventory.GetComponent<RectTransform>().transform.localPosition = new Vector2(0, 0);
    }

    public void ShowPlayerStats(InteractableObject target)
    {
        Character character = target.GetComponent<Character>();
        Tree tree = target.GetComponent<Tree>();
        portrait.GetComponent<Image>().sprite = (character != null) ? character.portrait : (tree != null) ? tree.overview : target.GetComponent<SpriteRenderer>().sprite;
        characterName.GetComponent<Text>().text = (character != null) ? character.name : target.name
            .Replace("(Clone)", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "")
            .Replace("5", "").Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "").Replace("0", "");
        healthText.GetComponent<Text>().text = target.GetHealth().ToString() + "/" + target.maxHealth;
        movesText.GetComponent<Text>().text = (character != null) ? (character.totalMoves.ToString()) : "";
        movesText.GetComponent<Text>().gameObject.SetActive(character != null);
        characterLevelLabel.gameObject.SetActive(character != null);
        characterLevel.text = (character != null) ? character.level + "" : "";
        playerStats.SetActive(true);
    }

    public void HidePlayerStats()
    {
        playerStats.SetActive(false);
    }

    public void ShowAttackInfo(Vector3 position, int damage, int hitPercent, int critPercent)
    {
        attackDamage.GetComponent<Text>().text = damage + "";
        attackHit.GetComponent<Text>().text = hitPercent + "%";
        attackCrit.GetComponent<Text>().text = critPercent + "%";
        attackInfo.GetComponent<RectTransform>().transform.position = position + new Vector3(0,130,0);
        attackInfo.SetActive(true);
    }

    public void HideAttackInfo()
    {
        attackInfo.SetActive(false);
    }

    public IEnumerator UpdateExperience(int start, int amount)
    {
        float maxWidth = expBarBackground.GetComponent<RectTransform>().rect.width;
        float startWidth = ((float)start/100)*maxWidth;
        Vector2 begin = new Vector2(startWidth, expBarBackground.GetComponent<RectTransform>().rect.height);
        expBar.GetComponent<RectTransform>().sizeDelta = begin;
        float targetWidth = ((float)amount/100)*maxWidth;
        Vector2 size = expBar.GetComponent<RectTransform>().sizeDelta;
        float remaining = targetWidth < size.x ? size.x - targetWidth : targetWidth - size.x;
        Vector2 end = new Vector2(targetWidth, expBarBackground.GetComponent<RectTransform>().rect.height);
        expText.text = start + "";
        ShowExperience();
        while (remaining > float.Epsilon)
        {
            size = expBar.GetComponent<RectTransform>().sizeDelta;
            Vector2 newSize = Vector2.MoveTowards(size, end, inverseUpdateTime * Time.fixedDeltaTime * maxWidth);
            remaining = targetWidth < size.x ? size.x - targetWidth : targetWidth - size.x;
            expBar.GetComponent<RectTransform>().sizeDelta = newSize;
            expText.text = (int)(newSize.x*100/maxWidth) + "";
            yield return null;
            if ((int)(remaining/targetWidth) % 20 == 0)
                SoundManager.instance.GetExp();
        }
        yield return new WaitForSeconds(expHideDelay);
        HideExperience();
    }

    public void ShowExperience()
    {
        experience.SetActive(true);
    }

    public void HideExperience()
    {
        experience.SetActive(false);
    }

    public void ShowLevelUp(Character character)
    {
        levelUpPortrait.GetComponent<Image>().sprite = character.portrait;
        levelUpCharacterName.GetComponent<Text>().text = character.name;
        stats[0].text = character.level + "";
        stats[1].text = character.GetStats().health + "";
        stats[2].text = character.GetStats().agility + "";
        stats[3].text = character.GetStats().strength + "";
        stats[4].text = character.GetStats().magic + "";
        stats[5].text = character.GetStats().defense + "";
        stats[6].text = character.GetStats().resistance + "";
        stats[7].text = character.GetStats().skill + "";
        stats[8].text = character.GetStats().dexterity + "";
        foreach (Image image in statIcons) {
            image.gameObject.SetActive(false);
        }
        levelUp.SetActive(true);
        SoundManager.instance.LevelUp();
    }

    public void HideLevelUp()
    {
        levelUp.SetActive(false);
    }

    public IEnumerator LevelUpStat(int i, int value)
    {
        yield return new WaitForSeconds(expHideDelay);
        stats[i].text = value + "";
        statIcons[i].gameObject.SetActive(true);
        SoundManager.instance.StatUp();
    }

    public void ShowBackButton()
    {
        backButton.SetActive(true);
    }

    public void HideBackButton()
    {
        backButton.SetActive(false);
    }

    public void ShowPaths(Dictionary<Vector2, Vector2[]> paths)
    {
        HideIndicators();

        foreach (KeyValuePair<Vector2, Vector2[]> entry in paths)
        {
            ShowIndicator(entry.Key, defaultPlayerColor);
        }
    }

    public void ShowIndicator(Vector2 coords, Color color)
    {
        indicators.Add(Instantiate(tileIndicatorSprite, coords, Quaternion.identity));
        indicators[indicators.Count - 1].GetComponent<SpriteRenderer>().material.color = color;
    }

    // public void HideIndicator(Vector2 coords)
    // {
    //     foreach (GameObject indicator in indicators)
    //     {
    //         if ((Vector2)indicator.transform.position == coords) {
    //             indicators.Remove(indicator);
    //             Destroy(indicator);
    //         }
    //     }
    // }

    public void ShowMouseIndicator(Vector2 coords)
    {
        mouseIndicator = Instantiate(mouseIndicatorSprite, coords, Quaternion.identity);
        mouseIndicator.GetComponent<SpriteRenderer>().material.color = defaultColor;
        InteractableObject overlap = mouseIndicator.GetComponent<MouseIndicator>().FindInteractableObject();
        if (overlap != null)
        {
            ShowPlayerStats(overlap);
        }
        else
        {
            HidePlayerStats();
        }
    }

    public void HideMouseIndicator()
    {
        Destroy(mouseIndicator);
    }

    public void ShowObjects(List<InteractableObject> objects)
    {
        HideIndicators();
        Color color = defaultColor;

        foreach (InteractableObject o in objects)
        {
            if (o.gameObject.tag == "Character") {
                color = o.gameObject.GetComponent<Player>().playable ? defaultPlayerColor : defaultEnemyColor;
            } else {
                color = defaultColor;
            }
            if (o.gameObject.GetComponent<Bed>() != null) {
                ShowIndicator(o.transform.position - new Vector3(0,1,1), color);
            }
            ShowIndicator(o.transform.position, color);
        }
    }

    public void HideIndicators()
    {
        foreach (GameObject indicator in indicators)
        {
            Destroy(indicator);
        }
        indicators.Clear();
    }

    public void HighlightPath(Vector2[] path)
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            Color currentColor = indicator.GetComponent<SpriteRenderer>().material.color;
            if (path.Contains((Vector2)indicator.transform.position))
            {
                currentColor.a = highlightedAlpha;
            }
            else
            {
                currentColor.a = unhighlightedAlpha;
            }
            indicator.GetComponent<SpriteRenderer>().material.color = currentColor;
        }
    }

    public void UnhighlightPath(Vector2[] path)
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            Color currentColor = indicator.GetComponent<SpriteRenderer>().material.color;
            if (path.Contains((Vector2)indicator.transform.position))
            {
                currentColor.a = unhighlightedAlpha;
                indicator.GetComponent<SpriteRenderer>().material.color = currentColor;
            }
        }
    }

    public void UnhighlightPaths()
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            Color currentColor = indicator.GetComponent<SpriteRenderer>().material.color;
            currentColor.a = unhighlightedAlpha;
            indicator.GetComponent<SpriteRenderer>().material.color = currentColor;
        }
    }

    public static void ActionButtonPressed(String action)
    {
        GameManager.instance.activeCharacter.GetActionInput(action);
        instance.HidePlayerStats();
    }

    public static void BackButtonPressed()
    {
        GameManager.instance.activeCharacter.Back();
    }
}
