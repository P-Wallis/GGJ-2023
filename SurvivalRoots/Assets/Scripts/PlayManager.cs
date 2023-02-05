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

    void Start()
    {
        ui = UIManager.instance;
        drawer.Init(this, collectables);
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
}
