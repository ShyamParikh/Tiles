/* BaseManager.cs
using UnityEngine;

public abstract class BaseManager : MonoBehaviour
{
    // Move gameState here so IsGame() can read it directly
    protected static EGameState gameState;

    protected virtual void Awake() => SubscribeToEvents();
    protected virtual void OnDestroy() => UnsubscribeFromEvents();
    protected virtual void SubscribeToEvents() { }
    protected virtual void UnsubscribeFromEvents() { }

    // Now reads the field directly — no more self-calling loop
    public bool IsGame() => gameState == EGameState.Game;

    protected void Log(string message) => Debug.Log($"[{GetType().Name}] {message}");
    protected void LogError(string message) => Debug.LogError($"[{GetType().Name}] {message}");
}*/