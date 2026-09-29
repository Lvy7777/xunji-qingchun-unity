using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ClueCardUI : MonoBehaviour
{
    private HometownInvestigationManager investigationManager;
    private TMP_Text clueTitleText;
    private TMP_Text clueDescriptionText;
    private TMP_Text audioNoticeText;
    private Button listenButton;
    private Button closeButton;

    public void Initialize(HometownInvestigationManager owner)
    {
        investigationManager = owner;
        clueTitleText = GetText("ClueTitle");
        clueDescriptionText = GetText("ClueDescription");
        audioNoticeText = GetText("AudioNotice");
        listenButton = GetButton("ListenButton");
        closeButton = GetButton("CloseButton");

        listenButton.onClick.RemoveListener(PlayAudioPlaceholder);
        listenButton.onClick.AddListener(PlayAudioPlaceholder);
        closeButton.onClick.RemoveListener(Close);
        closeButton.onClick.AddListener(Close);
    }

    public void Show(string title, string description)
    {
        clueTitleText.text = title;
        clueDescriptionText.text = description;
        audioNoticeText.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    private void PlayAudioPlaceholder()
    {
        Debug.Log("Audio placeholder played.");
        audioNoticeText.text = "声音素材将在此处补充";
        audioNoticeText.gameObject.SetActive(true);
    }

    private void Close()
    {
        gameObject.SetActive(false);
        investigationManager.CloseClueCard();
    }

    private TMP_Text GetText(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private Button GetButton(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<Button>() : null;
    }

    private void OnDestroy()
    {
        if (listenButton != null)
        {
            listenButton.onClick.RemoveListener(PlayAudioPlaceholder);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }
}
