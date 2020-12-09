using BazaarPlugin;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum KeyRecordSituations
{
    ADDED_SUCCESSFULLY,
    CONSUMED_SUCCESSFULLY,
    EXISTS,
    DOESNT_EXIST, 
}
public enum KeyConsumptionRecord
{
    CONSUMED, 
    NOT_CONSUMED,
    NOT_FOUND
}

[System.Serializable]
public class WalletProviderStub 
{
    private List<BazaarPurchaseRecord> m_keys;

    internal WalletProviderStub()
    {
        m_keys = new List<BazaarPurchaseRecord>();
    }

    internal async Task<KeyRecordSituations> AddPurchase(BazaarPurchase purchase)
    {
        if (m_keys.Exists(k => k.m_productId == purchase.ProductId))
        {
            var existingKey = m_keys.Find(k => k.m_productId == purchase.ProductId);
            if (!existingKey.m_isUsed) return KeyRecordSituations.EXISTS;
            m_keys.Remove(existingKey);
            m_keys.Add(new BazaarPurchaseRecord() { m_isUsed = false, m_purchaseToken = purchase.PurchaseToken, m_productId = purchase.ProductId });
            return KeyRecordSituations.ADDED_SUCCESSFULLY;
        }

        m_keys.Add(new BazaarPurchaseRecord() { m_isUsed = false, m_purchaseToken = purchase.PurchaseToken, m_productId = purchase.ProductId });
        return KeyRecordSituations.ADDED_SUCCESSFULLY;
    }

    internal async  Task<KeyRecordSituations> ConsumePurchase(BazaarPurchase purchase)
    {
        if (m_keys.Count <= 0) return KeyRecordSituations.DOESNT_EXIST;

        var key = m_keys.Find(x => x.m_productId == purchase.ProductId);
        if (key == null)
        {
            return KeyRecordSituations.DOESNT_EXIST;
        }
        key.m_isUsed = true;
        return KeyRecordSituations.CONSUMED_SUCCESSFULLY;
    }

    internal async Task<KeyConsumptionRecord> GetSinglePurchase(string m_skuDetail)
    {
        if (m_keys.Count <= 0) return KeyConsumptionRecord.NOT_FOUND;

        var key = m_keys.Find(k => k.m_productId == m_skuDetail);
        if (key != null)
        {
            return key.m_isUsed ? KeyConsumptionRecord.CONSUMED : KeyConsumptionRecord.NOT_CONSUMED;
        }
        return KeyConsumptionRecord.NOT_FOUND;
    }
}

internal class BazaarPurchaseRecord
{
    [SerializeField]
    [JsonProperty]
    /// <summary>
    /// the Id of the purchase which is returned by CafeBazaar API and should be kept in order to keep track of purchased keys
    /// </summary>
    internal string m_purchaseToken;
    /// <summary>
    /// the sku of the item according to what is defined on the bazaar dashboard
    /// </summary>
    [SerializeField]
    [JsonProperty]
    internal string m_productId;
    /// <summary>
    /// is the key used to unlocked an IABMailBox or what
    /// </summary>
    [SerializeField]
    [JsonProperty]
    internal bool m_isUsed;
}