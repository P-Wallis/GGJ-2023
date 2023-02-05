using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TimeOfDay
{
    DAWN,
    NOON,
    DUSK
}

public class DayVisualizer : MonoBehaviour
{
    public TimeOfDay time = TimeOfDay.DAWN;

    public Sprite dawn, noon, dusk;
    public Image sunDial;

    public TextMeshProUGUI dayText;
    private int day = 1;

    public void AdvanceTimeOfDay()
    {
        if(time == TimeOfDay.DUSK)
        {
            if(day == 30)
            {
                day = 1;
            }
            else
            {
                day += 1;
            }
            time = TimeOfDay.DAWN;
        }
        else
        {
            time = (TimeOfDay)((int)time + 1);
        }

        SetTime(time);
    }

    public void SetTime(TimeOfDay time)
    {
        this.time = time;

        switch(time)
        {
            case TimeOfDay.DAWN:
                dayText.text = day.ToString();
                sunDial.sprite = dawn;
                break;
            case TimeOfDay.NOON:
                sunDial.sprite = noon;
                break;
            case TimeOfDay.DUSK:
                sunDial.sprite = dusk;
                break;
        }
    }
}
