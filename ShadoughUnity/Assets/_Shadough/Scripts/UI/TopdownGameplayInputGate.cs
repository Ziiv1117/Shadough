using System.Collections.Generic;
using UnityEngine;

public class TopdownGameplayInputGate : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] blockedBehaviours;

    private readonly Dictionary<MonoBehaviour, bool> originalEnabledStates = new Dictionary<MonoBehaviour, bool>();
    private bool hasCachedStates;

    public bool GameplayInputEnabled { get; private set; } = true;

    public void SetGameplayInputEnabled(bool enabled)
    {
        CacheOriginalStates();
        GameplayInputEnabled = enabled;

        if (blockedBehaviours == null)
        {
            return;
        }

        for (int i = 0; i < blockedBehaviours.Length; i++)
        {
            MonoBehaviour behaviour = blockedBehaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            if (!enabled && behaviour is RevealViewController revealView)
            {
                revealView.SetRevealActive(false);
            }

            behaviour.enabled = enabled && GetOriginalState(behaviour);
        }
    }

    private void OnDisable()
    {
        RestoreOriginalStates();
    }

    private void OnDestroy()
    {
        RestoreOriginalStates();
    }

    private void CacheOriginalStates()
    {
        if (hasCachedStates)
        {
            return;
        }

        hasCachedStates = true;
        originalEnabledStates.Clear();

        if (blockedBehaviours == null)
        {
            return;
        }

        for (int i = 0; i < blockedBehaviours.Length; i++)
        {
            MonoBehaviour behaviour = blockedBehaviours[i];
            if (behaviour != null && !originalEnabledStates.ContainsKey(behaviour))
            {
                originalEnabledStates.Add(behaviour, behaviour.enabled);
            }
        }
    }

    private bool GetOriginalState(MonoBehaviour behaviour)
    {
        if (behaviour == null)
        {
            return false;
        }

        if (originalEnabledStates.TryGetValue(behaviour, out bool originalState))
        {
            return originalState;
        }

        return true;
    }

    private void RestoreOriginalStates()
    {
        if (!hasCachedStates)
        {
            return;
        }

        foreach (KeyValuePair<MonoBehaviour, bool> entry in originalEnabledStates)
        {
            if (entry.Key != null)
            {
                entry.Key.enabled = entry.Value;
            }
        }

        GameplayInputEnabled = true;
    }
}
