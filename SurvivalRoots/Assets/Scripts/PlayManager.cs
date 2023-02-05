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
    public List<CollectableSpot> collectables;

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

        ui.endTurnButton.onClick.AddListener(EndTurn);
        ui.resetButton.onClick.AddListener(drawer.Reset);

        foreach(CollectableSpot spot in collectables)
        {
            spot.Init(this);
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
