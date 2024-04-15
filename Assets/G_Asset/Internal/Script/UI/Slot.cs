using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Button btn;
    public Image icon;
    private Vector2Int position = new();
    bool isPlayerOwner = false;
    bool isMachineOwner = false;
    private void Start()
    {
        btn.onClick.AddListener(() =>
        {
            ClickSlot();
        });
    }
    public void ClickSlot()
    {
        if (SlotController.instance.CanPlay())
        {
            if (!GetChoose())
            {
                SlotController.instance.PlayerPlay(this);
            }
        }
    }
    public bool GetChoose()
    {
        return isPlayerOwner || isMachineOwner;
    }
    public void SlotInit(Vector2Int newPos, Sprite reloadSprite)
    {
        position = newPos;
        icon.sprite = reloadSprite;
        isPlayerOwner = false;
        isMachineOwner = false;
    }
    public void ChangeSlotItem(Sprite sprite, bool isPlayerOwner)
    {
        icon.sprite = sprite;
        isMachineOwner = !isPlayerOwner;
        this.isPlayerOwner = isPlayerOwner;
    }
    public void WinSprite(Sprite newSprite)
    {
        icon.sprite = newSprite;
    }
    public string GetPositionString()
    {
        return position.x + "-" + position.y;
    }
    public Vector2Int GetPosition()
    {
        return position;
    }
    public bool IsPlayerOwner()
    {
        return isPlayerOwner;
    }
    public bool IsMachineOwner()
    {
        return isMachineOwner;
    }
}
