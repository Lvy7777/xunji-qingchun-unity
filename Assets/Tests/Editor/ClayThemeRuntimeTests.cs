using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Category("ClayTheme")]
public sealed class ClayThemeRuntimeTests
{
    private GameObject testRoot;

    [TearDown]
    public void TearDown()
    {
        if (testRoot != null) Object.DestroyImmediate(testRoot);
    }

    [Test]
    public void ApplyToLoadedScene_StylesButtonsCardsTextAndDecorations()
    {
        GameObject canvasObject = Create("ClayTestCanvas", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject buttonObject = CreateUI("StartButton", canvas.transform, typeof(Image), typeof(Button));
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        GameObject cardObject = CreateUI("FeatureCard", canvas.transform, typeof(Image));
        GameObject dialogueObject = CreateUI("DialoguePanel", canvas.transform, typeof(Image));
        GameObject textObject = CreateUI("CardTitle", cardObject.transform, typeof(TextMeshProUGUI));

        ClayThemeRuntime.ApplyCanvas(canvas, "ClayThemeTest");

        Assert.That(buttonObject.GetComponent<UIButtonAnimator>(), Is.Not.Null);
        Assert.That(buttonObject.GetComponent<Image>().sprite, Is.Not.Null);
        Assert.That(buttonObject.GetComponent<Shadow>(), Is.Not.Null);
        Assert.That(buttonObject.transform.Find("ClayInnerHighlight"), Is.Not.Null);

        Assert.That(cardObject.GetComponent<ClayCardMotion>(), Is.Not.Null);
        Assert.That(cardObject.GetComponent<Image>().sprite, Is.Not.Null);
        Assert.That(dialogueObject.GetComponent<Image>().color, Is.EqualTo(ClayThemeTokens.WarmWhite));

        TMP_Text title = textObject.GetComponent<TMP_Text>();
        Assert.That((title.fontStyle & FontStyles.Bold) != 0, Is.True);
        Assert.That(title.color, Is.EqualTo(ClayThemeTokens.Ink));

        Transform decorationLayer = canvas.transform.Find("ClayDecorationLayer");
        Assert.That(decorationLayer, Is.Not.Null);
        Assert.That(decorationLayer.childCount, Is.EqualTo(6));
        for (int i = 0; i < decorationLayer.childCount; i++)
        {
            Assert.That(decorationLayer.GetChild(i).GetComponent<Image>().raycastTarget, Is.False);
        }
    }

    [Test]
    public void ApplyToLoadedScene_IsIdempotent()
    {
        GameObject canvasObject = Create("ClayTestCanvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        GameObject buttonObject = CreateUI("ContinueButton", canvasObject.transform, typeof(Image), typeof(Button));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        ClayThemeRuntime.ApplyCanvas(canvas, "ClayThemeTest");
        ClayThemeRuntime.ApplyCanvas(canvas, "ClayThemeTest");

        Assert.That(buttonObject.GetComponents<UIButtonAnimator>().Length, Is.EqualTo(1));
        Assert.That(buttonObject.GetComponents<Outline>().Length, Is.EqualTo(1));
        Assert.That(buttonObject.transform.Find("ClayInnerHighlight"), Is.Not.Null);
        Assert.That(canvasObject.transform.Find("ClayDecorationLayer").childCount, Is.EqualTo(6));
    }

    [Test]
    public void ApplyToLoadedScene_PreservesTransparentScreenAdvanceButton()
    {
        GameObject canvasObject = Create("ClayTestCanvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        GameObject blockerObject = CreateUI("ScreenAdvanceButton", canvasObject.transform, typeof(Image), typeof(Button));
        Image blockerImage = blockerObject.GetComponent<Image>();
        blockerImage.color = new Color(1f, 1f, 1f, 0f);
        blockerObject.GetComponent<Button>().targetGraphic = blockerImage;

        ClayThemeRuntime.ApplyCanvas(canvasObject.GetComponent<Canvas>(), "ClayThemeTest");

        Assert.That(blockerImage.sprite, Is.Null);
        Assert.That(blockerImage.color.a, Is.EqualTo(0f));
        Assert.That(blockerObject.GetComponent<UIButtonAnimator>(), Is.Null);
    }

    private GameObject Create(string name, params System.Type[] components)
    {
        GameObject item = new GameObject(name, components);
        testRoot = item;
        return item;
    }

    private GameObject CreateUI(string name, Transform parent, params System.Type[] components)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        item.SetActive(false);
        item.transform.SetParent(parent, false);
        for (int i = 0; i < components.Length; i++) item.AddComponent(components[i]);
        return item;
    }
}
