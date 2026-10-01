using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class RedMemoryIntroController : MonoBehaviour
{
    private static readonly Color Paper = new Color(0.965f, 0.91f, 0.79f, 0.985f);
    private static readonly Color PaperLight = new Color(1f, 0.965f, 0.89f, 0.99f);
    private static readonly Color Brick = new Color(0.55f, 0.12f, 0.105f, 1f);
    private static readonly Color BrickDark = new Color(0.30f, 0.065f, 0.055f, 1f);
    private static readonly Color Gold = new Color(0.79f, 0.57f, 0.24f, 1f);
    private static readonly Color Ink = new Color(0.20f, 0.115f, 0.09f, 1f);
    private static readonly Color MutedInk = new Color(0.39f, 0.285f, 0.235f, 1f);

    [Header("Typography (central replacement point)")]
    [Tooltip("正文统一字体。请在正式字体导入后指向汉仪傲娇体简 TMP Font Asset。")]
    [SerializeField] private TMP_FontAsset bodyFont;
    [Tooltip("章节标题可使用独立标题字体；未指定时沿用正文字体。")]
    [SerializeField] private TMP_FontAsset titleFont;
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
    [Header("LiTuoTuo gameplay sprite set (P-002.5)")]
    [SerializeField] private Sprite XH_Player_Idle;
    [SerializeField] private Sprite XH_Player_Run_01;
    [SerializeField] private Sprite XH_Player_Run_02;
    [SerializeField] private Sprite XH_Player_Jump;
    [SerializeField] private Sprite XH_Player_Fall;
    [SerializeField] private Sprite XH_Player_Hurt;
    [SerializeField] private Sprite XH_Player_Cheer;
    [Header("Replaceable P-002 audio slots")]
    [SerializeField] private AudioClip RedMemory_Story_BGM;
    [SerializeField] private AudioClip RedMemory_Gameplay_BGM;
    [SerializeField] private AudioClip FragmentCollect;
    [SerializeField] private AudioClip KeyCollect;
    [SerializeField] private AudioClip PlayerJump;
    [SerializeField] private AudioClip PlayerLand;
    [SerializeField] private AudioClip PlayerHurt;
    [SerializeField] private AudioClip PlayerRespawn;
    [SerializeField] private AudioClip PlayerStomp;
    [SerializeField] private AudioClip EnemyAlert;
    [SerializeField] private AudioClip EnemyHit;
    [SerializeField] private AudioClip EnemyDefeat;
    [SerializeField] private AudioClip GlowCollect;
    [SerializeField] private AudioClip HealCollect;
    [SerializeField] private AudioClip MissionOpen;
    [SerializeField] private AudioClip CheckpointActivate;
    [SerializeField] private AudioClip CrumbleWarning;
    [SerializeField] private AudioClip PlatformBreak;
    [SerializeField] private AudioClip RockWarning;
    [SerializeField] private AudioClip RockImpact;
    [SerializeField] private AudioClip BoulderRoll;
    [SerializeField] private AudioClip RepairStart;
    [SerializeField] private AudioClip RepairComplete;
    [SerializeField] private AudioClip GoalUnlock;
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
    private Image dialoguePanelImage;
    private Image speakerPlateImage;
    private TMP_Text speakerText;
    private TMP_Text dialogueText;
    private Button nextButton;
    private TMP_Text fragmentText;
    private TMP_Text keyText;
    private TMP_Text livesText;
    private TMP_Text glowText;
    private TMP_Text resultText;
    private TMP_Text toastText;
    private CanvasGroup toastGroup;
    private Image damageVignette;
    private Button missionStartButton;
    private Button[] choiceButtons;
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
    private int memoryGlowCount;
    private int hiddenAreasFound;
    private int enemiesDefeated;
    private int checkpointCount;
    private bool groundStompTutorialShown;
    private bool repairTutorialShown;

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
        fadePanel.raycastTarget = true;
        fadePanel.color = Color.black;
        yield return FadeGraphic(fadePanel, 1f, 0f, 0.75f);
        fadePanel.raycastTarget = false;
        fadePanel.gameObject.SetActive(false);
        yield return PlayChapterTitle();
        dialogueRoot.SetActive(true);
        yield return ShowLine("栗拓拓", "老师，他们是谁？");
        yield return ShowLine("志愿者", "他们，是曾经生活在这片土地上的人。");
        yield return ShowLine("栗拓拓", "他们做过什么？");
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

        Transform[] areas = new Transform[8];
        for (int i = 0; i < areas.Length; i++) areas[i] = Child("Area_" + (i + 1).ToString("00"), platforms).transform;

        // Area 01: broad tutorial start, low step and one forgiving gap.
        CreatePlatform("Area01_Ground_A", areas[0], new Vector2(5f, -2.7f), new Vector2(12f, 1f));
        CreatePlatform("Area01_LowStep", areas[0], new Vector2(10.8f, -1.95f), new Vector2(2.4f, 0.5f));
        CreatePlatform("Area01_Ground_B", areas[0], new Vector2(15f, -2.7f), new Vector2(5.5f, 1f));

        // Area 02: stone steps and a visible high-route fragment.
        CreatePlatform("Area02_Ground_A", areas[1], new Vector2(21f, -2.7f), new Vector2(5f, 1f));
        CreatePlatform("Area02_Step_A", areas[1], new Vector2(24f, -1.75f), new Vector2(2.2f, 0.55f));
        CreatePlatform("Area02_Step_B", areas[1], new Vector2(27f, -0.75f), new Vector2(2.5f, 0.55f));
        CreatePlatform("Area02_Roof", areas[1], new Vector2(30f, 0.15f), new Vector2(3.2f, 0.55f));
        CreatePlatform("Area02_Ground_B", areas[1], new Vector2(31f, -2.7f), new Vector2(5f, 1f));

        // Area 03: first enemy and a rest/checkpoint after the encounter.
        CreatePlatform("Area03_Ground", areas[2], new Vector2(40f, -2.7f), new Vector2(13f, 1f));
        CreatePlatform("Area03_TutorialPerch", areas[2], new Vector2(37f, -0.8f), new Vector2(2.8f, 0.5f));

        // Area 04: moving platforms above a shallow ravine.
        CreatePlatform("Area04_Entry", areas[3], new Vector2(48f, -2.7f), new Vector2(3.5f, 1f));
        CreatePlatform("Area04_Exit", areas[3], new Vector2(62f, -2.7f), new Vector2(5f, 1f));
        CreateMovingPlatform("Area04_MoveHorizontal", areas[3], new Vector2(51f, -1.0f), new Vector2(3f, 0.55f), new Vector3(3.2f, 0f, 0f), 1.25f);
        CreateMovingPlatform("Area04_MoveVertical", areas[3], new Vector2(57.3f, -0.9f), new Vector2(3f, 0.55f), new Vector3(0f, 2.1f, 0f), 1.05f);

        // Area 05: courtyard main road plus readable upper key branch and shortcut.
        CreatePlatform("Area05_Main_A", areas[4], new Vector2(68f, -2.7f), new Vector2(6f, 1f));
        CreatePlatform("Area05_Branch_A", areas[4], new Vector2(69f, -0.45f), new Vector2(3f, 0.5f));
        CreateMovingPlatform("Area05_Branch_Move", areas[4], new Vector2(73.5f, 0.5f), new Vector2(2.7f, 0.5f), new Vector3(2.5f, 0f, 0f), 1.15f);
        CreatePlatform("Area05_KeyRoof", areas[4], new Vector2(79f, 1.25f), new Vector2(5f, 0.55f));
        CreatePlatform("Area05_Main_B", areas[4], new Vector2(80f, -2.7f), new Vector2(13f, 1f));

        // Area 06: crumble + falling-rock timing without crowding the player.
        CreatePlatform("Area06_Entry", areas[5], new Vector2(89f, -2.7f), new Vector2(4f, 1f));
        CreateCrumblingPlatform("Area06_Crumble_A", areas[5], new Vector2(93f, -1.75f), new Vector2(3.2f, 0.55f));
        CreateCrumblingPlatform("Area06_Crumble_B", areas[5], new Vector2(97f, -0.9f), new Vector2(3.2f, 0.55f));
        CreatePlatform("Area06_Exit", areas[5], new Vector2(102f, -2.7f), new Vector2(7f, 1f));

        // Area 07: short boulder pursuit with two small pits and a low ledge.
        CreatePlatform("Area07_Run_A", areas[6], new Vector2(108f, -2.7f), new Vector2(5f, 1f));
        CreatePlatform("Area07_Run_B", areas[6], new Vector2(114f, -2.7f), new Vector2(4.8f, 1f));
        CreatePlatform("Area07_LowPlatform", areas[6], new Vector2(118f, -1.75f), new Vector2(2.5f, 0.5f));
        CreatePlatform("Area07_Run_C", areas[6], new Vector2(122f, -2.7f), new Vector2(5f, 1f));

        // Area 08: repair the memory fault, then enter the quiet memorial zone.
        CreatePlatform("Area08_Entry", areas[7], new Vector2(128f, -2.7f), new Vector2(5f, 1f));
        CreatePlatform("Area08_Memorial", areas[7], new Vector2(141f, -2.7f), new Vector2(14f, 1f));

        GameObject playerObject = new GameObject("LiTuoTuo_Player", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(RedMemoryPlayerCombat), typeof(RedMemoryPlayerController));
        playerObject.layer = GameplayLayer("Player");
        playerObject.transform.SetParent(gameplayRoot.transform, false);
        playerObject.transform.position = new Vector3(1f, -1.5f, 0f);
        BoxCollider2D playerCollider = playerObject.GetComponent<BoxCollider2D>();
        playerCollider.size = new Vector2(0.75f, 1.45f);
        Sprite idleSprite = XH_Player_Idle != null
            ? XH_Player_Idle
            : ClaySpriteFactory.Rounded(new Color(0.95f, 0.67f, 0.48f), 72, 118, 0.28f);
        GameObject visuals = WorldSprite("LiTuoTuoGameplayVisual", playerObject.transform, idleSprite, new Vector3(0f, -0.72f, 0f), Vector2.one * 0.78f, 4);
        Player = playerObject.GetComponent<RedMemoryPlayerController>();
        RedMemoryPlayerVisual playerVisual = visuals.AddComponent<RedMemoryPlayerVisual>();
        playerVisual.Configure(Player, idleSprite, XH_Player_Run_01, XH_Player_Run_02, XH_Player_Jump, XH_Player_Fall, XH_Player_Hurt, XH_Player_Cheer);
        Player.Configure(this, visuals.transform);
        Player.SetInputEnabled(false);
        lastSafePosition = playerObject.transform.position;
        cameraFollow.Configure(Player.transform);
        cameraFollow.Snap();

        CreatePickup("Fragment_1_Tutorial", collectibles, RedMemoryPickupKind.Fragment, 0, new Vector2(11f, -1.25f));
        CreatePickup("Fragment_2_Roof", collectibles, RedMemoryPickupKind.Fragment, 1, new Vector2(30f, 1f));
        CreatePickup("Fragment_3_MovingPlatform", collectibles, RedMemoryPickupKind.Fragment, 2, new Vector2(58f, 2.2f));
        CreatePickup("Fragment_4_Trap", collectibles, RedMemoryPickupKind.Fragment, 3, new Vector2(101f, -1.25f));
        CreatePickup("Fragment_5_Memorial", collectibles, RedMemoryPickupKind.Fragment, 4, new Vector2(137f, -1.3f));
        CreatePickup("ClueKey", keyArea, RedMemoryPickupKind.ClueKey, 0, new Vector2(79f, 2.1f));
        CreatePickup("HiddenGlow_A", collectibles, RedMemoryPickupKind.MemoryGlow, 10, new Vector2(29f, 1.65f));
        CreatePickup("HiddenGlow_B", collectibles, RedMemoryPickupKind.MemoryGlow, 11, new Vector2(76f, 1.25f));
        CreatePickup("HiddenFlower", collectibles, RedMemoryPickupKind.HealingFlower, 12, new Vector2(84f, -1.45f));

        CreateEnemy("MemoryCreeper_Area03", hazards, false, new Vector2(42f, -1.62f));
        CreateEnemy("JumpBlob_Area05", hazards, true, new Vector2(76.8f, 2.05f));
        CreateEnemy("MemoryCreeper_Area06", hazards, false, new Vector2(102f, -1.62f));
        CreateCheckpoint("Checkpoint_AfterArea03", hazards, new Vector2(46f, -1.25f));
        CreateCheckpoint("Checkpoint_AfterArea06", hazards, new Vector2(105f, -1.25f));
        CreateFallingRock("FallingRock_Area06", hazards, new Vector2(99f, -1.2f));
        CreateRollingBoulder("RollingBoulder_Area07", hazards, new Vector2(106f, -1.3f));
        CreateDiscoveryZone("HiddenRoute_Roof", hazards, new Vector2(29f, 1.3f), new Vector2(5f, 2f));
        CreateDiscoveryZone("HiddenRoute_Key", hazards, new Vector2(78f, 1.4f), new Vector2(6f, 2.4f));
        CreateMemoryRepair("MemoryRepairPoint_Area08", hazards, areas[7], new Vector2(130.5f, -1.35f), new Vector2(135f, -2.1f));

        GameObject goal = WorldSprite("GoalHistoricalPhoto", goalArea, ClaySpriteFactory.Rounded(new Color(0.92f, 0.78f, 0.50f), 100, 130, 0.12f), new Vector3(145f, -1.2f, 0f), new Vector2(1.6f, 2.2f), 3);
        BoxCollider2D goalCollider = goal.AddComponent<BoxCollider2D>();
        goalCollider.isTrigger = true;
        goalCollider.size = new Vector2(2.2f, 3f);
        goal.AddComponent<RedMemoryGoal>().Configure(this);
    }

    private void BuildUi()
    {
        if (FindObjectOfType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        GameObject canvasObject = new GameObject("RedMemoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(ClayThemeOptOut));
        canvasObject.transform.SetParent(uiRoot.transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;

        if (bodyFont == null) bodyFont = Resources.Load<TMP_FontAsset>("Fonts/HYAoJiaoTiJian/HYAoJiaoTiJian");
        if (bodyFont == null) bodyFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansSC-GameTextV3");
        if (titleFont == null) titleFont = bodyFont;

        Destroy(storyRoot);
        storyRoot = UiContainer("StoryRoot", canvas.transform);
        Image storyBackdrop = ImageObject("StoryBackground", storyRoot.transform, new Color(0.18f, 0.04f, 0.05f, 0.16f), Vector2.zero, Vector2.one);
        storyBackdrop.raycastTarget = false;
        CreateText("PlaceLabel", storyBackdrop.transform, "红色文化实践点", 30, new Color(1f, 0.88f, 0.72f), TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.88f), new Vector2(0.45f, 0.96f));
        storyDimmer = ImageObject("StoryDimmer", storyRoot.transform, new Color(0f, 0f, 0f, 0f), Vector2.zero, Vector2.one);
        storyDimmer.raycastTarget = false;

        GameObject portraitRoot = UiContainer("PortraitRoot", storyRoot.transform);
        dialogueRoot = UiContainer("DialogueRoot", storyRoot.transform);
        choiceRoot = UiContainer("ChoiceRoot", storyRoot.transform);

        storyBackdrop.transform.SetAsFirstSibling();
        storyDimmer.transform.SetSiblingIndex(1);
        portraitRoot.transform.SetSiblingIndex(2);
        dialogueRoot.transform.SetSiblingIndex(3);
        choiceRoot.transform.SetSiblingIndex(4);
        choiceRoot.transform.SetAsLastSibling();

        BuildPortraits(portraitRoot.transform);
        BuildDialogue(dialogueRoot.transform);
        BuildChoices(choiceRoot.transform);
        BuildMission(canvas.transform);
        BuildHud(canvas.transform);
        BuildHistory(canvas.transform);
        BuildFail(canvas.transform);
        BuildRewardHandbookCompletion(canvas.transform);

        fadePanel = ImageObject("FadePanel", canvas.transform, Color.black, Vector2.zero, Vector2.one);
        fadePanel.raycastTarget = false;
        fadePanel.gameObject.SetActive(false);
        fadePanel.transform.SetAsLastSibling();
        ApplyRedMemoryPresentation();
    }

    private void BuildPortraits(Transform parent)
    {
        GameObject xiaoHe = Panel("XiaoHe", parent, Color.clear, new Vector2(0.035f, 0.245f), new Vector2(0.275f, 0.86f));
        xiaoHeGroup = xiaoHe.GetComponent<CanvasGroup>();
        CreateText("PortraitPlaceholder", xiaoHe.transform, "栗拓拓", 38, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        GameObject volunteer = Panel("Volunteer", parent, Color.clear, new Vector2(0.725f, 0.245f), new Vector2(0.965f, 0.86f));
        volunteerGroup = volunteer.GetComponent<CanvasGroup>();
        CreateText("PortraitPlaceholder", volunteer.transform, "志愿者", 38, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        xiaoHe.SetActive(false);
        volunteer.SetActive(false);
    }

    private void BuildDialogue(Transform parent)
    {
        GameObject panel = Panel("DialoguePanel", parent, PaperLight, new Vector2(0.105f, 0.045f), new Vector2(0.895f, 0.255f));
        dialoguePanelImage = panel.GetComponent<Image>();
        AddAccentStrip(panel.transform, new Vector2(0f, 0f), new Vector2(0.014f, 1f), Brick);
        GameObject namePlate = Panel("SpeakerNamePlate", panel.transform, Brick, new Vector2(0.035f, 0.69f), new Vector2(0.23f, 0.94f));
        speakerPlateImage = namePlate.GetComponent<Image>();
        speakerText = CreateText("SpeakerName", namePlate.transform, string.Empty, 30, PaperLight, TextAlignmentOptions.Center, new Vector2(0.07f, 0.02f), new Vector2(0.93f, 0.98f));
        dialogueText = CreateText("DialogueText", panel.transform, string.Empty, 30, Ink, TextAlignmentOptions.TopLeft, new Vector2(0.045f, 0.13f), new Vector2(0.79f, 0.65f));
        dialogueText.lineSpacing = 12f;
        dialogueText.margin = new Vector4(4f, 2f, 4f, 2f);
        nextButton = ButtonObject("NextButton", panel.transform, "继续  ›", new Vector2(0.815f, 0.19f), new Vector2(0.955f, 0.61f), AdvanceDialogue);
        nextButton.gameObject.SetActive(false);
        dialogueRoot.SetActive(false);
    }

    private void BuildChoices(Transform parent)
    {
        GameObject panel = Panel("ChoicePanel", parent, PaperLight, new Vector2(0.285f, 0.30f), new Vector2(0.715f, 0.70f));
        AddAccentStrip(panel.transform, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.795f), Gold);
        CreateText("ChoicePrompt", panel.transform, "你想怎样寻找答案？", 31, BrickDark, TextAlignmentOptions.Center, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.91f));
        choiceButtons = new[]
        {
            ButtonObject("ChoiceOne", panel.transform, "我们一起去找答案吧", new Vector2(0.11f, 0.40f), new Vector2(0.89f, 0.59f), () => SelectChoice(0)),
            ButtonObject("ChoiceTwo", panel.transform, "先看看这里留下了什么", new Vector2(0.11f, 0.16f), new Vector2(0.89f, 0.35f), () => SelectChoice(1))
        };
        choiceRoot.SetActive(false);
    }

    private void BuildMission(Transform parent)
    {
        missionPanel = Panel("MissionPanel", parent, PaperLight, new Vector2(0.305f, 0.175f), new Vector2(0.695f, 0.825f));
        Image missionImage = missionPanel.GetComponent<Image>();
        if (R_ART_004_Mission_Panel != null) { missionImage.sprite = R_ART_004_Mission_Panel; missionImage.preserveAspect = false; }
        Image heading = ImageObject("MissionHeadingBand", missionPanel.transform, Brick, new Vector2(0.04f, 0.79f), new Vector2(0.96f, 0.96f));
        RoundSurface(heading, 0.22f, 101);
        CreateText("MissionTitle", heading.transform, "章节任务 · 红色寻迹", 38, PaperLight, TextAlignmentOptions.Center, new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.95f));
        CreateText("MissionKicker", missionPanel.transform, "沿着记忆留下的微光，走近那段故事", 23, MutedInk, TextAlignmentOptions.Center, new Vector2(0.08f, 0.69f), new Vector2(0.92f, 0.78f));
        GameObject taskArea = Panel("MissionObjectiveArea", missionPanel.transform, new Color(0.92f, 0.83f, 0.67f, 0.52f), new Vector2(0.09f, 0.28f), new Vector2(0.91f, 0.67f));
        TMP_Text missionBody = CreateText("MissionBody", taskArea.transform, "01   寻找 5 个红色记忆碎片\n02   找到线索钥匙\n03   抵达终点照片处", 27, Ink, TextAlignmentOptions.MidlineLeft, new Vector2(0.09f, 0.08f), new Vector2(0.91f, 0.92f));
        missionBody.lineSpacing = 18f;
        missionStartButton = ButtonObject("MissionStartButton", missionPanel.transform, "开始寻迹", new Vector2(0.29f, 0.075f), new Vector2(0.71f, 0.205f), StartInvestigation);
        if (R_ART_005_Mission_Start_Button != null) missionStartButton.image.sprite = R_ART_005_Mission_Start_Button;
        missionPanel.SetActive(false);
    }

    private void BuildHud(Transform parent)
    {
        gameplayHud = Panel("GameplayHUD", parent, new Color(0.965f, 0.91f, 0.79f, 0.88f), new Vector2(0.032f, 0.72f), new Vector2(0.285f, 0.952f));
        AddAccentStrip(gameplayHud.transform, Vector2.zero, new Vector2(0.022f, 1f), Brick);
        CreateText("HudChapterLabel", gameplayHud.transform, "第一章 · 红色寻迹", 19, MutedInk, TextAlignmentOptions.Left, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.94f));
        fragmentText = CreateText("FragmentProgress", gameplayHud.transform, "记忆碎片   0 / 5", 29, BrickDark, TextAlignmentOptions.Left, new Vector2(0.08f, 0.47f), new Vector2(0.92f, 0.79f));
        keyText = CreateText("KeyStatus", gameplayHud.transform, "线索钥匙   未获得", 22, Ink, TextAlignmentOptions.Left, new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.49f));
        livesText = CreateText("LivesStatus", gameplayHud.transform, "体力   ♥  ♥  ♥", 27, Brick, TextAlignmentOptions.Left, new Vector2(0.08f, 0.09f), new Vector2(0.92f, 0.30f));
        glowText = CreateText("GlowProgress", gameplayHud.transform, "记忆微光   0", 19, MutedInk, TextAlignmentOptions.Left, new Vector2(0.08f, 0.0f), new Vector2(0.92f, 0.14f));
        GameObject toast = Panel("ToastPanel", parent, new Color(0.22f, 0.075f, 0.06f, 0.93f), new Vector2(0.355f, 0.80f), new Vector2(0.645f, 0.88f));
        toastGroup = toast.GetComponent<CanvasGroup>();
        toastText = CreateText("ToastText", toast.transform, string.Empty, 25, PaperLight, TextAlignmentOptions.Center, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f));
        toast.SetActive(false);
        damageVignette = ImageObject("DamageVignette", parent, new Color(0.7f, 0.02f, 0.01f, 0f), Vector2.zero, Vector2.one);
        damageVignette.raycastTarget = false;
        damageVignette.transform.SetAsLastSibling();
    }

    private void BuildHistory(Transform parent)
    {
        photoPrompt = Panel("HistoricalPhotoPrompt", parent, PaperLight, new Vector2(0.34f, 0.25f), new Vector2(0.66f, 0.75f));
        CreateText("PhotoFoundText", photoPrompt.transform, "发现一张旧照片", 31, BrickDark, TextAlignmentOptions.Center, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.93f));
        CreateText("PhotoFoundHint", photoPrompt.transform, "轻触照片，翻开这段记忆", 21, MutedInk, TextAlignmentOptions.Center, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.80f));
        Button photo = ButtonObject("HistoricalPhoto", photoPrompt.transform, "历史照片\n点击查看", new Vector2(0.22f, 0.14f), new Vector2(0.78f, 0.66f), OpenHistoryCard);
        photo.image.color = new Color(0.78f, 0.70f, 0.57f, 1f);
        if (R_ART_011_History_Photo_Frame != null) photo.image.sprite = R_ART_011_History_Photo_Frame;
        photoPrompt.SetActive(false);

        historyCardPanel = Panel("HistoryCardPanel", parent, PaperLight, new Vector2(0.235f, 0.105f), new Vector2(0.765f, 0.895f));
        Image card = historyCardPanel.GetComponent<Image>(); if (R_ART_012_History_Card_Background != null) card.sprite = R_ART_012_History_Card_Background;
        AddAccentStrip(historyCardPanel.transform, new Vector2(0.06f, 0.865f), new Vector2(0.94f, 0.88f), Gold);
        CreateText("HistoryTitle", historyCardPanel.transform, "历史资料卡", 40, BrickDark, TextAlignmentOptions.Center, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.95f));
        CreateText("HistoryArchiveLabel", historyCardPanel.transform, "RED MEMORY · ARCHIVE 01", 17, MutedInk, TextAlignmentOptions.Center, new Vector2(0.08f, 0.785f), new Vector2(0.92f, 0.84f));
        GameObject photoArea = Panel("HistoricalPhotoArea", historyCardPanel.transform, new Color(0.73f, 0.655f, 0.52f, 1f), new Vector2(0.12f, 0.40f), new Vector2(0.88f, 0.76f));
        CreateText("HistoricalPlaceholder", photoArea.transform, "历史照片资料待补充", 24, PaperLight, TextAlignmentOptions.Center, new Vector2(0.07f, 0.08f), new Vector2(0.93f, 0.92f));
        CreateText("HistoryBody", historyCardPanel.transform, "展陈说明\n相关史料内容将在资料确认后补充。", 23, Ink, TextAlignmentOptions.TopLeft, new Vector2(0.13f, 0.22f), new Vector2(0.87f, 0.37f));
        ButtonObject("HistoryContinueButton", historyCardPanel.transform, "收好资料", new Vector2(0.35f, 0.075f), new Vector2(0.65f, 0.18f), ContinueFromHistory);
        historyCardPanel.SetActive(false);
    }

    private void BuildFail(Transform parent)
    {
        failPanel = Panel("FailPanel", parent, PaperLight, new Vector2(0.325f, 0.275f), new Vector2(0.675f, 0.725f));
        Image panelImage = failPanel.GetComponent<Image>(); if (R_ART_009_Fail_Panel != null) panelImage.sprite = R_ART_009_Fail_Panel;
        AddAccentStrip(failPanel.transform, new Vector2(0f, 0.91f), Vector2.one, Brick);
        CreateText("FailTitle", failPanel.transform, "这段记忆还没有找完整……", 36, BrickDark, TextAlignmentOptions.Center, new Vector2(0.08f, 0.65f), new Vector2(0.92f, 0.84f));
        CreateText("FailMessage", failPanel.transform, "从最近的记录点继续，已找到的核心碎片会保留。", 22, MutedInk, TextAlignmentOptions.Center, new Vector2(0.08f, 0.43f), new Vector2(0.92f, 0.62f));
        Button retry = ButtonObject("RetryButton", failPanel.transform, "重新尝试", new Vector2(0.16f, 0.17f), new Vector2(0.48f, 0.35f), RetryFromCheckpoint);
        if (R_ART_010_Retry_Button != null) retry.image.sprite = R_ART_010_Retry_Button;
        ButtonObject("ReturnChapterButton", failPanel.transform, "从头回顾", new Vector2(0.52f, 0.17f), new Vector2(0.84f, 0.35f), ReloadChapter);
        failPanel.SetActive(false);
    }

    private void BuildRewardHandbookCompletion(Transform parent)
    {
        memoryRewardPanel = Panel("MemoryRewardPanel", parent, new Color(0.35f, 0.075f, 0.06f, 0.985f), new Vector2(0.315f, 0.20f), new Vector2(0.685f, 0.80f));
        Image rewardBg = memoryRewardPanel.GetComponent<Image>(); if (R_ART_014_MemoryReward_Background != null) rewardBg.sprite = R_ART_014_MemoryReward_Background;
        CreateText("RewardHeading", memoryRewardPanel.transform, "记 忆 收 集 成 功", 27, new Color(0.95f, 0.79f, 0.49f), TextAlignmentOptions.Center, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.91f));
        Image rewardArt = ImageObject("RedMemoryRewardArt", memoryRewardPanel.transform, Gold, new Vector2(0.34f, 0.43f), new Vector2(0.66f, 0.73f));
        RoundSurface(rewardArt, 0.5f, 113);
        if (R_ART_013_RedMemory_Reward != null) rewardArt.sprite = R_ART_013_RedMemory_Reward;
        CreateText("RewardTitle", memoryRewardPanel.transform, "红色记忆", 49, PaperLight, TextAlignmentOptions.Center, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.42f));
        CreateText("RewardSubtitle", memoryRewardPanel.transform, "铭记过去", 27, new Color(0.95f, 0.79f, 0.49f), TextAlignmentOptions.Center, new Vector2(0.08f, 0.13f), new Vector2(0.92f, 0.25f));
        memoryRewardPanel.SetActive(false);

        handbookPanel = Panel("HandbookContentPanel", parent, PaperLight, new Vector2(0.235f, 0.11f), new Vector2(0.765f, 0.89f));
        AddAccentStrip(handbookPanel.transform, new Vector2(0.49f, 0.05f), new Vector2(0.51f, 0.95f), new Color(0.49f, 0.30f, 0.18f, 0.22f));
        CreateText("HandbookTitle", handbookPanel.transform, "研学手册", 43, BrickDark, TextAlignmentOptions.Center, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.95f));
        CreateText("HandbookProgress", handbookPanel.transform, "记忆收集  ·  1 / 4", 24, MutedInk, TextAlignmentOptions.Center, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.84f));
        GameObject obtained = Panel("RedMemoryEntry", handbookPanel.transform, new Color(0.72f, 0.16f, 0.12f, 0.12f), new Vector2(0.09f, 0.56f), new Vector2(0.91f, 0.72f));
        AddAccentStrip(obtained.transform, Vector2.zero, new Vector2(0.018f, 1f), Brick);
        CreateText("RedMemoryCard", obtained.transform, "红色记忆     已获得  ✓", 29, BrickDark, TextAlignmentOptions.Center, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f));
        CreateText("LockedMemories", handbookPanel.transform, "02  乡土记忆     未解锁\n\n03  青春记忆     未解锁\n\n04  乡村记忆     未解锁", 25, MutedInk, TextAlignmentOptions.Center, new Vector2(0.12f, 0.24f), new Vector2(0.88f, 0.54f));
        CreateText("HandbookPage", handbookPanel.transform, "—  01  —", 18, MutedInk, TextAlignmentOptions.Center, new Vector2(0.42f, 0.18f), new Vector2(0.58f, 0.23f));
        ButtonObject("HandbookCloseButton", handbookPanel.transform, "合上手册", new Vector2(0.35f, 0.065f), new Vector2(0.65f, 0.165f), CloseHandbook);
        handbookPanel.SetActive(false);

        completionPanel = Panel("ChapterCompletionPanel", parent, PaperLight, new Vector2(0.315f, 0.265f), new Vector2(0.685f, 0.735f));
        AddAccentStrip(completionPanel.transform, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.815f), Gold);
        CreateText("CompletionTitle", completionPanel.transform, "第一章 · 完成", 46, BrickDark, TextAlignmentOptions.Center, new Vector2(0.08f, 0.65f), new Vector2(0.92f, 0.84f));
        CreateText("CompletionSubtitle", completionPanel.transform, "红色记忆  ·  铭记过去", 29, MutedInk, TextAlignmentOptions.Center, new Vector2(0.08f, 0.43f), new Vector2(0.92f, 0.62f));
        resultText = CreateText("ChapterResult", completionPanel.transform, string.Empty, 20, Ink, TextAlignmentOptions.Center, new Vector2(0.08f, 0.29f), new Vector2(0.92f, 0.49f));
        ButtonObject("ContinueJourneyButton", completionPanel.transform, "继续旅程", new Vector2(0.28f, 0.15f), new Vector2(0.72f, 0.34f), ContinueJourney);
        completionPanel.SetActive(false);
    }

    private IEnumerator PlayChapterTitle()
    {
        GameObject title = Panel("ChapterTitle", canvas.transform, new Color(0.18f, 0.035f, 0.03f, 0.82f), new Vector2(0.29f, 0.31f), new Vector2(0.71f, 0.69f));
        CanvasGroup group = title.GetComponent<CanvasGroup>(); group.alpha = 0f;
        AddAccentStrip(title.transform, new Vector2(0.18f, 0.76f), new Vector2(0.82f, 0.775f), Gold);
        TMP_Text chapter = CreateText("ChapterLabel", title.transform, "第 一 章", 27, new Color(0.95f, 0.79f, 0.49f), TextAlignmentOptions.Center, new Vector2(0.08f, 0.69f), new Vector2(0.92f, 0.82f), true);
        TMP_Text main = CreateText("ChapterMainTitle", title.transform, "红色记忆", 62, PaperLight, TextAlignmentOptions.Center, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.69f), true);
        TMP_Text sub = CreateText("ChapterSubtitle", title.transform, "那段不能被忘记的故事", 26, new Color(0.95f, 0.86f, 0.70f), TextAlignmentOptions.Center, new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.36f), true);
        chapter.alpha = 0f; main.alpha = 0f; sub.alpha = 0f;
        yield return FadeGroup(group, 0f, 1f, 0.22f);
        yield return FadeText(chapter, 0f, 1f, 0.28f);
        yield return new WaitForSecondsRealtime(0.12f);
        yield return FadeText(main, 0f, 1f, 0.36f);
        yield return new WaitForSecondsRealtime(0.16f);
        yield return FadeText(sub, 0f, 1f, 0.32f);
        yield return new WaitForSecondsRealtime(0.62f);
        yield return FadeGroup(group, 1f, 0f, 0.28f);
        Destroy(title);
    }

    private IEnumerator ShowLine(string speaker, string line)
    {
        dialogueRoot.SetActive(true);
        yield return SwitchPortrait(speaker);
        speakerText.text = speaker;
        bool xiaoHeSpeaking = speaker == "栗拓拓";
        if (speakerPlateImage != null) speakerPlateImage.color = xiaoHeSpeaking ? new Color(0.68f, 0.25f, 0.18f, 1f) : Brick;
        if (dialoguePanelImage != null) dialoguePanelImage.color = xiaoHeSpeaking ? new Color(1f, 0.955f, 0.86f, 0.99f) : PaperLight;
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
        yield return FadePortrait(xiaoHeGroup, speaker == "栗拓拓");
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
        yield return new WaitForSecondsRealtime(0.16f);
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
        if (choice >= 0) return;
        choice = selected;
        if (choiceButtons == null) return;
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i == selected)
            {
                UIButtonAnimator animator = choiceButtons[i].GetComponent<UIButtonAnimator>();
                if (animator != null) animator.enabled = false;
                choiceButtons[i].image.color = Gold;
                choiceButtons[i].transform.localScale = Vector3.one * 1.025f;
            }
            else
            {
                choiceButtons[i].interactable = false;
            }
        }
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
        ShowToast("A / D  移动    SPACE  跳跃", 2.6f);
        transitionBusy = false;
    }

    public void Collect(RedMemoryPickupKind kind)
    {
        if (kind == RedMemoryPickupKind.Fragment)
        {
            fragmentCount = Mathf.Min(5, fragmentCount + 1);
            fragmentText.text = "记忆碎片  " + fragmentCount + " / 5";
            PlaySfx(FragmentCollect);
            ShowToast("获得记忆碎片 " + fragmentCount + " / 5", 1.25f);
            if (fragmentCount == 5) ShowToast("记忆已经逐渐清晰……", 1.5f);
        }
        else if (kind == RedMemoryPickupKind.ClueKey && !hasKey)
        {
            hasKey = true;
            keyText.text = "线索钥匙：已获得 ✓";
            PlaySfx(KeyCollect);
            ShowToast("获得线索钥匙！", 1.5f);
        }
        else if (kind == RedMemoryPickupKind.MemoryGlow)
        {
            memoryGlowCount += 10;
            glowText.text = "记忆微光   " + memoryGlowCount;
            PlaySfx(GlowCollect);
            ShowToast("记忆微光 +10", 0.8f);
        }
        else if (kind == RedMemoryPickupKind.HealingFlower)
        {
            lives = Mathf.Min(3, lives + 1);
            UpdateLives();
            PlaySfx(HealCollect);
            ShowToast("小红花：体力恢复", 0.9f);
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
        StartCoroutine(DamageFeedback());
        yield return CameraShake(0.07f);
        if (lives <= 0)
        {
            Player.SetInputEnabled(false);
            Player.PlayDefeat();
            failPanel.SetActive(true);
            invulnerable = false;
            yield break;
        }
        if (fell)
        {
            Player.SetInputEnabled(false);
            fadePanel.transform.SetAsLastSibling(); fadePanel.gameObject.SetActive(true);
            fadePanel.raycastTarget = true;
            yield return FadeGraphic(fadePanel, 0f, 0.62f, 0.15f);
            Player.Respawn(lastSafePosition);
            cameraFollow.Snap();
            PlaySfx(PlayerRespawn);
            yield return FadeGraphic(fadePanel, 0.62f, 0f, 0.18f);
            fadePanel.raycastTarget = false;
            fadePanel.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(1f);
        invulnerable = false;
    }

    private IEnumerator CameraShake(float strength = 0.06f)
    {
        Vector3 basePosition = gameplayCamera.transform.position;
        for (int i = 0; i < 4; i++)
        {
            gameplayCamera.transform.position = basePosition + (Vector3)Random.insideUnitCircle * strength;
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
        PlaySfx(GoalUnlock);
        StartCoroutine(GoalRoutine());
    }

    private IEnumerator GoalRoutine()
    {
        Player.SetInputEnabled(false);
        Player.PlayCheer();
        yield return new WaitForSecondsRealtime(0.55f);
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
        yield return ShowLine("栗拓拓", "原来我们生活的地方，还有这么多故事。");
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
        if (resultText != null)
        {
            resultText.text = "红色记忆碎片  5 / 5    记忆微光  " + memoryGlowCount + "\n发现隐藏区域  " + hiddenAreasFound + " / 2    击败墨团仔  " + enemiesDefeated + (hiddenAreasFound >= 2 ? "\n探索得很仔细！" : string.Empty);
        }
        PlaySfx(ChapterClear);
    }

    private void RetryFromCheckpoint()
    {
        if (transitionBusy) return;
        lives = 3;
        invulnerable = false;
        UpdateLives();
        failPanel.SetActive(false);
        Player.Respawn(lastSafePosition);
        cameraFollow.Snap();
        PlaySfx(PlayerRespawn);
        ShowToast("从记录点继续", 1.1f);
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
        livesText.text = "体力   " + (lives == 3 ? "♥  ♥  ♥" : lives == 2 ? "♥  ♥  ♡" : lives == 1 ? "♥  ♡  ♡" : "♡  ♡  ♡");
    }

    private IEnumerator DamageFeedback()
    {
        if (damageVignette == null) yield break;
        CanvasGroup hud = gameplayHud.GetComponent<CanvasGroup>();
        Vector3 hudStart = gameplayHud.transform.localPosition;
        for (int i = 0; i < 6; i++)
        {
            float alpha = i < 3 ? 0.18f - i * 0.04f : 0.04f;
            damageVignette.color = new Color(0.7f, 0.02f, 0.01f, alpha);
            gameplayHud.transform.localPosition = hudStart + (Vector3)Random.insideUnitCircle * 5f;
            yield return new WaitForSecondsRealtime(0.035f);
        }
        gameplayHud.transform.localPosition = hudStart;
        damageVignette.color = new Color(0.7f, 0.02f, 0.01f, 0f);
        if (hud != null) hud.alpha = 1f;
    }

    public void ActivateCheckpoint(Vector2 position)
    {
        lastSafePosition = position;
        checkpointCount++;
        PlaySfx(CheckpointActivate);
        ShowToast("记录点已更新", 1.2f);
    }

    public void DiscoverHiddenArea()
    {
        hiddenAreasFound = Mathf.Min(2, hiddenAreasFound + 1);
        ShowToast("发现隐藏支路 " + hiddenAreasFound + " / 2", 1.1f);
    }

    public void NotifyGroundStompStarted()
    {
        if (!groundStompTutorialShown) groundStompTutorialShown = true;
    }

    public void NotifyGroundStompImpact(Vector2 position)
    {
        PlaySfx(PlayerStomp);
        StartCoroutine(CameraShake(0.055f));
    }

    public void NotifyEnemyStomp(Vector2 position)
    {
        PlaySfx(EnemyHit);
        StartCoroutine(CameraShake(0.045f));
    }

    public void NotifyEnemyAlert(Vector2 position)
    {
        PlaySfx(EnemyAlert);
        ShowToast("！ 跃团仔即将跳跃", 0.55f);
    }

    public void NotifyEnemyDefeated(EnemyBase enemy)
    {
        enemiesDefeated++;
        PlaySfx(EnemyDefeat);
        if (Random.value <= 0.18f) CreatePickup("EnemyHealingFlower", gameplayRoot.transform.Find("Collectibles"), RedMemoryPickupKind.HealingFlower, enemiesDefeated + 20, enemy.transform.position + Vector3.up * 0.5f);
        else CreatePickup("EnemyMemoryGlow", gameplayRoot.transform.Find("Collectibles"), RedMemoryPickupKind.MemoryGlow, enemiesDefeated + 20, enemy.transform.position + Vector3.up * 0.5f);
    }

    public void NotifyCrumbleWarning(Vector2 position) => PlaySfx(CrumbleWarning);
    public void NotifyPlatformBroken(Vector2 position) => PlaySfx(PlatformBreak);
    public void NotifyRockWarning(Vector2 position) => PlaySfx(RockWarning);
    public void NotifyRockImpact(Vector2 position) { PlaySfx(RockImpact); StartCoroutine(CameraShake(0.07f)); }
    public void NotifyBoulderStarted() { PlaySfx(BoulderRoll); ShowToast("滚石来了，向前跑！", 1.1f); }
    public void NotifyRepairStarted() => PlaySfx(RepairStart);
    public void NotifyRepairCompleted() { PlaySfx(RepairComplete); ShowToast("记忆断层已修复", 1.4f); }
    public void ShowRepairTutorial()
    {
        if (repairTutorialShown) return;
        repairTutorialShown = true;
        ShowToast("空中按 S 下压唤醒记忆", 2.2f);
    }

    public void PlayJumpSfx() => PlaySfx(PlayerJump);
    public void PlayLandSfx() => PlaySfx(PlayerLand);
    private static void PlaySfx(AudioClip clip) { if (clip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(clip); }
    private static void PlayBgm(AudioClip clip) { if (clip != null && AudioManager.Instance != null) AudioManager.Instance.PlayBgm(clip); }

    private void CreateParallaxLayer(string name, Transform parent, Sprite art, Color fallback, float factor, float y, float height, int sorting)
    {
        Sprite sprite = art != null ? art : ClaySpriteFactory.Rounded(fallback, 256, 128, 0.05f, sorting);
        GameObject layer;
        if (art != null)
        {
            layer = new GameObject(name, typeof(SpriteRenderer));
            layer.transform.SetParent(parent, false);
            layer.transform.position = new Vector3(gameplayCamera.transform.position.x, gameplayCamera.transform.position.y, 4f + sorting * 0.1f);
            float targetHeight = gameplayCamera.orthographicSize * 2f * 1.04f;
            // Overscan prevents transparent/empty edges across the long 8-area camera path.
            float targetWidth = targetHeight * gameplayCamera.aspect * 1.35f;
            float heightScale = targetHeight / Mathf.Max(0.01f, sprite.bounds.size.y);
            float widthScale = targetWidth / Mathf.Max(0.01f, sprite.bounds.size.x);
            float uniformScale = Mathf.Max(heightScale, widthScale);
            layer.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
            SpriteRenderer renderer = layer.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -10 - sorting;
        }
        else
        {
            layer = WorldSprite(name, parent, sprite, new Vector3(0f, y - 20f, 4f + sorting * 0.1f), new Vector2(120f, height), -10 - sorting);
        }
        layer.AddComponent<RedMemoryParallaxLayer>().Configure(gameplayCamera.transform, factor * 0.12f);
    }

    private void CreatePlatform(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject platform = WorldSprite(name, parent, ClaySpriteFactory.Rounded(new Color(0.32f, 0.31f, 0.28f), 160, 48, 0.22f, name.GetHashCode()), position, size, 0);
        platform.layer = GameplayLayer("Ground");
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>(); collider.size = Vector2.one;
    }

    private GameObject CreatePlatformObject(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject platform = WorldSprite(name, parent, ClaySpriteFactory.Rounded(new Color(0.32f, 0.31f, 0.28f), 160, 48, 0.22f, name.GetHashCode()), position, size, 0);
        platform.layer = GameplayLayer("Ground");
        platform.AddComponent<BoxCollider2D>().size = Vector2.one;
        return platform;
    }

    private void CreateMovingPlatform(string name, Transform parent, Vector2 position, Vector2 size, Vector3 offset, float speed)
    {
        GameObject platform = CreatePlatformObject(name, parent, position, size);
        Rigidbody2D body = platform.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.interpolation = RigidbodyInterpolation2D.Interpolate;
        platform.AddComponent<MovingPlatform>().Configure(offset, speed);
    }

    private void CreateCrumblingPlatform(string name, Transform parent, Vector2 position, Vector2 size)
    {
        CreatePlatformObject(name, parent, position, size).AddComponent<CrumblingPlatform>().Configure(this);
    }

    private void CreateEnemy(string name, Transform parent, bool jumping, Vector2 position)
    {
        Color color = jumping ? new Color(0.86f, 0.38f, 0.18f) : new Color(0.22f, 0.16f, 0.20f);
        GameObject enemy = WorldSprite(name, parent, ClaySpriteFactory.Rounded(color, 80, 62, 0.48f, name.GetHashCode()), position, jumping ? new Vector2(1.15f, 0.95f) : new Vector2(1.05f, 0.82f), 4);
        enemy.layer = GameplayLayer("Enemy");
        EnemyBase controller = jumping ? (EnemyBase)enemy.AddComponent<JumpBlobEnemy>() : enemy.AddComponent<MemoryCreeperEnemy>();
        GameObject hurt = Child("Hurtbox", enemy.transform);
        hurt.layer = GameplayLayer("Enemy");
        BoxCollider2D hurtCollider = hurt.AddComponent<BoxCollider2D>(); hurtCollider.isTrigger = true; hurtCollider.size = new Vector2(0.96f, 0.78f);
        hurt.AddComponent<EnemyHurtbox>();
        GameObject attack = Child("AttackBox", enemy.transform);
        attack.layer = GameplayLayer("Hazard");
        BoxCollider2D attackCollider = attack.AddComponent<BoxCollider2D>(); attackCollider.isTrigger = true; attackCollider.size = new Vector2(0.92f, 0.58f); attackCollider.offset = new Vector2(0f, -0.1f);
        attack.AddComponent<EnemyAttackBox>();
        controller.Configure(this, Player, jumping ? 2 : 1);
    }

    private void CreateCheckpoint(string name, Transform parent, Vector2 position)
    {
        GameObject checkpoint = WorldSprite(name, parent, ClaySpriteFactory.Rounded(new Color(0.78f, 0.16f, 0.10f), 40, 100, 0.16f, name.GetHashCode()), position, new Vector2(0.55f, 1.6f), 3);
        BoxCollider2D trigger = checkpoint.AddComponent<BoxCollider2D>(); trigger.isTrigger = true; trigger.size = new Vector2(2f, 2.5f);
        checkpoint.AddComponent<RedMemoryCheckpoint>().Configure(this, position + Vector2.up * 0.2f);
    }

    private void CreateFallingRock(string name, Transform parent, Vector2 position)
    {
        GameObject triggerObject = Child(name, parent); triggerObject.transform.position = position;
        triggerObject.layer = GameplayLayer("Trigger");
        BoxCollider2D trigger = triggerObject.AddComponent<BoxCollider2D>(); trigger.isTrigger = true; trigger.size = new Vector2(5f, 3f);
        GameObject shadowObject = WorldSprite("WarningShadow", triggerObject.transform, ClaySpriteFactory.Rounded(new Color(0.25f, 0.10f, 0.05f, 0.42f), 64, 24, 0.5f, 301), new Vector3(0f, -1.15f, 0f), new Vector2(1.3f, 0.35f), 3);
        GameObject rock = WorldSprite("FallingRock", triggerObject.transform, ClaySpriteFactory.Rounded(new Color(0.29f, 0.27f, 0.24f), 72, 72, 0.48f, 302), new Vector3(0f, 5.5f, 0f), new Vector2(1.25f, 1.25f), 5);
        rock.layer = GameplayLayer("Hazard");
        triggerObject.AddComponent<FallingRockTrap>().Configure(this, Player, rock.transform, shadowObject.GetComponent<SpriteRenderer>());
    }

    private void CreateRollingBoulder(string name, Transform parent, Vector2 position)
    {
        GameObject triggerObject = Child(name, parent); triggerObject.transform.position = position;
        triggerObject.layer = GameplayLayer("Trigger");
        BoxCollider2D trigger = triggerObject.AddComponent<BoxCollider2D>(); trigger.isTrigger = true; trigger.size = new Vector2(2.2f, 3f);
        GameObject boulder = WorldSprite("BoulderVisual", parent, ClaySpriteFactory.Rounded(new Color(0.26f, 0.24f, 0.22f), 110, 110, 0.5f, 303), new Vector3(position.x - 3f, position.y + 0.2f, 0f), new Vector2(2.3f, 2.3f), 5);
        boulder.layer = GameplayLayer("Hazard");
        triggerObject.AddComponent<RollingBoulder>().Configure(this, boulder.transform, 5.4f);
    }

    private void CreateMemoryRepair(string name, Transform parent, Transform bridgeParent, Vector2 pointPosition, Vector2 bridgePosition)
    {
        GameObject bridge = CreatePlatformObject("RepairedMemoryBridge", bridgeParent, bridgePosition, new Vector2(5f, 0.55f));
        GameObject point = WorldSprite(name, parent, ClaySpriteFactory.Rounded(new Color(1f, 0.42f, 0.12f), 64, 64, 0.5f, 304), pointPosition, new Vector2(0.8f, 0.8f), 4);
        point.layer = GameplayLayer("Trigger");
        CircleCollider2D trigger = point.AddComponent<CircleCollider2D>(); trigger.isTrigger = true; trigger.radius = 2f;
        point.AddComponent<MemoryRepairPoint>().Configure(this, bridge);
    }

    private void CreateDiscoveryZone(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject zone = Child(name, parent); zone.transform.position = position;
        zone.layer = GameplayLayer("Trigger");
        BoxCollider2D trigger = zone.AddComponent<BoxCollider2D>(); trigger.isTrigger = true; trigger.size = size;
        zone.AddComponent<RedMemoryDiscoveryZone>().Configure(this);
    }

    private void CreatePickup(string name, Transform parent, RedMemoryPickupKind kind, int index, Vector2 position)
    {
        Sprite sprite = kind == RedMemoryPickupKind.Fragment ? R_ART_006_RedMemory_Fragment : kind == RedMemoryPickupKind.ClueKey ? R_ART_007_Clue_Key : null;
        Color fallback = kind == RedMemoryPickupKind.Fragment ? new Color(0.86f, 0.20f, 0.16f) : kind == RedMemoryPickupKind.ClueKey ? new Color(1f, 0.72f, 0.18f) : kind == RedMemoryPickupKind.HealingFlower ? new Color(1f, 0.45f, 0.52f) : new Color(1f, 0.58f, 0.12f);
        if (sprite == null) sprite = ClaySpriteFactory.Circle(fallback, 96, index + 20);
        GameObject pickup = WorldSprite(name, parent, sprite, position, kind == RedMemoryPickupKind.ClueKey ? new Vector2(0.9f, 0.9f) : new Vector2(0.75f, 0.75f), 3);
        pickup.layer = GameplayLayer("Pickup");
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

    private static int GameplayLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        return layer >= 0 ? layer : 0;
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
        image.raycastTarget = false;
        if (color.a > 0.08f)
        {
            RoundSurface(image, name.Contains("Toast") ? 0.46f : 0.18f, name.GetHashCode());
            AddSoftDepth(image.gameObject, name.Contains("HUD") ? 5f : 9f);
        }
        image.gameObject.AddComponent<CanvasGroup>();
        return image.gameObject;
    }

    private static Image ImageObject(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        Image image = item.GetComponent<Image>(); image.color = color; image.raycastTarget = false; return image;
    }

    private TMP_Text CreateText(string name, Transform parent, string value, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max, bool useTitleFont = false)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>(); text.text = value; text.fontSize = size; text.color = color; text.alignment = alignment; text.enableWordWrapping = true; text.raycastTarget = false;
        text.font = useTitleFont ? titleFont : bodyFont;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.characterSpacing = useTitleFont ? 2f : 0.4f;
        return text;
    }

    private Button ButtonObject(string name, Transform parent, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        Image image = ImageObject(name, parent, Brick, min, max);
        RoundSurface(image, 0.32f, name.GetHashCode());
        AddSoftDepth(image.gameObject, 6f);
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>(); button.onClick.AddListener(action); image.gameObject.AddComponent<UIButtonAnimator>();
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.93f, 0.85f, 1f);
        colors.pressedColor = new Color(0.84f, 0.78f, 0.70f, 1f);
        colors.selectedColor = new Color(1f, 0.90f, 0.70f, 1f);
        colors.disabledColor = new Color(0.58f, 0.50f, 0.45f, 0.65f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        TMP_Text buttonLabel = CreateText("Label", image.transform, label, 25, PaperLight, TextAlignmentOptions.Center, new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.95f));
        buttonLabel.fontStyle = FontStyles.Bold;
        return button;
    }

    private void ApplyRedMemoryPresentation()
    {
        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].GetComponent<UIButtonAnimator>() == null) buttons[i].gameObject.AddComponent<UIButtonAnimator>();
        }

        TMP_Text[] texts = canvas.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            bool isChapterTitle = texts[i].name == "ChapterLabel" || texts[i].name == "ChapterMainTitle" || texts[i].name == "ChapterSubtitle";
            texts[i].font = isChapterTitle ? titleFont : bodyFont;
        }
    }

    private static void AddAccentStrip(Transform parent, Vector2 min, Vector2 max, Color color)
    {
        Image accent = ImageObject("RedMemoryAccent", parent, color, min, max);
        RoundSurface(accent, 0.45f, parent.name.GetHashCode() + 17);
        accent.raycastTarget = false;
        accent.transform.SetAsFirstSibling();
    }

    private static void RoundSurface(Image image, float roundness, int seed)
    {
        if (image == null) return;
        image.sprite = ClayShapeFactory.RoundedSurface(128, 72, roundness, seed);
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 1f;
    }

    private static void AddSoftDepth(GameObject target, float depth)
    {
        Shadow shadow = target.GetComponent<Shadow>();
        if (shadow == null) shadow = target.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.16f, 0.075f, 0.045f, 0.22f);
        shadow.effectDistance = new Vector2(0f, -depth);
        shadow.useGraphicAlpha = true;

        Outline outline = target.GetComponent<Outline>();
        if (outline == null) outline = target.AddComponent<Outline>();
        outline.effectColor = new Color(0.50f, 0.31f, 0.18f, 0.22f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
        outline.useGraphicAlpha = true;
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
