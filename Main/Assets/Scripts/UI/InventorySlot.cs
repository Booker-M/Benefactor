using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Button button;
    public Text text;

    HoldableObject item;

    public void AddItem (HoldableObject newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;
        text.text = "x" + newItem.uses;
        button.interactable = true;
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
        text.text = "";
        button.interactable = false;
    }

    public void OnPress()
    {
        StartCoroutine(GameManager.instance.activeCharacter.ChooseItem(item));
        MenuManager.instance.HideAttackInfo();
    }

    public void OnHover()
    {
        Debug.Log("ON HOVER");
        Debug.Log(item);
        if (item != null)
            GameManager.instance.activeCharacter.PreviewItem(item, button.transform.position);
    }

    public void EndHover()
    {
        if (item != null)
            MenuManager.instance.HideAttackInfo();
    }
}
