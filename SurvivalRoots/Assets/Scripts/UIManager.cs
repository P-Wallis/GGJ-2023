using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        waterMeter.SetValue(0);
        mineralMeter.SetValue(0);
    }

    public ResourcesMeter waterMeter;
    public ResourcesMeter mineralMeter;
    public Button endTurnButton;
    public Button resetButton;
    public DayVisualizer dayVisualizer;
}
