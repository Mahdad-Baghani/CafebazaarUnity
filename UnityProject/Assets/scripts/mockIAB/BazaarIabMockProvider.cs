namespace neo.BazaarMock
{
    using BazaarPlugin;
    using SimpleJSON;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BazaarIabMockProvider : MonoBehaviour
    {
        public string[] m_availableSkus;

        [Header("Bazaar IAB properties")]
        [SerializeField] private bool m_billingSupported = true;


        public IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            BazaarIAB.Initialize(m_availableSkus);
        }

    }

    public class BazaarIAB
    {
        // billing properties
        public static bool m_billingSupported;

        // private fields
        private static IABEventManager m_eventManager;
        private static Dictionary<string, BazaarPurchase> m_inventory;
        private static string[] m_availableSkus;

        public static void Initialize(string[] skus)
        {
            m_eventManager = GameObject.Find("BazaarIABPlugin").GetComponentInChildren<IABEventManager>();
            m_availableSkus = skus;
            m_inventory = new Dictionary<string, BazaarPurchase>();

            for (int i = 0; i < m_availableSkus.Length; i++)
            {
                m_inventory.Add(m_availableSkus[i], null);
            }
        }

        internal static void init(string key)
        {
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
            JSONNode skuinfs = new JSONArray();
            for (int i = 0; i < m_availableSkus.Length; i++)
            {
                JSONNode node = new JSONClass();
                node["productId"] = m_availableSkus[i];
                skuinfs.Add(node);
            }

            JSONNode purchases = new JSONNode();
            foreach (var key in m_inventory.Keys)
            {
                if (m_inventory[key] == null)
                {
                    continue;
                }
                var node = new JSONNode();
                node["productId"] =  new JSONNode() { Value = key };
                node.Add("purchaseToken", new JSONNode() { Value = UnityEngine.Random.value.ToString() });
                purchases.Add(node);
            }
            JSONNode inv = new JSONClass();
            inv.Add("skus", skuinfs);

            m_eventManager.queryInventorySucceeded(inv);
        }

        internal static void consumeProduct(string currentSku)
        {
        }

        internal static void purchaseProduct(string currentSku)
        {
        }

        internal static void unbindService()
        {
        }
    }
}