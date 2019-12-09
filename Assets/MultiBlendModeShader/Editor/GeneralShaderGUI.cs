using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ブレンドモード切替可能なシェーダーのインスペクタ拡張
/// </summary>
public class GeneralShaderGUI : ShaderGUI
{
    /// <summary>
    /// ブレンドモード
    /// </summary>
    enum BlendMode
    {
        Opaque,
        Cutout,
        Transparent,
        Additive,
        Multiply,
    }

    /// <summary>
    /// インスペクタを拡張します
    /// </summary>
    /// <param name="materialEditor">MaterialEditor</param>
    /// <param name="properties">Properties</param>
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        DrawBlendMode(materialEditor, properties);

        var mainTex = FindProperty("_MainTex", properties);
        materialEditor.ShaderProperty(mainTex, mainTex.displayName);

        var tintColor = FindProperty("_TintColor", properties);
        materialEditor.ShaderProperty(tintColor, tintColor.displayName);

        var cullMode = FindProperty("_CullMode", properties);
        materialEditor.ShaderProperty(cullMode, cullMode.displayName);

        materialEditor.RenderQueueField();
    }

    /// <summary>
    /// BlendModeのインスペクタを描画します
    /// </summary>
    /// <param name="materialEditor">MaterialEditor</param>
    /// <param name="properties">Properties</param>
    void DrawBlendMode(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var blendMode = FindProperty("_BlendMode", properties);
        var cutoff = FindProperty("_Cutoff", properties);
        var mode = (BlendMode) blendMode.floatValue;

        using (var scope = new EditorGUI.ChangeCheckScope())
        {
            mode = (BlendMode) EditorGUILayout.Popup("Blend Mode", (int) mode, Enum.GetNames(typeof(BlendMode)));

            if (scope.changed)
            {
                blendMode.floatValue = (float) mode;
                foreach (UnityEngine.Object obj in blendMode.targets)
                {
                    ApplyBlendMode(obj as Material, mode);
                }
            }
        }

        if (mode == BlendMode.Cutout)
        {
            materialEditor.ShaderProperty(cutoff, cutoff.displayName);
        }
    }

    /// <summary>
    /// マテリアルのブレンドモードを変更します
    /// </summary>
    /// <param name="material">マテリアル</param>
    /// <param name="blendMode">ブレンドモード</param>
    static void ApplyBlendMode(Material material, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.renderQueue = -1;
                break;

            case BlendMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;

            case BlendMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                break;

            case BlendMode.Additive:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                break;

            case BlendMode.Multiply:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.DstColor);
                material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                break;

            default:
                throw new ArgumentOutOfRangeException("blendMode", blendMode, null);
        }
    }

    /// <summary>
    /// シェーダー切り替え時の処理をします
    /// </summary>
    /// <param name="material">マテリアル</param>
    /// <param name="oldShader">切り替え前のシェーダー</param>
    /// <param name="newShader">切り替え後のシェーダー</param>
    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        // MaterialのShader切り替え時にBlend指定が変更されてしまうので再設定します。
        ApplyBlendMode(material, (BlendMode) material.GetFloat("_BlendMode"));
    }
}
