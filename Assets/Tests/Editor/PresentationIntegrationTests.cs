using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Category("ClayTheme")]
public sealed class PresentationIntegrationTests
{
    private GameObject testRoot;

    [TearDown]
    public void TearDown()
    {
        if (testRoot != null) Object.DestroyImmediate(testRoot);
    }

    [Test]
    public void Portraits_UseTransparentArtworkAndSharedDialoguePlacement()
    {
        testRoot = new GameObject("PortraitTestCanvas", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = testRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        RectTransform xiaoHe = CreatePortrait("XiaoHe", canvas.transform);
        RectTransform volunteer = CreatePortrait("Volunteer", canvas.transform);
        PortraitDisplayController.ApplyToLoadedScene(SceneManager.GetActiveScene());

        Image xiaoHeArtwork = xiaoHe.Find("ClayCharacterArtwork").GetComponent<Image>();
        Image volunteerArtwork = volunteer.Find("ClayCharacterArtwork").GetComponent<Image>();

        Assert.That(xiaoHeArtwork.sprite, Is.Not.Null);
        Assert.That(volunteerArtwork.sprite, Is.Not.Null);
        Assert.That(xiaoHeArtwork.sprite.name, Is.EqualTo("XiaoHe_Front"));
        Assert.That(volunteerArtwork.sprite.name, Is.EqualTo("Volunteer_Front"));
        Assert.That(xiaoHeArtwork.raycastTarget, Is.False);
        Assert.That(volunteerArtwork.raycastTarget, Is.False);
        Assert.That(xiaoHe.anchorMin.x, Is.LessThan(0.1f));
        Assert.That(volunteer.anchorMin.x, Is.GreaterThan(0.9f));
        Assert.That(volunteer.sizeDelta.y, Is.GreaterThan(xiaoHe.sizeDelta.y));
    }

    [Test]
    public void CursorEffect_DoesNotBlockUiRaycasts()
    {
        testRoot = new GameObject("CursorEffectTest");
        CursorEffectController effect = testRoot.AddComponent<CursorEffectController>();
        typeof(CursorEffectController)
            .GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Invoke(effect, null);

        Transform cursorRoot = testRoot.transform.Find("CursorEffectRoot");
        Assert.That(cursorRoot, Is.Not.Null);
        Assert.That(cursorRoot.GetComponent<GraphicRaycaster>().enabled, Is.False);

        Image[] markers = cursorRoot.GetComponentsInChildren<Image>(true);
        Assert.That(markers.Length, Is.GreaterThanOrEqualTo(9));
        for (int i = 0; i < markers.Length; i++)
        {
            Assert.That(markers[i].raycastTarget, Is.False);
        }
    }

    private static RectTransform CreatePortrait(string name, Transform parent)
    {
        GameObject portrait = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portrait.transform.SetParent(parent, false);
        return portrait.GetComponent<RectTransform>();
    }
}
