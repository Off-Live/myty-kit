using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEditor;
using System;

namespace MYTYKit.Components
{
    [Serializable]
    public class AvatarTemplate
    {
        public string PSBPath;
        public GameObject instance;
        public SpriteLibraryAsset spriteLibrary;
        public GameObject boneRootObj;
    }

    public class AvatarSelector : MonoBehaviour
    {
        public List<AvatarTemplate> templates;
        public MYTYAssetScriptableObject mytyAssetStorage;
        public int id;

        private GameObject m_activeInstance;
        private SpriteLibraryAsset m_activeSLA;
        private GameObject m_activeBoneRoot;

        private ShaderMapAsset m_shaderMap;

        private void Start()
        {
            var shaderMapGO = FindObjectOfType<ShaderMap>();
            if (shaderMapGO != null)
            {
                m_shaderMap = shaderMapGO.shaderMap;
            }

            Configure();
        }

        public void ResetAvatar()
        {
            foreach (var template in templates)
            {
                var childCount = template.instance.transform.childCount;
                template.instance.SetActive(true);
                for (int i = 0; i < childCount; i++)
                {
                    if(template.instance.transform.GetChild(i).gameObject!=template.boneRootObj) FixName(template.instance.transform.GetChild(i).gameObject);
                    EnableLayer(template.instance.transform.GetChild(i).gameObject);
                }

            }
        }

        public bool CheckRootBoneValidity()
        {
            if (templates == null || templates.Count==0) return false;
            foreach (var template in templates)
            {
                if (template.boneRootObj == null) return false;
                if (!template.boneRootObj.name.ToLower().StartsWith("bone")) return false;
            }
            return true;
        }

        public void Configure()
        {
            m_activeInstance = null;


            var index = -1;
            for (int i = 0; i < mytyAssetStorage.traits.Count; i++)
            {
                if (id == mytyAssetStorage.traits[i].id)
                {
                    index = i;
                }
            }

            if (index < 0)
            {
                Debug.LogWarning("No trait item with id " + id);
                return;
            }

            var traitItem = mytyAssetStorage.traits[index];


            foreach (var template in templates)
            {

                if (template.PSBPath == traitItem.filename)
                {
                    m_activeInstance = template.instance;
                    m_activeSLA = template.spriteLibrary;
                    m_activeBoneRoot = template.boneRootObj;
                }
                else
                {
                    template.instance.SetActive(false);

                }
            }

            if (m_activeInstance == null)
            {
                Debug.Log("Proper psb template is not found. " + traitItem.filename);
                return;
            }

            int childCount = m_activeInstance.transform.childCount;
            var library = m_activeInstance.GetComponent<SpriteLibrary>();
            if (library == null) library = m_activeInstance.AddComponent<SpriteLibrary>();
            library.spriteLibraryAsset = m_activeSLA;
            if (m_activeSLA == null)
            {
                Debug.LogWarning("Sprite Library is not created yet!");
                return;
            }

            for (int i = 0; i < childCount; i++)
            {
                if (m_activeInstance.transform.GetChild(i).gameObject == m_activeBoneRoot) continue;
                FixName(m_activeInstance.transform.GetChild(i).gameObject);
            }

            DisableAllBones();
            for (int i = 0; i < childCount; i++)
            {
                FixLayer(m_activeInstance.transform.GetChild(i).gameObject, new List<string>(),
                    traitItem.traits.ToArray());
            }

        }

        public void FindWithTraitObj(GameObject trait)
        {
#if UNITY_EDITOR
            if (!Application.isEditor) return;
            if (trait == null) return;
            var curr = trait.transform;
            var path = trait.name;
            var templateIdx = -1;

            while (curr != null)
            {
                bool flag = false;
                for (int i = 0; i < templates.Count; i++)
                {
                    if (templates[i].instance == curr.gameObject)
                    {
                        templateIdx = i;
                        flag = true;
                    }
                }

                if (flag) break;
                curr = curr.parent;

                if (curr != null) path = curr.name + "/" + path;

            }

            if (curr == null) return;


            path = path.Substring(path.IndexOf('/') + 1);
            Debug.Log("Trait path : " + path);

            var found = false;
            for (int i = id + 1, count = 0; count < 10000; i++, count++)
            {
                i %= 10000;
                var traitItem = mytyAssetStorage.traits[i];
                var psbPath = templates[templateIdx].PSBPath;
                if (traitItem.filename != psbPath) continue;

                foreach (var traitPath in traitItem.traits)
                {
                    if (path == traitPath || path.StartsWith(traitPath + "/"))
                    {
                        id = i;
                        found = true;
                    }
                }

                if (found) break;
            }


            if (found)
            {
                var so = new SerializedObject(this);
                so.FindProperty("id").intValue = id;
                so.ApplyModifiedProperties();
                Configure();
            }

#endif
        }


        private void FixName(GameObject templateNode)
        {
            int childCount = templateNode.transform.childCount;

            string name = templateNode.name;
            int sufIdx = name.LastIndexOf("_");

            if (sufIdx >= 0)
            {
                string surfix = name.Substring(sufIdx + 1);
                if (int.TryParse(surfix, out _))
                {
                    name = name.Substring(0, sufIdx);
                }
            }

            templateNode.name = name;

            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    FixName(templateNode.transform.GetChild(i).gameObject);
                }
            }
        }

        private string HistoryToKey(List<string> history)
        {
            string ret = "";
            foreach (var elem in history)
            {
                ret += "/" + elem;
            }

            return ret;
        }

        private void EnableLayer(GameObject node)
        {
            node.SetActive(true);
            for (int i = 0; i < node.transform.childCount; i++)
            {
                EnableLayer(node.transform.GetChild(i).gameObject);
            }
        }

        private void FixLayer(GameObject templateNode, List<string> history, string[] traits)
        {
            int childCount = templateNode.transform.childCount;
            string name = templateNode.name;
            history.Add(name);

            string key = HistoryToKey(history);

            bool active = false;

            for (int i = 0; i < traits.Length; i++)
            {
                string trait = "/" + traits[i];
                if (key == trait || key.StartsWith(trait + "/"))
                {
                    active = true;
                }
            }

            if (m_activeBoneRoot != null && key.StartsWith("/" + m_activeBoneRoot.name)) return;

            if (active)
            {
                //Debug.Log(key);
                var node = templateNode.transform;
                while (node != null)
                {
                    node.gameObject.SetActive(active);

                    node = node.transform.parent;
                }
            }
            else
            {
                templateNode.SetActive(active);
            }

            for (int i = 0; i < childCount; i++)
            {
                FixLayer(templateNode.transform.GetChild(i).gameObject, history, traits);
            }

            if (childCount == 0)
            {
                string catName = "Animation" + key;
                var labels = m_activeSLA.GetCategoryLabelNames(catName);
                var labelList = new List<string>();

                foreach (var label in labels)
                {
                    labelList.Add(label);
                }

                if (labelList.Count > 0)
                {
                    var resolver = templateNode.GetComponent<SpriteResolver>();
                    if (resolver == null) resolver = templateNode.AddComponent<SpriteResolver>();
                    resolver.SetCategoryAndLabel(catName, labelList[labelList.Count - 1]);
                    var mytySR = templateNode.GetComponent<MYTYSpriteResolver>();
                    if (mytySR == null) mytySR = templateNode.AddComponent<MYTYSpriteResolver>();
                    mytySR.spriteLibraryAsset = m_activeSLA;
#if UNITY_EDITOR
                    if (Application.isEditor)
                    {
                        var so = new SerializedObject(mytySR);
                        so.FindProperty("m_spriteLibraryAsset").objectReferenceValue = m_activeSLA;
                        so.ApplyModifiedProperties();
                    }
#endif

                    mytySR.SetCategoryAndLabel(catName, labelList[labelList.Count - 1]);
                }

                if (active)
                {
                    var spriteSkin = templateNode.GetComponent<SpriteSkin>();
                    if (spriteSkin != null)
                    {
                        foreach (var bone in spriteSkin.boneTransforms)
                        {
                            EnableBone(bone);
                        }
                    }
                }

#if !UNITY_EDITOR
            if (Application.isPlaying)
            {
                var render = templateNode.GetComponent<SpriteRenderer>();
                if (render != null && m_shaderMap != null)
                {

                    foreach (var item in m_shaderMap.shaderMapList)
                    {
                        if (render.material != null)
                        {
                            var matName = render.material.name.Split(" ")[0];
                            if (item.name == matName)
                            {
                                render.material = item.material;
                            }
                        }

                    }
                }
            }
#endif
            }



            history.RemoveAt(history.Count - 1);
        }


        void DisableAllBones()
        {
            if (m_activeBoneRoot == null) return;
            Stack<GameObject> stack = new();
            stack.Push(m_activeBoneRoot);

            while (stack.Count > 0)
            {
                var curBone = stack.Pop();
                curBone.SetActive(false);

                for (int i = 0; i < curBone.transform.childCount; i++)
                {
                    stack.Push(curBone.transform.GetChild(i).gameObject);
                }
            }

        }

        void EnableBone(Transform bone)
        {
            Transform current = bone;
            while (current != null)
            {
                current.gameObject.SetActive(true);
                current = current.parent;
            }
        }

    }
}
