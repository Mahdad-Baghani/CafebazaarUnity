namespace neo.BazaarRewrite
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BazaarSkuInfo
    {
        [JsonProperty]
        internal string Title;
        [JsonProperty]
        internal string Price;
        [JsonProperty]
        internal string Type;
        [JsonProperty]
        internal string Description;
        [JsonProperty]
        internal string ProductId;

        public BazaarSkuInfo() { }
        public override string ToString()
        {
            return string.Format("<BazaarSkuInfo> title: {0}, price: {1}, type: {2}, description: {3}, productId: {4}",
                Title, Price, Type, Description, ProductId);
        }
    }
}
