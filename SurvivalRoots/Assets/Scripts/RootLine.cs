using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootLine : MonoBehaviour
{
    public LineRenderer rootLine;
    float resamplingSize = 0.001f;
    float resamplingNoise = 0.01f;
    float maxLength = 5;
    Vector3[] points;
    Bounds bounds;
    RootLine parent;

    public void Init(RootLine parent, Vector3[] playerPoints, float maxLength, float resamplingSize, float resamplingNoise)
    {
        this.parent = parent;
        this.maxLength = maxLength;
        this.resamplingSize = resamplingSize;
        this.resamplingNoise = resamplingNoise;

        List<Vector3> listPoints = new List<Vector3>();
        listPoints.AddRange(playerPoints);
        points = GetRootPoints(listPoints);
        CalculateBounds();

        if(parent != null)
        {
            rootLine.widthMultiplier = parent.rootLine.widthMultiplier / 2f;
        }

        if (rootDrawAnimation != null)
            StopCoroutine(rootDrawAnimation);
        rootDrawAnimation = StartCoroutine(DrawRoot());
    }

    public RootCollision GetCollision(Vector2 point, float radius)
    {
        if(!bounds.Contains(point) && Vector2.Distance((Vector2)bounds.ClosestPoint(point), point) > radius)
        {
            return null;
        }

        Vector2 minNode = Vector2.zero;
        float minDist = Mathf.Infinity;
        float dist;
        for (int i = 0; i < points.Length; i++)
        {
            dist = Vector2.Distance(point, points[i]);
            if (dist < minDist)
            {
                minDist = dist;
                minNode = points[i];
            }
        }

        if(minDist>radius)
        {
            return null;
        }

        RootCollision collision = new RootCollision
        {
            parent = this,
            radius = minDist,
            point = minNode,
            maxRadius = radius
        };

        return collision;
    }

    void CalculateBounds()
    {
        bounds = new Bounds(points[0], new Vector3(0.001f, 0.001f, 1f));
        foreach(Vector3 point in points)
        {
            bounds.Encapsulate(point);
        }    
    }

    Vector3[] GetRootPoints(List<Vector3> points)
    {
        if (points == null || points.Count < 2)
        {
            return new Vector3[0];
        }

        // Resample points
        float step = resamplingSize * maxLength;
        List<Vector3> resampled = ResamplePoints(points, step);

        // Add a touch of noise
        float noise = resamplingNoise * maxLength;
        for (int i = 1; i < resampled.Count - 1; i++)
        {
            resampled[i] += (Vector3)(Random.insideUnitCircle * noise);
        }

        // Smooth samples
        List<Vector3> smoothed = SmoothPoints(SmoothPoints(resampled));

        return smoothed.ToArray();
    }

    private static List<Vector3> ResamplePoints(List<Vector3> points, float step)
    {
        List<Vector3> resampled = new List<Vector3>();
        float remainingDistance = 0;
        float delta;
        float totalDelta;
        resampled.Add(points[0]);
        for (int i = 1; i < points.Count; i++)
        {
            totalDelta = (points[i - 1] - points[i]).magnitude;
            delta = totalDelta;

            while (delta > step - remainingDistance)
            {
                delta -= step;
                remainingDistance = 0;
                resampled.Add(Vector3.Lerp(points[i - 1], points[i], 1 - (delta / totalDelta)));
            }

            remainingDistance += delta;
        }
        resampled.Add(points[points.Count - 1]);
        return resampled;
    }

    private static List<Vector3> SmoothPoints(List<Vector3> points)
    {
        List<Vector3> smoothed = new List<Vector3>();
        smoothed.Add(points[0]);
        Vector3[] sampler = new Vector3[5];
        for (int i = 0; i < points.Count; i++)
        {
            sampler[0] = i > 1 ? points[i - 2] : (i > 0 ? points[i - 1] : points[i]);
            sampler[1] = i > 0 ? points[i - 1] : points[i];
            sampler[2] = points[i];
            sampler[3] = i < points.Count - 1 ? points[i + 1] : points[i];
            sampler[4] = i < points.Count - 2 ? points[i + 2] : (i < points.Count - 1 ? points[i + 1] : points[i]);

            smoothed.Add(
                (sampler[2] * 0.383f) +
                ((sampler[1] + sampler[3]) * 0.2417f) +
                ((sampler[0] + sampler[4]) * 0.0668f)
                );
        }
        smoothed.Add(points[points.Count - 1]);
        return smoothed;
    }

    Coroutine rootDrawAnimation;
    IEnumerator DrawRoot()
    {
        rootLine.positionCount = points.Length;
        rootLine.SetPositions(points);

        float percent = 0;

        while (percent < 1)
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(percent * 0.75f, 1), new Keyframe(percent, 0, -10, .5f));
            rootLine.widthCurve = curve;
            percent += Time.deltaTime;
            yield return null;
        }
    }
}
