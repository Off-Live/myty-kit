using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.AvatarImporter.MASUtil
{
    public class AsyncJsonParser
    {
        class StackItem
        {
            public JsonToken tokenType;
            public JToken token;
            public string propertyName;
        }
        JObject m_parsedObject;

        public JObject parsedObject => m_parsedObject;

        public IEnumerator Parse(string text, float timeout)
        {
            var jsonReader = new JsonTextReader(new StringReader(text));
            var stack = new Stack<StackItem>();
            var resumeTs = Time.realtimeSinceStartup;
            
            while (jsonReader.Read())
            {
                var curTs = Time.realtimeSinceStartup;
                if (curTs - resumeTs > timeout)
                {
                    yield return null;
                    resumeTs = Time.realtimeSinceStartup;
                }
                switch (jsonReader.TokenType)
                {
                    case JsonToken.StartObject: 
                        JToken item = new JObject();
                        stack.Push(new ()
                        {
                            tokenType = jsonReader.TokenType,
                            token = item,
                        });
                        break;
                    case JsonToken.EndObject:
                    case JsonToken.EndArray:
                        var stackItem = stack.Pop();
                        if (stack.Count == 0)
                        {
                            m_parsedObject = stackItem.token as JObject;
                        }
                        else
                        {
                            var upperItem = stack.Peek();
                            if (upperItem.tokenType == JsonToken.PropertyName)
                            {
                                stack.Pop();
                                stack.Peek().token[upperItem.propertyName] = stackItem.token;
                            }
                            else if (upperItem.tokenType == JsonToken.StartArray)
                            {
                                var array = upperItem.token as JArray;
                                array.Add(stackItem.token);
                            }
                            else
                            {
                                Debug.LogError("Cannot be here");
                                Debug.Assert(false);
                            }
                        }

                        break;
                    
                    case JsonToken.StartArray:
                        item = new JArray();
                        stack.Push(new ()
                        {
                            tokenType = jsonReader.TokenType,
                            token = item,
                        });
                        break;
                    
                    case JsonToken.PropertyName:
                        stack.Push(new ()
                        {
                            tokenType = jsonReader.TokenType,
                            propertyName = (string) jsonReader.Value,
                        });
                        break;
                    case JsonToken.Boolean:
                    case JsonToken.Float:
                    case JsonToken.Integer:
                    case JsonToken.String:
                    case JsonToken.Bytes:
                    case JsonToken.Date:
                    
                        stackItem = stack.Peek();
                        if (stackItem.tokenType == JsonToken.PropertyName)
                        {
                            stack.Pop();
                            var objItem = stack.Peek();
                            Debug.Assert(objItem.tokenType==JsonToken.StartObject);
                            SetJsonValue(objItem.token as JObject,  stackItem.propertyName, jsonReader.Value, jsonReader.TokenType);
                        }else if (stackItem.tokenType == JsonToken.StartArray)
                        {
                            var array = stackItem.token as JArray;
                            array.Add(jsonReader.Value);
                        }
                        else
                        {
                            Debug.LogError($"Cannot be here {stackItem.tokenType} {jsonReader.TokenType}");
                            Debug.Assert(false);
                        }
                        break;
                }
            }
            
        }
        
        void SetJsonValue(JObject jObject, string property, object value, JsonToken type)
        {
            switch (type)
            {
                case JsonToken.Boolean:
                    jObject[property] = (bool)value;
                    break;
                case JsonToken.Float:
                    jObject[property] = (float)(double)value;
                    break;
                case JsonToken.Integer:
                    jObject[property] = (int)(long)value;
                    break;
                case JsonToken.String:
                    jObject[property] = (string)value;
                    break;
            }
        }
    }
}