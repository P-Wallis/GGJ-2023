using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    NONE,
    WATER,
    MINERAL,
    SKULL
}

public class CollectableSpot : MonoBehaviour
{
    public Collider2D spotCollider;
    public Transform resourcePrefab;
    public ResourceType type;
    public float startingResources = 4;
    float resources;
    public float Resources { get { return resources; } }
    private SpriteRenderer sr;
    private AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1, 2, -2));

    List<RootSpot> spots = new List<RootSpot>();
    private List<Transform> resourcePool = new List<Transform>();
    PlayManager manager;

    public void Init(PlayManager manager)
    {
        this.manager = manager;
        resources = startingResources;
        sr = spotCollider.GetComponent<SpriteRenderer>();
    }

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

    public void SendResources()
    {
        for(int i=0; i<spots.Count; i++)
        {
            if(spots[i].waitedForGrow)
            {
                StartCollecting(i);
            }
        }
    }

    bool MeterFull()
    {
        switch(type)
        {
            case ResourceType.WATER:
                return manager.water + manager.bankedWaterIncrements >= 4;

            case ResourceType.MINERAL:
                return manager.minerals + manager.bankedMineralIncrements >= 4;

            case ResourceType.SKULL:
                return manager.minerals + manager.bankedMineralIncrements <= 0;
        }

        return false;
    }

    void BankIncrement()
    {
        switch (type)
        {
            case ResourceType.WATER:
                manager.bankedWaterIncrements += manager.increment;
                break;

            case ResourceType.MINERAL:
                manager.bankedMineralIncrements += manager.increment;
                break;

            case ResourceType.SKULL:
                manager.bankedMineralIncrements -= manager.increment;
                break;
        }
    }

    void StartCollecting(int index)
    {
        manager.PlaySFX(SFX.GATHERING);
        spots[index].waitedForGrow = true;

        if(resources <= 0 || MeterFull())
        {
            return;
        }

        BankIncrement();

        if (spots[index].collecting != null)
            StopCoroutine(spots[index].collecting);
        spots[index].collecting = StartCoroutine(Collect(index));
    }

    IEnumerator Collect(int index)
    {
        yield return new WaitForSeconds(Random.Range(0, 1f));
        Transform resource;
        if (resourcePool.Count > 0)
        {
            resource = resourcePool[resourcePool.Count - 1];
            resourcePool.RemoveAt(resourcePool.Count - 1);
            resource.gameObject.SetActive(true);
        }
        else
        {
            resource = Instantiate(resourcePrefab, manager.transform);
        }

        resources -= manager.increment;
        sr.color = new Color(1, 1, 1, fadeCurve.Evaluate(resources / startingResources));

        yield return StartCoroutine(spots[index].root.AnimateOnPathToTree(resource, spots[index].spotIndex));

        manager.Increment(type);

        resource.gameObject.SetActive(false);
        resourcePool.Add(resource);
    }

    [System.Serializable]
    public class RootSpot
    {
        public Vector2 spot;
        public int spotIndex;
        public RootLine root;
        public Coroutine collecting;
        public bool waitedForGrow;

        public RootSpot(Vector2 spot, int spotIndex, RootLine root)
        {
            this.spot = spot;
            this.spotIndex = spotIndex;
            this.root = root;
            waitedForGrow = false;
        }
    }
}
