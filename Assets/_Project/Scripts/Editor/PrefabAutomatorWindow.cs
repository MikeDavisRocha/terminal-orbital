using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PrefabAutomatorWindow : EditorWindow
{
    private DefaultAsset sourceFolder;

    [MenuItem("Tools/Pipeline Toolkit")]
    public static void ShowWindow()
    {
        GetWindow<PrefabAutomatorWindow>("Pipeline Toolkit");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Pasta de Modelos de Origem", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Arraste a pasta que contém seus modelos (.fbx) para o campo abaixo. Ambas as ferramentas usarão esta pasta como fonte.", MessageType.Info);

        sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField("Pasta de Modelos", sourceFolder, typeof(DefaultAsset), false);

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Ferramenta 1: Extrair Materiais", EditorStyles.boldLabel);
        if (GUILayout.Button("Extrair Materiais da Pasta Inteira"))
        {
            BatchExtractMaterials();
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Ferramenta 2: Criar Prefabs", EditorStyles.boldLabel);
        if (GUILayout.Button("Criar Prefabs da Pasta Inteira"))
        {
            BatchCreatePrefabs();
        }
    }

    private void BatchExtractMaterials()
    {
        if (sourceFolder == null)
        {
            EditorUtility.DisplayDialog("Erro", "Por favor, arraste uma pasta para o campo 'Pasta de Modelos'.", "OK");
            return;
        }

        string sourceModelPath = AssetDatabase.GetAssetPath(sourceFolder);
        string centralMaterialsFolder = "Assets/_Project/Art/Materials";

        var modelPaths = Directory.GetFiles(sourceModelPath, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase) || path.EndsWith(".obj", System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        Debug.Log("--- INICIANDO PASSO 1: Criação de todos os arquivos de material...");
        foreach (var path in modelPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null || importer.materialLocation == ModelImporterMaterialLocation.External) continue;

            string modelName = Path.GetFileNameWithoutExtension(path);
            string subfolderPath = $"{centralMaterialsFolder}/{modelName}";

            if (!Directory.Exists(subfolderPath))
            {
                AssetDatabase.CreateFolder(centralMaterialsFolder, modelName);
            }

            var materials = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>();
            foreach (var material in materials)
            {
                if (AssetDatabase.IsSubAsset(material))
                {
                    string materialPath = $"{subfolderPath}/{material.name}.mat";
                    if (File.Exists(Path.GetFullPath(materialPath))) continue;

                    var newMaterial = new Material(material);
                    AssetDatabase.CreateAsset(newMaterial, materialPath);
                }
            }
        }
        Debug.Log("--- PASSO 1 CONCLUÍDO: Todos os arquivos .mat foram criados no disco.");

        
        Debug.Log("Sincronizando AssetDatabase...");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("--- INICIANDO PASSO 2: Re-importando modelos para conectar aos novos materiais...");
        int modelsProcessed = 0;
        foreach (var path in modelPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null && importer.materialLocation != ModelImporterMaterialLocation.External)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.materialLocation = ModelImporterMaterialLocation.External;
                importer.materialSearch = ModelImporterMaterialSearch.Everywhere;

                importer.SaveAndReimport();
                modelsProcessed++;

                string modelFolder = Path.GetDirectoryName(path);
                string unwantedFolderPath = Path.Combine(modelFolder, "Materials").Replace("\\", "/");
                if (AssetDatabase.IsValidFolder(unwantedFolderPath))
                {
                    AssetDatabase.DeleteAsset(unwantedFolderPath);
                }
            }
        }
        Debug.Log("--- PASSO 2 CONCLUÍDO: Modelos re-importados.");

        EditorUtility.DisplayDialog("Sucesso", $"Processo concluído! {modelsProcessed} modelos tiveram seus materiais extraídos e conectados.", "OK");
    }

    private void BatchCreatePrefabs()
    {
        if (sourceFolder == null)
        {
            EditorUtility.DisplayDialog("Erro", "Por favor, arraste uma pasta para o campo 'Pasta de Modelos'.", "OK");
            return;
        }

        string sourceModelPath = AssetDatabase.GetAssetPath(sourceFolder);
        string prefabsFolder = "Assets/_Project/Prefabs";

        var modelPaths = Directory.GetFiles(sourceModelPath, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase) || path.EndsWith(".obj", System.StringComparison.OrdinalIgnoreCase));

        int prefabsCreated = 0;
        foreach (var path in modelPaths)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (modelAsset == null) continue;

            string modelName = Path.GetFileNameWithoutExtension(path);
            string prefabPath = $"{prefabsFolder}/{modelName}.prefab";
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(modelAsset, prefabPath);

            if (prefab != null)
            {
                prefabsCreated++;
                Debug.Log($"Prefab criado com sucesso em: {prefabPath}");
            }
            else
            {
                Debug.LogError($"Falha ao criar o Prefab para o modelo: {modelName}");
            }
        }

        var prefabsFolderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(prefabsFolder);
        if (prefabsFolderObject != null) EditorGUIUtility.PingObject(prefabsFolderObject);

        EditorUtility.DisplayDialog("Sucesso", $"Processo concluído! {prefabsCreated} prefabs foram criados na pasta '{prefabsFolder}'.", "OK");
    }
}