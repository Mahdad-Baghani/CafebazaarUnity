using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkuButtonAddon : MonoBehaviour
{
    public Text m_text;
    IABKeyDemo _mKeyDemo;

    public void Initialize(IABKeyDemo keytest, string sku)
    {
        _mKeyDemo = keytest;
        GetComponent<Button>().onClick.AddListener(() => _mKeyDemo.SetSKU(sku));
        m_text.text = sku;
    }
}
