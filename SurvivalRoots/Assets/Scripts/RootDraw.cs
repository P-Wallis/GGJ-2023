using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootDraw : MonoBehaviour
{
    public LineRenderer playerLinePrefab;
    public RootLine rootLinePrefab;
    private Camera mainCamera;
    public GameObject cursor;
    public Transform[] startNodes;

    private PlayManager manager;
    private List<CollectableSpot> collectables;

    [Range(0, 1)] public float rootStartWidth = 0.2f;
    [Range(0,10)]public float maxLength = 5;
    [Range(0, 5)] public float startRadius = 1;
    [Range(0, 5)] public float rootRadius = 1;
    [HideInInspector] public int maxChildIndex = 0;
    private float currentLength = 0;
    [Range(0.000001f, 0.01f)] public float resamplingSize = 0.001f;
    [Range(0, 0.05f)] public float resamplingNoise = 0.01f;
    private bool isDrawing = false;
    private bool startedGrowing = false;
    private List<RootLine> roots = new List<RootLine>();
    private List<PendingRoot> pendingRoots = new List<PendingRoot>();
    LineRenderer playerLine;
    private RootCollision currentRootStart;

    private class PendingRoot
    {
        public LineRenderer line;
        public Bounds bounds;
        public int childLevel;

        public PendingRoot(LineRenderer line, int childLevel = 0)
        {
            this.line = line;
            this.childLevel = childLevel;
        }
    }


    public void Init(PlayManager manager, List<CollectableSpot> collectables)
    {
        this.manager = manager;
        this.collectables = collectables;

        mainCamera = Camera.main;
        rootLinePrefab.rootLine.widthMultiplier = rootStartWidth;
        cursor.transform.localScale = Vector3.one * (startRadius * 2);
    }


    void CreatePlayerLine(Vector2 startPoint, int childLevel)
    {
        playerLine = Instantiate(playerLinePrefab, transform);
        playerLine.widthMultiplier = 1.5f * rootStartWidth;
        playerLine.positionCount = 1;
        playerLine.SetPosition(0, startPoint);
        pendingRoots.Add(new PendingRoot(playerLine, childLevel));
    }

    public void Reset()
    {
        isDrawing = false;
        for (int i = 0; i < pendingRoots.Count; i++)
        {
            if (manager.Phase == GamePhase.PLAYER_ACTION)
            {
                manager.RefundResources();
            }
            Destroy(pendingRoots[i].line.gameObject);
        }
        pendingRoots.Clear();
    }

    private void Update()
    {
        switch(manager.Phase)
        {
            case GamePhase.PLAYER_ACTION:

                Vector2 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                if (!isDrawing)
                {
                    RootCollision rootStart = GetRootStartPoint(pos, true);
                    bool canDraw = rootStart != null;

                    if(!cursor.activeSelf && canDraw)
                    {
                        manager.PlaySFX(SFX.HOVER);
                    }
                    cursor.SetActive(canDraw);
                    if (canDraw)
                    {
                        cursor.transform.localScale = Vector3.one * (rootStart.maxRadius * 2);
                        cursor.transform.position = rootStart.point;

                        if (Input.GetMouseButtonDown(0))
                        {
                            if (manager.water < 1)
                            {
                                manager.PlaySFX(SFX.ERROR);
                                manager.OnNotEnoughWater();
                            }
                            else
                            {
                                manager.PlaySFX(SFX.CLICK);
                                manager.SpendResources();
                                CreatePlayerLine(rootStart.point, rootStart.pointIndex < 0 ? (-rootStart.pointIndex) + 1 : rootStart.parent == null ? 1 : rootStart.parent.ChildLevel + 1);
                                currentRootStart = rootStart;
                                currentLength = 0;
                                isDrawing = true;
                                cursor.SetActive(false);
                            }

                        }
                    }
                }

                if (isDrawing)
                {

                    if (Input.GetMouseButton(0))
                    {
                        float delta = playerLine.positionCount > 1 ? ((Vector2)playerLine.GetPosition(playerLine.positionCount - 1) - pos).magnitude : 0;

                        if (currentLength + delta <= maxLength)
                        {
                            currentLength += delta;
                            playerLine.positionCount++;
                            playerLine.SetPosition(playerLine.positionCount - 1, pos);
                        }
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        Vector3[] positions = new Vector3[playerLine.positionCount];
                        playerLine.GetPositions(positions);
                        List<Vector3> list = new List<Vector3>();
                        list.AddRange(positions);
                        positions = RootLine.ResamplePoints(list, resamplingSize * maxLength).ToArray();;

                        playerLine.positionCount = positions.Length;
                        playerLine.SetPositions(positions);

                        pendingRoots[pendingRoots.Count - 1].bounds = RootLine.CalculateBounds(positions);

                        isDrawing = false;
                    }
                }

                break;


            case GamePhase.TREE_GROWTH:
                if(!startedGrowing)
                {
                    if(pendingRoots.Count>0)
                    {
                        manager.PlaySFX(SFX.GROWING);
                    }

                    for(int i=0; i<pendingRoots.Count; i++)
                    {
                        Vector3[] positions = new Vector3[pendingRoots[i].line.positionCount];
                        pendingRoots[i].line.GetPositions(positions);

                        currentRootStart = GetRootStartPoint(positions[0]);

                        RootLine root = Instantiate(rootLinePrefab, transform);
                        root.Init(currentRootStart.parent, positions, collectables, maxLength, resamplingSize, resamplingNoise, rootStartWidth);
                        roots.Add(root);
                    }

                    foreach (CollectableSpot spot in collectables)
                    {
                        spot.SendResources();
                    }

                    Reset();
                    startedGrowing = true;
                }
                break;


            case GamePhase.WORLD_UPDATES:
                startedGrowing = false;
                
                break;
        }
    }

    RootCollision GetRootStartPoint(Vector2 mousePos, bool usePending = false)
    {
        RootCollision collision;
        for (int i=0; i<roots.Count; i++)
        {
            collision = roots[i].GetCollision(mousePos, rootRadius);
            if(collision != null && collision.parent.ChildLevel < maxChildIndex)
            {
                return collision;
            }
        }

        Vector2 minNode = Vector2.zero;
        float minDist = Mathf.Infinity;
        float dist;
        for (int i = 0; i < startNodes.Length; i++)
        {
            dist = Vector2.Distance(mousePos, startNodes[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                minNode = startNodes[i].position;
            }
        }
        if(minDist < startRadius)
        {
            collision = new RootCollision
            {
                parent = null,
                radius = minDist,
                point = minNode,
                maxRadius = startRadius
            };

            return collision;
        }

        if(usePending)
        {
            for (int p = 0; p < pendingRoots.Count; p++)
            {
                if (!pendingRoots[p].bounds.Contains(mousePos) && Vector2.Distance((Vector2)pendingRoots[p].bounds.ClosestPoint(mousePos), mousePos) > rootRadius)
                {
                    continue;
                }

                int childLevel = 0;
                for (int i = 0; i < pendingRoots[p].line.positionCount; i++)
                {
                    dist = Vector2.Distance(mousePos, pendingRoots[p].line.GetPosition(i));
                    if (dist < minDist && pendingRoots[p].childLevel < maxChildIndex)
                    {
                        minDist = dist;
                        minNode = pendingRoots[p].line.GetPosition(i);
                        childLevel = pendingRoots[p].childLevel;
                    }
                }

                if (minDist > rootRadius)
                {
                    continue;
                }

                collision = new RootCollision
                {
                    parent = null,
                    radius = minDist,
                    point = minNode,
                    pointIndex = -childLevel,
                    maxRadius = rootRadius
                };

                return collision;
            }
        }

        return null;
    }
    
}
