using UnityEngine;

public class ShadowInventory : MonoBehaviour
{
    [Header("Current Shadow")]
    [SerializeField] private ShadowType currentShadowType = ShadowType.None;
    [SerializeField] private ShadowItemData currentShadowData;

    public ShadowType CurrentShadowType => currentShadowType;
    public ShadowItemData CurrentShadowData => currentShadowData;

    public bool HasShadow()
    {
        return currentShadowData != null && currentShadowData.IsValid();
    }

    public bool CanCarry()
    {
        return !HasShadow();
    }

    public bool PickUpShadow(ShadowType type)
    {
        if (type == ShadowType.None || !CanCarry())
        {
            return false;
        }

        ShadowItemData data = new ShadowItemData
        {
            shadowType = type
        };

        return PickUpShadow(data);
    }

    public bool PickUpShadow(ShadowItemData data)
    {
        if (data == null || !data.IsValid() || !CanCarry())
        {
            return false;
        }

        currentShadowData = data;
        currentShadowType = data.shadowType;
        return true;
    }

    public ShadowType ConsumeShadow()
    {
        ShadowType consumedType = currentShadowType;
        ClearShadow();
        return consumedType;
    }

    public ShadowItemData ConsumeShadowData()
    {
        ShadowItemData consumedData = currentShadowData;
        ClearShadow();
        return consumedData;
    }

    public void ClearShadow()
    {
        currentShadowType = ShadowType.None;
        currentShadowData = null;
    }
}
