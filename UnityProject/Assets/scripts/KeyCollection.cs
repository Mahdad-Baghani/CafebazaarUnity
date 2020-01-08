using BazaarPlugin;
using UnityEngine;

public class KeyCollection : MonoBehaviour, ICachable<IABRecord>
{
    private SaveSystem m_saveSystem;
    #region class methods
    private void OnEnable()
    {
        m_saveSystem = new SaveSystem();
        InvalidateCache();
    }
    
    private void OnDisable()
    {
        m_saveSystem = null;
        InvalidateCache();

    }
    #endregion

    #region ICachable impl
    public bool isCacheValid { get; private set; } = false;

    public IABRecord cachedObj
    {
        get
        {
            if (!isCacheValid || _cached == null)
            {
                _cached = m_saveSystem.LoadPurchase();
                isCacheValid = true;
            }
            return _cached;
        }
        set
        {
            _cached = value;
        }
    }


    private IABRecord _cached = null;

    public void InvalidateCache()
    {
        _cached = null;
        isCacheValid = false;
    }
    #endregion

    internal KeyConsumptionRecord RetrieveKey(string m_skuDetail)
    {
        return cachedObj.GetSinglePurchase(m_skuDetail);
    }

    internal KeyRecordSituations BuyKeyAsync(BazaarPurchase purchase)
    {
        var res = cachedObj.AddKeyRecord(purchase);
        if (res == KeyRecordSituations.ADDED_SUCCESSFULLY)
        {
            m_saveSystem.SavePurchases(cachedObj);
        }
        return res;
    }
    internal KeyRecordSituations ConsumeKey(BazaarPurchase purchase)
    {
        var res = cachedObj.ConsumeKeyRecord(purchase);
        if (res == KeyRecordSituations.CONSUMED_SUCCESSFULLY)
        {
            m_saveSystem.SavePurchases(cachedObj);
        }
        return res;
    }
}
