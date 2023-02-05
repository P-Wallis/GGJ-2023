using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public Sprite fruit;
    public SpriteRenderer[] leaves;

    public void Chop()
    {
        for(int i=0; i< leaves.Length; i++)
        {
            leaves[i].gameObject.SetActive(false);
        }
    }

    public void Fruit()
    {
        for (int i = 1; i < leaves.Length; i++)
        {
            leaves[i].sprite = fruit;
        }
    }
}
