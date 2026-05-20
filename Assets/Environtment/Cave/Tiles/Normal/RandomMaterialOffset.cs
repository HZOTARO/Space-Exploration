using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RandomMaterialOffset : MonoBehaviour
{
    [Header("Shader Configuration")]
    [Tooltip("The internal name of your texture. Use '_MainTex' for Built-In pipeline, or '_BaseMap' for URP/HDRP.")]
    public string texturePropertyName = "_BaseMap";

    [Header("Randomization Axes")]
    public bool randomizeX = true;
    public bool randomizeY = true;

    void Start()
    {
        Renderer meshRenderer = GetComponent<Renderer>();

        float offsetX = randomizeX ? Random.value : 0f;
        float offsetY = randomizeY ? Random.value : 0f;

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        meshRenderer.GetPropertyBlock(propBlock);

        Vector2 originalTiling = new Vector2(1, 1);

        if (meshRenderer.sharedMaterial != null && meshRenderer.sharedMaterial.HasProperty(texturePropertyName))
        {
            originalTiling = meshRenderer.sharedMaterial.GetTextureScale(texturePropertyName);
        }

        string stPropertyName = texturePropertyName + "_ST";

        Vector4 finalScaleAndOffset = new Vector4(originalTiling.x, originalTiling.y, offsetX, offsetY);

        propBlock.SetVector(stPropertyName, finalScaleAndOffset);

        meshRenderer.SetPropertyBlock(propBlock);
    }
}