using BazaarPlugin;
using SimpleJSON;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName ="BazaarMock/MockSkuDefinition")]
public class BazaarMockSkuDefinition : ScriptableObject
{
    public string Title;
    public string Price;
    public string Type;
    public string Description;
    public string ProductId;

    /// <summary>
    /// creates a mock of a bazaar purchase
    /// </summary>
    /// <param name="currentSku">the sku we're looking for</param>
    /// <param name="purchase">the raw bazaar purchase to be initialized</param>
    /// <returns>returns the json node of the bazaar purchase mock</returns>
    public static JSONNode createMockedBazaarPurchase(string currentSku, BazaarMockSkuDefinition[] defs, out BazaarPurchase purchase)
    {
        BazaarMockSkuDefinition p = defs.ToList().Find(def => def.ProductId == currentSku);
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
