#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class FolderStructureCreator
{
    [MenuItem("Tools/Create Folder Structure")]
    public static void CreateFolders()
    {
        string[] folders = new string[]
        {
            "Assets/_Project",
            "Assets/_Project/Art/Materials",
            "Assets/_Project/Art/Models",
            "Assets/_Project/Art/Textures",
            "Assets/_Project/Art/Animations",
            "Assets/_Project/Art/VFX",
            "Assets/_Project/Audio/Music",
            "Assets/_Project/Audio/SFX",
            "Assets/_Project/Audio/Mixers",
            "Assets/_Project/Data/Items",
            "Assets/_Project/Data/Recipes",
            "Assets/_Project/Data/Equipment",
            "Assets/_Project/Data/Progression",
            "Assets/_Project/Data/Configuration",
            "Assets/_Project/Prefabs/Containers",
            "Assets/_Project/Prefabs/Equipment",
            "Assets/_Project/Prefabs/Food",
            "Assets/_Project/Prefabs/UI",
            "Assets/_Project/Prefabs/VFX",
            "Assets/_Project/Scenes/Core",
            "Assets/_Project/Scenes/Gameplay",
            "Assets/_Project/Scenes/Menus",
            "Assets/_Project/Scenes/Testing",
            "Assets/_Project/Scripts/Core",
            "Assets/_Project/Scripts/Data",
            "Assets/_Project/Scripts/Equipment",
            "Assets/_Project/Scripts/Food",
            "Assets/_Project/Scripts/Input",
            "Assets/_Project/Scripts/Interaction",
            "Assets/_Project/Scripts/Inventory",
            "Assets/_Project/Scripts/Orders",
            "Assets/_Project/Scripts/Player",
            "Assets/_Project/Scripts/Progression",
            "Assets/_Project/Scripts/UI",
            "Assets/_Project/Scripts/Utilities",
            "Assets/_Project/Settings/InputActions",
            "Assets/_Project/Settings/Cinemachine",
            "Assets/_Project/Settings/PostProcessing"
        };

        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"Created: {folder}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Folder structure created successfully!");
    }
}
#endif