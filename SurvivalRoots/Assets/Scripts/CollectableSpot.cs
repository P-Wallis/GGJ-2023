using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableSpot : MonoBehaviour
{
    public Collider2D spotCollider;
    public Color color;
    public List<RootSpot> spots = new List<RootSpot>();
    public Transform resourcePrefab;

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
