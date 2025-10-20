#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using PizzaShop.Data;

namespace PizzaShop.Editor
{
    /// <summary>
    /// Editor tool to quickly create data ScriptableObjects.
    /// </summary>
    public class DataCreationTool : EditorWindow
    {
        [MenuItem("Pizza Shop/Data Creation Tool")]
        public static void ShowWindow()
        {
            GetWindow<DataCreationTool>("Data Creation Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("Quick Data Creation", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Sample Ingredients"))
            {
                CreateSampleIngredients();
            }

            if (GUILayout.Button("Create Sample Container"))
            {
                CreateSampleContainer();
            }

            if (GUILayout.Button("Create Sample Make Table"))
            {
                CreateSampleMakeTable();
            }
        }

        private void CreateSampleIngredients()
        {
            // Create Dough
            var dough = CreateInstance<IngredientData>();
            CreateAsset(dough, "Resources/Data/Ingredients", "Dough");

            // Create Marinara Sauce
            var sauce = CreateInstance<IngredientData>();
            CreateAsset(sauce, "Resources/Data/Ingredients", "MarinaraSauce");

            // Create Mozzarella
            var cheese = CreateInstance<IngredientData>();
            CreateAsset(cheese, "Resources/Data/Ingredients", "Mozzarella");

            // Create Pepperoni
            var pepperoni = CreateInstance<IngredientData>();
            CreateAsset(pepperoni, "Resources/Data/Ingredients", "Pepperoni");

            Debug.Log("Sample ingredients created in Resources/Data/Ingredients/");
            AssetDatabase.Refresh();
        }

        private void CreateSampleContainer()
        {
            var container = CreateInstance<ContainerData>();
            CreateAsset(container, "Resources/Data/Containers", "SmallContainer");

            Debug.Log("Sample container created in Resources/Data/Containers/");
            AssetDatabase.Refresh();
        }

        private void CreateSampleMakeTable()
        {
            var table = CreateInstance<MakeTableData>();
            CreateAsset(table, "Resources/Data/MakeTables", "BasicMakeTable");

            Debug.Log("Sample make table created in Resources/Data/MakeTables/");
            AssetDatabase.Refresh();
        }

        private void CreateAsset(ScriptableObject obj, string path, string name)
        {
            string fullPath = $"Assets/{path}";

            // Create directory if it doesn't exist
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                string[] folders = path.Split('/');
                string currentPath = "Assets";

                for (int i = 0; i < folders.Length; i++)
                {
                    string newPath = $"{currentPath}/{folders[i]}";
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }

            AssetDatabase.CreateAsset(obj, $"{fullPath}/{name}.asset");
        }
    }
}
#endif