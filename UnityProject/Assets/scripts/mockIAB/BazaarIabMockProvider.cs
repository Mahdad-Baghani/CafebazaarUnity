namespace neo.BazaarMock
{
    using BazaarPlugin;
    using SimpleJSON;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static neo.BazaarMock.BazaarIabMockProvider;

    /// <summary>
    /// add this to a gameobject so that it initialized the mocked BazaarIAB instance
    /// </summary>
    public class BazaarIabMockProvider : MonoBehaviour
    {
        [System.Serializable]
        public class IabMockParameters
        {
            public BazaarMockSkuDefinition[] m_availableSkus;
            public bool m_billingSupported = true,
                        m_inventoryQueryFails = true,
                        m_purchaseQueryFails = false,
                        m_consumeQueryFails = false;
                        // TO-DO: check if more parameters are needed for better test iterations
        }
        [Header("Bazaar IAB properties")]
        [SerializeField] private IabMockParameters m_mockParameters;

        public IEnumerator Start()
        {
            // wait a little while so the BazaarPlugin event manager initializes and can be used inside the Initialize function of BazaarIAB mock
            yield return new WaitForSeconds(.1f); 
            BazaarIAB.Initialize(m_mockParameters);
        }
    }

    /// <summary>
    /// the mocked Bazaar provider, which is decoupled from android platform and is suitable for quick test iterations inside Unity editor 
    /// HOW TO USE:
    /// BazaarIAB is mocked in a new namespace so the code changes would be minimal,
    /// just change your code to use "neo.BazaarMock.BazaarIAB" instead of "BazaarIAB"
    /// TO-DOs
    /// right now, the mock only includes inventory, buy and consume functionalities. 
    /// I have to add subscriptions, non-consumables and other stuff which are within the bounds of In-App Billing
    /// </summary>
    public class BazaarIAB
    {
        // billing properties
        public static bool m_billingSupported;

        // private fields
        private static IABEventManager m_eventManager;
        private static Dictionary<string, BazaarPurchase> m_inventory;
        private static IabMockParameters m_mockParameters;

        /// <summary>
        /// Initialize the mocked provider using MockParameters. <br></br>
        /// the original BazaarIAB 
        /// </summary>
        /// <param name="parameters"></param>
        public static void Initialize(IabMockParameters parameters)
        {
            m_eventManager = GameObject.Find("BazaarIABPlugin").GetComponentInChildren<IABEventManager>();
            m_inventory = new Dictionary<string, BazaarPurchase>();
            // set the parameters of the mock
            m_mockParameters = parameters;

            for (int i = 0; i < m_mockParameters.m_availableSkus.Length; i++)
            {
                m_inventory.Add(m_mockParameters.m_availableSkus[i].ProductId, null);
            }
        }

        internal static void init(string key)
        {
            // no need to do anything special 
            // I put this function here, so there would be no code change while switching between actual Bazaar servers and the mocked version
        }

        public static void areSubscriptionsSupported()
        {
            if (m_billingSupported)
                m_eventManager.billingSupported("in-app billing is supported");
            else
                m_eventManager.billingNotSupported("billing not supported. if you wanna enable it, change billing support from the mock provider component");

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
            for (int i = 0; i < m_mockParameters.m_availableSkus.Length; i++)
            {
                JSONNode node       = new JSONClass();
                node["title"]       = m_mockParameters.m_availableSkus[i].Title;
                node["price"]       = m_mockParameters.m_availableSkus[i].Price;
                node["type"]        = m_mockParameters.m_availableSkus[i].Type;
                node["description"] = m_mockParameters.m_availableSkus[i].Description;
                node["productId"]   = m_mockParameters.m_availableSkus[i].ProductId;

                skuinfs.Add(node);
            }

            // then, I have to create purchases based on available items in mocked inventory (as what bazaar does when it gathers the purchases from its database)
            foreach (var key in m_inventory.Keys)
            {
                // the purchase is not available if its value is equal to null
                if (m_inventory[key] == null)
                {
                    continue;
                }
                // create purchase in a json representation from what is available in the mocked inventory
                JSONNode node = new JSONClass();
                node["packageName"]      = m_inventory[key].PackageName;
                node["orderId"]          = m_inventory[key].OrderId;
                node["productId"]        = m_inventory[key].ProductId;
                node["developerPayload"] = m_inventory[key].DeveloperPayload;
                node["type"]             = m_inventory[key].Type;
                node["purchaseTime"]     = string.Format("{0}", m_inventory[key].PurchaseTime);
                node["purchaseState"]    = string.Format("{0}", (int)m_inventory[key].PurchaseState);
                node["purchaseToken"]    = m_inventory[key].PurchaseToken;
                node["signature"]        = m_inventory[key].Signature;
                node["originalJson"]     = m_inventory[key].OriginalJson;

                purchases.Add(node);
            }
            // create the root object, representing skus and purchases in json format
            root.Add("skus", skuinfs.AsArray);
            root.Add("purchases", purchases.AsArray);

            m_eventManager.queryInventorySucceeded(root.ToString());
        }

        internal static void consumeProduct(string currentSku)
        {
            if (m_inventory.ContainsKey(currentSku) && m_inventory[currentSku] != null)
            {
                if (!m_mockParameters.m_consumeQueryFails)
                {
                    BazaarPurchase purchase;
                    var resultNode = createMockedBazaarPurchase(currentSku, out purchase);
                    m_eventManager.consumePurchaseSucceeded(resultNode.ToString());

                    // actually consume the purchase form the mock inventory
                    m_inventory[currentSku] = null; 
                }
                else
                {
                    m_eventManager.consumePurchaseFailed("consume Err: unknown");
                }
                return;
            }

            m_eventManager.consumePurchaseFailed("Consume Err: you do not own the item");
        }

        internal static void purchaseProduct(string currentSku)
        {
            if (!m_mockParameters.m_purchaseQueryFails)
            {
                BazaarPurchase purchase;
                JSONNode node = createMockedBazaarPurchase(currentSku, out purchase);
                purchase.fromJson(node.AsObject);
                // add the item to the mock inventory
                m_inventory[currentSku] = purchase;
                // raise the purchase success event
                m_eventManager.purchaseSucceeded(node.ToString());
                return;
            }
            m_eventManager.purchaseFailed("Purchase Err: unknown");
        }

        internal static void unbindService()
        {
            // no need to do anything special 
            // I put this function here, so there would be no code change while switching between actual Bazaar servers and the mocked version
        }

        /// <summary>
        /// creates a mock of a bazaar purchase
        /// </summary>
        /// <param name="currentSku">the sku we're looking for</param>
        /// <param name="purchase">the raw bazaar purchase to be initialized</param>
        /// <returns>returns the json node of the bazaar purchase mock</returns>
        private static JSONNode createMockedBazaarPurchase(string currentSku, out BazaarPurchase purchase)
        {
            BazaarMockSkuDefinition p = m_mockParameters.m_availableSkus.ToList().Find(def => def.ProductId == currentSku);
            JSONNode node = new JSONClass();
            node["packageName"] = "test";
            node["orderId"] = "test";
            node["productId"] = p.ProductId;
            node["developerPayload"] = "test";
            node["type"] = p.Type;
            node["purchaseTime"] = "0";
            node["purchaseState"] = string.Format("{0}", (int)BazaarPurchase.BazaarPurchaseState.Purchased);
            node["purchaseToken"] = "test";
            node["signature"] = "test";

            purchase = new BazaarPurchase();
            purchase.fromJson(node.AsObject);
            return node;
        }
    }
}