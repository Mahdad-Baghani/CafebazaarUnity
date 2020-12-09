using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class StorageStub
{
    [JsonProperty]
    internal List<BazaarPurchaseRecord> purchaseRecord;
    
    private readonly bool _persistantSave = false;

    public StorageStub(bool persistantSave)
    {
        this._persistantSave = persistantSave;
    }

    #region IAB save/load
    internal void SavePurchases(WalletProviderStub rec)
    {
        if (rec == null || !_persistantSave) return;
        
        var json = JsonConvert.SerializeObject(rec, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        PlayerPrefs.SetString("saved_data", json);
        PlayerPrefs.Save();
    }

    internal WalletProviderStub LoadPurchase()
    {
        if (!_persistantSave)
        {
            return new WalletProviderStub();
        }
        
        try
        {
            var rec = PlayerPrefs.GetString("saved_data", null);
            if (string.IsNullOrEmpty(rec)) // showing that we do not have a save! a fresh start is always good news!!! :neo
            {
                var freshSave = new WalletProviderStub();
                return freshSave;
            }
            // by far we know that we have save! Hurray!! return the shit out of it! :neo
            var res = JsonConvert.DeserializeObject<WalletProviderStub>(rec);
            return res;
        }
        catch (Exception e)
        {
            // #debug 
            throw e;
        }
    }
    #endregion

}