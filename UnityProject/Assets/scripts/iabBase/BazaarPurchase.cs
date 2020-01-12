namespace neo.BazaarRewrite
{
    using Newtonsoft.Json;

    public class BazaarPurchase
    {
        public enum BazaarPurchaseState
        {
            Purchased,
            Canceled,
            Refunded
        }

        [JsonProperty]
        internal string PackageName;
        [JsonProperty]
        internal string OrderId;
        [JsonProperty]
        internal string ProductId;
        [JsonProperty]
        internal string DeveloperPayload;
        [JsonProperty]
        internal string Type;
        [JsonProperty]
        internal string PurchaseTime;
        [JsonProperty]
        internal BazaarPurchaseState PurchaseState;
        [JsonProperty]
        internal string PurchaseToken;
        [JsonProperty]
        internal string Signature;
        [JsonProperty]
        internal string OriginalJson;

        public BazaarPurchase() { }
        public override string ToString()
        {
            return string.Format("<BazaarPurchase> packageName: {0}, orderId: {1}, productId: {2}, developerPayload: {3}, purchaseToken: {4}, purchaseState: {5}, signature: {6}, type: {7}, json: {8}",
                PackageName, OrderId, ProductId, DeveloperPayload, PurchaseToken, PurchaseState, Signature, Type, OriginalJson);
        }

    }
}