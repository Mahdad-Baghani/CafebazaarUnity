﻿using BazaarPlugin;
using System.Collections;
using UnityEngine;

public class IABKeyTests : MonoBehaviour, IEventListner
{
    public GameObject buttonPrefab;
    public Transform skuButtonContentPanel;
    public string[] skus;
    public TMPro.TextMeshProUGUI m_text;
    // UI fields
    public RectTransform m_debugContentPanel;
    public float m_lerpTime = 1f;
    
    KeyCollection _keys;
    string _currentSku;

    private void OnDestroy()
    {
        UnhookEvents();
        BazaarIAB.unbindService();
    }

    private void Start()
    {
        try
        {
            var key = "MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwC5nloPoQjrAbAsTYl4ZTluzzRA0My6JyPup/2Aoi23EnPpV16A3bReFCcXRYIkGrEYkV8sQOLF9OM3oqcEnZvRMbq+Ux9SEpo3pKAN9LnQ+JnhaJRzodgSgUNJ0C6GpOjcBX0csELsz8w68s0FokYKpysrjbRn9KMUa+Gcq3wJeOhtJGUfvkfByG0itSERfmwD0xhPm49FCRtorhYE6qkmavV2G+fBc8xF+Os19QkCAwEAAQ==";
            BazaarIAB.init(key);
            HookEvents();
            if (skus.Length <= 0)
            {
                Log("Unrecoverable error: add some sku through the inspector and relaunch the project");
                return;
            }
            _currentSku = skus[0];
            _keys = GetComponent<KeyCollection>();
            for (int i = 0; i < skus.Length; i++)
            {
                var bAddon = Instantiate(buttonPrefab, skuButtonContentPanel).GetComponent<SkuButtonAddon>();
                bAddon.Initialize(this, skus[i]);
            }
        }
        catch (System.Exception e)
        {
            Log(e.Message);
        }
    }

    public void SetSKU(string sku)
    {
        _currentSku = sku;
        Log("Current SKu was changed to " + _currentSku);
    }

    public void GetInventory()
    {
        BazaarIAB.queryInventory(skus);
    }

    public void BuyKey()
    {
        var res = _keys.RetrieveKey(_currentSku);
        switch (res)
        {
            case KeyConsumptionRecord.NOT_FOUND:
            case KeyConsumptionRecord.CONSUMED:
                BazaarIAB.purchaseProduct(_currentSku);
                break;
            case KeyConsumptionRecord.NOT_CONSUMED:
                Log(string.Format("buy key: {0} is not consumed yet", _currentSku));
                break;
            default:
                break;
        }
    }

    public void Consume()
    {
        BazaarIAB.consumeProduct(_currentSku);
    }

    public void Retrieve()
    {
        var res = _keys.RetrieveKey(_currentSku);
        switch (res)
        {
            case KeyConsumptionRecord.CONSUMED:
                Log(string.Format("Retrieve succeeded: {0} is consumed", _currentSku));
                break;
            case KeyConsumptionRecord.NOT_CONSUMED:
                Log(string.Format("Retrieve succeeded: {0} is not consumed", _currentSku));
                break;
            case KeyConsumptionRecord.NOT_FOUND:
                Log("Failed to retrive the " + _currentSku + " sku");
                break;
            default:
                break;
        }
    }

    public void ClearLogs()
    {
        m_text.text = "";
    }

    #region IEventlistner impl
    public void HookEvents()
    {
        IABEventManager.purchaseFailedEvent += IABEventManager_purchaseFailedEvent;
        IABEventManager.purchaseSucceededEvent += IABEventManager_purchaseSucceededEvent;
        IABEventManager.consumePurchaseSucceededEvent += IABEventManager_consumePurchaseSucceededEvent;
        IABEventManager.consumePurchaseFailedEvent += IABEventManager_consumePurchaseFailedEvent;
        IABEventManager.queryInventoryFailedEvent += IABEventManager_queryInventoryFailedEvent;
        IABEventManager.queryInventorySucceededEvent += IABEventManager_queryInventorySucceededEvent;
    }

    public void UnhookEvents()
    {
        IABEventManager.purchaseFailedEvent -= IABEventManager_purchaseFailedEvent;
        IABEventManager.purchaseSucceededEvent -= IABEventManager_purchaseSucceededEvent;
        IABEventManager.consumePurchaseSucceededEvent -= IABEventManager_consumePurchaseSucceededEvent;
        IABEventManager.consumePurchaseFailedEvent -= IABEventManager_consumePurchaseFailedEvent;
        IABEventManager.queryInventoryFailedEvent -= IABEventManager_queryInventoryFailedEvent;
        IABEventManager.queryInventorySucceededEvent -= IABEventManager_queryInventorySucceededEvent;
    }

    private void IABEventManager_queryInventoryFailedEvent(string obj)
    {
        Log(obj);
    }

    private void IABEventManager_queryInventorySucceededEvent(System.Collections.Generic.List<BazaarPurchase> purchases, System.Collections.Generic.List<BazaarSkuInfo> skus)
    {
        if (purchases.Count == 0)
        {
            Log("no item in inventory");
            return;
        }
        for (int i = 0; i < purchases.Count; i++)
        {
            var res = _keys.BuyKeyAsync(purchases[i]);
            Log(string.Format("inv succeeded: pId: {0}, pToken: {1}, client-side res: {2}", purchases[i].ProductId, purchases[i].PurchaseToken, res));
        }
    }

    private void IABEventManager_purchaseFailedEvent(string obj)
    {
        Log("buy failed; errCode : " + obj);
    }

    private void IABEventManager_purchaseSucceededEvent(BazaarPurchase obj)
    {
        var res = _keys.BuyKeyAsync(obj);
        Log(string.Format("buy succeeded : pId: {0}, pToken: {1}, clinet-side res: {2}", obj.ProductId, obj.PurchaseToken, res));
    }
     
    private void IABEventManager_consumePurchaseSucceededEvent(BazaarPurchase purchase)
    {
        var res = _keys.ConsumeKey(purchase);
        Log("Consume sucseeded:" + purchase.ProductId + "; client-side res: " + res);
    }

    private void IABEventManager_consumePurchaseFailedEvent(string err)
    {
        Log("Consume failed" + _currentSku + "; errCode: " + err);
    }
  
    private void Log(string str)
    {
        m_text.text += "\n==========\n" + str;

        StartCoroutine(ExtendDebugTextScrollCr());
    }

    private IEnumerator ExtendDebugTextScrollCr()
    {
        yield return new WaitForEndOfFrame();
        float currentTime = 0;
        while (currentTime < m_lerpTime)
        {
            currentTime += Time.deltaTime;
            float perc = currentTime / m_lerpTime;
            m_debugContentPanel.anchoredPosition = new Vector2(m_debugContentPanel.anchoredPosition.x,
                                                               Mathf.Lerp(m_debugContentPanel.anchoredPosition.y, m_text.rectTransform.rect.height, perc));
            yield return new WaitForEndOfFrame();
        }
    }
    #endregion
}
