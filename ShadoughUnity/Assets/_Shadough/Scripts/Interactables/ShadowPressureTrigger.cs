using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ShadowPressureTrigger : MonoBehaviour
{
    [SerializeField] private PressurePlateController pressurePlate;
    [SerializeField] private float failurePromptCooldown = 1.5f;

    private Collider2D triggerCollider;
    private readonly List<PastedShadowObject> pressingShadows = new List<PastedShadowObject>();
    private PastedShadowObject lastRejectedShadow;
    private float nextFailurePromptTime;

    private void Awake()
    {
        CacheComponents();
        SetTriggerCollider();
    }

    private void Update()
    {
        RefreshPressureState();
    }

    private void Reset()
    {
        CacheComponents();
        SetTriggerCollider();
    }

    private void OnValidate()
    {
        failurePromptCooldown = Mathf.Max(0.25f, failurePromptCooldown);
        CacheComponents();
        SetTriggerCollider();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryActivateFrom(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryActivateFrom(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PastedShadowObject pastedShadow = GetPastedShadow(other);
        if (pastedShadow == null)
        {
            return;
        }

        pressingShadows.Remove(pastedShadow);
        if (pastedShadow == lastRejectedShadow)
        {
            lastRejectedShadow = null;
        }

        RefreshPressureState();
    }

    private void OnDisable()
    {
        pressingShadows.Clear();

        if (pressurePlate != null)
        {
            pressurePlate.Deactivate();
        }
    }

    private void TryActivateFrom(Collider2D other)
    {
        if (pressurePlate == null)
        {
            return;
        }

        PastedShadowObject pastedShadow = GetPastedShadow(other);
        if (pastedShadow == null)
        {
            return;
        }

        if (!pastedShadow.CanPress)
        {
            ShowFailurePrompt(pastedShadow, "This shadow cannot press plates.");
            return;
        }

        if (!pressingShadows.Contains(pastedShadow))
        {
            pressingShadows.Add(pastedShadow);
        }

        RefreshPressureState();
    }

    private PastedShadowObject GetPastedShadow(Collider2D other)
    {
        PastedShadowObject pastedShadow = other.GetComponent<PastedShadowObject>();
        if (pastedShadow == null)
        {
            pastedShadow = other.GetComponentInParent<PastedShadowObject>();
        }

        return pastedShadow;
    }

    private void ShowFailurePrompt(PastedShadowObject pastedShadow, string message)
    {
        if (pastedShadow == lastRejectedShadow || Time.time < nextFailurePromptTime)
        {
            return;
        }

        lastRejectedShadow = pastedShadow;
        nextFailurePromptTime = Time.time + failurePromptCooldown;
        TutorialFailurePromptController.Show(message);
    }

    private void RefreshPressureState()
    {
        for (int i = pressingShadows.Count - 1; i >= 0; i--)
        {
            if (pressingShadows[i] == null || !pressingShadows[i].CanPress)
            {
                pressingShadows.RemoveAt(i);
            }
        }

        if (pressurePlate == null)
        {
            return;
        }

        pressurePlate.SetPressed(pressingShadows.Count > 0);
    }

    private void CacheComponents()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }

        if (pressurePlate == null)
        {
            pressurePlate = GetComponent<PressurePlateController>();
        }

        if (pressurePlate == null)
        {
            pressurePlate = GetComponentInParent<PressurePlateController>();
        }
    }

    private void SetTriggerCollider()
    {
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}
