using System;

public enum DialogueSpeaker
{
    System,
    XiaoHe,
    Volunteer
}

[Serializable]
public sealed class DialogueLine
{
    public DialogueSpeaker Speaker;
    public string Text;

    public DialogueLine(DialogueSpeaker speaker, string text)
    {
        Speaker = speaker;
        Text = text;
    }
}
