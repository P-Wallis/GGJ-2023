using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootDraw : MonoBehaviour
{
    public LineRenderer playerLine;
    public RootLine rootLinePrefab;
    private Camera mainCamera;
    public GameObject cursor;
    public Transform[] startNodes;

    [Range(0,10)]public float maxLength = 5;
    [Range(0, 5)] public float startRadius = 1;
    [Range(0, 5)] public float rootRadius = 1;
    [Range(0, 5)] public int maxChildIndex = 2;
    private float currentLength = 0;
    [Range(0.000001f, 0.01f)] public float resamplingSize = 0.001f;
    [Range(0, 0.05f)] public float resamplingNoise = 0.01f;
    private bool isDrawing = false;
    private List<RootLine> roots = new List<RootLine>();
    private RootCollision currentRootStart;


    private void Start()
    {
        mainCamera = Camera.main;
        cursor.transform.localScale = Vector3.one * (startRadius * 2);
    }


    private void Update()
    {
        Vector2 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RootCollision rootStart = GetRootStartPoint(pos);
        bool canDraw = rootStart != null;

        cursor.SetActive(canDraw);
        if (canDraw)
        {
            cursor.transform.localScale = Vector3.one * (rootStart.maxRadius * 2);
            cursor.transform.position = rootStart.point;

            if (Input.GetMouseButtonDown(0))
            {
                currentRootStart = rootStart;
                playerLine.positionCount = 1;
                playerLine.SetPosition(0, rootStart.point);
                currentLength = 0;
                isDrawing = true;
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

                RootLine root = Instantiate(rootLinePrefab, transform);
                root.Init(currentRootStart.parent, positions, maxLength, resamplingSize, resamplingNoise);
                roots.Add(root);

                isDrawing = false;
            }
        }
    }

    RootCollision GetRootStartPoint(Vector2 mousePos)
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

        return null;
    }
    
}
