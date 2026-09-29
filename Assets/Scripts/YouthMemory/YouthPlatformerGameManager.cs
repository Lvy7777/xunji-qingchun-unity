using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class YouthPlatformerGameManager : MonoBehaviour
{
    private const float WorldWidth = 1920f;
    private const float GroundHeight = 180f;
    private const float PlayerWidth = 76f;
    private const float PlayerHeight = 112f;
    private const float MoveSpeed = 640f;
    private const float JumpSpeed = 960f;
    private const float Gravity = -2300f;
    private const int StartingLives = 3;
    private const float CompletionDelay = 1.2f;

    private enum GameState { Inactive, Playing, Failed, Completing }

    private sealed class Collectible
    {
        public RectTransform Rect;
        public bool Collected;
    }

    public event Action ChallengeCompleted;

    private readonly List<Rect> platforms = new List<Rect>(5);
    private readonly List<Collectible> collectibles = new List<Collectible>(4);

    private RectTransform gameRoot;
    private RectTransform player;
    private TMP_Text progressText;
    private TMP_Text livesText;
    private TMP_Text statusText;
    private Button retryButton;
    private CanvasGroup gameGroup;

    private Vector2 playerPosition;
    private float verticalVelocity;
    private bool grounded;
    private bool leftHeld;
    private bool rightHeld;
    private bool completionRaised;
    private int collectedCount;
    private int lives;
    private GameState state;

    private void Update()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        TickMovement(Time.unscaledDeltaTime);
    }

    public void StartGame()
    {
        if (state == GameState.Playing || state == GameState.Completing)
        {
            return;
        }

        EnsureInterface();
        gameRoot.gameObject.SetActive(true);
        gameGroup.alpha = 1f;
        gameGroup.blocksRaycasts = true;

        collectedCount = 0;
        lives = StartingLives;
        completionRaised = false;
        foreach (Collectible collectible in collectibles)
        {
            collectible.Collected = false;
            collectible.Rect.gameObject.SetActive(true);
        }

        retryButton.gameObject.SetActive(false);
        statusText.text = "用方向键移动，空格键跳跃\n收集四枚青春记忆徽章";
        UpdateHud();
        ResetPlayer();
        state = GameState.Playing;
    }

    private void EnsureInterface()
    {
        if (gameRoot != null)
        {
            return;
        }

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            Debug.LogError("YouthPlatformerGameManager requires the chapter Canvas.");
            return;
        }

        gameRoot = CreatePanel("YouthPlatformerGame", canvas.transform, new Color(0.37f, 0.74f, 0.92f, 1f),
            Vector2.zero, new Vector2(WorldWidth, 1080f));
        Stretch(gameRoot);
        gameRoot.SetAsLastSibling();
        gameGroup = gameRoot.gameObject.AddComponent<CanvasGroup>();

        BuildBackdrop(gameRoot);
        BuildWorld(gameRoot);
        BuildHud(gameRoot);
        BuildTouchControls(gameRoot);
    }

    private void BuildBackdrop(Transform parent)
    {
        CreatePanel("SkyGlow", parent, new Color(0.72f, 0.89f, 0.98f, 0.45f),
            new Vector2(0f, 550f), new Vector2(WorldWidth, 1080f));
        CreatePanel("DistantHillLeft", parent, new Color(0.56f, 0.72f, 0.46f, 1f),
            new Vector2(0f, 190f), new Vector2(720f, 450f));
        CreatePanel("DistantHillRight", parent, new Color(0.45f, 0.64f, 0.43f, 1f),
            new Vector2(1180f, 180f), new Vector2(WorldWidth, 470f));

        CreateCloud(parent, "CloudOne", new Vector2(260f, 820f), 180f);
        CreateCloud(parent, "CloudTwo", new Vector2(1280f, 760f), 225f);
        CreateText("ExploreTitle", parent, "青春记忆 · 课堂探险", 34, Color.white,
            TextAlignmentOptions.Center, new Vector2(640f, 925f), new Vector2(1280f, 1010f));
    }

    private void BuildWorld(Transform parent)
    {
        AddPlatform(parent, "Ground", new Rect(0f, 0f, WorldWidth, GroundHeight), new Color(0.63f, 0.36f, 0.16f, 1f));
        AddPlatform(parent, "PlatformOne", new Rect(460f, 330f, 310f, 58f), new Color(0.76f, 0.45f, 0.20f, 1f));
        AddPlatform(parent, "PlatformTwo", new Rect(890f, 480f, 310f, 58f), new Color(0.82f, 0.52f, 0.24f, 1f));
        AddPlatform(parent, "PlatformThree", new Rect(1320f, 350f, 340f, 58f), new Color(0.71f, 0.41f, 0.18f, 1f));

        CreateCollectible(parent, new Vector2(300f, GroundHeight + 55f), "青春\n徽章");
        CreateCollectible(parent, new Vector2(570f, 445f), "青春\n徽章");
        CreateCollectible(parent, new Vector2(1000f, 595f), "青春\n徽章");
        CreateCollectible(parent, new Vector2(1450f, 465f), "青春\n徽章");

        player = CreatePanel("Player", parent, new Color(0.96f, 0.52f, 0.20f, 1f),
            Vector2.zero, new Vector2(PlayerWidth, PlayerHeight));
        CreateText("Face", player, "小禾", 22, Color.white, TextAlignmentOptions.Center,
            new Vector2(0f, 36f), new Vector2(PlayerWidth, 92f));
        CreatePanel("Backpack", player, new Color(0.25f, 0.48f, 0.34f, 1f),
            new Vector2(-15f, 22f), new Vector2(4f, 78f));
    }

    private void BuildHud(Transform parent)
    {
        RectTransform hud = CreatePanel("Hud", parent, new Color(0.06f, 0.12f, 0.18f, 0.72f),
            new Vector2(46f, 900f), new Vector2(585f, 1035f));
        progressText = CreateText("Progress", hud, string.Empty, 26, Color.white, TextAlignmentOptions.TopLeft,
            new Vector2(22f, 63f), new Vector2(505f, 118f));
        livesText = CreateText("Lives", hud, string.Empty, 22, new Color(1f, 0.86f, 0.52f, 1f), TextAlignmentOptions.TopLeft,
            new Vector2(22f, 18f), new Vector2(505f, 62f));

        RectTransform tip = CreatePanel("TaskTip", parent, new Color(0.04f, 0.10f, 0.16f, 0.80f),
            new Vector2(570f, 780f), new Vector2(1350f, 870f));
        statusText = CreateText("Text", tip, string.Empty, 25, Color.white, TextAlignmentOptions.Center,
            new Vector2(20f, 8f), new Vector2(760f, 82f));

        retryButton = CreateButton("RetryButton", parent, "重新挑战",
            new Vector2(780f, 400f), new Vector2(1140f, 500f), RestartAfterFailure);
        retryButton.gameObject.SetActive(false);
    }

    private void BuildTouchControls(Transform parent)
    {
        CreateHeldButton(parent, "MoveLeft", "◀", new Vector2(70f, 245f), new Vector2(225f, 390f), value => leftHeld = value);
        CreateHeldButton(parent, "MoveRight", "▶", new Vector2(245f, 245f), new Vector2(400f, 390f), value => rightHeld = value);

        Button jump = CreateButton("Jump", parent, "跳跃\n▲",
            new Vector2(1590f, 240f), new Vector2(1780f, 410f), TryJump);
        jump.GetComponent<Image>().color = new Color(0.92f, 0.43f, 0.16f, 0.94f);
    }

    private void TickMovement(float deltaTime)
    {
        float horizontal = Mathf.Clamp(Input.GetAxisRaw("Horizontal") + (leftHeld ? -1f : 0f) + (rightHeld ? 1f : 0f), -1f, 1f);
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            playerPosition.x = Mathf.Clamp(playerPosition.x + horizontal * MoveSpeed * deltaTime, 0f, WorldWidth - PlayerWidth);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            TryJump();
        }

        float previousBottom = playerPosition.y;
        verticalVelocity += Gravity * deltaTime;
        playerPosition.y += verticalVelocity * deltaTime;
        grounded = false;

        if (verticalVelocity <= 0f)
        {
            for (int i = 0; i < platforms.Count; i++)
            {
                Rect platform = platforms[i];
                float platformTop = platform.yMax;
                bool horizontalOverlap = playerPosition.x + PlayerWidth > platform.xMin && playerPosition.x < platform.xMax;
                bool crossedTop = previousBottom >= platformTop && playerPosition.y <= platformTop;

                if (horizontalOverlap && crossedTop)
                {
                    playerPosition.y = platformTop;
                    verticalVelocity = 0f;
                    grounded = true;
                    break;
                }
            }
        }

        if (playerPosition.y < -180f)
        {
            LoseLife();
            return;
        }

        player.anchoredPosition = playerPosition;
        CheckCollectibles();
    }

    private void TryJump()
    {
        if (state != GameState.Playing || !grounded)
        {
            return;
        }

        verticalVelocity = JumpSpeed;
        grounded = false;
        AudioManager.Instance?.PlayUi(UISound.Correct);
    }

    private void CheckCollectibles()
    {
        Rect playerBounds = new Rect(playerPosition.x, playerPosition.y, PlayerWidth, PlayerHeight);
        for (int i = 0; i < collectibles.Count; i++)
        {
            Collectible collectible = collectibles[i];
            if (collectible.Collected || !playerBounds.Overlaps(new Rect(collectible.Rect.anchoredPosition, collectible.Rect.sizeDelta)))
            {
                continue;
            }

            collectible.Collected = true;
            collectible.Rect.gameObject.SetActive(false);
            collectedCount++;
            UpdateHud();
            AudioManager.Instance?.PlayUi(UISound.Collect);

            if (collectedCount == collectibles.Count)
            {
                BeginCompletion();
            }
        }
    }

    private void BeginCompletion()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        state = GameState.Completing;
        statusText.text = "四枚青春记忆徽章已经收集完成！";
        StartCoroutine(CompleteRoutine());
    }

    private IEnumerator CompleteRoutine()
    {
        yield return new WaitForSecondsRealtime(CompletionDelay);

        float elapsed = 0f;
        while (elapsed < 0.35f)
        {
            elapsed += Time.unscaledDeltaTime;
            gameGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.35f);
            yield return null;
        }

        gameRoot.gameObject.SetActive(false);
        if (!completionRaised)
        {
            completionRaised = true;
            ChallengeCompleted?.Invoke();
        }
    }

    private void LoseLife()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        lives = Mathf.Max(0, lives - 1);
        UpdateHud();
        if (lives <= 0)
        {
            state = GameState.Failed;
            statusText.text = "先休息一下，再重新出发吧。";
            retryButton.gameObject.SetActive(true);
            return;
        }

        statusText.text = "没有关系，从旗帜处重新出发。";
        ResetPlayer();
    }

    private void RestartAfterFailure()
    {
        if (state == GameState.Failed)
        {
            StartGame();
        }
    }

    private void ResetPlayer()
    {
        playerPosition = new Vector2(115f, GroundHeight);
        verticalVelocity = 0f;
        grounded = true;
        player.anchoredPosition = playerPosition;
    }

    private void UpdateHud()
    {
        progressText.text = "记忆徽章  " + collectedCount + " / " + collectibles.Count;
        livesText.text = "体力  " + new string('●', lives) + new string('○', StartingLives - lives);
    }

    private void AddPlatform(Transform parent, string name, Rect bounds, Color color)
    {
        platforms.Add(bounds);
        RectTransform platform = CreatePanel(name, parent, color, bounds.position, bounds.position + bounds.size);
        CreatePanel("Top", platform, new Color(1f, 0.71f, 0.38f, 0.48f),
            new Vector2(0f, bounds.height - 12f), new Vector2(bounds.width, bounds.height));
    }

    private void CreateCollectible(Transform parent, Vector2 position, string label)
    {
        RectTransform collectible = CreatePanel("MemoryBadge", parent, new Color(1f, 0.79f, 0.20f, 1f),
            position, position + new Vector2(92f, 76f));
        CreateText("Label", collectible, label, 15, new Color(0.30f, 0.16f, 0.04f, 1f), TextAlignmentOptions.Center,
            new Vector2(0f, 0f), new Vector2(92f, 76f));
        collectibles.Add(new Collectible { Rect = collectible });
    }

    private static void CreateCloud(Transform parent, string name, Vector2 position, float width)
    {
        RectTransform cloud = CreatePanel(name, parent, new Color(1f, 1f, 1f, 0.78f),
            position, position + new Vector2(width, 55f));
        CreatePanel("CloudTop", cloud, new Color(1f, 1f, 1f, 0.78f),
            new Vector2(width * 0.25f, 35f), new Vector2(width * 0.72f, 95f));
    }

    private static void CreateHeldButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Action<bool> callback)
    {
        RectTransform root = CreatePanel(name, parent, new Color(0.06f, 0.10f, 0.16f, 0.62f), min, max);
        PlatformerTouchButton touchButton = root.gameObject.AddComponent<PlatformerTouchButton>();
        touchButton.HeldChanged += callback;
        CreateText("Label", root, label, 48, Color.white, TextAlignmentOptions.Center, Vector2.zero, max - min);
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        RectTransform root = CreatePanel(name, parent, new Color(0.90f, 0.47f, 0.18f, 0.96f), min, max);
        Button button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = root.GetComponent<Image>();
        button.onClick.AddListener(action);
        CreateText("Label", root, label, 28, Color.white, TextAlignmentOptions.Center, Vector2.zero, max - min);
        return button;
    }

    private static RectTransform CreatePanel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        item.transform.SetParent(parent, false);

        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = min;
        rect.sizeDelta = max - min;

        item.GetComponent<Image>().color = color;
        return rect;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, float size, Color color,
        TextAlignmentOptions alignment, Vector2 min, Vector2 max)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        item.transform.SetParent(parent, false);

        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = min;
        rect.sizeDelta = max - min;

        TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
