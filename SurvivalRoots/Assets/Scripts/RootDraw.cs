using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootDraw : MonoBehaviour
{
    public LineRenderer playerLine;
    public LineRenderer rootLine;
    private Camera mainCamera;

    [Range(0,10)]public float maxLength = 5;
    private float currentLength = 0;
    [Range(0.000001f, 0.01f)] public float resamplingSize = 0.001f;
    [Range(0, 0.05f)] public float resamplingNoise = 0.01f;


    private void Start()
    {
        mainCamera = Camera.main;
    }


    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            playerLine.positionCount = 0;
            currentLength = 0;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            float delta = playerLine.positionCount > 1 ? ((Vector2)playerLine.GetPosition(playerLine.positionCount - 1) - pos).magnitude : 0;

            if (currentLength + delta <= maxLength)
            {
                currentLength += delta;
                playerLine.positionCount++;
                playerLine.SetPosition(playerLine.positionCount - 1, pos);
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            if(rootDrawAnimation != null)
            {
                StopCoroutine(rootDrawAnimation);
            }
            rootDrawAnimation = StartCoroutine(DrawRoot());
        }
    }

    Vector3[] GetSmoothPoints(Vector3[] points)
    {
        if(points == null || points.Length<2)
        {
            return new Vector3[0];
        }

        // Resample points
        List<Vector3> resampled = new List<Vector3>();
        float step = resamplingSize * maxLength;
        float remainingDistance = 0;
        float delta = 0;
        float totalDelta = 0;
        resampled.Add(points[0]);
        for(int i=1; i<points.Length; i++)
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
        resampled.Add(points[points.Length - 1]);

        // Add a touch of noise
        float noise = resamplingNoise * maxLength;
        for (int i = 1; i < resampled.Count-1; i++)
        {
            resampled[i] += Random.insideUnitSphere * noise;
        }

        // Smooth samples
        List<Vector3> smoothed = new List<Vector3>();
        smoothed.Add(points[0]);
        Vector3[] sampler = new Vector3[5];
        for(int i=0; i<resampled.Count; i++)
        {
            sampler[0] = i > 1 ? resampled[i - 2] : (i > 0 ? resampled[i - 1] : resampled[i]);
            sampler[1] = i > 0 ? resampled[i - 1] : resampled[i];
            sampler[2] = resampled[i];
            sampler[3] = i < resampled.Count - 1 ? resampled[i + 1] : resampled[i];
            sampler[4] = i < resampled.Count - 2 ? resampled[i + 2] : (i < resampled.Count - 1 ? resampled[i + 1] : resampled[i]);

            smoothed.Add(
                (sampler[2] * 0.383f) +
                ((sampler[1] + sampler[3]) * 0.2417f) +
                ((sampler[0] + sampler[4]) * 0.0668f)
                );
        }
        smoothed.Add(resampled[resampled.Count - 1]);

        return smoothed.ToArray();
    }

    Coroutine rootDrawAnimation;
    IEnumerator DrawRoot()
    {
        Vector3[] positions = new Vector3[playerLine.positionCount];
        playerLine.GetPositions(positions);
        positions = GetSmoothPoints(positions);
        rootLine.positionCount = positions.Length;
        rootLine.SetPositions(positions);

        float percent = 0;

        while (percent < 1)
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(percent*0.75f, 1), new Keyframe(percent, 0, -10, .5f));
            rootLine.widthCurve = curve;
            percent += Time.deltaTime;
            yield return null;
        }
    }
}
