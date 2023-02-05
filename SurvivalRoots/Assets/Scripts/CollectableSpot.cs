using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    NONE,
    WATER,
    MINERAL,
    SWAMP
}

public class CollectableSpot : MonoBehaviour
{
    public Collider2D spotCollider;
    public Color color;
    public List<RootSpot> spots = new List<RootSpot>();
    public Transform resourcePrefab;
    public ResourceType type;

    public bool CollidesWith(Vector2 point)
    {
        return spotCollider.OverlapPoint(point);
    }

    public void SetRootSpot(Vector2 spot, int spotIndex, RootLine root)
    {
        int newSpot = spots.Count;
        spots.Add(new RootSpot(spot, spotIndex, root));
        root.onGrowingComplete += () => { StartCollecting(newSpot); };
    }

    void StartCollecting(int index)
    {
        if (spots[index].collecting != null)
            StopCoroutine(spots[index].collecting);
        spots[index].collecting = StartCoroutine(Collect(index));
    }

    IEnumerator Collect(int index)
    {
        yield return new WaitForSeconds(Random.Range(0, 1f));
        Transform resource = Instantiate(resourcePrefab, transform);

        while(true)
        {
            yield return StartCoroutine(spots[index].root.AnimateOnPathToTree(resource, spots[index].spotIndex));

            if(type == ResourceType.WATER)
            {
                UIManager.instance.waterMeter.IncrementValue(0.1f);
            }
            else if(type == ResourceType.MINERAL)
            {
                UIManager.instance.mineralMeter.IncrementValue(0.1f);
            }
        }
    }

    [System.Serializable]
    public class RootSpot
    {
        public Vector2 spot;
        public int spotIndex;
        public RootLine root;
        public Coroutine collecting;

        public RootSpot(Vector2 spot, int spotIndex, RootLine root)
        {
            this.spot = spot;
            this.spotIndex = spotIndex;
            this.root = root;
        }
    }
}
