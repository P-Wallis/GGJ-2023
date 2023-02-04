using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableSpot : MonoBehaviour
{
    public Collider2D spotCollider;
    public Color color;

    bool CollidesWith(Vector2 point)
    {
        return spotCollider.OverlapPoint(point);
    }
}
