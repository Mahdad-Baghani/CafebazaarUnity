using System;
using Newtonsoft.Json;
using UnityEngine;

public class SaveSystem
{
    #region IAB save/load
    internal void SavePurchases(IABRecord rec)
    {
        if (rec != null)
        {
            var json = JsonConvert.SerializeObject(rec, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            PlayerPrefs.SetString("payiz_iab", json);
            PlayerPrefs.Save();
        }
    }

    internal IABRecord LoadPurchase()
    {
        try
        {
            string rec = PlayerPrefs.GetString("payiz_iab", null);
            if (string.IsNullOrEmpty(rec)) // showing that we do not have a save! a fresh start is always good news!!! :neo
            {
                var fresh_save = new IABRecord();
                return fresh_save;
            }
            // by far we know that we have save! Hurray!! return the shit out of it! :neo
            var res = JsonConvert.DeserializeObject<IABRecord>(rec);
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