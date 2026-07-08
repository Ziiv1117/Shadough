using UnityEngine;

[DisallowMultipleComponent]
public class HeroPlayerAnimatorDriver : MonoBehaviour
{
    private enum FacingDirection
    {
        Down,
        Left,
        Right,
        Up
    }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerLanternController lanternController;
    [SerializeField] private ShadowInventory shadowInventory;

    [Header("Timing")]
    [SerializeField] private float moveThreshold = 0.01f;
    [SerializeField] private float crossFadeDuration = 0.04f;
    [SerializeField] private float actionLockDuration = 0.32f;
    [SerializeField] private float lanternTransitionLockDuration = 0.45f;

    private FacingDirection facingDirection = FacingDirection.Down;
    private bool wasLanternPlanted;
    private bool wasPlacingLantern;
    private bool skipNextPlacedTransitionAction;
    private bool hadShadowLastFrame;
    private float actionUntilTime;
    private int currentStateHash;
    private TopdownFinalClockCore[] cachedClockCores;

    private void Awake()
    {
        ResolveReferences();
        wasLanternPlanted = IsLanternPlanted();
        wasPlacingLantern = IsPlacingLantern();
        hadShadowLastFrame = HasShadow();
        PlayBaseState(true);
    }

    private void LateUpdate()
    {
        ResolveReferences();
        UpdateFacingDirection();
        PlayRequestedAction();
        PlayLanternTransitionIfNeeded();

        if (Time.time >= actionUntilTime)
        {
            PlayBaseState(false);
        }

        hadShadowLastFrame = HasShadow();
    }

    private void ResolveReferences()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (lanternController == null)
        {
            lanternController = GetComponent<PlayerLanternController>();
        }

        if (shadowInventory == null)
        {
            shadowInventory = GetComponent<ShadowInventory>();
        }
    }

    private void UpdateFacingDirection()
    {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.sqrMagnitude <= moveThreshold * moveThreshold)
        {
            return;
        }

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            facingDirection = moveInput.x < 0f ? FacingDirection.Left : FacingDirection.Right;
        }
        else
        {
            facingDirection = moveInput.y < 0f ? FacingDirection.Down : FacingDirection.Up;
        }
    }

    private void PlayRequestedAction()
    {
        if (Input.GetKeyDown(KeyCode.F) && (hadShadowLastFrame || HasShadow()))
        {
            PlayAction("PasteShadow", false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && IsRevealActive())
        {
            PlayAction("CutShadow", false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (IsRevealActive())
            {
                PlayAction("CutShadow", false);
                return;
            }

            if (IsClockCoreReady())
            {
                PlayAction("ActivateCore", true);
                return;
            }

            if (!IsLanternHeld())
            {
                PlayAction("Interact", false);
            }
        }
    }

    private void PlayLanternTransitionIfNeeded()
    {
        bool placingLantern = IsPlacingLantern();
        if (placingLantern && !wasPlacingLantern)
        {
            PlayAction("PlaceLantern", true);
            skipNextPlacedTransitionAction = true;
        }

        wasPlacingLantern = placingLantern;

        bool lanternPlanted = IsLanternPlanted();
        if (lanternPlanted == wasLanternPlanted)
        {
            return;
        }

        wasLanternPlanted = lanternPlanted;
        if (lanternPlanted && skipNextPlacedTransitionAction)
        {
            skipNextPlacedTransitionAction = false;
            return;
        }

        skipNextPlacedTransitionAction = false;
        PlayAction(lanternPlanted ? "PlaceLantern" : "PickupLantern", true);
    }

    private void PlayBaseState(bool force)
    {
        if (IsRevealActive())
        {
            PlayState(BuildStateName("RevealFocus", IsLanternHeld()), force, false);
            return;
        }

        string baseName = IsMoving() ? "Walk" : "Idle";
        PlayState(BuildStateName(baseName, IsLanternHeld()), force, false);
    }

    private void PlayAction(string actionName, bool lanternHeld)
    {
        if (PlayState(BuildStateName(actionName, lanternHeld), true, true))
        {
            actionUntilTime = Time.time + GetActionLockDuration(actionName);
        }
    }

    public void PlayHurtFeedback()
    {
        ResolveReferences();
        PlayAction("Hurt", IsLanternHeld());
    }

    private float GetActionLockDuration(string actionName)
    {
        if (actionName == "PlaceLantern" || actionName == "PickupLantern")
        {
            return Mathf.Max(actionLockDuration, lanternTransitionLockDuration);
        }

        return actionLockDuration;
    }

    private bool PlayState(string stateName, bool force, bool immediate)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return false;
        }

        int stateHash = Animator.StringToHash(stateName);
        if (!animator.HasState(0, stateHash))
        {
            return false;
        }

        if (!force && currentStateHash == stateHash)
        {
            return true;
        }

        if (immediate || crossFadeDuration <= 0f)
        {
            animator.Play(stateHash, 0, 0f);
        }
        else
        {
            animator.CrossFade(stateHash, crossFadeDuration, 0);
        }

        currentStateHash = stateHash;
        return true;
    }

    private string BuildStateName(string actionName, bool lanternHeld)
    {
        string lanternPart = lanternHeld ? "Lantern" : "NoLantern";
        return "Hero_" + actionName + "_" + lanternPart + "_" + facingDirection;
    }

    private bool IsMoving()
    {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        return moveInput.sqrMagnitude > moveThreshold * moveThreshold;
    }

    private bool IsRevealActive()
    {
        return RevealViewController.HasInstance && RevealViewController.IsActive;
    }

    private bool IsLanternHeld()
    {
        return !IsLanternPlanted();
    }

    private bool IsLanternPlanted()
    {
        return lanternController != null && lanternController.IsLanternPlanted;
    }

    private bool IsPlacingLantern()
    {
        return lanternController != null && lanternController.IsPlacingLantern;
    }

    private bool HasShadow()
    {
        return shadowInventory != null && shadowInventory.HasShadow();
    }

    private bool IsClockCoreReady()
    {
        if (cachedClockCores == null || cachedClockCores.Length == 0)
        {
            cachedClockCores = FindObjectsOfType<TopdownFinalClockCore>();
        }

        for (int i = 0; i < cachedClockCores.Length; i++)
        {
            TopdownFinalClockCore clockCore = cachedClockCores[i];
            if (clockCore != null && clockCore.PlayerInRange && !clockCore.IsActivated)
            {
                return true;
            }
        }

        return false;
    }

    private void OnValidate()
    {
        moveThreshold = Mathf.Max(0.001f, moveThreshold);
        crossFadeDuration = Mathf.Max(0f, crossFadeDuration);
        actionLockDuration = Mathf.Max(0f, actionLockDuration);
        lanternTransitionLockDuration = Mathf.Max(actionLockDuration, lanternTransitionLockDuration);
    }
}
