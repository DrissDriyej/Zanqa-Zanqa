using UnityEngine;
using UnityEditor;

public class WaterSetup : MonoBehaviour
{
    [ContextMenu("Setup Fountain Water")]
    public void Setup()
    {
        // 1. Setup Material
        string shaderName = "Custom/WaterPBR";
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogError($"Shader {shaderName} not found!");
            return;
        }

        Material waterMat = new Material(shader);
        waterMat.name = "WaterPBR_Mat";
        
        // Settings PBR sympas (Plus clairs)
        waterMat.SetColor("_BaseColor", new Color(0.4f, 0.8f, 0.9f, 0.2f));
        waterMat.SetColor("_ShallowColor", new Color(0.4f, 0.9f, 1.0f, 0.3f));
        waterMat.SetColor("_DeepColor", new Color(0.1f, 0.4f, 0.7f, 0.8f));
        waterMat.SetFloat("_Smoothness", 0.95f);
        waterMat.SetFloat("_RefractionStrength", 0.2f);
        waterMat.SetFloat("_DepthDistance", 1.0f);
        
        // Sauvegarder le matériau dans le projet si possible (Editor only)
        #if UNITY_EDITOR
        string path = "Assets/Prefabs/Driss/Water/WaterPBR_Mat.mat";
        AssetDatabase.CreateAsset(waterMat, path);
        Debug.Log("Created Water Material at " + path);
        #endif

        // 2. Appliquer à l'objet actuel
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            r.sharedMaterial = waterMat;
        }
    }
}
