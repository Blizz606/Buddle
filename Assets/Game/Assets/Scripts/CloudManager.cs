
using UnityEngine;

public class UICloudManager : MonoBehaviour
{
    [Header("Clouds")]
    [SerializeField] private RectTransform[] clouds;

    [Header("Spawn Points")]
    [SerializeField] private RectTransform spawnPoint1;
    [SerializeField] private RectTransform spawnPoint2;

    [Header("Movement")]
    [SerializeField] private float minSpeed = 35f;
    [SerializeField] private float maxSpeed = 75f;

    [Header("Spacing")]
    [SerializeField] private float minSpacing = 60f;
    [SerializeField] private int maxSameLane = 2;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnDelay = 2f;
    [SerializeField] private float maxSpawnDelay = 5f;

    private float[] speeds;
    private bool[] active;
    private int[] order;

    private float nextSpawnTime;
    private int lastLane = -1;
    private int laneStreak;

    private Camera uiCamera;
    private readonly Vector3[] corners = new Vector3[4];

    private void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null &&
            canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.rootCanvas.worldCamera;

            if (uiCamera == null)
                uiCamera = Camera.main;
        }

        speeds = new float[clouds.Length];
        active = new bool[clouds.Length];
        order = new int[clouds.Length];

        foreach (RectTransform cloud in clouds)
        {
            if (cloud != null)
                cloud.gameObject.SetActive(false);
        }

        nextSpawnTime = Time.time + 0.5f;
    }

    private void Update()
    {
        if (spawnPoint1 == null || spawnPoint2 == null)
            return;

        MoveClouds();

        if (Time.time >= nextSpawnTime)
            TrySpawn();
    }

    private void MoveClouds()
    {
        int count = 0;

        for (int i = 0; i < clouds.Length; i++)
        {
            if (active[i] && clouds[i] != null)
                order[count++] = i;
        }

        // Move clouds from right to left to prevent collisions.
        for (int i = 1; i < count; i++)
        {
            int current = order[i];
            float x = GetBounds(clouds[current]).xMax;
            int j = i - 1;

            while (j >= 0 &&
                   GetBounds(clouds[order[j]]).xMax < x)
            {
                order[j + 1] = order[j];
                j--;
            }

            order[j + 1] = current;
        }

        for (int i = 0; i < count; i++)
        {
            int index = order[i];
            RectTransform cloud = clouds[index];

            Rect bounds = GetBounds(cloud);
            float movement = speeds[index] * Time.deltaTime;

            for (int j = 0; j < i; j++)
            {
                Rect other = GetBounds(clouds[order[j]]);

                bool sameHeight =
                    bounds.yMin < other.yMax &&
                    bounds.yMax > other.yMin;

                if (!sameHeight)
                    continue;

                float available =
                    other.xMin - bounds.xMax - minSpacing;

                movement = Mathf.Min(
                    movement,
                    Mathf.Max(0f, available)
                );
            }

            cloud.anchoredPosition +=
                Vector2.right * movement;

            if (IsOffscreen(cloud))
            {
                active[index] = false;
                cloud.gameObject.SetActive(false);
            }
        }
    }

    private void TrySpawn()
    {
        int start = Random.Range(0, clouds.Length);
        int index = -1;

        for (int i = 0; i < clouds.Length; i++)
        {
            int candidate = (start + i) % clouds.Length;

            if (!active[candidate] && clouds[candidate] != null)
            {
                index = candidate;
                break;
            }
        }

        if (index == -1)
        {
            nextSpawnTime = Time.time + 0.5f;
            return;
        }

        int lane = Random.Range(0, 2);

        // Prefer alternating lanes without making it predictable.
        if (lastLane != -1)
        {
            if (laneStreak >= Mathf.Max(1, maxSameLane) ||
                Random.value < 0.7f)
            {
                lane = 1 - lastLane;
            }
        }

        bool spawned = SpawnAt(index, lane);

        if (!spawned &&
            !(lane == lastLane && laneStreak >= Mathf.Max(1, maxSameLane)))
        {
            lane = 1 - lane;
            spawned = SpawnAt(index, lane);
        }

        if (!spawned)
        {
            nextSpawnTime = Time.time + 0.3f;
            return;
        }

        active[index] = true;
        speeds[index] = Random.Range(minSpeed, maxSpeed);
        clouds[index].gameObject.SetActive(true);

        laneStreak = lane == lastLane ? laneStreak + 1 : 1;
        lastLane = lane;

        nextSpawnTime = Time.time +
            Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    private bool SpawnAt(int index, int lane)
    {
        RectTransform spawn = lane == 0
            ? spawnPoint1
            : spawnPoint2;

        RectTransform cloud = clouds[index];
        cloud.position = spawn.position;

        Rect bounds = GetBounds(cloud);

        for (int i = 0; i < clouds.Length; i++)
        {
            if (!active[i] || i == index)
                continue;

            Rect other = GetBounds(clouds[i]);

            bool sameHeight =
                bounds.yMin < other.yMax &&
                bounds.yMax > other.yMin;

            bool tooClose =
                bounds.xMin < other.xMax + minSpacing &&
                bounds.xMax > other.xMin - minSpacing;

            if (sameHeight && tooClose)
                return false;
        }

        return true;
    }

    private bool IsOffscreen(RectTransform cloud)
    {
        cloud.GetWorldCorners(corners);

        float left = float.MaxValue;

        foreach (Vector3 corner in corners)
        {
            float x = RectTransformUtility
                .WorldToScreenPoint(uiCamera, corner).x;

            left = Mathf.Min(left, x);
        }

        return left > Screen.width;
    }

    private Rect GetBounds(RectTransform cloud)
    {
        cloud.GetWorldCorners(corners);

        Vector3 min = cloud.parent.InverseTransformPoint(corners[0]);
        Vector3 max = min;

        for (int i = 1; i < 4; i++)
        {
            Vector3 point =
                cloud.parent.InverseTransformPoint(corners[i]);

            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
}
