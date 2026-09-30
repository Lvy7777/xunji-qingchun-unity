using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class RedMemoryIntroController : MonoBehaviour
{
    [Header("Replaceable P-002 background art slots")]
    [SerializeField] private Sprite R_ART_001_RedMemory_Background_Back;
    [SerializeField] private Sprite R_ART_002_RedMemory_Background_Mid;
    [SerializeField] private Sprite R_ART_003_RedMemory_Background_Front;
    [Header("Replaceable P-002 UI/gameplay art slots")]
    [SerializeField] private Sprite R_ART_004_Mission_Panel;
    [SerializeField] private Sprite R_ART_005_Mission_Start_Button;
    [SerializeField] private Sprite R_ART_006_RedMemory_Fragment;
    [SerializeField] private Sprite R_ART_007_Clue_Key;
    [SerializeField] private Sprite R_ART_008_Hazard_Set;
    [SerializeField] private Sprite R_ART_009_Fail_Panel;
    [SerializeField] private Sprite R_ART_010_Retry_Button;
    [SerializeField] private Sprite R_ART_011_History_Photo_Frame;
    [SerializeField] private Sprite R_ART_012_History_Card_Background;
    [SerializeField] private Sprite R_ART_013_RedMemory_Reward;
    [SerializeField] private Sprite R_ART_014_MemoryReward_Background;
    [Header("Replaceable P-002 audio slots")]
    [SerializeField] private AudioClip RedMemory_Story_BGM;
    [SerializeField] private AudioClip RedMemory_Gameplay_BGM;
    [SerializeField] private AudioClip FragmentCollect;
    [SerializeField] private AudioClip KeyCollect;
    [SerializeField] private AudioClip PlayerJump;
    [SerializeField] private AudioClip PlayerLand;
    [SerializeField] private AudioClip PlayerHurt;
    [SerializeField] private AudioClip PlayerRespawn;
    [SerializeField] private AudioClip MissionOpen;
    [SerializeField] private AudioClip PhotoOpen;
    [SerializeField] private AudioClip MemoryReward;
    [SerializeField] private AudioClip ChapterClear;

    private Canvas canvas;
    private GameObject storyRoot;
    private GameObject gameplayRoot;
    private GameObject uiRoot;
    private GameObject dialogueRoot;
    private GameObject choiceRoot;
    private GameObject missionPanel;
    private GameObject gameplayHud;
    private GameObject historyCardPanel;
    private GameObject failPanel;
    private GameObject memoryRewardPanel;
    private GameObject handbookPanel;
    private GameObject completionPanel;
    private GameObject photoPrompt;
    private Image fadePanel;
    private Image storyDimmer;
    private CanvasGroup xiaoHeGroup;
    private CanvasGroup volunteerGroup;
    private TMP_Text speakerText;
    private TMP_Text dialogueText;
    private Button nextButton;
    private TMP_Text fragmentText;
    private TMP_Text keyText;
    private TMP_Text livesText;
    private TMP_Text toastText;
    private CanvasGroup toastGroup;
    private Button missionStartButton;
    private RedMemoryCameraFollow cameraFollow;
    private Camera gameplayCamera;
    private Vector2 lastSafePosition;
    private bool revealLine;
    private bool advanceLine;
    private bool transitionBusy;
    private bool invulnerable;
    private int choice = -1;
    private int fragmentCount;
    private int lives = 3;
    private bool hasKey;
    private bool goalResolved;
    private bool rewardGranted;

    public RedMemoryPlayerController Player { get; private set; }

    private void Awake()
    {
        BuildSceneStructure();
        BuildWorld();
        BuildUi();
        PortraitDisplayController.ApplyToLoadedScene(gameObject.scene);
    }

    private IEnumerator Start()
    {
        GameProgress.Ensure();
        gameplayRoot.SetActive(false);
        gameplayHud.SetActive(false);
        PlayBgm(RedMemory_Story_BGM);
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = Color.black;
        yield return FadeGraphic(fadePanel, 1f, 0f, 0.75f);
        yield return PlayChapterTitle();
        dialogueRoot.SetActive(true);
        yield return ShowLine("小禾", "老师，他们是谁？");
        yield return ShowLine("志愿者", "他们，是曾经生活在这片土地上的人。");
        yield return ShowLine("小禾", "他们做过什么？");
        yield return ShowChoice();
        if (choice == 0)
        {
            yield return ShowLine("志愿者", "这个问题……我们一起去找答案吧。");
        }
        else
        {
            yield return ShowLine("志愿者", "也许答案，就藏在我们接下来看到的东西里。");
            yield return ShowLine("志愿者", "我们一起去找答案吧。");
        }
        yield return ShowMission();
    }

    private void BuildSceneStructure()
    {
        transform.name = "RedMemoryRoot";
        GameObject backgroundRoot = Child("BackgroundRoot", transform);
        storyRoot = Child("StoryRoot", transform);
        Child("PortraitRoot", storyRoot.transform);
        dialogueRoot = Child("DialogueRoot", storyRoot.transform);
        choiceRoot = Child("ChoiceRoot", storyRoot.transform);
        gameplayRoot = Child("GameplayRoot", transform);
        Child("Environment", gameplayRoot.transform);
        Child("Platforms", gameplayRoot.transform);
        Child("Collectibles", gameplayRoot.transform);
        Child("Hazards", gameplayRoot.transform);
        Child("KeyArea", gameplayRoot.transform);
        Child("GoalArea", gameplayRoot.transform);
        uiRoot = Child("UIRoot", transform);
        Child("Managers", transform);

        gameplayCamera = Camera.main;
        if (gameplayCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            gameplayCamera = cameraObject.GetComponent<Camera>();
        }
        gameplayCamera.orthographic = true;
        gameplayCamera.orthographicSize = 5.2f;
        gameplayCamera.transform.position = new Vector3(9f, 1.5f, -10f);
        cameraFollow = gameplayCamera.GetComponent<RedMemoryCameraFollow>() ?? gameplayCamera.gameObject.AddComponent<RedMemoryCameraFollow>();

        CreateParallaxLayer("BG_Back", backgroundRoot.transform, R_ART_001_RedMemory_Background_Back, new Color(0.20f, 0.12f, 0.16f), 0.04f, 20f, 12f, 5);
        CreateParallaxLayer("BG_Mid", backgroundRoot.transform, R_ART_002_RedMemory_Background_Mid, new Color(0.35f, 0.17f, 0.18f), 0.09f, 19f, 8f, 4);
        CreateParallaxLayer("BG_Front", backgroundRoot.transform, R_ART_003_RedMemory_Background_Front, new Color(0.50f, 0.25f, 0.18f), 0.14f, 18f, 4f, 3);
    }

    private void BuildWorld()
    {
        Transform platforms = gameplayRoot.transform.Find("Platforms");
        Transform collectibles = gameplayRoot.transform.Find("Collectibles");
        Transform hazards = gameplayRoot.transform.Find("Hazards");
        Transform keyArea = gameplayRoot.transform.Find("KeyArea");
        Transform goalArea = gameplayRoot.transform.Find("GoalArea");

        CreatePlatform("Area1_Ground", platforms, new Vector2(6f, -2.7f), new Vector2(16f, 1f));
        CreatePlatform("Area2_Ground", platforms, new Vector2(22f, -2.7f), new Vector2(14f, 1f));
        CreatePlatform("Area3_Lower", platforms, new Vector2(35f, -2.7f), new Vector2(10f, 1f));
        CreatePlatform("Area3_PlatformA", platforms, new Vector2(31f, 0.2f), new Vector2(5f, 0.6f));
        CreatePlatform("Area3_PlatformB", platforms, new Vector2(37f, 2.2f), new Vector2(5f, 0.6f));
        CreatePlatform("Area4_Ground", platforms, new Vector2(47f, -2.7f), new Vector2(14f, 1f));
        CreatePlatform("Area5_Ground", platforms, new Vector2(59f, -2.7f), new Vector2(10f, 1f));
        CreatePlatform("Area5_KeyBranch", platforms, new Vector2(56f, 1.2f), new Vector2(5f, 0.6f));
        CreatePlatform("Area5_KeyTop", platforms, new Vector2(61f, 3.4f), new Vector2(5f, 0.6f));
        CreatePlatform("Area6_Ground", platforms, new Vector2(72f, -2.7f), new Vector2(18f, 1f));

        CreateSafePoint("Safe_Area1", new Vector2(2f, -1.5f), new Vector2(2f, 2f));
        CreateSafePoint("Safe_Area3", new Vector2(33f, -1.5f), new Vector2(2f, 2f));
        CreateSafePoint("Safe_Area5", new Vector2(56f, -1.5f), new Vector2(2f, 2f));

        GameObject playerObject = new GameObject("Player", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(RedMemoryPlayerController));
        playerObject.transform.SetParent(gameplayRoot.transform, false);
        playerObject.transform.position = new Vector3(1f, -1.5f, 0f);
        BoxCollider2D playerCollider = playerObject.GetComponent<BoxCollider2D>();
        playerCollider.size = new Vector2(0.75f, 1.45f);
        GameObject visuals = WorldSprite("PlayerVisual", playerObject.transform, ClaySpriteFactory.Rounded(new Color(0.95f, 0.67f, 0.48f), 72, 118, 0.28f), Vector3.zero, new Vector2(1f, 1.55f), 4);
        Player = playerObject.GetComponent<RedMemoryPlayerController>();
        Player.Configure(this, visuals.transform);
        Player.SetInputEnabled(false);
        lastSafePosition = playerObject.transform.position;
        cameraFollow.Configure(Player.transform);
        cameraFollow.Snap();

        CreatePickup("Fragment_1_Tutorial", collectibles, RedMemoryPickupKind.Fragment, 0, new Vector2(7f, -1.45f));
        CreatePickup("Fragment_2_Jump", collectibles, RedMemoryPickupKind.Fragment, 1, new Vector2(23f, 0.2f));
        CreatePickup("Fragment_3_Platform", collectibles, RedMemoryPickupKind.Fragment, 2, new Vector2(37f, 3.15f));
        CreatePickup("Fragment_4_Hazard", collectibles, RedMemoryPickupKind.Fragment, 3, new Vector2(49f, -1.25f));
        CreatePickup("Fragment_5_Goal", collectibles, RedMemoryPickupKind.Fragment, 4, new Vector2(70f, -1.35f));
        CreatePickup("ClueKey", keyArea, RedMemoryPickupKind.ClueKey, 0, new Vector2(61f, 4.25f));

        CreateHazard("Hazard_Area4_A", hazards, new Vector2(44f, -1.8f));
        CreateHazard("Hazard_Area4_B", hazards, new Vector2(48f, -1.8f));
        CreateHazard("Hazard_Area4_C", hazards, new Vector2(52f, -1.8f));

        GameObject goal = WorldSprite("GoalHistoricalPhoto", goalArea, ClaySpriteFactory.Rounded(new Color(0.92f, 0.78f, 0.50f), 100, 130, 0.12f), new Vector3(77f, -1.2f, 0f), new Vector2(1.6f, 2.2f), 3);
        BoxCollider2D goalCollider = goal.AddComponent<BoxCollider2D>();
        goalCollider.isTrigger = true;
        goalCollider.size = new Vector2(2.2f, 3f);
        goal.AddComponent<RedMemoryGoal>().Configure(this);
    }

    private void BuildUi()
    {
        if (FindObjectOfType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        GameObject canvasObject = new GameObject("RedMemoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(uiRoot.transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Destroy(storyRoot);
        storyRoot = UiContainer("StoryRoot", canvas.transform);
        UiContainer("PortraitRoot", storyRoot.transform);
        dialogueRoot = UiContainer("DialogueRoot", storyRoot.transform);
        choiceRoot = UiContainer("ChoiceRoot", storyRoot.transform);

        Image storyBackdrop = ImageObject("StoryBackground", storyRoot.transform, new Color(0.24f, 0.10f, 0.11f, 1f), Vector2.zero, Vector2.one);
        CreateText("PlaceLabel", storyBackdrop.transform, "红色文化实践点", 30, new Color(1f, 0.88f, 0.72f), TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.88f), new Vector2(0.45f, 0.96f));
        storyDimmer = ImageObject("StoryDimmer", storyRoot.transform, new Color(0f, 0f, 0f, 0f), Vector2.zero, Vector2.one);
        storyDimmer.raycastTarget = false;

        BuildPortraits(storyRoot.transform.Find("PortraitRoot"));
        BuildDialogue(dialogueRoot.transform);
        BuildChoices(choiceRoot.transform);
        BuildMission(canvas.transform);
        BuildHud(canvas.transform);
        BuildHistory(canvas.transform);
        BuildFail(canvas.transform);
        BuildRewardHandbookCompletion(canvas.transform);

        fadePanel = ImageObject("FadePanel", canvas.transform, Color.black, Vector2.zero, Vector2.one);
        fadePanel.transform.SetAsLastSibling();
        ClayThemeRuntime.ApplyCanvas(canvas, "RedMemory");
    }

    private void BuildPortraits(Transform parent)
    {
        GameObject xiaoHe = Panel("XiaoHe", parent, Color.clear, new Vector2(0.03f, 0.16f), new Vector2(0.31f, 0.78f));
        xiaoHeGroup = xiaoHe.GetComponent<CanvasGroup>();
        CreateText("PortraitPlaceholder", xiaoHe.transform, "小禾", 38, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        GameObject volunteer = Panel("Volunteer", parent, Color.clear, new Vector2(0.69f, 0.16f), new Vector2(0.97f, 0.78f));
        volunteerGroup = volunteer.GetComponent<CanvasGroup>();
        CreateText("PortraitPlaceholder", volunteer.transform, "志愿者", 38, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        xiaoHe.SetActive(false);
        volunteer.SetActive(false);
    }

    private void BuildDialogue(Transform parent)
    {
        GameObject panel = Panel("DialoguePanel", parent, new Color(0.98f, 0.92f, 0.82f, 0.98f), new Vector2(0.11f, 0.04f), new Vector2(0.89f, 0.25f));
        speakerText = CreateText("SpeakerName", panel.transform, string.Empty, 31, new Color(0.48f, 0.14f, 0.12f), TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.66f), new Vector2(0.45f, 0.93f));
        dialogueText = CreateText("DialogueText", panel.transform, string.Empty, 31, new Color(0.18f, 0.12f, 0.12f), TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.13f), new Vector2(0.80f, 0.68f));
        nextButton = ButtonObject("NextButton", panel.transform, "继续", new Vector2(0.82f, 0.18f), new Vector2(0.96f, 0.66f), AdvanceDialogue);
        nextButton.gameObject.SetActive(false);
        dialogueRoot.SetActive(false);
    }

    private void BuildChoices(Transform parent)
    {
        GameObject panel = Panel("ChoicePanel", parent, new Color(0.98f, 0.93f, 0.84f, 0.98f), new Vector2(0.27f, 0.31f), new Vector2(0.73f, 0.69f));
        CreateText("ChoicePrompt", panel.transform, "你想怎样寻找答案？", 30, new Color(0.35f, 0.15f, 0.12f), TextAlignmentOptions.Center, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.91f));
        ButtonObject("ChoiceOne", panel.transform, "① 我们一起去找答案吧", new Vector2(0.12f, 0.40f), new Vector2(0.88f, 0.58f), () => SelectChoice(0));
        ButtonObject("ChoiceTwo", panel.transform, "② 先看看这里留下了什么", new Vector2(0.12f, 0.17f), new Vector2(0.88f, 0.35f), () => SelectChoice(1));
        choiceRoot.SetActive(false);
    }

    private void BuildMission(Transform parent)
    {
        missionPanel = Panel("MissionPanel", parent, Color.white, new Vector2(0.29f, 0.20f), new Vector2(0.71f, 0.80f));
        Image missionImage = missionPanel.GetComponent<Image>();
        if (R_ART_004_Mission_Panel != null) { missionImage.sprite = R_ART_004_Mission_Panel; missionImage.preserveAspect = true; }
        CreateText("MissionTitle", missionPanel.transform, "红色记忆 · 寻迹", 43, new Color(0.54f, 0.13f, 0.11f), TextAlignmentOptions.Center, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.93f));
        CreateText("MissionBody", missionPanel.transform, "任务目标\n\n◆ 寻找 5 个红色记忆碎片\n◆ 找到线索钥匙\n◆ 抵达终点\n\n当前状态\n记忆碎片：0 / 5\n线索钥匙：未获得", 27, new Color(0.22f, 0.13f, 0.12f), TextAlignmentOptions.Center, new Vector2(0.09f, 0.25f), new Vector2(0.91f, 0.74f));
        missionStartButton = ButtonObject("MissionStartButton", missionPanel.transform, "开始寻迹", new Vector2(0.30f, 0.07f), new Vector2(0.70f, 0.20f), StartInvestigation);
        if (R_ART_005_Mission_Start_Button != null) missionStartButton.image.sprite = R_ART_005_Mission_Start_Button;
        missionPanel.SetActive(false);
    }

    private void BuildHud(Transform parent)
    {
        gameplayHud = Panel("GameplayHUD", parent, new Color(0.98f, 0.93f, 0.84f, 0.94f), new Vector2(0.025f, 0.79f), new Vector2(0.27f, 0.965f));
        fragmentText = CreateText("FragmentProgress", gameplayHud.transform, "记忆碎片  0 / 5", 27, Color.white, TextAlignmentOptions.Left, new Vector2(0.07f, 0.59f), new Vector2(0.93f, 0.91f));
        keyText = CreateText("KeyStatus", gameplayHud.transform, "线索钥匙：未获得", 25, Color.white, TextAlignmentOptions.Left, new Vector2(0.07f, 0.31f), new Vector2(0.93f, 0.62f));
        livesText = CreateText("LivesStatus", gameplayHud.transform, "♥  ♥  ♥", 27, new Color(0.72f, 0.12f, 0.16f), TextAlignmentOptions.Left, new Vector2(0.07f, 0.04f), new Vector2(0.93f, 0.35f));
        GameObject toast = Panel("ToastPanel", parent, new Color(0.12f, 0.08f, 0.08f, 0.88f), new Vector2(0.32f, 0.76f), new Vector2(0.68f, 0.86f));
        toastGroup = toast.GetComponent<CanvasGroup>();
        toastText = CreateText("ToastText", toast.transform, string.Empty, 26, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        toast.SetActive(false);
    }

    private void BuildHistory(Transform parent)
    {
        photoPrompt = Panel("HistoricalPhotoPrompt", parent, new Color(0.98f, 0.93f, 0.84f, 0.97f), new Vector2(0.31f, 0.30f), new Vector2(0.69f, 0.70f));
        CreateText("PhotoFoundText", photoPrompt.transform, "你找到了一张旧照片。", 31, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.77f), new Vector2(0.92f, 0.93f));
        Button photo = ButtonObject("HistoricalPhoto", photoPrompt.transform, "历史照片\n点击查看", new Vector2(0.25f, 0.18f), new Vector2(0.75f, 0.73f), OpenHistoryCard);
        if (R_ART_011_History_Photo_Frame != null) photo.image.sprite = R_ART_011_History_Photo_Frame;
        photoPrompt.SetActive(false);

        historyCardPanel = Panel("HistoryCardPanel", parent, Color.white, new Vector2(0.24f, 0.13f), new Vector2(0.76f, 0.87f));
        Image card = historyCardPanel.GetComponent<Image>(); if (R_ART_012_History_Card_Background != null) card.sprite = R_ART_012_History_Card_Background;
        CreateText("HistoryTitle", historyCardPanel.transform, "历史记忆", 42, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.94f));
        GameObject photoArea = Panel("HistoricalPhotoArea", historyCardPanel.transform, new Color(0.78f, 0.71f, 0.60f, 1f), new Vector2(0.13f, 0.33f), new Vector2(0.87f, 0.78f));
        CreateText("HistoricalPlaceholder", photoArea.transform, "[照片区域]\n\n真实历史资料将在此处展示。", 26, Color.white, TextAlignmentOptions.Center, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f));
        ButtonObject("HistoryContinueButton", historyCardPanel.transform, "继续", new Vector2(0.34f, 0.11f), new Vector2(0.66f, 0.24f), ContinueFromHistory);
        historyCardPanel.SetActive(false);
    }

    private void BuildFail(Transform parent)
    {
        failPanel = Panel("FailPanel", parent, Color.white, new Vector2(0.31f, 0.28f), new Vector2(0.69f, 0.72f));
        Image panelImage = failPanel.GetComponent<Image>(); if (R_ART_009_Fail_Panel != null) panelImage.sprite = R_ART_009_Fail_Panel;
        CreateText("FailTitle", failPanel.transform, "寻迹失败", 46, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.88f));
        CreateText("FailMessage", failPanel.transform, "别灰心，再找一次吧。", 28, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.49f), new Vector2(0.92f, 0.66f));
        Button retry = ButtonObject("RetryButton", failPanel.transform, "重新挑战", new Vector2(0.12f, 0.18f), new Vector2(0.46f, 0.36f), ReloadChapter);
        if (R_ART_010_Retry_Button != null) retry.image.sprite = R_ART_010_Retry_Button;
        ButtonObject("ReturnChapterButton", failPanel.transform, "返回章节", new Vector2(0.54f, 0.18f), new Vector2(0.88f, 0.36f), ReloadChapter);
        failPanel.SetActive(false);
    }

    private void BuildRewardHandbookCompletion(Transform parent)
    {
        memoryRewardPanel = Panel("MemoryRewardPanel", parent, Color.white, new Vector2(0.30f, 0.24f), new Vector2(0.70f, 0.76f));
        Image rewardBg = memoryRewardPanel.GetComponent<Image>(); if (R_ART_014_MemoryReward_Background != null) rewardBg.sprite = R_ART_014_MemoryReward_Background;
        CreateText("RewardHeading", memoryRewardPanel.transform, "记忆获得", 34, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.88f));
        Image rewardArt = ImageObject("RedMemoryRewardArt", memoryRewardPanel.transform, new Color(0.76f, 0.18f, 0.14f, 1f), new Vector2(0.34f, 0.37f), new Vector2(0.66f, 0.69f));
        if (R_ART_013_RedMemory_Reward != null) rewardArt.sprite = R_ART_013_RedMemory_Reward;
        CreateText("RewardTitle", memoryRewardPanel.transform, "红色记忆", 48, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.37f));
        CreateText("RewardSubtitle", memoryRewardPanel.transform, "铭记过去", 28, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.21f));
        memoryRewardPanel.SetActive(false);

        handbookPanel = Panel("HandbookContentPanel", parent, new Color(0.99f, 0.94f, 0.82f, 0.99f), new Vector2(0.23f, 0.14f), new Vector2(0.77f, 0.86f));
        CreateText("HandbookTitle", handbookPanel.transform, "研学手册", 45, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.94f));
        CreateText("HandbookProgress", handbookPanel.transform, "记忆收集：1 / 4", 31, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.81f));
        CreateText("RedMemoryCard", handbookPanel.transform, "红色记忆 —— 已获得 ✓", 31, new Color(0.68f, 0.12f, 0.12f), TextAlignmentOptions.Center, new Vector2(0.12f, 0.55f), new Vector2(0.88f, 0.68f));
        CreateText("LockedMemories", handbookPanel.transform, "乡土记忆 —— 未解锁\n\n青春记忆 —— 未解锁\n\n乡村记忆 —— 未解锁", 27, Color.white, TextAlignmentOptions.Center, new Vector2(0.12f, 0.22f), new Vector2(0.88f, 0.53f));
        ButtonObject("HandbookCloseButton", handbookPanel.transform, "关闭手册", new Vector2(0.34f, 0.07f), new Vector2(0.66f, 0.18f), CloseHandbook);
        handbookPanel.SetActive(false);

        completionPanel = Panel("ChapterCompletionPanel", parent, new Color(0.99f, 0.94f, 0.82f, 0.99f), new Vector2(0.30f, 0.28f), new Vector2(0.70f, 0.72f));
        CreateText("CompletionTitle", completionPanel.transform, "第一章完成", 48, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.65f), new Vector2(0.92f, 0.86f));
        CreateText("CompletionSubtitle", completionPanel.transform, "红色记忆 · 铭记过去", 31, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.43f), new Vector2(0.92f, 0.64f));
        ButtonObject("ContinueJourneyButton", completionPanel.transform, "继续旅程", new Vector2(0.28f, 0.15f), new Vector2(0.72f, 0.34f), ContinueJourney);
        completionPanel.SetActive(false);
    }

    private IEnumerator PlayChapterTitle()
    {
        GameObject title = Panel("ChapterTitle", canvas.transform, new Color(0f, 0f, 0f, 0.54f), new Vector2(0.25f, 0.29f), new Vector2(0.75f, 0.71f));
        CanvasGroup group = title.GetComponent<CanvasGroup>(); group.alpha = 0f;
        TMP_Text chapter = CreateText("ChapterLabel", title.transform, "第一章", 31, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.67f), new Vector2(0.92f, 0.83f));
        TMP_Text main = CreateText("ChapterMainTitle", title.transform, "红色记忆", 64, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.69f));
        TMP_Text sub = CreateText("ChapterSubtitle", title.transform, "那段不能被忘记的故事", 29, Color.white, TextAlignmentOptions.Center, new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.36f));
        chapter.alpha = 0f; main.alpha = 0f; sub.alpha = 0f;
        yield return FadeGroup(group, 0f, 1f, 0.25f);
        yield return FadeText(chapter, 0f, 1f, 0.35f);
        yield return new WaitForSecondsRealtime(0.25f);
        yield return FadeText(main, 0f, 1f, 0.45f);
        yield return new WaitForSecondsRealtime(0.3f);
        yield return FadeText(sub, 0f, 1f, 0.45f);
        yield return new WaitForSecondsRealtime(0.75f);
        yield return FadeGroup(group, 1f, 0f, 0.35f);
        Destroy(title);
    }

    private IEnumerator ShowLine(string speaker, string line)
    {
        dialogueRoot.SetActive(true);
        yield return SwitchPortrait(speaker);
        speakerText.text = speaker;
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        revealLine = false; advanceLine = false;
        nextButton.gameObject.SetActive(false);
        for (int i = 0; i < line.Length; i++)
        {
            if (revealLine) break;
            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSecondsRealtime(0.03f);
        }
        dialogueText.maxVisibleCharacters = int.MaxValue;
        nextButton.gameObject.SetActive(true);
        while (!advanceLine) yield return null;
        nextButton.gameObject.SetActive(false);
    }

    private IEnumerator SwitchPortrait(string speaker)
    {
        yield return FadePortrait(xiaoHeGroup, speaker == "小禾");
        yield return FadePortrait(volunteerGroup, speaker == "志愿者");
    }

    private IEnumerator FadePortrait(CanvasGroup group, bool visible)
    {
        if (group == null) yield break;
        if (visible) group.gameObject.SetActive(true);
        float from = group.alpha;
        float to = visible ? 1f : 0f;
        float elapsed = 0f;
        while (elapsed < 0.18f)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / 0.18f);
            yield return null;
        }
        group.alpha = to;
        if (!visible) group.gameObject.SetActive(false);
    }

    private IEnumerator ShowChoice()
    {
        nextButton.gameObject.SetActive(false);
        choice = -1;
        choiceRoot.SetActive(true);
        while (choice < 0) yield return null;
        choiceRoot.SetActive(false);
    }

    private IEnumerator ShowMission()
    {
        yield return SwitchPortrait(string.Empty);
        CanvasGroup dialogueGroup = dialogueRoot.GetComponentInChildren<CanvasGroup>();
        if (dialogueGroup != null) yield return FadeGroup(dialogueGroup, 1f, 0f, 0.25f);
        dialogueRoot.SetActive(false);
        storyDimmer.color = new Color(0f, 0f, 0f, 0.32f);
        missionPanel.SetActive(true);
        PlaySfx(MissionOpen);
        CanvasGroup group = missionPanel.GetComponent<CanvasGroup>();
        group.alpha = 0f; missionPanel.transform.localScale = Vector3.one * 0.78f;
        float elapsed = 0f;
        while (elapsed < 0.32f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.32f);
            group.alpha = t;
            missionPanel.transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.78f, Vector3.one * 1.04f, t);
            yield return null;
        }
        missionPanel.transform.localScale = Vector3.one;
    }

    private void AdvanceDialogue()
    {
        if (dialogueText.maxVisibleCharacters < dialogueText.text.Length) revealLine = true;
        else advanceLine = true;
    }

    private void SelectChoice(int selected)
    {
        if (choice < 0) choice = selected;
    }

    private void StartInvestigation()
    {
        if (transitionBusy || !missionStartButton.interactable) return;
        transitionBusy = true;
        missionStartButton.interactable = false;
        StartCoroutine(StartInvestigationRoutine());
    }

    private IEnumerator StartInvestigationRoutine()
    {
        CanvasGroup group = missionPanel.GetComponent<CanvasGroup>();
        Vector3 start = missionPanel.transform.localScale;
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.25f);
            missionPanel.transform.localScale = Vector3.Lerp(start, Vector3.one * 0.82f, t);
            group.alpha = 1f - t;
            yield return null;
        }
        missionPanel.SetActive(false);
        storyRoot.SetActive(false);
        gameplayRoot.SetActive(true);
        gameplayHud.SetActive(true);
        cameraFollow.Snap();
        PlayBgm(RedMemory_Gameplay_BGM);
        Player.SetInputEnabled(true);
        transitionBusy = false;
    }

    public void Collect(RedMemoryPickupKind kind)
    {
        if (kind == RedMemoryPickupKind.Fragment)
        {
            fragmentCount = Mathf.Min(5, fragmentCount + 1);
            fragmentText.text = "记忆碎片  " + fragmentCount + " / 5";
            PlaySfx(FragmentCollect);
            if (fragmentCount == 5) ShowToast("记忆已经逐渐清晰……", 1.5f);
        }
        else if (!hasKey)
        {
            hasKey = true;
            keyText.text = "线索钥匙：已获得 ✓";
            PlaySfx(KeyCollect);
            ShowToast("获得线索钥匙", 1.5f);
        }
    }

    public void HandleHazard(Vector2 source)
    {
        if (invulnerable || lives <= 0 || goalResolved) return;
        StartCoroutine(DamageRoutine(source, false));
    }

    public void HandleFall()
    {
        if (lives <= 0 || goalResolved) return;
        StartCoroutine(DamageRoutine(Player.transform.position, true));
    }

    private IEnumerator DamageRoutine(Vector2 source, bool fell)
    {
        invulnerable = true;
        lives--;
        UpdateLives();
        PlaySfx(PlayerHurt);
        if (!fell) Player.Knockback(source);
        yield return CameraShake();
        if (lives <= 0)
        {
            Player.SetInputEnabled(false);
            failPanel.SetActive(true);
            yield break;
        }
        if (fell)
        {
            Player.SetInputEnabled(false);
            fadePanel.transform.SetAsLastSibling(); fadePanel.gameObject.SetActive(true);
            yield return FadeGraphic(fadePanel, 0f, 0.62f, 0.15f);
            Player.Respawn(lastSafePosition);
            cameraFollow.Snap();
            PlaySfx(PlayerRespawn);
            yield return FadeGraphic(fadePanel, 0.62f, 0f, 0.18f);
            fadePanel.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(1f);
        invulnerable = false;
    }

    private IEnumerator CameraShake()
    {
        Vector3 basePosition = gameplayCamera.transform.position;
        for (int i = 0; i < 4; i++)
        {
            gameplayCamera.transform.position = basePosition + (Vector3)Random.insideUnitCircle * 0.06f;
            yield return new WaitForSeconds(0.025f);
        }
        gameplayCamera.transform.position = basePosition;
    }

    public void SetSafePosition(Vector2 position) => lastSafePosition = position;

    public void TryReachGoal()
    {
        if (goalResolved) return;
        if (fragmentCount < 5 && !hasKey) { ShowToast("这里还不是终点，再仔细找找吧。", 1.8f); return; }
        if (fragmentCount < 5) { ShowToast("还有一些记忆没有找到……", 1.8f); return; }
        if (!hasKey) { ShowToast("似乎还缺少打开记忆的线索……", 1.8f); return; }
        goalResolved = true;
        StartCoroutine(GoalRoutine());
    }

    private IEnumerator GoalRoutine()
    {
        Player.SetInputEnabled(false);
        CanvasGroup hud = gameplayHud.GetComponent<CanvasGroup>();
        yield return FadeGroup(hud, 1f, 0f, 0.3f);
        gameplayHud.SetActive(false);
        yield return new WaitForSeconds(0.45f);
        photoPrompt.SetActive(true);
    }

    private void OpenHistoryCard()
    {
        if (transitionBusy) return;
        transitionBusy = true;
        PlaySfx(PhotoOpen);
        photoPrompt.SetActive(false);
        historyCardPanel.SetActive(true);
        transitionBusy = false;
    }

    private void ContinueFromHistory()
    {
        if (transitionBusy) return;
        transitionBusy = true;
        historyCardPanel.SetActive(false);
        gameplayRoot.SetActive(false);
        storyRoot.SetActive(true);
        storyDimmer.gameObject.SetActive(true);
        StartCoroutine(EndingStoryRoutine());
    }

    private IEnumerator EndingStoryRoutine()
    {
        dialogueRoot.SetActive(true);
        CanvasGroup dialogueGroup = dialogueRoot.GetComponentInChildren<CanvasGroup>();
        if (dialogueGroup != null) { dialogueGroup.alpha = 1f; }
        PlayBgm(RedMemory_Story_BGM);
        yield return ShowLine("小禾", "原来我们生活的地方，还有这么多故事。");
        yield return ShowLine("志愿者", "所以我们今天走的每一步，都是在和过去见面。");
        yield return SwitchPortrait(string.Empty);
        dialogueRoot.SetActive(false);
        yield return RewardRoutine();
    }

    private IEnumerator RewardRoutine()
    {
        if (!rewardGranted)
        {
            rewardGranted = GameProgress.Ensure().TryGrantRedMemory();
        }
        storyDimmer.color = new Color(0f, 0f, 0f, 0.48f);
        memoryRewardPanel.SetActive(true);
        CanvasGroup group = memoryRewardPanel.GetComponent<CanvasGroup>(); group.alpha = 0f;
        Vector3 from = Vector3.one * 0.8f; memoryRewardPanel.transform.localScale = from;
        PlaySfx(MemoryReward);
        float elapsed = 0f;
        while (elapsed < 0.45f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.45f);
            group.alpha = t;
            float scale = t < 0.72f ? Mathf.Lerp(0.8f, 1.08f, t / 0.72f) : Mathf.Lerp(1.08f, 1f, (t - 0.72f) / 0.28f);
            memoryRewardPanel.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1.35f);
        memoryRewardPanel.SetActive(false);
        handbookPanel.SetActive(true);
        transitionBusy = false;
    }

    private void CloseHandbook()
    {
        handbookPanel.SetActive(false);
        completionPanel.SetActive(true);
        PlaySfx(ChapterClear);
    }

    private void ContinueJourney()
    {
        if (transitionBusy) return;
        transitionBusy = true;
        SceneManager.LoadScene("HometownMemory");
    }

    private void ReloadChapter()
    {
        if (transitionBusy) return;
        transitionBusy = true;
        SceneManager.LoadScene("RedMemory");
    }

    private void ShowToast(string message, float duration)
    {
        StopCoroutine(nameof(ToastRoutine));
        StartCoroutine(ToastRoutine(message, duration));
    }

    private IEnumerator ToastRoutine(string message, float duration)
    {
        toastText.transform.parent.gameObject.SetActive(true);
        toastText.text = message;
        yield return FadeGroup(toastGroup, 0f, 1f, 0.15f);
        yield return new WaitForSecondsRealtime(duration);
        yield return FadeGroup(toastGroup, 1f, 0f, 0.2f);
        toastText.transform.parent.gameObject.SetActive(false);
    }

    private void UpdateLives()
    {
        livesText.text = lives == 3 ? "♥  ♥  ♥" : lives == 2 ? "♥  ♥  ♡" : lives == 1 ? "♥  ♡  ♡" : "♡  ♡  ♡";
    }

    public void PlayJumpSfx() => PlaySfx(PlayerJump);
    public void PlayLandSfx() => PlaySfx(PlayerLand);
    private static void PlaySfx(AudioClip clip) { if (clip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(clip); }
    private static void PlayBgm(AudioClip clip) { if (clip != null && AudioManager.Instance != null) AudioManager.Instance.PlayBgm(clip); }

    private void CreateParallaxLayer(string name, Transform parent, Sprite art, Color fallback, float factor, float y, float height, int sorting)
    {
        Sprite sprite = art != null ? art : ClaySpriteFactory.Rounded(fallback, 256, 128, 0.05f, sorting);
        GameObject layer = WorldSprite(name, parent, sprite, new Vector3(0f, y - 20f, 4f + sorting * 0.1f), new Vector2(120f, height), -10 - sorting);
        layer.AddComponent<RedMemoryParallaxLayer>().Configure(gameplayCamera.transform, factor);
    }

    private void CreatePlatform(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject platform = WorldSprite(name, parent, ClaySpriteFactory.Rounded(new Color(0.52f, 0.29f, 0.20f), 160, 48, 0.26f, name.GetHashCode()), position, size, 0);
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>(); collider.size = Vector2.one;
    }

    private void CreatePickup(string name, Transform parent, RedMemoryPickupKind kind, int index, Vector2 position)
    {
        Sprite sprite = kind == RedMemoryPickupKind.Fragment ? R_ART_006_RedMemory_Fragment : R_ART_007_Clue_Key;
        if (sprite == null) sprite = ClaySpriteFactory.Circle(kind == RedMemoryPickupKind.Fragment ? new Color(0.86f, 0.20f, 0.16f) : new Color(1f, 0.72f, 0.18f), 96, index + 20);
        GameObject pickup = WorldSprite(name, parent, sprite, position, kind == RedMemoryPickupKind.Fragment ? new Vector2(0.75f, 0.75f) : new Vector2(0.9f, 0.9f), 3);
        CircleCollider2D collider = pickup.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = 0.6f;
        pickup.AddComponent<RedMemoryPickup>().Configure(this, kind, index);
    }

    private void CreateHazard(string name, Transform parent, Vector2 position)
    {
        Sprite sprite = R_ART_008_Hazard_Set != null ? R_ART_008_Hazard_Set : ClaySpriteFactory.Rounded(new Color(0.36f, 0.10f, 0.12f), 96, 64, 0.22f, name.GetHashCode());
        GameObject hazard = WorldSprite(name, parent, sprite, position, new Vector2(1.2f, 0.8f), 2);
        BoxCollider2D collider = hazard.AddComponent<BoxCollider2D>(); collider.isTrigger = true; collider.size = Vector2.one;
        hazard.AddComponent<RedMemoryHazard>().Configure(this);
    }

    private void CreateSafePoint(string name, Vector2 position, Vector2 size)
    {
        GameObject safe = new GameObject(name, typeof(BoxCollider2D), typeof(RedMemorySafePoint));
        safe.transform.SetParent(gameplayRoot.transform, false); safe.transform.position = position;
        BoxCollider2D collider = safe.GetComponent<BoxCollider2D>(); collider.isTrigger = true; collider.size = size;
        safe.GetComponent<RedMemorySafePoint>().Configure(this, position);
    }

    private static GameObject WorldSprite(string name, Transform parent, Sprite sprite, Vector3 position, Vector2 size, int sortingOrder)
    {
        GameObject item = new GameObject(name, typeof(SpriteRenderer));
        item.transform.SetParent(parent, false); item.transform.position = position; item.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = item.GetComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = sortingOrder;
        return item;
    }

    private static GameObject Child(string name, Transform parent)
    {
        GameObject child = new GameObject(name); child.transform.SetParent(parent, false); return child;
    }

    private static GameObject UiContainer(string name, Transform parent)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        return child;
    }

    private static GameObject Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        Image image = ImageObject(name, parent, color, min, max);
        image.gameObject.AddComponent<CanvasGroup>();
        return image.gameObject;
    }

    private static Image ImageObject(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        Image image = item.GetComponent<Image>(); image.color = color; return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>(); text.text = value; text.fontSize = size; text.color = color; text.alignment = alignment; text.enableWordWrapping = true; text.raycastTarget = false;
        return text;
    }

    private static Button ButtonObject(string name, Transform parent, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        Image image = ImageObject(name, parent, new Color(0.66f, 0.18f, 0.15f, 1f), min, max);
        Button button = image.gameObject.AddComponent<Button>(); button.onClick.AddListener(action); image.gameObject.AddComponent<UIButtonAnimator>();
        CreateText("Label", image.transform, label, 26, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        return button;
    }

    private static IEnumerator FadeGraphic(Graphic graphic, float from, float to, float duration)
    {
        Color color = graphic.color; color.a = from; graphic.color = color; float elapsed = 0f;
        while (elapsed < duration) { elapsed += Time.unscaledDeltaTime; color.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)); graphic.color = color; yield return null; }
        color.a = to; graphic.color = color;
    }

    private static IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        group.alpha = from; float elapsed = 0f;
        while (elapsed < duration) { elapsed += Time.unscaledDeltaTime; group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)); yield return null; }
        group.alpha = to;
    }

    private static IEnumerator FadeText(TMP_Text text, float from, float to, float duration)
    {
        Color color = text.color; color.a = from; text.color = color; float elapsed = 0f;
        while (elapsed < duration) { elapsed += Time.unscaledDeltaTime; color.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)); text.color = color; yield return null; }
        color.a = to; text.color = color;
    }
}
