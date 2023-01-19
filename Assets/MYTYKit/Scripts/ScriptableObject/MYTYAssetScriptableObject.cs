using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace MYTYKit
{
    [Serializable]
    public class TemplateMapEntry
    {
        public string templateName;
        public string fileName;
    }

    [Serializable]
    public class TraitItem
    {
        public string filename;
        public int id;
        public string tokenId;
        public List<string> traits;
    }

    [Serializable]
    public class AvatarTemplateInfo
    {
        public string template;
        public string instance;
        public string spriteLibrary;
    }

    public class MYTYAssetScriptableObject : ScriptableObject
    {
        public List<TraitItem> traits;
        public List<string> rootControllers;
        public List<string> motionAdapters;
        public List<AvatarTemplateInfo> templateInfos;

    }


    public class JsonHelper
    {
        public static T[] getJsonArray<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";

            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);

            return wrapper.array;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
