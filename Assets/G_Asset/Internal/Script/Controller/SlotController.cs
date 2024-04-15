using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class SlotController : MonoBehaviour
{
    public static SlotController instance;
    public Transform containter_ui;
    public Slot slot;
    public GridLayoutGroup layoutGroup;

    [SerializeField] private Vector2Int size = new(20, 20);
    private Dictionary<Vector2Int, Slot> slots = new();

    bool canPlay = false;

    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite machineSprite;
    [SerializeField] private Sprite defaultSprite;

    [SerializeField] private int checkSlotAmount = 5;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    private void Start()
    {
        SpawnSlot();
        canPlay = true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Refresh();
        }
    }
    public void SpawnSlot()
    {
        foreach (Transform child in containter_ui.transform)
        {
            Destroy(child.gameObject);
        }
        int x = size.x;
        int y = size.y;
        layoutGroup.constraintCount = x;
        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                Slot tempSlot = Instantiate(slot, containter_ui.transform);
                Vector2Int pos = new(i, j);
                tempSlot.SlotInit(pos, defaultSprite);
                slots[pos] = tempSlot;
            }
        }
    }
    public void Refresh()
    {
        int x = size.x;
        int y = size.y;
        layoutGroup.constraintCount = x;
        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                Vector2Int pos = new(i, j);
                Slot tempSlot = slots[pos];
                tempSlot.SlotInit(pos, defaultSprite);
            }
        }
        canPlay = true;
    }
    public bool CanPlay()
    {
        return canPlay;
    }
    public void PlayerPlay(Slot currentSlot)
    {
        currentSlot.ChangeSlotItem(playerSprite, true);
        canPlay = false;
        bool isDone = SlotCheck(currentSlot);
        if (isDone)
        {
            Debug.Log("End game");
        }
        else
        {
            MachinePlay();
        }
    }
    public bool SlotCheck(Slot checkSlot)
    {
        Vector2Int pos = checkSlot.GetPosition();
        int topDown = TotalCount(pos, new(0, -1), new(0, 1));
        int leftRight = TotalCount(pos, new(-1, 0), new(1, 0));
        int crossTopRight = TotalCount(pos, new(1, 1), new(-1, -1));
        int crossBottomRight = TotalCount(pos, new(-1, 1), new(1, -1));
        if (topDown >= checkSlotAmount)
        {
            return true;
        }

        if (leftRight >= checkSlotAmount)
        {
            return true;
        }

        if (crossTopRight >= checkSlotAmount)
        {
            return true;
        }

        if (crossBottomRight >= checkSlotAmount)
        {
            return true;
        }

        return false;
    }
    public int TotalCount(Vector2Int pos, Vector2Int check1, Vector2Int check2)
    {
        int up = SlotCount(pos, check1.x, check1.y);
        int down = SlotCount(pos, check2.x, check2.y);
        return up + down + 1;
    }
    public int SlotCount(Vector2Int checkPos, int x, int y, bool isPlayerOwner = true)
    {
        int count = 0;
        Vector2Int previousPos = checkPos;
        for (int i = 0; i < checkSlotAmount; i++)
        {
            Vector2Int nextPosition = new(previousPos.x + x, previousPos.y + y);
            if (!slots.ContainsKey(nextPosition))
            {
                return count;
            }
            else
            {
                Slot checkSlot = slots[nextPosition];
                bool isChoose = checkSlot.GetChoose();
                if (isChoose)
                {
                    bool ofPlayer = checkSlot.IsPlayerOwner();
                    if (ofPlayer && isPlayerOwner)
                    {
                        count += 1;
                    }
                    else if (!ofPlayer && !isPlayerOwner)
                    {
                        count += 1;
                    }
                    previousPos = nextPosition;
                }
                else
                {
                    return count;
                }
            }
        }
        return count;
    }
    public void MachinePlay()
    {
        canPlay = true;
    }
}
