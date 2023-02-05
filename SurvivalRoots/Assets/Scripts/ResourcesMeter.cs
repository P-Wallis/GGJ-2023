using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesMeter : MonoBehaviour
{
    public Sprite full, mid, empty;
    public Image[] images;
    private float value;

    public void SetValue(float inValue)
    {
        value = inValue;
        for(int i=0; i<images.Length; i++)
        {
            if(i <= value-1)
            {
                images[i].sprite = full;
            }
            else if(i >= value)
            {
                images[i].sprite = empty;
            }
            else
            {
                images[i].sprite = mid;
            }
        }
    }

    public void IncrementValue(float increment)
    {
        SetValue(value + increment);
    }
}
