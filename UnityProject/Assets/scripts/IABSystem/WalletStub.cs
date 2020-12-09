using System.Threading.Tasks;
using BazaarPlugin;

public class WalletStub : ICachable<WalletProviderStub>
{
    // private SaveSystemStub _mSaveSystemStub;
    
    #region class methods
    public WalletStub(bool persistantSave = false)
    {
        // _mSaveSystemStub = new SaveSystemStub(persistantSave);
        InvalidateCache();
    }

    ~WalletStub()
    {
        // _mSaveSystemStub = null;
        InvalidateCache();
    }
    #endregion

    #region ICachable impl
    public bool isCacheValid { get; private set; } = false;

    public WalletProviderStub cachedObj
    {
        get
        {
            if (isCacheValid && _cached != null) return _cached;
            // _cached = _mSaveSystemStub.LoadPurchase();
            _cached = new WalletProviderStub();
            isCacheValid = true;
            return _cached;
        }
    }

    private WalletProviderStub _cached = null;

    public void InvalidateCache()
    {
        _cached = null;
        isCacheValid = false;
    }
    #endregion

    internal async Task<KeyConsumptionRecord> RetrieveKey(string m_skuDetail)
    {
        return await cachedObj.GetSinglePurchase(m_skuDetail);
    }

    internal async Task<KeyRecordSituations> BuyKeyAsync(BazaarPurchase purchase)
    {
        var res = await cachedObj.AddPurchase(purchase);
        // if (res == KeyRecordSituations.ADDED_SUCCESSFULLY)
        // {
        //     _mSaveSystemStub.SavePurchases(cachedObj);
        // }
        return res;
    }
    internal async Task<KeyRecordSituations> ConsumeKey(BazaarPurchase purchase)
    {
        var res = await cachedObj.ConsumePurchase(purchase);
        // if (res == KeyRecordSituations.CONSUMED_SUCCESSFULLY)
        // {
        //     _mSaveSystemStub.SavePurchases(cachedObj);
        // }
        return res;
    }
}
