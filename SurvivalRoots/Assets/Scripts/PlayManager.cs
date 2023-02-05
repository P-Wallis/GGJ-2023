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

        foreach(CollectableSpot spot in collectables)
        {
            spot.Init(this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (phase == GamePhase.WORLD_UPDATES)
            {
                phase = GamePhase.PLAYER_ACTION;
            }
            else
            {
                phase = (GamePhase)((int)phase + 1);
            }
        }
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
}
