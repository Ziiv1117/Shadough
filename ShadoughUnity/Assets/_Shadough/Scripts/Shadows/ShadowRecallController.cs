using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShadowInventory))]
public class ShadowRecallController : MonoBehaviour
{
    [SerializeField] private ShadowInventory inventory;
    [SerializeField] private KeyCode recallKey = KeyCode.R;
    [SerializeField] private string noShadowMessage = "No shadow to recall.";
    [SerializeField] private string recalledMessage = "Shadow recalled.";
    [SerializeField] private string abandonedMessage = "Shadow returned.";

    private readonly List<PastedShadowObject> pastedShadows = new List<PastedShadowObject>();

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<ShadowInventory>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(recallKey))
        {
            TryRecallOrAbandon();
        }
    }

    public void RegisterPastedShadow(PastedShadowObject pastedShadow)
    {
        if (pastedShadow == null || pastedShadows.Contains(pastedShadow))
        {
            return;
        }

        pastedShadows.Add(pastedShadow);
    }

    private void TryRecallOrAbandon()
    {
        PastedShadowObject recallablePastedShadow = FindMostRecentRecallablePastedShadow();
        if (recallablePastedShadow != null)
        {
            RecallPastedShadow(recallablePastedShadow);
            return;
        }

        if (TryAbandonHeldShadow())
        {
            return;
        }

        string blockedMessage = FindMostRecentBlockedMessage();
        ShowPrompt(string.IsNullOrEmpty(blockedMessage) ? noShadowMessage : blockedMessage);
    }

    private PastedShadowObject FindMostRecentRecallablePastedShadow()
    {
        for (int i = pastedShadows.Count - 1; i >= 0; i--)
        {
            PastedShadowObject pastedShadow = pastedShadows[i];
            if (pastedShadow == null)
            {
                pastedShadows.RemoveAt(i);
                continue;
            }

            if (pastedShadow.RecallBlocked)
            {
                continue;
            }

            ShadowItemData sourceData = pastedShadow.SourceData != null
                ? pastedShadow.SourceData
                : pastedShadow.CreateItemData();

            if (sourceData != null && sourceData.HasRecallSource())
            {
                return pastedShadow;
            }
        }

        return null;
    }

    private string FindMostRecentBlockedMessage()
    {
        for (int i = pastedShadows.Count - 1; i >= 0; i--)
        {
            PastedShadowObject pastedShadow = pastedShadows[i];
            if (pastedShadow == null)
            {
                pastedShadows.RemoveAt(i);
                continue;
            }

            if (pastedShadow.RecallBlocked)
            {
                return pastedShadow.RecallBlockedMessage;
            }
        }

        if (inventory != null && inventory.HasShadow())
        {
            ShadowItemData data = inventory.CurrentShadowData;
            if (data != null && data.recallBlocked)
            {
                return data.recallBlockedMessage;
            }
        }

        return string.Empty;
    }

    private void RecallPastedShadow(PastedShadowObject pastedShadow)
    {
        ShadowItemData sourceData = pastedShadow.SourceData != null
            ? pastedShadow.SourceData
            : pastedShadow.CreateItemData();

        if (!TryRestoreSource(sourceData))
        {
            ShowPrompt(noShadowMessage);
            return;
        }

        pastedShadows.Remove(pastedShadow);
        Destroy(pastedShadow.gameObject);

        if (inventory != null)
        {
            inventory.ClearShadow();
        }

        ShowPrompt(recalledMessage);
    }

    private bool TryAbandonHeldShadow()
    {
        if (inventory == null || !inventory.HasShadow())
        {
            return false;
        }

        ShadowItemData data = inventory.CurrentShadowData;
        if (data == null)
        {
            return false;
        }

        if (data.recallBlocked)
        {
            ShowPrompt(data.recallBlockedMessage);
            return true;
        }

        if (!data.HasRecallSource())
        {
            return false;
        }

        data.TryRestoreSource();
        inventory.ClearShadow();
        ShowPrompt(abandonedMessage);
        return true;
    }

    private bool TryRestoreSource(ShadowItemData data)
    {
        if (data == null)
        {
            return false;
        }

        if (data.recallBlocked)
        {
            return false;
        }

        return data.TryRestoreSource();
    }

    private void ShowPrompt(string message)
    {
        TutorialFailurePromptController.Show(message);
        Debug.Log(message);
    }

    private void OnValidate()
    {
        if (inventory == null)
        {
            inventory = GetComponent<ShadowInventory>();
        }
    }
}
