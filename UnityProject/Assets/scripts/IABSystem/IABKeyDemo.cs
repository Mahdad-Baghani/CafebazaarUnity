#if USE_FAKE_BAZAAR
    using BazaarIAB = neo.BazaarPluginStub.BazaarIAB;
#else
    using BazaarPlugin;
#endif

using System.Collections;
using BazaarPlugin;
using UnityEngine;

public class IABKeyDemo : MonoBehaviour, IEventListner
{
    public GameObject buttonPrefab;
    public Transform skuButtonContentPanel;
    public string[] skus;
    public TMPro.TextMeshProUGUI m_text;
    // UI fields
    public RectTransform m_debugContentPanel;
    public float m_lerpTime = 1f;
    
    WalletStub _walletStub;
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
            _walletStub = new WalletStub(false);
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

    public async void BuyKey()
    {
        var res = await _walletStub.RetrieveKey(_currentSku);
        switch (res)
        {
            case KeyConsumptionRecord.NOT_FOUND:
            case KeyConsumptionRecord.CONSUMED:
                BazaarIAB.purchaseProduct(_currentSku);
                break;
            case KeyConsumptionRecord.NOT_CONSUMED:
                Log($"buy key: {_currentSku} is not consumed yet");
                break;
            default:
                break;
        }
    }

    public void Consume()
    {
        BazaarIAB.consumeProduct(_currentSku);
    }

    public async void Retrieve()
    {
        var res = await _walletStub.RetrieveKey(_currentSku);
        switch (res)
        {
            case KeyConsumptionRecord.CONSUMED:
                Log($"Retrieve succeeded: {_currentSku} is consumed");
                break;
            case KeyConsumptionRecord.NOT_CONSUMED:
                Log($"Retrieve succeeded: {_currentSku} is not consumed");
                break;
            case KeyConsumptionRecord.NOT_FOUND:
                Log("Failed to retrieve the " + _currentSku + " sku");
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

    private void IABEventManager_queryInventoryFailedEvent(string err)
    {
        Log(err);
    }

    private void IABEventManager_queryInventorySucceededEvent(System.Collections.Generic.List<BazaarPlugin.BazaarPurchase> purchases, System.Collections.Generic.List<BazaarPlugin.BazaarSkuInfo> skus)
    {
        if (purchases.Count == 0)
        {
            Log("no item in inventory");
            return;
        }
        for (int i = 0; i < purchases.Count; i++)
        {
            var res = _walletStub.BuyKeyAsync(purchases[i]);
            Log(
                $"inv succeeded: pId: {purchases[i].ProductId}, pToken: {purchases[i].PurchaseToken}, client-side res: {res}");
        }
    }

    private void IABEventManager_purchaseFailedEvent(string err)
    {
        Log($"buy failed; errCode: {err}");
    }

    private void IABEventManager_purchaseSucceededEvent(BazaarPlugin.BazaarPurchase purchase)
    {
        var res = _walletStub.BuyKeyAsync(purchase);
        Log($"buy succeeded : pId: {purchase.ProductId}, pToken: {purchase.PurchaseToken}, clinet-side res: {res}");
    }
     
    private void IABEventManager_consumePurchaseSucceededEvent(BazaarPlugin.BazaarPurchase purchase)
    {
        var res = _walletStub.ConsumeKey(purchase);
        Log($"Consume succeeded: {purchase.ProductId}; client-side res: {res}");
    }

    private void IABEventManager_consumePurchaseFailedEvent(string err)
    {
        Log($"Consume failed: {_currentSku}; errCode: {err}");
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
