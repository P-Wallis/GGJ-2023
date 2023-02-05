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
    public SoundManager soundManagerPrefab;
    private SoundManager soundManager;
    public void PlaySFX(SFX fx) { soundManager.PlaySFX(fx); }

    public RootDraw drawer;
    public Tree tree;

    [Header("Object Generation")]
    [Range(0, 10)] public int maxDeathPools;
    public CollectableSpot waterPool, mineralChunk, deathPool;
    public GameObject rock;
    public GameObject[] plants;
    public float radiusIncrement = 2;
    public float objectDensity = 1;
    [Range(0, 1)] public float posjitter = 1;
    public int rings = 5;
    private List<CollectableSpot> collectables = new List<CollectableSpot>();
    private List<CollectableSpot> deathPools = new List<CollectableSpot>();
    private List<Collider2D> obstacles = new List<Collider2D>();

    private GamePhase phase;
    public GamePhase Phase { get { return phase; } }


    [Header("Resources")]
    [Range(0, 4)] public float water;
    [Range(0, 4)] public float minerals;

    [Range(0,0.1f)]public float increment;
    [HideInInspector] public float bankedWaterIncrements = 0;
    [HideInInspector] public float bankedMineralIncrements = 0;
    [Range(0, 4f)] public float waterDailyIncrease = 0.5f;


    [Header("Camera")]
    [Range(0, 15)] public float startZoom = 8;
    [Range(0, 15)] public float endZoom = 12;
    [Range(0, 2)] public float zoomIncrement = 1;
    [Range(0, 10)] public float zoomTime = 1;
    public AnimationCurve zoomCurve;
    private Camera mainCam;
    private float currentZoom;
    private Coroutine zoomAnimation;

    void Start()
    {
        mainCam = Camera.main;
        mainCam.orthographicSize = startZoom;
        currentZoom = startZoom;

        ui = UIManager.instance;
        soundManager = Instantiate(soundManagerPrefab);
        drawer.Init(this, collectables, obstacles);
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
        int deathBudget =  maxDeathPools / (rings/2);
        for (int i=1; i<=rings; i++)
        {
            float radius = radiusIncrement * i;
            float randomOffset = Random.Range(-Mathf.PI, Mathf.PI);
            int currentDeaths = 0;

            float circumference = radius * 2 * Mathf.PI;

            for(int o = 0; o<(circumference/objectDensity)-1; o++)
            {
                float rad = o * objectDensity * 2 * Mathf.PI / circumference;
                rad += randomOffset;
                Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
                pos += Random.insideUnitCircle * posjitter;
                Quaternion randAngle = Quaternion.Euler(0, 0, Random.Range(-180f, 180));

                if (good)
                {
                    if (Random.value < 0.7f)
                    {
                        collectables.Add(Instantiate(waterPool, pos, randAngle, transform));
                        collectables[collectables.Count - 1].startingResources = i;
                    }
                    else
                    {
                        collectables.Add(Instantiate(mineralChunk, pos, Quaternion.identity, transform));
                        collectables[collectables.Count - 1].startingResources = i;
                    }
                }
                else
                {
                    if (currentDeaths < deathBudget && (Mathf.Repeat(rad, Mathf.PI * 2) < Mathf.PI / 4 || (Mathf.Repeat(rad - Mathf.PI, Mathf.PI * 2) < Mathf.PI / 4)))
                    {
                        CollectableSpot pool = Instantiate(deathPool, pos, Quaternion.identity, transform);
                        collectables.Add(pool);
                        deathPools.Add(pool);
                        currentDeaths++;
                    }
                    else
                    {
                        PlaceRandomObstacle(pos, randAngle);
                    }
                }
            }


            good = !good;
        }
    }

    private void PlaceRandomObstacle(Vector2 pos, Quaternion randAngle)
    {
        if (Random.value < 0.3f)
        {
            Instantiate(plants[Random.Range(0, plants.Length)], pos, randAngle, transform);
        }
        else
        {
            GameObject rockGO = Instantiate(rock, pos, randAngle, transform);
            Collider2D rockCollider = rockGO.GetComponent<PolygonCollider2D>();

            if (rockCollider != null)
            {
                obstacles.Add(rockCollider);
            }
        }
    }

    private void EndTurn()
    {
        PlaySFX(SFX.SUCCESS);
        StartCoroutine(DoComputerTurn());
    }

    IEnumerator DoComputerTurn()
    {
        // Growth
        phase = GamePhase.TREE_GROWTH;
        {
            ui.dayVisualizer.AdvanceTimeOfDay();
            soundManager.TransitionMusicTo(MusicTrack.GROWING);

            ui.endTurnButton.interactable = false;
            ui.resetButton.interactable = false;
            yield return new WaitForSeconds(5);
        }

        // World
        phase = GamePhase.WORLD_UPDATES;
        {
            ui.dayVisualizer.AdvanceTimeOfDay();
            UpdateZoom();
            yield return new WaitForSeconds(5);
        }

        // Player
        if (IsSuccess())
        {
            // End screen
            PlaySFX(SFX.SUCCESS);
            tree.Fruit();
            Debug.Log("The End");
        }
        else if(IsFailure())
        {
            PlaySFX(SFX.BREAK);
            tree.Chop();
            Debug.Log("You Lose");
        }
        else
        {
            phase = GamePhase.PLAYER_ACTION;

            ui.dayVisualizer.AdvanceTimeOfDay();
            soundManager.TransitionMusicTo(MusicTrack.BACKGROUND);
            PlaySFX(SFX.SUNRISE);

            //if (water < 4)
            //{
            //    water += waterDailyIncrease;
            //    ui.waterMeter.SetValue(water);
            //    ui.waterUpdate.Increase();
            //}

            if (drawer.maxChildIndex < 5)
            {
                drawer.maxChildIndex++;
                ui.powerLevel.text = (drawer.maxChildIndex + 1).ToString();
            }

            ui.endTurnButton.interactable = true;
            ui.resetButton.interactable = true;


            water = Mathf.Clamp(water, 0, 4);
            minerals = Mathf.Clamp(minerals, 0, 4);
            bankedWaterIncrements = 0;
            bankedMineralIncrements = 0;
        }

    }

    public bool IsSuccess()
    {
        for(int i=0; i<deathPools.Count; i++)
        {
            if (deathPools[i].Resources > 0)
                return false;
        }

        return true;
    }

    public bool IsFailure()
    {
        return (water < 1 && drawer.PendingRootNum <= 0);
    }

    public void OnNotEnoughWater()
    {
        ui.noWaterAlert.Animate();
    }

    public void Increment(ResourceType type)
    {
        switch(type)
        {
            case ResourceType.WATER:
                water += increment;
                ui.waterUpdate.Increase();
                bankedWaterIncrements -= increment;
                break;
            case ResourceType.MINERAL:
                minerals += increment;
                ui.mineralUpdate.Increase();
                bankedMineralIncrements -= increment;
                break;
            case ResourceType.SKULL:
                minerals -= increment;
                ui.mineralUpdate.Decrease();
                bankedMineralIncrements += increment;
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
                ui.waterUpdate.Decrease();
                water -= 1;
                break;
            case ResourceType.MINERAL:
                ui.mineralUpdate.Decrease();
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
                ui.waterUpdate.Increase();
                water += 1;
                break;
            case ResourceType.MINERAL:
                ui.mineralUpdate.Increase();
                minerals += 1;
                break;
        }
        ui.waterMeter.SetValue(water);
        ui.mineralMeter.SetValue(minerals);
    }

    private void UpdateZoom()
    {
        if(zoomAnimation != null)
        {
            StopCoroutine(zoomAnimation);
        }
        zoomAnimation = StartCoroutine(CoZoom());
    }

    IEnumerator CoZoom()
    {
        float percent = 0, dt = 1 / zoomTime;
        float start = currentZoom;
        float end = Mathf.Clamp(currentZoom + zoomIncrement, startZoom, endZoom);
        currentZoom = end;
        while (percent < 1)
        {
            mainCam.orthographicSize = Mathf.Lerp(start, end, zoomCurve.Evaluate(percent));

            percent += Time.deltaTime * dt;
            yield return null;
        }
    }
}
