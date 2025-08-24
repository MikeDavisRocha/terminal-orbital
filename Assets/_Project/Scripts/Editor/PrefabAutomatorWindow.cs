using UnityEngine;
using UnityEditor;
using System.IO;

public class PrefabAutomatorWindow : EditorWindow
{
    private GameObject modelAsset;

    [MenuItem("Tools/Prefab Automator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabAutomatorWindow>("Prefab Automator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Arraste seu modelo 3D abaixo:", EditorStyles.boldLabel);

        modelAsset = (GameObject)EditorGUILayout.ObjectField("Modelo 3D (.fbx)", modelAsset, typeof(GameObject), false);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Criar Prefab"))
        {
            CreatePrefab();
        }
    }

    private void CreatePrefab()
    {
        if (modelAsset == null)
        {
            Debug.LogError("Erro: Por favor, arraste um modelo para o campo acima.");
            return;
        }

        string modelPath = AssetDatabase.GetAssetPath(modelAsset);
        string modelName = Path.GetFileNameWithoutExtension(modelPath);
        string prefabPath = "_Project/Prefabs/" + modelName + ".prefab";
        prefabPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + prefabPath);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(modelAsset, prefabPath);

        if (prefab != null)
        {
            Debug.Log("Prefab criado com sucesso em: " + prefabPath);
            EditorGUIUtility.PingObject(prefab);
        }
        else
        {
            Debug.LogError("Falha ao criar o Prefab.");
        }
    }
}