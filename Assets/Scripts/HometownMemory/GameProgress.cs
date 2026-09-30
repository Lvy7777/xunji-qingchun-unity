using UnityEngine;

public sealed class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    [SerializeField] private bool hasRedMemory;
    [SerializeField] private bool hasHometownMemory;
    [SerializeField] private bool hasYouthMemory;
    [SerializeField] private bool hasVillageMemory;

    public bool HasRedMemory => hasRedMemory;
    public bool HasHometownMemory => hasHometownMemory;
    public bool HasYouthMemory => hasYouthMemory;
    public bool HasVillageMemory => hasVillageMemory;
    public int MemoryCount => (hasRedMemory ? 1 : 0)
        + (hasHometownMemory ? 1 : 0)
        + (hasYouthMemory ? 1 : 0)
        + (hasVillageMemory ? 1 : 0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static GameProgress Ensure()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject progressObject = new GameObject("GameProgress");
        return progressObject.AddComponent<GameProgress>();
    }

    public bool TryGrantRedMemory()
    {
        if (hasRedMemory)
        {
            return false;
        }

        hasRedMemory = true;
        return true;
    }

    public bool TryGrantHometownMemory()
    {
        if (hasHometownMemory)
        {
            return false;
        }

        hasHometownMemory = true;
        return true;
    }

    public bool TryGrantYouthMemory()
    {
        if (hasYouthMemory)
        {
            return false;
        }

        hasYouthMemory = true;
        return true;
    }


public bool TryGrantVillageMemory()
    {
        if (hasVillageMemory)
        {
            return false;
        }

        hasVillageMemory = true;
        return true;
    }


public void ResetAllMemories()
    {
        hasRedMemory = false;
        hasHometownMemory = false;
        hasYouthMemory = false;
        hasVillageMemory = false;
    }
}
