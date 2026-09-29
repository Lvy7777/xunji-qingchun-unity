using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class YouthClayPlatformerManager : MonoBehaviour
{
    private const int FallbackStartingLives = 4;
    private const int TotalCollectibles = 4;
    private const float LevelEndX = 62.0f;

    private enum GameState
    {
        Inactive,
        Playing,
        Respawning,
        Failed,
        Paused,
        Completing
    }

    public event Action ChallengeCompleted;

    private readonly List<YouthPlatformerCollectible> collectibles = new List<YouthPlatformerCollectible>(TotalCollectibles);
    private readonly List<YouthCrumblePlatform> crumblePlatforms = new List<YouthCrumblePlatform>(4);

    private readonly Vector2[] checkpoints =
    {
        new Vector2(1.5f, -1.92f),
        new Vector2(16.8f, -1.92f),
        new Vector2(32.3f, -1.92f),
        new Vector2(49.8f, -1.92f)
    };

    private GameObject originalCanvas;
    private GameObject worldRoot;
    private Camera gameCamera;
    private YouthPlatformerCameraFollow cameraFollow;
    private YouthPlatformerPlayer player;
    private Canvas hudCanvas;
    private CanvasGroup hudGroup;
    private TMP_Text progressText;
    private TMP_Text livesText;
    private TMP_Text zoneText;
    private TMP_Text statusText;
    private GameObject retryPanel;
    private Button pauseButton;
    private YouthPlatformerDifficultyProfile difficulty;
    private int maxLives = FallbackStartingLives;


    private GameState state;
    private int collectedCount;
    private int lives;
    private int checkpointIndex;
    private int currentZone = -1;
    private bool completionRaised;
    private bool finishReminderShown;

    private Coroutine statusRoutine;

public void StartGame()
    {
        if (state == GameState.Playing || state == GameState.Completing)
        {
            return;
        }

        EnsureWorld();
        if (worldRoot == null || player == null)
        {
            Debug.LogError("[YouthMemory] Clay platformer could not be created.");
            return;
        }

        if (originalCanvas != null)
        {
            originalCanvas.SetActive(false);
        }

        worldRoot.SetActive(true);
        gameCamera.gameObject.SetActive(true);
        hudCanvas.gameObject.SetActive(true);
        hudGroup.alpha = 1f;
        hudGroup.blocksRaycasts = true;

        completionRaised = false;
        lives = maxLives;
        collectedCount = 0;
        checkpointIndex = 0;
        currentZone = -1;
        finishReminderShown = false;
        retryPanel.SetActive(false);

        for (int i = 0; i < collectibles.Count; i++)
        {
            collectibles[i].ResetCollectible();
        }

        for (int i = 0; i < crumblePlatforms.Count; i++)
        {
            crumblePlatforms[i].ResetPlatform();
        }

        player.gameObject.SetActive(true);
        player.Body.simulated = true;
        player.Respawn(checkpoints[0]);
        cameraFollow.SnapToTarget();
        state = GameState.Playing;
        UpdateHud();
        UpdateZone(true);
        ShowStatus("移动平台会托住你 · 裂纹平台会坍塌 · 小心泥刺", 3.4f);
    }

private void Update()
    {
        if (state != GameState.Playing || player == null)
        {
            return;
        }

        float playerX = player.transform.position.x;
        if (playerX > 49f && checkpointIndex < 3)
        {
            checkpointIndex = 3;
            ShowStatus("终点前检查点已记录", 1.5f);
        }
        else if (playerX > 32f && checkpointIndex < 2)
        {
            checkpointIndex = 2;
            ShowStatus("已进入青春试炼", 1.5f);
        }
        else if (playerX > 16f && checkpointIndex < 1)
        {
            checkpointIndex = 1;
            ShowStatus("村落检查点已记录", 1.5f);
        }

        UpdateZone(false);

        if (playerX >= LevelEndX)
        {
            if (collectedCount >= TotalCollectibles)
            {
                BeginCompletion();
            }
            else if (!finishReminderShown)
            {
                finishReminderShown = true;
                ShowStatus("还差 " + (TotalCollectibles - collectedCount) + " 枚记忆徽章", 1.2f);
            }
        }
        else if (playerX < LevelEndX - 1.5f)
        {
            finishReminderShown = false;
        }
    }

    public void Collect(int index, GameObject collectibleObject)
    {
        if (state != GameState.Playing || collectibleObject == null || !collectibleObject.activeSelf)
        {
            return;
        }

        collectibleObject.SetActive(false);
        collectedCount = Mathf.Min(TotalCollectibles, collectedCount + 1);
        UpdateHud();
        AudioManager.Instance?.PlayUi(UISound.Collect);

        if (collectedCount >= TotalCollectibles)
        {
            ShowStatus("记忆集齐！前往终点红旗", 2.5f);
        }
        else
        {
            ShowStatus("找到一枚青春记忆", 1.2f);
        }
    }

public void HandlePlayerFall()
    {
        if (state == GameState.Playing)
        {
            StartCoroutine(RespawnRoutine("脚下一空，从最近的红旗再来"));
        }
    }

public void HandleHazardHit(Vector2 source)
    {
        if (state != GameState.Playing)
        {
            return;
        }

        string message = source.x >= 40f ? "青春试炼碰壁了，再稳一点" : "碰到了泥刺，从检查点再来";
        StartCoroutine(RespawnRoutine(message));
    }


private IEnumerator RespawnRoutine(string reason)
    {
        state = GameState.Respawning;
        player.SetInputEnabled(false);
        lives = Mathf.Max(0, lives - 1);
        UpdateHud();

        if (lives <= 0)
        {
            state = GameState.Failed;
            retryPanel.SetActive(true);
            statusText.text = "旅途暂停，观察节奏后再出发";
            yield break;
        }

        ShowStatus(reason, 1.0f);
        float delay = difficulty != null ? difficulty.hazardRespawnDelay : 0.6f;
        yield return new WaitForSecondsRealtime(delay);
        player.Respawn(checkpoints[checkpointIndex]);
        cameraFollow.SnapToTarget();
        state = GameState.Playing;
    }

    private void RestartGame()
    {
        if (state != GameState.Failed)
        {
            return;
        }

        StartGame();
    }

    private void TogglePause()
    {
        if (state == GameState.Playing)
        {
            state = GameState.Paused;
            player.SetInputEnabled(false);
            player.Body.simulated = false;
            pauseButton.GetComponentInChildren<TMP_Text>().text = "▶";
            statusText.text = "已暂停";
        }
        else if (state == GameState.Paused)
        {
            player.Body.simulated = true;
            player.SetInputEnabled(true);
            state = GameState.Playing;
            pauseButton.GetComponentInChildren<TMP_Text>().text = "Ⅱ";
            ShowStatus("继续探索", 1.0f);
        }
    }

    private void BeginCompletion()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        state = GameState.Completing;
        player.SetInputEnabled(false);
        AudioManager.Instance?.PlayUi(UISound.Complete);
        ShowStatus("青春记忆收集完成！", 4f);
        StartCoroutine(CompleteRoutine());
    }

    private IEnumerator CompleteRoutine()
    {
        yield return new WaitForSecondsRealtime(1.2f);

        float elapsed = 0f;
        while (elapsed < 0.45f)
        {
            elapsed += Time.unscaledDeltaTime;
            hudGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.45f);
            yield return null;
        }

        worldRoot.SetActive(false);
        gameCamera.gameObject.SetActive(false);
        hudCanvas.gameObject.SetActive(false);

        if (originalCanvas != null)
        {
            originalCanvas.SetActive(true);
        }

        if (!completionRaised)
        {
            completionRaised = true;
            ChallengeCompleted?.Invoke();
        }
    }

private void EnsureWorld()
    {
        if (worldRoot != null)
        {
            return;
        }

        difficulty = Resources.Load<YouthPlatformerDifficultyProfile>("Data/YouthMemory/YouthPlatformerDifficulty");
        if (difficulty == null)
        {
            difficulty = ScriptableObject.CreateInstance<YouthPlatformerDifficultyProfile>();
        }

        maxLives = Mathf.Max(1, difficulty.startingLives);
        Transform existingCanvas = transform.Find("YouthMemoryCanvas");
        originalCanvas = existingCanvas != null ? existingCanvas.gameObject : null;

        worldRoot = new GameObject("YouthClayPlatformerWorld");
        worldRoot.transform.SetParent(transform, false);

        CreateCamera();
        BuildBackground();
        BuildPlatforms();
        BuildDecorations();
        BuildHazardsAndModifiers();
        BuildCollectibles();
        BuildPlayer();
        BuildFinishGate();
        BuildHud();
    }

    private void CreateCamera()
    {
        GameObject cameraObject = new GameObject("YouthPlatformerCamera", typeof(Camera), typeof(AudioListener), typeof(YouthPlatformerCameraFollow));
        cameraObject.transform.SetParent(worldRoot.transform, false);
        cameraObject.transform.position = new Vector3(8.8f, 0.15f, -10f);
        cameraObject.tag = "MainCamera";

        gameCamera = cameraObject.GetComponent<Camera>();
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = 5.4f;
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = new Color(0.28f, 0.68f, 0.90f, 1f);
        gameCamera.allowHDR = false;

        cameraFollow = cameraObject.GetComponent<YouthPlatformerCameraFollow>();
    }

private void BuildBackground()
    {
        Texture2D backgroundTexture = Resources.Load<Texture2D>("Art/YouthMemory/ClayPlatformer/clay_countryside_bg");
        Sprite backgroundSprite = ClaySpriteFactory.FromTexture(backgroundTexture, 100f);

        for (int i = 0; i < 4; i++)
        {
            GameObject segment = new GameObject("ClayBackground_" + (i + 1), typeof(SpriteRenderer));
            segment.transform.SetParent(worldRoot.transform, false);
            segment.transform.position = new Vector3(7.8f + i * 17.2f, 0.1f, 6f);

            SpriteRenderer renderer = segment.GetComponent<SpriteRenderer>();
            renderer.sprite = backgroundSprite;
            renderer.sortingOrder = -100;
            renderer.color = Color.white;

            if (backgroundSprite != null)
            {
                Vector2 bounds = backgroundSprite.bounds.size;
                float scaleX = 17.2f / Mathf.Max(0.01f, bounds.x);
                float scaleY = 10.8f / Mathf.Max(0.01f, bounds.y);
                segment.transform.localScale = new Vector3((i % 2 == 1 ? -1f : 1f) * scaleX, scaleY, 1f);
            }
        }

        CreateClayShape("SkyHaze", new Vector2(32f, 3.9f), new Vector2(68f, 3.5f),
            new Color(0.65f, 0.88f, 1f, 0.20f), -90, 0.18f, 70);
    }

private void BuildPlatforms()
    {
        CreatePlatform("Ground_A", new Vector2(6.5f, -3.5f), new Vector2(13f, 1.6f), 1);
        CreatePlatform("Ground_B", new Vector2(22.75f, -3.5f), new Vector2(13.5f, 1.6f), 2);
        CreatePlatform("Ground_C", new Vector2(38.75f, -3.5f), new Vector2(14.5f, 1.6f), 3);
        CreatePlatform("Ground_D", new Vector2(56.75f, -3.5f), new Vector2(15.5f, 1.6f), 4);

        CreatePlatform("FieldWarmup", new Vector2(4.2f, -1.35f), new Vector2(2.8f, 0.58f), 10);
        CreatePlatform("FieldHigh", new Vector2(8.0f, 0.25f), new Vector2(2.5f, 0.58f), 11);
        CreatePlatform("FieldLanding", new Vector2(11.1f, -0.90f), new Vector2(1.9f, 0.58f), 12);
        CreateMovingPlatform("FieldFerry", new Vector2(12.7f, -1.15f), new Vector2(15.35f, -0.42f),
            new Vector2(2.2f, 0.58f), 13, 0f);

        CreatePlatform("VillageStart", new Vector2(18.2f, -1.30f), new Vector2(2.4f, 0.58f), 20);
        CreateMovingPlatform("VillageLift", new Vector2(21.0f, -1.05f), new Vector2(21.0f, 1.15f),
            new Vector2(2.1f, 0.58f), 21, 0.18f);
        CreatePlatform("VillageRoof", new Vector2(24.0f, 1.55f), new Vector2(2.6f, 0.58f), 22);
        CreateCrumblePlatform("VillageCrumble", new Vector2(27.2f, 0.15f), new Vector2(2.2f, 0.58f), 23);
        CreateMovingPlatform("VillageBridge", new Vector2(29.2f, -1.10f), new Vector2(31.7f, -0.58f),
            new Vector2(2.1f, 0.58f), 24, 0.42f);

        CreatePlatform("YouthEntry", new Vector2(34.4f, -1.20f), new Vector2(2.4f, 0.58f), 30);
        CreateCrumblePlatform("YouthCrumbleLow", new Vector2(37.4f, 0.15f), new Vector2(2.0f, 0.58f), 31);
        CreateCrumblePlatform("YouthCrumbleHigh", new Vector2(40.2f, 1.45f), new Vector2(2.0f, 0.58f), 32);
        CreateMovingPlatform("YouthLift", new Vector2(43.2f, -0.20f), new Vector2(43.2f, 1.80f),
            new Vector2(2.0f, 0.58f), 33, 0.30f);
        CreatePlatform("WindLanding", new Vector2(45.5f, 0.78f), new Vector2(1.7f, 0.58f), 34);
        CreateMovingPlatform("FinalFerry", new Vector2(46.4f, -0.20f), new Vector2(49.5f, 0.48f),
            new Vector2(2.1f, 0.58f), 35, 0.64f);

        CreatePlatform("FinalStep_1", new Vector2(51.2f, -1.20f), new Vector2(2.3f, 0.58f), 40);
        CreatePlatform("FinalStep_2", new Vector2(54.0f, 0.25f), new Vector2(2.2f, 0.58f), 41);
        CreateCrumblePlatform("FinalCrumble", new Vector2(57.0f, 1.70f), new Vector2(2.2f, 0.58f), 42);
        CreatePlatform("FinalStep_3", new Vector2(60.0f, 0.40f), new Vector2(2.3f, 0.58f), 43);
    }

private void BuildHazardsAndModifiers()
    {
        CreateSpikeStrip("FieldMudThorns", new Vector2(6.25f, -2.43f), 1.45f, 100);
        CreatePatrolHazard("FieldRollingMud", new Vector2(9.6f, -2.18f), new Vector2(11.8f, -2.18f), 110, 0.1f);

        CreatePatrolHazard("VillageRollingMud", new Vector2(17.4f, -2.18f), new Vector2(20.0f, -2.18f), 120, 0.45f);
        CreateSpikeStrip("VillageMudThorns", new Vector2(25.3f, -2.43f), 1.55f, 130);

        CreateSpikeStrip("YouthMudThorns", new Vector2(35.2f, -2.43f), 1.45f, 140);
        CreatePatrolHazard("YouthRollingMud", new Vector2(41.6f, -2.18f), new Vector2(45.0f, -2.18f), 150, 0.25f);
        CreateWindZone(new Vector2(43.0f, 0.75f), new Vector2(7.0f, 4.4f), 160);

        CreateSpikeStrip("FinalMudThorns", new Vector2(52.2f, -2.43f), 1.35f, 170);
        CreatePatrolHazard("FinalRollingMud", new Vector2(55.2f, -2.18f), new Vector2(59.2f, -2.18f), 180, 0.6f);
    }

    private GameObject CreateMovingPlatform(string name, Vector2 start, Vector2 end, Vector2 size, int seed, float phase)
    {
        GameObject platform = CreatePlatform(name, start, size, seed);
        YouthMovingPlatform mover = platform.AddComponent<YouthMovingPlatform>();
        mover.Configure(start, end, difficulty.movingPlatformCycle, phase);
        return platform;
    }

private GameObject CreateCrumblePlatform(string name, Vector2 position, Vector2 size, int seed)
    {
        GameObject platform = CreatePlatform(name, position, size, seed);
        SpriteRenderer renderer = platform.GetComponent<SpriteRenderer>();
        renderer.color = new Color(1f, 0.82f, 0.72f, 1f);

        float scaleX = Mathf.Max(0.001f, Mathf.Abs(platform.transform.localScale.x));
        float scaleY = Mathf.Max(0.001f, Mathf.Abs(platform.transform.localScale.y));
        Vector2 crackSize = new Vector2(0.07f / scaleX, 0.34f / scaleY);
        GameObject crackA = CreateClayShape("CrackA", platform.transform,
            new Vector2(-0.09f / scaleX, 0.02f / scaleY), crackSize,
            new Color(0.34f, 0.10f, 0.06f, 1f), 7, 0.12f, seed + 700);
        crackA.transform.localRotation = Quaternion.Euler(0f, 0f, -32f);
        GameObject crackB = CreateClayShape("CrackB", platform.transform,
            new Vector2(0.10f / scaleX, 0.02f / scaleY), crackSize,
            new Color(0.34f, 0.10f, 0.06f, 1f), 7, 0.12f, seed + 701);
        crackB.transform.localRotation = Quaternion.Euler(0f, 0f, 32f);

        YouthCrumblePlatform crumble = platform.AddComponent<YouthCrumblePlatform>();
        crumble.Configure(difficulty.crumbleWarningTime, difficulty.crumbleRespawnTime);
        crumblePlatforms.Add(crumble);
        return platform;
    }

    private void CreateSpikeStrip(string name, Vector2 position, float width, int seed)
    {
        GameObject root = new GameObject(name, typeof(BoxCollider2D), typeof(YouthPlatformerHazard));
        root.transform.SetParent(worldRoot.transform, false);
        root.transform.position = position;

        BoxCollider2D trigger = root.GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(width, 0.48f);

        Transform visuals = new GameObject("ClayThorns").transform;
        visuals.SetParent(root.transform, false);
        for (int i = 0; i < 5; i++)
        {
            float x = Mathf.Lerp(-width * 0.42f, width * 0.42f, i / 4f);
            GameObject thorn = CreateClayShape("Thorn", visuals, new Vector2(x, 0f), new Vector2(0.25f, 0.58f),
                new Color(0.48f, 0.18f, 0.10f, 1f), 8, 0.12f, seed + i);
            thorn.transform.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? -12f : 12f);
        }

        root.GetComponent<YouthPlatformerHazard>().Configure(this, visuals, position, position, 0f, 0f);
    }

    private void CreatePatrolHazard(string name, Vector2 start, Vector2 end, int seed, float phase)
    {
        GameObject root = new GameObject(name, typeof(CircleCollider2D), typeof(YouthPlatformerHazard));
        root.transform.SetParent(worldRoot.transform, false);
        CircleCollider2D trigger = root.GetComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.46f;

        Transform visuals = new GameObject("RollingClayVisual").transform;
        visuals.SetParent(root.transform, false);
        CreateClayCircle("MudCore", visuals, Vector2.zero, 0.88f, new Color(0.52f, 0.20f, 0.10f, 1f), 9, seed);
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.46f;
            CreateClayCircle("MudNub", visuals, offset, 0.23f, new Color(0.70f, 0.30f, 0.13f, 1f), 10, seed + i + 1);
        }

        root.GetComponent<YouthPlatformerHazard>().Configure(
            this, visuals, start, end, difficulty.patrolHazardSpeed, phase);
    }

    private void CreateWindZone(Vector2 position, Vector2 size, int seed)
    {
        GameObject root = new GameObject("YouthWindZone", typeof(BoxCollider2D), typeof(YouthWindZone));
        root.transform.SetParent(worldRoot.transform, false);
        root.transform.position = position;
        root.GetComponent<BoxCollider2D>().size = size;
        root.GetComponent<YouthWindZone>().Configure(difficulty.windAcceleration);

        CreateClayShape("WindVeil", root.transform, Vector2.zero, size,
            new Color(0.62f, 0.90f, 1f, 0.16f), -3, 0.28f, seed);
        CreateWorldText("WindHint", "逆风区  ◀ ◀", new Vector2(0f, size.y * 0.36f), 0.13f,
            new Color(0.14f, 0.38f, 0.52f, 1f), 4, root.transform);
    }


private void BuildDecorations()
    {
        CreateZoneSign(new Vector2(1.4f, -1.45f), "田野热身");
        CreateZoneSign(new Vector2(16.7f, -1.45f), "村落进阶");
        CreateZoneSign(new Vector2(32.2f, -1.45f), "青春试炼");

        CreateBushCluster(new Vector2(2.8f, -2.55f), 200);
        CreateBushCluster(new Vector2(11.5f, -2.55f), 210);
        CreateBushCluster(new Vector2(18.2f, -2.55f), 220);
        CreateBushCluster(new Vector2(28.0f, -2.55f), 230);
        CreateBushCluster(new Vector2(33.0f, -2.55f), 240);
        CreateBushCluster(new Vector2(45.0f, -2.55f), 250);
        CreateBushCluster(new Vector2(50.2f, -2.55f), 260);
        CreateBushCluster(new Vector2(62.0f, -2.55f), 270);

        CreateRock(new Vector2(3.3f, -2.50f), 280);
        CreateRock(new Vector2(20.8f, -2.48f), 281);
        CreateRock(new Vector2(38.0f, -2.48f), 282);
        CreateRock(new Vector2(61.0f, -2.48f), 283);

        CreateCheckpointFlag(checkpoints[0] + new Vector2(0f, -0.08f), "起点");
        CreateCheckpointFlag(checkpoints[1] + new Vector2(0f, -0.08f), "村落");
        CreateCheckpointFlag(checkpoints[2] + new Vector2(0f, -0.08f), "试炼");
        CreateCheckpointFlag(checkpoints[3] + new Vector2(0f, -0.08f), "冲刺");
    }

private void BuildCollectibles()
    {
        CreateCollectible(0, new Vector2(8.0f, 1.18f));
        CreateCollectible(1, new Vector2(24.0f, 2.48f));
        CreateCollectible(2, new Vector2(40.2f, 2.38f));
        CreateCollectible(3, new Vector2(57.0f, 2.62f));
    }

private void BuildPlayer()
    {
        GameObject playerObject = new GameObject("XiaoHePlayer", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(YouthPlatformerPlayer));
        playerObject.transform.SetParent(worldRoot.transform, false);
        playerObject.transform.position = checkpoints[0];

        BoxCollider2D collider = playerObject.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(0.72f, 1.42f);
        collider.offset = new Vector2(0f, 0.02f);

        GameObject sensorObject = new GameObject("GroundSensor", typeof(BoxCollider2D), typeof(YouthGroundSensor));
        sensorObject.transform.SetParent(playerObject.transform, false);
        sensorObject.transform.localPosition = new Vector3(0f, -0.76f, 0f);
        BoxCollider2D sensorCollider = sensorObject.GetComponent<BoxCollider2D>();
        sensorCollider.isTrigger = true;
        sensorCollider.size = new Vector2(0.58f, 0.16f);

        Transform visuals = new GameObject("ClayCharacterVisual").transform;
        visuals.SetParent(playerObject.transform, false);
        BuildPlayerVisuals(visuals);

        player = playerObject.GetComponent<YouthPlatformerPlayer>();
        player.Configure(sensorObject.GetComponent<YouthGroundSensor>(), visuals, difficulty);
        player.Fell += HandlePlayerFall;
        cameraFollow.Configure(player.transform, 8.8f, 52.5f);
    }

    private void BuildPlayerVisuals(Transform parent)
    {
        CreateClayShape("Body", parent, new Vector2(0f, -0.05f), new Vector2(0.68f, 0.86f),
            new Color(0.94f, 0.67f, 0.22f, 1f), 20, 0.35f, 30);
        CreateClayCircle("Head", parent, new Vector2(0f, 0.58f), 0.64f,
            new Color(0.94f, 0.72f, 0.50f, 1f), 22, 31);
        CreateClayCircle("HairLeft", parent, new Vector2(-0.20f, 0.84f), 0.32f,
            new Color(0.20f, 0.10f, 0.06f, 1f), 23, 32);
        CreateClayCircle("HairTop", parent, new Vector2(0.03f, 0.91f), 0.37f,
            new Color(0.23f, 0.12f, 0.07f, 1f), 23, 33);
        CreateClayCircle("HairRight", parent, new Vector2(0.23f, 0.82f), 0.28f,
            new Color(0.18f, 0.09f, 0.05f, 1f), 23, 34);
        CreateClayShape("Backpack", parent, new Vector2(-0.34f, 0.00f), new Vector2(0.28f, 0.58f),
            new Color(0.33f, 0.49f, 0.29f, 1f), 19, 0.40f, 35);
        CreateClayShape("BootLeft", parent, new Vector2(-0.19f, -0.57f), new Vector2(0.28f, 0.20f),
            new Color(0.18f, 0.12f, 0.08f, 1f), 21, 0.35f, 36);
        CreateClayShape("BootRight", parent, new Vector2(0.19f, -0.57f), new Vector2(0.28f, 0.20f),
            new Color(0.18f, 0.12f, 0.08f, 1f), 21, 0.35f, 37);
    }

private void BuildFinishGate()
    {
        CreateClayShape("FinishPole", new Vector2(61.8f, -1.15f), new Vector2(0.22f, 3.1f),
            new Color(0.25f, 0.12f, 0.06f, 1f), 8, 0.40f, 300);
        CreateClayShape("FinishFlag", new Vector2(62.55f, 0.10f), new Vector2(1.35f, 0.75f),
            new Color(0.87f, 0.18f, 0.12f, 1f), 9, 0.25f, 301);
        CreateWorldText("FinishLabel", "青春记忆", new Vector2(61.8f, 1.05f), 0.20f, Color.white, 12);
    }

    private void BuildHud()
    {
        GameObject canvasObject = new GameObject("YouthPlatformerHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasObject.transform.SetParent(transform, false);
        hudCanvas = canvasObject.GetComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 1500;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        hudGroup = canvasObject.GetComponent<CanvasGroup>();

        RectTransform progressPill = CreateUIPanel("ProgressPill", canvasObject.transform,
            new Color(0.12f, 0.12f, 0.10f, 0.72f), new Vector2(0.025f, 0.86f), new Vector2(0.25f, 0.965f), 50);
        progressText = CreateUIText("ProgressText", progressPill, "", 30, Color.white, TextAlignmentOptions.Left,
            new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.88f));

        RectTransform livesPill = CreateUIPanel("LivesPill", canvasObject.transform,
            new Color(0.12f, 0.12f, 0.10f, 0.62f), new Vector2(0.025f, 0.78f), new Vector2(0.19f, 0.85f), 51);
        livesText = CreateUIText("LivesText", livesPill, "", 24, new Color(1f, 0.82f, 0.36f, 1f), TextAlignmentOptions.Left,
            new Vector2(0.10f, 0.12f), new Vector2(0.92f, 0.88f));

        RectTransform zonePill = CreateUIPanel("ZonePill", canvasObject.transform,
            new Color(0.95f, 0.42f, 0.16f, 0.92f), new Vector2(0.38f, 0.89f), new Vector2(0.62f, 0.97f), 52);
        zoneText = CreateUIText("ZoneText", zonePill, "", 28, Color.white, TextAlignmentOptions.Center,
            new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));

        RectTransform statusPill = CreateUIPanel("StatusPill", canvasObject.transform,
            new Color(0.08f, 0.08f, 0.07f, 0.74f), new Vector2(0.30f, 0.78f), new Vector2(0.70f, 0.86f), 53);
        statusText = CreateUIText("StatusText", statusPill, "", 24, Color.white, TextAlignmentOptions.Center,
            new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));

        pauseButton = CreateUIButton("PauseButton", canvasObject.transform, "Ⅱ",
            new Vector2(0.91f, 0.88f), new Vector2(0.97f, 0.975f), new Color(0.12f, 0.12f, 0.10f, 0.70f), TogglePause, 54);

        CreateHoldControl(canvasObject.transform, "LeftControl", "◀", new Vector2(0.035f, 0.065f), new Vector2(0.12f, 0.215f), -1f, 55);
        CreateHoldControl(canvasObject.transform, "RightControl", "▶", new Vector2(0.13f, 0.065f), new Vector2(0.215f, 0.215f), 1f, 56);
        CreateJumpControl(canvasObject.transform);

        retryPanel = CreateUIPanel("RetryPanel", canvasObject.transform, new Color(0.09f, 0.07f, 0.06f, 0.90f),
            new Vector2(0.32f, 0.32f), new Vector2(0.68f, 0.66f), 57).gameObject;
        CreateUIText("RetryTitle", retryPanel.transform, "旅途暂停", 46, new Color(1f, 0.75f, 0.32f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.10f, 0.58f), new Vector2(0.90f, 0.84f));
        CreateUIText("RetryCopy", retryPanel.transform, "再试一次，徽章会重新出现", 25, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.10f, 0.40f), new Vector2(0.90f, 0.57f));
        CreateUIButton("RetryButton", retryPanel.transform, "重新出发", new Vector2(0.28f, 0.13f), new Vector2(0.72f, 0.34f),
            new Color(0.94f, 0.42f, 0.15f, 1f), RestartGame, 58);
        retryPanel.SetActive(false);
    }

private void CreateJumpControl(Transform parent)
    {
        RectTransform root = CreateUIPanel("JumpControl", parent, new Color(0.94f, 0.42f, 0.15f, 0.90f),
            new Vector2(0.86f, 0.06f), new Vector2(0.96f, 0.235f), 60);
        PlatformerTouchButton touch = root.gameObject.AddComponent<PlatformerTouchButton>();
        touch.HeldChanged += held =>
        {
            if (player == null || state != GameState.Playing)
            {
                return;
            }

            if (held)
            {
                player.RequestJump();
            }
            else
            {
                player.ReleaseJump();
            }
        };
        CreateUIText("Label", root, "▲\n跳跃", 30, Color.white, TextAlignmentOptions.Center,
            new Vector2(0f, 0.08f), new Vector2(1f, 0.92f));
    }

    private void CreateHoldControl(Transform parent, string name, string label, Vector2 min, Vector2 max, float direction, int seed)
    {
        RectTransform root = CreateUIPanel(name, parent, new Color(0.11f, 0.10f, 0.08f, 0.58f), min, max, seed);
        PlatformerTouchButton touch = root.gameObject.AddComponent<PlatformerTouchButton>();
        touch.HeldChanged += held =>
        {
            if (player != null)
            {
                player.SetTouchDirection(held ? direction : 0f);
            }
        };
        CreateUIText("Label", root, label, 46, Color.white, TextAlignmentOptions.Center,
            new Vector2(0f, 0f), new Vector2(1f, 1f));
    }

private void UpdateHud()
    {
        if (progressText != null)
        {
            progressText.text = "青春徽章  " + collectedCount + " / " + TotalCollectibles;
        }

        if (livesText != null)
        {
            livesText.text = "体力  " + new string('●', lives) + new string('○', Mathf.Max(0, maxLives - lives));
        }
    }

private void UpdateZone(bool force)
    {
        if (player == null)
        {
            return;
        }

        float x = player.transform.position.x;
        int zone = x < 16f ? 0 : (x < 32f ? 1 : 2);
        if (!force && zone == currentZone)
        {
            return;
        }

        currentZone = zone;
        zoneText.text = zone == 0 ? "田野热身" : (zone == 1 ? "村落进阶" : "青春试炼");
    }

    private void ShowStatus(string message, float duration)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = message;
        if (statusRoutine != null)
        {
            StopCoroutine(statusRoutine);
        }

        statusRoutine = StartCoroutine(ClearStatusRoutine(duration));
    }

    private IEnumerator ClearStatusRoutine(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (statusText != null && state == GameState.Playing)
        {
            statusText.text = collectedCount >= TotalCollectibles ? "前往终点红旗" : "寻找散落的青春记忆";
        }

        statusRoutine = null;
    }

private GameObject CreatePlatform(string name, Vector2 position, Vector2 size, int seed)
    {
        GameObject platform = CreateClayShape(name, position, size, new Color(0.76f, 0.29f, 0.10f, 1f), 2, 0.22f, seed);
        platform.layer = LayerMask.NameToLayer("Platform");

        SpriteRenderer renderer = platform.GetComponent<SpriteRenderer>();
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        Vector2 localSpriteSize = renderer.sprite.bounds.size;
        collider.size = new Vector2(localSpriteSize.x * 0.96f, localSpriteSize.y * 0.88f);

        float scaleX = Mathf.Max(0.001f, Mathf.Abs(platform.transform.localScale.x));
        float scaleY = Mathf.Max(0.001f, Mathf.Abs(platform.transform.localScale.y));
        Vector2 rimLocalSize = new Vector2(size.x * 0.92f / scaleX, 0.11f / scaleY);
        Vector2 rimLocalPosition = new Vector2(0f, size.y * 0.34f / scaleY);
        CreateClayShape("TopRim", platform.transform, rimLocalPosition, rimLocalSize,
            new Color(0.96f, 0.48f, 0.18f, 1f), 3, 0.32f, seed + 500);
        return platform;
    }

    private void CreateCollectible(int index, Vector2 position)
    {
        GameObject item = new GameObject("YouthMemoryBadge_" + (index + 1), typeof(CircleCollider2D), typeof(YouthPlatformerCollectible));
        item.transform.SetParent(worldRoot.transform, false);
        item.transform.position = position;

        CircleCollider2D collider = item.GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.38f;

        CreateClayCircle("BadgeOuter", item.transform, Vector2.zero, 0.75f,
            new Color(1f, 0.67f, 0.08f, 1f), 12, 100 + index);
        CreateClayCircle("BadgeInner", item.transform, Vector2.zero, 0.44f,
            new Color(1f, 0.91f, 0.42f, 1f), 13, 110 + index);
        CreateWorldText("BadgeMark", "记", Vector2.zero, 0.14f, new Color(0.46f, 0.20f, 0.04f, 1f), 14, item.transform);

        YouthPlatformerCollectible collectible = item.GetComponent<YouthPlatformerCollectible>();
        collectible.Initialize(this, index);
        collectibles.Add(collectible);
    }

    private void CreateBushCluster(Vector2 position, int seed)
    {
        CreateClayCircle("Bush", worldRoot.transform, position + new Vector2(-0.35f, 0f), 0.75f,
            new Color(0.24f, 0.55f, 0.22f, 1f), -2, seed);
        CreateClayCircle("Bush", worldRoot.transform, position + new Vector2(0.10f, 0.18f), 0.95f,
            new Color(0.31f, 0.64f, 0.25f, 1f), -1, seed + 1);
        CreateClayCircle("Bush", worldRoot.transform, position + new Vector2(0.55f, -0.02f), 0.72f,
            new Color(0.20f, 0.50f, 0.18f, 1f), -2, seed + 2);
    }

    private void CreateRock(Vector2 position, int seed)
    {
        CreateClayShape("ClayRock", position, new Vector2(0.95f, 0.58f),
            new Color(0.47f, 0.36f, 0.28f, 1f), -1, 0.42f, seed);
    }

    private void CreateZoneSign(Vector2 position, string label)
    {
        CreateClayShape("SignPost", position, new Vector2(0.15f, 1.5f),
            new Color(0.29f, 0.15f, 0.07f, 1f), -1, 0.30f, 80);
        CreateClayShape("SignBoard", position + new Vector2(0f, 0.62f), new Vector2(1.7f, 0.60f),
            new Color(0.90f, 0.62f, 0.28f, 1f), 0, 0.22f, 81);
        CreateWorldText("SignText", label, position + new Vector2(0f, 0.64f), 0.12f,
            new Color(0.25f, 0.12f, 0.05f, 1f), 1);
    }

    private void CreateCheckpointFlag(Vector2 position, string label)
    {
        CreateClayShape("FlagPole", position + new Vector2(0f, 0.45f), new Vector2(0.12f, 1.8f),
            new Color(0.28f, 0.13f, 0.06f, 1f), 4, 0.38f, 90);
        CreateClayShape("Flag", position + new Vector2(0.55f, 1.05f), new Vector2(1.0f, 0.55f),
            new Color(0.88f, 0.20f, 0.12f, 1f), 5, 0.25f, 91);
        CreateWorldText("FlagText", label, position + new Vector2(0.56f, 1.05f), 0.09f, Color.white, 6);
    }

    private GameObject CreateClayShape(string name, Vector2 position, Vector2 size, Color color, int sortingOrder, float roundness, int seed)
    {
        return CreateClayShape(name, worldRoot.transform, position, size, color, sortingOrder, roundness, seed);
    }

    private static GameObject CreateClayShape(string name, Transform parent, Vector2 localPosition, Vector2 size, Color color, int sortingOrder, float roundness, int seed)
    {
        GameObject item = new GameObject(name, typeof(SpriteRenderer));
        item.transform.SetParent(parent, false);
        item.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);

        SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
        renderer.sprite = ClaySpriteFactory.Rounded(color, 128, 64, roundness, seed);
        renderer.sortingOrder = sortingOrder;

        Vector2 spriteSize = renderer.sprite.bounds.size;
        item.transform.localScale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1f);
        return item;
    }

    private static GameObject CreateClayCircle(string name, Transform parent, Vector2 localPosition, float diameter, Color color, int sortingOrder, int seed)
    {
        GameObject item = new GameObject(name, typeof(SpriteRenderer));
        item.transform.SetParent(parent, false);
        item.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);

        SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
        renderer.sprite = ClaySpriteFactory.Circle(color, 96, seed);
        renderer.sortingOrder = sortingOrder;

        float spriteSize = renderer.sprite.bounds.size.x;
        item.transform.localScale = Vector3.one * (diameter / spriteSize);
        return item;
    }

private void CreateWorldText(string name, string content, Vector2 position, float size, Color color, int sortingOrder, Transform parent = null)
    {
        GameObject textObject = new GameObject(name, typeof(TextMeshPro));
        Transform actualParent = parent != null ? parent : worldRoot.transform;
        textObject.transform.SetParent(actualParent, false);
        textObject.transform.localPosition = position;
        textObject.transform.localScale = Vector3.one * size;

        TextMeshPro text = textObject.GetComponent<TextMeshPro>();
        text.text = content;
        text.fontSize = 4f;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.sortingOrder = sortingOrder;
        text.rectTransform.sizeDelta = new Vector2(8f, 2f);
    }

    private static RectTransform CreateUIPanel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, int seed)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        item.transform.SetParent(parent, false);

        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = item.GetComponent<Image>();
        image.sprite = ClaySpriteFactory.Rounded(color, 160, 80, 0.35f, seed);
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        return rect;
    }

    private static TMP_Text CreateUIText(string name, Transform parent, string content, float size, Color color,
        TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        item.transform.SetParent(parent, false);

        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        return text;
    }

    private static Button CreateUIButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
        Color color, UnityEngine.Events.UnityAction action, int seed)
    {
        RectTransform root = CreateUIPanel(name, parent, color, anchorMin, anchorMax, seed);
        Button button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = root.GetComponent<Image>();
        button.onClick.AddListener(action);
        if (root.GetComponent<UIButtonFeedback>() == null)
        {
            root.gameObject.AddComponent<UIButtonFeedback>();
        }

        CreateUIText("Label", root, label, 28, Color.white, TextAlignmentOptions.Center,
            new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f));
        return button;
    }
}
