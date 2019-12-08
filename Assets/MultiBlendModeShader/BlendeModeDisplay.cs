using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlendeModeDisplay : MonoBehaviour
{
    enum BlendMode
    {
        Opaque,
        Cutout,
        Transparent,
        Additive,
        Multiply,
    }

    [SerializeField] Material material;

    [SerializeField] TextMeshPro textMeshPro;

    // Update is called once per frame
    void Update()
    {
        var mode = (BlendMode)material.GetFloat("_BlendMode");
        textMeshPro.text = mode.ToString();
    }
}
