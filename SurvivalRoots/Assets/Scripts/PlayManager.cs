using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    PLAYER_ACTION,
    TREE_GROWTH,
    WORLD_UPDATES
}

public class PlayManager : MonoBehaviour
{
    UIManager ui;
    public RootDraw drawer;
    public CollectableSpot waterPool, mineralChunk, deathPool;
    public GameObject rock;
    public GameObject[] plants;
    public float radiusIncrement = 2;
    public float objectDensity = 1;
    [Range(0, 1)] public float posjitter = 1;
    public int rings = 5;
    private List<CollectableSpot> collectables = new List<CollectableSpot>();

    private GamePhase phase;
    public GamePhase Phase { get { return phase; } }

    [Range(0, 4)] public float water;
    [Range(0, 4)] public float minerals;

    [Range(0,0.1f)]public float increment;

    void Start()
    {
        ui = UIManager.instance;
        drawer.Init(this, collectables);
        ui.waterMeter.SetValue(water);
        ui.mineralMeter.SetValue(minerals);

        ui.dayVisualizer.SetTime(TimeOfDay.DAWN);
        drawer.maxChildIndex = 0;
        ui.powerLevel.text = (drawer.maxChildIndex + 1).ToString();

        ui.endTurnButton.onClick.AddListener(EndTurn);
        ui.resetButton.onClick.AddListener(drawer.Reset);

        DistributeStuff();

        foreach (CollectableSpot spot in collectables)
        {
            spot.Init(this);
        }
    }

    public void DistributeStuff()
    {
        bool good = true;
        for(int i=1; i<=rings; i++)
        {
            float radius = radiusIncrement * i;
            float randomOffset = Random.Range(-Mathf.PI, Mathf.PI);

            float circumference = radius * 2 * Mathf.PI;

            for(int o = 0; o<(circumference/objectDensity)-1; o++)
            {
                float rad = o * objectDensity * 2 * Mathf.PI / circumference;
                rad += randomOffset;
                Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
                pos += Random.insideUnitCircle * posjitter;

                if (Random.value < 0.7f)
                {
                    if(good)
                    {
                        collectables.Add(Instantiate(waterPool, pos, Quaternion.identity, transform));
                        collectables[collectables.Count - 1].startingResources = i;
                    }
                    else
                    {
                        Instantiate(rock, pos, Quaternion.identity, transform);
                    }
                }
                else
                {
                    collectables.Add(Instantiate(good ? mineralChunk : deathPool, pos, Quaternion.identity, transform));
                    collectables[collectables.Count - 1].startingResources = i;
                }
            }

            good = !good;
        }
    }

    private void EndTurn()
    {
        StartCoroutine(DoComputerTurn());
    }

    IEnumerator DoComputerTurn()
    {
        phase = GamePhase.TREE_GROWTH;
        ui.endTurnButton.interactable = false;
        ui.resetButton.interactable = false;
        yield return new WaitForSeconds(5);
        phase = GamePhase.WORLD_UPDATES;
        yield return new WaitForSeconds(1);
        phase = GamePhase.PLAYER_ACTION;

        bool dayRollover = ui.dayVisualizer.AdvanceTimeOfDay();
        if(dayRollover)
        {
            if (drawer.maxChildIndex < 5 && minerals >= 1)
            {
                SpendResources(ResourceType.MINERAL);
                drawer.maxChildIndex++;
                ui.powerLevel.text = (drawer.maxChildIndex + 1).ToString();
            }

            if (water < 4 && minerals >= 1)
            {
                SpendResources(ResourceType.MINERAL);
                RefundResources(ResourceType.WATER);
            }
        }

        ui.endTurnButton.interactable = true;
        ui.resetButton.interactable = true;

    }

    public void Increment(ResourceType type)
    {
        switch(type)
        {
            case ResourceType.WATER:
                water += increment;
                break;
            case ResourceType.MINERAL:
                minerals += increment;
                break;
            case ResourceType.SKULL:
                water -= increment;
                minerals -= increment;
                break;
        }
        ui.waterMeter.SetValue(water);
        ui.mineralMeter.SetValue(minerals);
    }

    public void SpendResources(ResourceType type = ResourceType.WATER)
    {
        switch (type)
        {
            case ResourceType.WATER:
                water -= 1;
                break;
            case ResourceType.MINERAL:
                minerals -= 1;
                break;
        }
        ui.waterMeter.SetValue(water);
        ui.mineralMeter.SetValue(minerals);
    }

    public void RefundResources(ResourceType type = ResourceType.WATER)
    {
        switch (type)
        {
            case ResourceType.WATER:
                water += 1;
                break;
            case ResourceType.MINERAL:
                minerals += 1;
                break;
        }
        ui.waterMeter.SetValue(water);
        ui.mineralMeter.SetValue(minerals);
    }
}
