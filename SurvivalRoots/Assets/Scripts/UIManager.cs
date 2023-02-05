using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        instructions.SetActive(instructionsShown);
    }

    bool instructionsShown = true;

    public ResourcesMeter waterMeter;
    public ResourcesMeter mineralMeter;
    public Button endTurnButton;
    public Button resetButton;
    public DayVisualizer dayVisualizer;
    public TextMeshProUGUI powerLevel;
    public UIRiseAndFade waterUpdate;
    public UIRiseAndFade mineralUpdate;
    public UIRiseAndFade noWaterAlert;
    public GameObject instructions;

    private void Update()
    {
        if(instructionsShown && Input.GetMouseButtonDown(0))
        {
            instructionsShown = false;
            instructions.SetActive(instructionsShown);
        }
    }
}
