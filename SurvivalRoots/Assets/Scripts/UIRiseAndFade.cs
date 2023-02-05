using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIRiseAndFade : MonoBehaviour
{
    RectTransform rt;
    Vector2 startPos;
    Vector2 endPos;
    Color startColor, endColor;

    public AnimationCurve moveCurve, alphaCurve;
    public float riseDistance;
    [Range(0.001f, 10)]public float time = 1;
    public Image image;
    public TextMeshProUGUI text;

    public Sprite plus, minus;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;
        endPos = new Vector2(0, riseDistance) + startPos;
        if (image != null)
        {
            startColor = image.color;
            endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
            image.color = endColor;
        }
        if (text != null)
        {
            startColor = text.color;
            endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
            text.color = endColor;
        }
    }

    public void Increase()
    {
        if(image != null)
        {
            image.sprite = plus;
        }
        Animate();
    }
    public void Decrease()
    {
        if (image != null)
        {
            image.sprite = minus;
        }
        Animate();
    }

    public void Animate()
    {
        if(rise != null)
        {
            StopCoroutine(rise);
        }
        rise = StartCoroutine(CoRise());
    }

    Coroutine rise;
    IEnumerator CoRise()
    {
        float percent = 0, dt = 1/time;
        while (percent < 1)
        {
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, moveCurve.Evaluate(percent));

            if(image != null)
            {
                image.color = Color.Lerp(startColor, endColor, alphaCurve.Evaluate(percent));
            }
            if (text != null)
            {
                text.color = Color.Lerp(startColor, endColor, alphaCurve.Evaluate(percent));
            }

            percent += Time.deltaTime * dt;
            yield return null;
        }
    }
}
