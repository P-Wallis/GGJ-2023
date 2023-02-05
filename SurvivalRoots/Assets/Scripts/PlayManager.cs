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

    [Header("Object Generation")]
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


    [Header("Resources")]
    [Range(0, 4)] public float water;
    [Range(0, 4)] public float minerals;

    [Range(0,0.1f)]public float increment;
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
                Quaternion randAngle = Quaternion.Euler(0, 0, Random.Range(-180f, 180));

                if (Random.value < 0.7f)
                {
                    if(good)
                    {
                        collectables.Add(Instantiate(waterPool, pos, randAngle, transform));
                        collectables[collectables.Count - 1].startingResources = i;
                    }
                    else
                    {
                        if (Random.value < 0.7f)
                        {
                            Instantiate(plants[Random.Range(0,plants.Length)], pos, randAngle, transform);
                        }
                        else
                        {
                            Instantiate(rock, pos, randAngle, transform);
                        }
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
        phase = GamePhase.PLAYER_ACTION;
        {
            ui.dayVisualizer.AdvanceTimeOfDay();
            soundManager.TransitionMusicTo(MusicTrack.BACKGROUND);
            PlaySFX(SFX.SUNRISE);

            water += waterDailyIncrease;
            ui.waterMeter.SetValue(water);
            ui.waterUpdate.Increase();

            if (drawer.maxChildIndex < 5 && minerals >= 2)
            {
                SpendResources(ResourceType.MINERAL);
                SpendResources(ResourceType.MINERAL);
                drawer.maxChildIndex++;
                ui.powerLevel.text = (drawer.maxChildIndex + 1).ToString();
            }

            ui.endTurnButton.interactable = true;
            ui.resetButton.interactable = true;
        }

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
                break;
            case ResourceType.MINERAL:
                minerals += increment;
                ui.mineralUpdate.Increase();
                break;
            case ResourceType.SKULL:
                water -= increment;
                minerals -= increment;
                ui.waterUpdate.Decrease();
                ui.mineralUpdate.Decrease();
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
