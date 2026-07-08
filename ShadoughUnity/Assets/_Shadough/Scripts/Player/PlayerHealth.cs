using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth = 3;
    [SerializeField] private float invulnerabilityDuration = 1.2f;

    [Header("Feedback")]
    [SerializeField] private string damageMessage = "The seeker caught you.";
    [SerializeField] private string defeatedMessage = "You were consumed by shadow.";
    [SerializeField] private float defeatedPromptDuration = 2f;
    [SerializeField] private bool playHurtAnimation = true;
    [SerializeField] private HeroPlayerAnimatorDriver animatorDriver;

    [Header("Defeat Recovery")]
    [SerializeField] private Transform safePoint;
    [SerializeField] private bool restoreHealthOnDefeat = true;
    [SerializeField] private bool clearAttractingPastedShadowsOnDefeat = true;
    [SerializeField] private bool clearHeldAttractingShadowOnDefeat = true;
    [SerializeField] private bool resetSeekersOnDefeat = true;
    [SerializeField] private EnemyShadowSeeker[] seekersToReset;

    [Header("References")]
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private ShadowInventory shadowInventory;

    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnDamage;
    public event Action OnDefeated;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsInvulnerable => Time.time < invulnerableUntilTime;
    public float InvulnerabilityDuration => invulnerabilityDuration;

    private Vector3 fallbackSafePosition;
    private float invulnerableUntilTime;

    private void Awake()
    {
        CacheReferences();
        fallbackSafePosition = transform.position;
        currentHealth = Mathf.Clamp(currentHealth <= 0 ? maxHealth : currentHealth, 1, maxHealth);
    }

    private void Start()
    {
        NotifyHealthChanged();
    }

    public bool TakeDamage(int amount)
    {
        return TakeDamage(amount, damageMessage);
    }

    public bool TakeDamage(int amount, string hitMessage)
    {
        if (amount <= 0 || currentHealth <= 0 || IsInvulnerable)
        {
            return false;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        invulnerableUntilTime = Time.time + invulnerabilityDuration;

        OnDamage?.Invoke(amount);
        NotifyHealthChanged();
        PlayDamageFeedback(currentHealth > 0 ? hitMessage : string.Empty);

        if (currentHealth <= 0)
        {
            HandleDefeated();
        }

        return true;
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }

    private void HandleDefeated()
    {
        OnDefeated?.Invoke();

        if (!string.IsNullOrEmpty(defeatedMessage))
        {
            TutorialFailurePromptController.Show(defeatedMessage, defeatedPromptDuration);
        }

        if (clearAttractingPastedShadowsOnDefeat)
        {
            ClearAttractingPastedShadows();
        }

        if (clearHeldAttractingShadowOnDefeat)
        {
            ClearHeldAttractingShadow();
        }

        MoveToSafePoint();

        if (resetSeekersOnDefeat)
        {
            ResetSeekers();
        }

        if (restoreHealthOnDefeat)
        {
            RestoreFullHealth();
            invulnerableUntilTime = Time.time + invulnerabilityDuration;
        }
    }

    private void PlayDamageFeedback(string hitMessage)
    {
        if (!string.IsNullOrEmpty(hitMessage))
        {
            TutorialFailurePromptController.Show(hitMessage);
        }

        if (!playHurtAnimation)
        {
            return;
        }

        CacheReferences();
        if (animatorDriver != null)
        {
            animatorDriver.PlayHurtFeedback();
        }
    }

    private void MoveToSafePoint()
    {
        Vector3 targetPosition = safePoint != null ? safePoint.position : fallbackSafePosition;
        targetPosition.z = transform.position.z;

        CacheReferences();
        if (playerBody != null)
        {
            playerBody.velocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
            playerBody.position = targetPosition;
        }

        transform.position = targetPosition;
        Physics2D.SyncTransforms();
    }

    private void ResetSeekers()
    {
        EnemyShadowSeeker[] seekers = seekersToReset;
        if (seekers == null || seekers.Length == 0)
        {
            seekers = FindObjectsOfType<EnemyShadowSeeker>();
        }

        for (int i = 0; i < seekers.Length; i++)
        {
            if (seekers[i] != null)
            {
                seekers[i].ResetToHome();
            }
        }
    }

    private void ClearAttractingPastedShadows()
    {
        PastedShadowObject[] pastedShadows = FindObjectsOfType<PastedShadowObject>();
        for (int i = 0; i < pastedShadows.Length; i++)
        {
            PastedShadowObject pastedShadow = pastedShadows[i];
            if (pastedShadow != null && pastedShadow.CanAttractEnemy)
            {
                Destroy(pastedShadow.gameObject);
            }
        }
    }

    private void ClearHeldAttractingShadow()
    {
        CacheReferences();
        if (shadowInventory == null || !shadowInventory.HasShadow())
        {
            return;
        }

        ShadowItemData data = shadowInventory.CurrentShadowData;
        if (data != null && data.canAttractEnemy)
        {
            shadowInventory.ClearShadow();
        }
    }

    private void CacheReferences()
    {
        if (playerBody == null)
        {
            playerBody = GetComponent<Rigidbody2D>();
        }

        if (shadowInventory == null)
        {
            shadowInventory = GetComponent<ShadowInventory>();
        }

        if (animatorDriver == null)
        {
            animatorDriver = GetComponent<HeroPlayerAnimatorDriver>();
        }
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        invulnerabilityDuration = Mathf.Clamp(invulnerabilityDuration, 0.1f, 5f);
        defeatedPromptDuration = Mathf.Clamp(defeatedPromptDuration, 1f, 3f);
        CacheReferences();
    }
}
