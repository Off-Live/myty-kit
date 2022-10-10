using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MYTYKit
{
    public class MYTY2DAssetImporter : EditorWindow
    {
    
        public VisualTreeAsset UITemplate;
        public VisualTreeAsset PSBListTemplate;

        private ListView m_psbListView;
        private List<TemplateMapEntry> m_templateList;
        private string m_traitPath = "";
        private TraitItem[] m_traits;

    
        [MenuItem("MYTY Kit/Import 2D Avatar Asset", false, 0)]
        public static void ShowGUI()
        {
            GetWindow<MYTY2DAssetImporter>().titleContent = new GUIContent("Import 2D Avatar Asset");
        }


        private void CreateGUI()
        {
            UITemplate.CloneTree(rootVisualElement);
            rootVisualElement.Q<Button>("BTNTraitOpen").clicked += OpenTraitMap;
            rootVisualElement.Q<Button>("BTNImport").clicked += ImportAsset;

            maxSize = new Vector2(720, 50);
            minSize = maxSize;
       
            m_psbListView = rootVisualElement.Q<ListView>("LSTPSB");
        
            m_psbListView.makeItem = () =>
            {
                var newElement = PSBListTemplate.Instantiate();
                var listDelegate = new ListDelegate(newElement);
                newElement.userData = listDelegate;
                return newElement;
            };

            m_psbListView.bindItem = (item, index) =>
            {
                (item.userData as ListDelegate).SetData(m_templateList[index], m_traitPath , (string filePath)=>
                {
                    m_templateList[index].fileName = filePath;
                });
            };
        }

        private void OpenTraitMap()
        {
            string path = EditorUtility.OpenFilePanel("Select trait mapping file", "", "json");
            if (path.Length == 0)
            {
                return;
            }
            var info = new FileInfo(path);
            m_traitPath = "";
        
            if (Directory.Exists(MYTYUtil.AssetPath))
            {
                var reply = EditorUtility.DisplayDialog("Import Avatar Asset", "To proceed, the previous MYTY Avatar asset will be deleted.", "Proceed", "Calcel");
                if (reply) Directory.Delete(MYTYUtil.AssetPath, true);
                else return;
            }
        
        
            rootVisualElement.Q<TextField>("TXTTraitFile").value = path;
            m_traitPath = info.DirectoryName;

            MYTYUtil.BuildAssetPath(MYTYUtil.AssetPath);
            ProcessTraitText(File.ReadAllText(path));
            rootVisualElement.Q("PANPhotoshop").RemoveFromClassList("hide");
            rootVisualElement.Q("PANBtn").RemoveFromClassList("hide");
            rootVisualElement.Q("PANPadding").RemoveFromClassList("hide");
            maxSize = new Vector2(720, 400);
            minSize = maxSize;

        }

        private void ImportAsset()
        {
            var fileSet = new SortedSet<string>();
            var filenameMap = new Dictionary<string, string>();
            var traitMap = new Dictionary<string, string>();
            foreach(var template in m_templateList)
            {
                if (template.fileName == null || template.fileName.Length ==0)
                {
                    EditorUtility.DisplayDialog("Import Avatar Asset", "Please fill all fields", "OK");
                    return;
                }
                fileSet.Add(template.fileName);
            }

            var mytyManager = GameObject.FindObjectOfType<MYTYAssetTemplate>();
            if (mytyManager == null)
            {
                var mytyManagerGo = new GameObject("MYTYAssetTemplate");
                mytyManagerGo.AddComponent<MYTYAssetTemplate>();
            }


            foreach (var filename in fileSet)
            {
                var fileinfo = new FileInfo(filename);
                var templateAssetPath = MYTYUtil.AssetPath + "/" + fileinfo.Name;
                if (!File.Exists(templateAssetPath))
                {
                    File.Copy(filename, templateAssetPath);
                }
                filenameMap[filename] = templateAssetPath;  
            }


            foreach (var template in m_templateList)
            {
                template.fileName = filenameMap[template.fileName];
                traitMap[template.templateName] = template.fileName;
            }

            var assetSCO = ScriptableObject.CreateInstance<MYTYAssetScriptableObject>();
            AssetDatabase.CreateAsset(assetSCO, MYTYUtil.AssetPath + "/" + "MYTYAssetData.asset");

            assetSCO.traits = new();
            foreach (var trait in m_traits)
            {
                trait.filename = traitMap[trait.filename];
                assetSCO.traits.Add(trait);
            }
            EditorUtility.SetDirty(assetSCO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Import Avatar Asset", "All files are imported. Please check the option for PSB", "OK");

            Close();

        }

        private void ProcessTraitText(string json)
        {
            var templateSet = new SortedSet<string>();
            m_traits = JsonHelper.getJsonArray<TraitItem>(json);
            m_templateList = new();

            foreach(var trait in m_traits)
            {
                templateSet.Add(trait.filename);
            }
            foreach(var template in templateSet)
            {
                m_templateList.Add(new TemplateMapEntry() {templateName = template });
            }

            m_psbListView.itemsSource = m_templateList;
            m_psbListView.Rebuild();
        }
    }

    internal class ListDelegate
    {
        private VisualElement m_visualElement;
        private string m_searchPath;
        private TextField m_textField;
        private Action<string> m_updater;

        public ListDelegate(VisualElement ve)
        {
            m_visualElement = ve;
            m_visualElement.Q<Button>().clicked += BrowsePSBFile;
            m_textField = m_visualElement.Q<TextField>();
        }

        public void SetData(TemplateMapEntry entry, string path, System.Action<string> updateFunc)
        {
            var textField = m_visualElement.Q<TextField>();
            m_updater = updateFunc;
            textField.label = entry.templateName;
            textField.value = entry.fileName;

            var filename = path + "/" + entry.templateName;

            if (File.Exists(filename))
            {
                textField.value =filename;
                m_updater(filename);
            }

            m_searchPath = path;
       
        }

        private void BrowsePSBFile()
        {
            string path = EditorUtility.OpenFilePanel("Select PSB",m_searchPath, "psb");
            m_textField.value = path;
            m_updater(path);
        
        }
    }
}