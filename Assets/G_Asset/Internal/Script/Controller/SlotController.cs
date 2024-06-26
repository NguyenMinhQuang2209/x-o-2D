﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
    public static SlotController instance;
    public Transform containter_ui;
    public Slot slot;
    public GridLayoutGroup layoutGroup;

    [SerializeField] private Vector2Int size = new(20, 20);
    private Dictionary<Vector2Int, Slot> slots = new();

    private List<Vector2Int> remainSlots = new();

    private List<Vector2Int> storeRemainSlots = new();

    bool canPlay = false;

    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite machineSprite;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite winSprite;

    [SerializeField] private int checkSlotAmount = 5;

    private List<MachinePlay> machineSlots = new();

    private List<SlotCheck> topDownStore = new();
    private List<SlotCheck> leftRightStore = new();
    private List<SlotCheck> crossLeftStore = new();
    private List<SlotCheck> crossRightStore = new();

    private List<SlotCheck> winSlot = new();

    private bool hasNewPos = false;
    private Vector2Int newPos = new();

    bool endGame = false;

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
                storeRemainSlots.Add(pos);
            }
        }
        remainSlots = new(storeRemainSlots);
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
        remainSlots = new(storeRemainSlots);
        canPlay = true;
        endGame = false;
        machineSlots?.Clear();
        topDownStore?.Clear();
        leftRightStore?.Clear();
        crossLeftStore?.Clear();
        crossRightStore?.Clear();
        winSlot?.Clear();
    }
    public bool CanPlay()
    {
        return canPlay;
    }
    public bool IsUseSlot(Vector2Int pos)
    {
        if (!slots.ContainsKey(pos))
        {
            return true;
        }
        else
        {
            return slots[pos].GetChoose();
        }
    }
    public bool IsExistSlot(Vector2Int pos)
    {
        return slots.ContainsKey(pos);
    }
    public void PlayerPlay(Slot currentSlot)
    {
        if (endGame)
        {
            return;
        }

        remainSlots.Remove(currentSlot.GetPosition());
        currentSlot.ChangeSlotItem(playerSprite, true);
        canPlay = false;

        int count = SlotCheck(currentSlot, true);
        if (count >= checkSlotAmount)
        {
            EndGame(true);
        }
        else
        {
            MachinePlay(currentSlot);
        }
    }
    public int SlotCheck(Slot checkSlot, bool isPlayer)
    {
        int count = 0;
        Vector2Int pos = checkSlot.GetPosition();

        int topDown = TotalCount(topDownStore, true, pos, new(0, -1), new(0, 1), true, isPlayer);
        int leftRight = TotalCount(leftRightStore, true, pos, new(-1, 0), new(1, 0), true, isPlayer);
        int crossTopRight = TotalCount(crossLeftStore, true, pos, new(1, 1), new(-1, -1), true, isPlayer);
        int crossBottomRight = TotalCount(crossRightStore, true, pos, new(-1, 1), new(1, -1), true, isPlayer);


        count = count > topDown ? count : topDown;

        if (topDown >= checkSlotAmount)
        {
            winSlot = topDownStore;
        }

        count = count > leftRight ? count : leftRight;

        if (leftRight >= checkSlotAmount)
        {
            winSlot = leftRightStore;
        }

        count = count > crossTopRight ? count : crossBottomRight;

        if (crossTopRight >= checkSlotAmount)
        {
            winSlot = crossLeftStore;
        }

        count = count > crossBottomRight ? count : crossBottomRight;

        if (crossBottomRight >= checkSlotAmount)
        {
            winSlot = crossRightStore;
        }

        return count;
    }
    public int TotalCount(List<SlotCheck> store, bool storing, Vector2Int pos, Vector2Int check1, Vector2Int check2, bool stopWhenNotChoose = true, bool isPlayerOwner = true)
    {
        if (storing)
        {
            store?.Clear();
        }
        int up = SlotCount(store, storing, pos, check1.x, check1.y, stopWhenNotChoose, isPlayerOwner);
        int down = SlotCount(store, storing, pos, check2.x, check2.y, stopWhenNotChoose, isPlayerOwner);
        return up + down + 1;
    }
    public int SlotCount(List<SlotCheck> store, bool storing, Vector2Int checkPos, int x, int y, bool stopWhenNotChoose = true, bool isPlayerOwner = true)
    {
        int count = 0;
        Vector2Int previousPos = checkPos;
        if (storing) store.Add(new(checkPos, true));
        for (int i = 1; i < checkSlotAmount; i++)
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
                    else
                    {
                        return count;
                    }
                }
                else
                {
                    if (stopWhenNotChoose)
                    {
                        return count;
                    }
                }
                previousPos = nextPosition;
                if (storing) store.Add(new(nextPosition, isChoose));
            }
        }
        return count;
    }
    public void MachinePlay(Slot currentSlot)
    {
        Vector2Int pos = currentSlot.GetPosition();

        int topDown = TotalCount(topDownStore, false, pos, new(-1, 0), new(1, 0), false, true);
        int leftRight = TotalCount(leftRightStore, false, pos, new(0, -1), new(0, 1), false, true);
        int crossTopRight = TotalCount(crossLeftStore, false, pos, new(1, 1), new(-1, -1), false, true);
        int crossBottomRight = TotalCount(crossRightStore, false, pos, new(-1, 1), new(1, -1), false, true);

        hasNewPos = false;

        bool needCheck = true;

        if (machineSlots.Count > 0)
        {
            if (machineSlots[0].count >= checkSlotAmount - 1)
            {
                needCheck = false;
            }
        }

        if (needCheck)
        {
            if (topDown >= checkSlotAmount - 2)
            {
                PlayPointMachineCheck(pos, new(1, 0), new(-1, 0));
            }
            if (leftRight >= checkSlotAmount - 2)
            {
                PlayPointMachineCheck(pos, new(0, -1), new(0, 1));
            }
            if (crossTopRight >= checkSlotAmount - 2)
            {
                PlayPointMachineCheck(pos, new(1, 1), new(-1, -1));
            }
            if (crossBottomRight >= checkSlotAmount - 2)
            {
                PlayPointMachineCheck(pos, new(1, -1), new(-1, 1));
            }
        }

        if (!hasNewPos)
        {
            newPos = MachinePlay(pos);
        }
        remainSlots.Remove(newPos);

        CheckMachinePlayPosition(newPos);

        canPlay = true;
    }
    public void PlayPointMachineCheck(Vector2Int pos, Vector2Int firstCheck, Vector2Int secondCheck)
    {
        List<SlotCheck> left = new();
        List<SlotCheck> right = new();

        int leftCount = SlotCount(left, true, pos, firstCheck.x, firstCheck.y, false, true);
        int rightCount = SlotCount(right, true, pos, secondCheck.x, secondCheck.y, false, true);
        if (left.Count + right.Count > checkSlotAmount - 1)
        {
            if (left.Count == 0)
            {
                if (right.Count >= checkSlotAmount - 1)
                {
                    right.Add(new(pos, true));
                    for (int i = 0; i < right.Count; i++)
                    {
                        SlotCheck current = right[i];
                        if (!current.isCheck)
                        {
                            newPos = current.pos;
                            hasNewPos = true;
                        }
                    }
                }
            }
            else
            {
                if (right.Count == 0)
                {
                    if (left.Count >= checkSlotAmount - 1)
                    {
                        left.Add(new(pos, true));
                        for (int i = 0; i < left.Count; i++)
                        {
                            SlotCheck current = left[i];
                            if (!current.isCheck)
                            {
                                newPos = current.pos;
                                hasNewPos = true;
                            }
                        }
                    }
                }
                else
                {
                    List<SlotCheck> totalCheck = new();
                    totalCheck.AddRange(left);
                    totalCheck.AddRange(right);
                    SlotCheck rootItem = new(pos, true);

                    for (int i = 0; i < totalCheck.Count; i++)
                    {
                        SlotCheck checkItem = totalCheck[i];
                        if (checkItem.pos == rootItem.pos)
                        {
                            totalCheck.RemoveAt(i);
                            break;
                        }
                    }

                    totalCheck.Sort((a, b) =>
                    {
                        int compareY = a.pos.x.CompareTo(b.pos.x);
                        if (compareY != 0)
                        {
                            return compareY;
                        }
                        return a.pos.y.CompareTo(b.pos.y);
                    });
                    int count = 0;
                    List<SlotCheck> predictList = new();
                    for (int i = 0; i < totalCheck.Count - checkSlotAmount + 1; i++)
                    {
                        SlotCheck item = totalCheck[i];
                        Slot itemSlot = slots[item.pos];
                        List<SlotCheck> tempList = totalCheck.GetRange(i, checkSlotAmount);
                        int newCount = 0;
                        foreach (SlotCheck check in tempList)
                        {
                            if (check.isCheck)
                            {
                                newCount++;
                            }
                        }
                        if (newCount > count)
                        {
                            predictList = new(tempList);
                            count = newCount;
                        }
                    }
                    int centerPoint = (int)Mathf.Floor(predictList.Count / 2);
                    for (int i = 0; i < Mathf.Ceil((float)predictList.Count / 2); i++)
                    {
                        SlotCheck item = predictList[centerPoint + i];
                        if (!item.isCheck)
                        {
                            newPos = item.pos;
                            hasNewPos = true;
                            break;
                        }
                        if (i != 0)
                        {
                            SlotCheck secondItem = predictList[centerPoint - i];
                            if (!secondItem.isCheck)
                            {
                                newPos = secondItem.pos;
                                hasNewPos = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    public Vector2Int MachinePlay(Vector2Int pos)
    {
        if (machineSlots.Count > 0)
        {
            for (int i = 0; i < machineSlots.Count; i++)
            {
                MachinePlay item = machineSlots[i];
                List<Vector2Int> list = item.list;
                Vector2Int nextPos = new();
                bool havingNextPos = false;
                for (int j = 0; j < list.Count; j++)
                {
                    Vector2Int currentPos = list[j];
                    if (slots.ContainsKey(currentPos))
                    {
                        Slot currentSlot = slots[currentPos];
                        if (!currentSlot.GetChoose())
                        {
                            if (!havingNextPos)
                            {
                                nextPos = currentPos;
                                havingNextPos = true;
                            }
                        }
                        else
                        {
                            if (currentSlot.IsPlayerOwner())
                            {
                                havingNextPos = false;
                                break;
                            }
                        }
                    }
                }
                if (havingNextPos)
                {
                    return nextPos;
                }
                else
                {
                    machineSlots.RemoveAt(i);
                    i--;
                }
            }
        }
        else
        {
            Vector2Int top = new(pos.x, pos.y - 1);
            if (slots.ContainsKey(top))
            {
                return top;
            }
            Vector2Int bottom = new(pos.x, pos.y + 1);
            if (slots.ContainsKey(bottom))
            {
                return bottom;
            }
            Vector2Int left = new(pos.x - 1, pos.y);
            if (slots.ContainsKey(left))
            {
                return left;
            }
            Vector2Int right = new(pos.x + 1, pos.y);
            if (slots.ContainsKey(right))
            {
                return right;
            }
        }

        int newPos = Random.Range(0, remainSlots.Count);
        return remainSlots[newPos];
    }
    public void CheckPosition(Vector2Int pos)
    {
        if (endGame)
        {
            return;
        }

        List<SlotCheck> top = new();
        List<SlotCheck> bottom = new();
        List<SlotCheck> left = new();
        List<SlotCheck> right = new();
        List<SlotCheck> crossTopLeft = new();
        List<SlotCheck> crossTopRight = new();
        List<SlotCheck> crossBottomLeft = new();
        List<SlotCheck> crossBottomRight = new();

        SlotCount(top, true, pos, 0, -1, false, false);
        SlotCount(bottom, true, pos, 0, 1, false, false);
        SlotCount(left, true, pos, -1, 0, false, false);
        SlotCount(right, true, pos, 1, 0, false, false);
        SlotCount(crossTopLeft, true, pos, -1, -1, false, false);
        SlotCount(crossTopRight, true, pos, 1, 1, false, false);
        SlotCount(crossBottomLeft, true, pos, -1, 1, false, false);
        SlotCount(crossBottomRight, true, pos, 1, -1, false, false);

        MachineNewList(top, pos, "top");
        MachineNewList(bottom, pos, "bottom");
        MachineNewList(left, pos, "left");
        MachineNewList(right, pos, "right");
        MachineNewList(crossTopLeft, pos, "crossTopLeft");
        MachineNewList(crossTopRight, pos, "crossTopRight");
        MachineNewList(crossBottomLeft, pos, "crossBottomLeft");
        MachineNewList(crossBottomRight, pos, "crossBottomRight");
    }
    public void CheckMachinePlayPosition(Vector2Int pos)
    {
        List<SlotCheck> top = new();
        List<SlotCheck> bottom = new();
        List<SlotCheck> left = new();
        List<SlotCheck> right = new();
        List<SlotCheck> crossTopLeft = new();
        List<SlotCheck> crossTopRight = new();
        List<SlotCheck> crossBottomLeft = new();
        List<SlotCheck> crossBottomRight = new();

        SlotCount(top, true, pos, 0, -1, false, false);
        SlotCount(bottom, true, pos, 0, 1, false, false);
        SlotCount(left, true, pos, -1, 0, false, false);
        SlotCount(right, true, pos, 1, 0, false, false);
        SlotCount(crossTopLeft, true, pos, -1, -1, false, false);
        SlotCount(crossTopRight, true, pos, 1, 1, false, false);
        SlotCount(crossBottomLeft, true, pos, -1, 1, false, false);
        SlotCount(crossBottomRight, true, pos, 1, -1, false, false);

        MachineNewList(top, pos, "top");
        MachineNewList(bottom, pos, "bottom");
        MachineNewList(left, pos, "left");
        MachineNewList(right, pos, "right");
        MachineNewList(crossTopLeft, pos, "crossTopLeft");
        MachineNewList(crossTopRight, pos, "crossTopRight");
        MachineNewList(crossBottomLeft, pos, "crossBottomLeft");
        MachineNewList(crossBottomRight, pos, "crossBottomRight");

        List<SlotCheck> reCheckList = new();
        reCheckList.AddRange(top);
        reCheckList.AddRange(bottom);
        reCheckList.AddRange(left);
        reCheckList.AddRange(right);
        reCheckList.AddRange(crossBottomLeft);
        reCheckList.AddRange(crossBottomRight);
        reCheckList.AddRange(crossTopLeft);
        reCheckList.AddRange(crossTopRight);

        Slot currentSlot = slots[pos];
        currentSlot.ChangeSlotItem(machineSprite, false);
        int count = SlotCheck(currentSlot, false);
        if (count >= checkSlotAmount)
        {
            EndGame(false);
            return;
        }
        for (int i = 0; i < reCheckList.Count; i++)
        {
            SlotCheck current = reCheckList[i];
            Slot tempSlot = slots[current.pos];
            if (!tempSlot.GetChoose())
            {
                reCheckList.Remove(current);
                i--;
            }
        }

        for (int i = 0; i < reCheckList.Count; i++)
        {
            CheckPosition(reCheckList[i].pos);
        }

    }
    public void MachineNewList(List<SlotCheck> nextSlot, Vector2Int root, string side)
    {
        if (endGame)
        {
            return;
        }

        if (nextSlot.Count < checkSlotAmount)
        {
            return;
        }
        List<Vector2Int> list = new();
        int total = 0;
        for (int i = 0; i < nextSlot.Count; i++)
        {
            SlotCheck next = nextSlot[i];
            list.Add(next.pos);
            if (next.isCheck)
            {
                if (slots.ContainsKey(next.pos))
                {
                    Slot pos = slots[next.pos];
                    if (pos.IsMachineOwner())
                    {
                        total++;
                    }
                }
            }
        }
        if (total >= checkSlotAmount)
        {
            EndGame(false);
            winSlot = nextSlot;
            return;
        }

        MachinePlay newItem = new(side, root, list, total);

        MachinePlay existItem = machineSlots.Find((x) => x.root == root && x.side == side);
        if (existItem != null)
        {
            existItem.UpdateValues(list, total);
        }
        else
        {
            machineSlots.Add(newItem);
        }
        machineSlots.Sort((x, y) => y.count.CompareTo(x.count));
    }
    public void EndGame(bool isPlayer)
    {
        if (isPlayer)
        {
            EndGameController.instance.EndGame("Bạn đã thắng!");
        }
        else
        {
            EndGameController.instance.EndGame("Máy đã thắng!");
        }
        for (int i = 0; i < winSlot.Count; i++)
        {
            Vector2Int pos = winSlot[i].pos;
            Slot slot = slots[pos];
            if (slot.GetChoose())
            {
                bool isOwnerPlayer = slot.IsPlayerOwner();
                if (isOwnerPlayer && isPlayer)
                {
                    slot.WinSprite(winSprite);
                }
                else if (!isOwnerPlayer && !isPlayer)
                {
                    slot.WinSprite(winSprite);
                }
            }
        }
    }
}
public class SlotCheck
{
    public Vector2Int pos;
    public bool isCheck;
    public SlotCheck(Vector2Int pos, bool isCheck)
    {
        this.pos = pos;
        this.isCheck = isCheck;
    }
}
public class MachinePlay
{
    public string side = "";
    public Vector2Int root;
    public List<Vector2Int> list = new();
    public int count = 0;
    public MachinePlay(string side, Vector2Int root, List<Vector2Int> list, int count)
    {
        this.root = root;
        this.list = list;
        this.count = count;
        this.side = side;
    }
    public void UpdateValues(List<Vector2Int> list, int count)
    {
        this.list = list;
        this.count = count;
    }
}