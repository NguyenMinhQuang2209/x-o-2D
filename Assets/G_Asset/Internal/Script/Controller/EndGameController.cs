using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndGameController : MonoBehaviour
{
    public static EndGameController instance;
    public GameObject endgame_ui;
    public TextMeshProUGUI txt;
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
        endgame_ui.SetActive(false);
    }
    public void EndGame(string text)
    {
        txt.text = text;
        endgame_ui.SetActive(true);
    }
    public void RefreshGame()
    {
        SlotController.instance.Refresh();
        endgame_ui.SetActive(false);
    }
    public void Close()
    {
        endgame_ui.SetActive(false);
    }
}
