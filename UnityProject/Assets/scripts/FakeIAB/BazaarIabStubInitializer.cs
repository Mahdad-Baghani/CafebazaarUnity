using UnityEngine.Serialization;

namespace neo.BazaarPluginStub
{
    using BazaarPlugin;
    using SimpleJSON;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using static BazaarIabStubInitializer;

    /// <summary>
    /// the Bazaar Stub provider, which is decoupled from android platform and is suitable for quick test iterations inside Unity editor
    /// <br></br>
    /// HOW TO USE:
    /// <br></br>
    /// the stub for BazaarIAB in a new namespace so the code changes would be minimal,
    /// just change your code to use "neo.BazaarPluginStub.BazaarIAB" instead of "BazaarIAB"
    /// TO-DOs
    /// right now, the stub only includes inventory, buy and consume functionalities. 
    /// I have to add subscriptions, non-consumables and other stuff which are within the bounds of In-App Billing
    /// </summary>
    public class BazaarIabStubInitializer : MonoBehaviour
    {
        [System.Serializable]
        public class IabStubParameters
        {
            public BazaarSkuDefinition[] m_availableSkus;
            public bool m_billingSupported = true,
                        m_inventoryQueryFails = true,
                        m_purchaseQueryFails = false,
                        m_consumeQueryFails = false;
                        // TO-DO: check if more parameters are needed for better test iterations
        }
        [FormerlySerializedAs("mStubParameters")]
        [FormerlySerializedAs("m_mockParameters")]
        [Header("Bazaar IAB properties")]
        [SerializeField] private IabStubParameters _stubParameters;

        public IEnumerator Start()
        {
            // wait a little while so the BazaarPlugin event manager initializes and can be used inside the Initialize function of BazaarIAB mock
            yield return new WaitForSeconds(.1f); 
            BazaarIAB.Initialize(_stubParameters);
        }
    }

    public class BazaarIAB
    {
        // billing properties
        public static bool _billingSupported;

        // private fields
        private static IABEventManager _eventManager;
        private static Dictionary<string, BazaarPurchase> _bazaarDB;
        private static IabStubParameters _stubParameters;

        /// <summary>
        /// Initialize the mocked provider using MockParameters. <br></br>
        /// the original BazaarIAB 
        /// </summary>
        /// <param name="parameters"></param>
        public static void Initialize(IabStubParameters parameters)
        {
            _eventManager = GameObject.Find("BazaarIABPlugin").GetComponentInChildren<IABEventManager>();
            _bazaarDB = new Dictionary<string, BazaarPurchase>();
            // set the parameters of the mock
            _stubParameters = parameters;

            for (int i = 0; i < _stubParameters.m_availableSkus.Length; i++)
            {
                _bazaarDB.Add(_stubParameters.m_availableSkus[i].ProductId, null);
            }
        }

        internal static void init(string key)
        {
            // no need to do anything special 
            // I put this function here, so there would be no code change while switching between actual Bazaar servers and the mocked version
        }

        public static void areSubscriptionsSupported()
        {
            if (_billingSupported)
                _eventManager.billingSupported("in-app billing is supported");
            else
                _eventManager.billingNotSupported("billing not supported. if you wanna enable it, change billing support from the mock provider component");

        }

        public static void queryInventory(string[] skus)
        {
            // the root of mock result of the inventory query 
            // it is a jsonClass which holds two main keys: "skus" and "purchases"
            JSONNode root = new JSONClass();
            // the sku info mock results of the inventory query as an array of specific sku infos
            JSONNode skuinfs = new JSONArray();
            // the purchase mock results of the inventory query as an array of specific bazaar purchases
            JSONNode purchases = new JSONArray();

            // first, I have to create the sku infos based on the mock definitions of sku infos
            for (int i = 0; i < _stubParameters.m_availableSkus.Length; i++)
            {
                JSONNode node       = new JSONClass();
                node["title"]       = _stubParameters.m_availableSkus[i].Title;
                node["price"]       = _stubParameters.m_availableSkus[i].Price;
                node["type"]        = _stubParameters.m_availableSkus[i].Type;
                node["description"] = _stubParameters.m_availableSkus[i].Description;
                node["productId"]   = _stubParameters.m_availableSkus[i].ProductId;

                skuinfs.Add(node);
            }

            // then, I have to create purchases based on available items in mocked inventory (as what bazaar does when it gathers the purchases from its database)
            foreach (var key in _bazaarDB.Keys)
            {
                // the purchase is not available if its value is equal to null
                if (_bazaarDB[key] == null)
                {
                    continue;
                }
                // create purchase in a json representation from what is available in the mocked inventory
                JSONNode node = new JSONClass();
                node["packageName"]      = _bazaarDB[key].PackageName;
                node["orderId"]          = _bazaarDB[key].OrderId;
                node["productId"]        = _bazaarDB[key].ProductId;
                node["developerPayload"] = _bazaarDB[key].DeveloperPayload;
                node["type"]             = _bazaarDB[key].Type;
                node["purchaseTime"]     = $"{_bazaarDB[key].PurchaseTime}";
                node["purchaseState"]    = $"{(int) _bazaarDB[key].PurchaseState}";
                node["purchaseToken"]    = _bazaarDB[key].PurchaseToken;
                node["signature"]        = _bazaarDB[key].Signature;
                node["originalJson"]     = _bazaarDB[key].OriginalJson;

                purchases.Add(node);
            }

            // create the root object, representing skus and purchases in json format
            root.Add("skus", skuinfs.AsArray);
            root.Add("purchases", purchases.AsArray);

            _eventManager.queryInventorySucceeded(root.ToString());
        }

        internal static void consumeProduct(string currentSku)
        {
            if (_bazaarDB.ContainsKey(currentSku) && _bazaarDB[currentSku] != null)
            {
                if (!_stubParameters.m_consumeQueryFails)
                {
                    var resultNode = BazaarSkuDefinition.createBazaarPurchaseMock(currentSku, _stubParameters.m_availableSkus, out _);
                    _eventManager.consumePurchaseSucceeded(resultNode.ToString());

                    // actually consume the purchase form the mock inventory
                    _bazaarDB[currentSku] = null; 
                }
                else
                {
                    _eventManager.consumePurchaseFailed("consume Err: unknown");
                }
                return;
            }

            _eventManager.consumePurchaseFailed("Consume Err: you do not own the item");
        }

        internal static void purchaseProduct(string currentSku)
        {
            if (!_stubParameters.m_purchaseQueryFails)
            {
                var node = BazaarSkuDefinition.createBazaarPurchaseMock(currentSku, _stubParameters.m_availableSkus, out var purchase);
                // add the item to the mock inventory
                _bazaarDB[currentSku] = purchase;
                // raise the purchase success event
                _eventManager.purchaseSucceeded(node.ToString());
                return;
            }
            _eventManager.purchaseFailed("Purchase Err: unknown");
        }

        internal static void unbindService()
        {
            // no need to do anything special 
            // I put this function here, so there would be no code change while switching between actual Bazaar servers and the mocked version
        }
    }
}