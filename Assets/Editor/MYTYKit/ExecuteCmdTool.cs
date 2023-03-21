using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Timers;
using System;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace MYTYKit
{
    [Serializable]
    public class LayerEffectInfo
    {
        public string layerPath;
        public string blendMode;
    }

    public class ExecuteCmdTool
    {
        class LayerToolArg
        {
            public string path;
            public GameObject go;
            public string output;
        }

        static LayerEffectList m_effects;
        static string m_layerToolPath = MYTYPath.LayerToolPathMacOSIntel;
        public void ExecuteLayerTool(string psbPath, GameObject rootNode)
        {
            
            var worker = new BackgroundWorker();

            if (m_effects == null)
            {
                m_effects = AssetDatabase.LoadAssetAtPath<LayerEffectList>(
                    MYTYPath.EffectListPath);
                if (m_effects == null)
                {
                    AssetDatabase.CopyAsset(MYTYPath.EffectListPackagePath, MYTYPath.EffectListPath);
                    m_effects = AssetDatabase.LoadAssetAtPath<LayerEffectList>(
                        MYTYPath.EffectListPath);
                }
            }


            Debug.Log("processor " + SystemInfo.processorType);
            Debug.Log("os : " + SystemInfo.operatingSystem);


            if (SystemInfo.operatingSystem.StartsWith("Mac"))
            {
                if (SystemInfo.processorType.StartsWith("Apple"))
                {
                    m_layerToolPath = MYTYPath.LayerToolPathMacOSAppleSilicon;
                }

            }
            else
            {
                m_layerToolPath = MYTYPath.LayerToolPathWindows;
            }

            if (!File.Exists(m_layerToolPath))
            {
                Extract();
            }

            EnsureFileMode();

            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (_, progress) =>
            {
                EditorUtility.DisplayProgressBar("Import", "layer effect", progress.ProgressPercentage / 100.0f);
            };

            worker.DoWork += (e, args) =>
            {
                var w = e as BackgroundWorker;
                var timer = new Timer(250);
                int progressVal = 0;
                timer.AutoReset = true;
                timer.Enabled = true;
                timer.Elapsed += (_, _) =>
                {
                    w.ReportProgress(progressVal);
                    progressVal += 2;

                };

                var toolArg = args.Argument as LayerToolArg;
                var psbPath = toolArg.path;
                Debug.Log("selected tool path : " + m_layerToolPath);

                using Process process = new Process();
                process.StartInfo.FileName = m_layerToolPath;
                process.StartInfo.Arguments = psbPath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                process.WaitForExit();
                w.ReportProgress(100);
                timer.Enabled = false;
                toolArg.output = output;
                args.Result = toolArg;

            };

            worker.RunWorkerAsync(new LayerToolArg
            {
                path = psbPath,
                go = rootNode
            });
            worker.RunWorkerCompleted += (e, args) =>
            {
                var toolArg = args.Result as LayerToolArg;

                Debug.Log("result : " + toolArg.output);
                var layerInfos = JsonHelper.getJsonArray<LayerEffectInfo>(toolArg.output);
                if (toolArg.go != null)
                {
                    for (int i = 0; i < toolArg.go.transform.childCount; i++)
                    {
                        ApplyLayerEffect(toolArg.go.transform.GetChild(i).gameObject, layerInfos, "");

                    }
                }

                EditorUtility.ClearProgressBar();
            };

        }

        static void ApplyLayerEffect(GameObject node, LayerEffectInfo[] effect, string history)
        {
            var curr_history = history + "/" + node.name;

            foreach (var elem in effect)
            {

                if (curr_history == "/" + elem.layerPath)
                {
                    var renderer = node.GetComponent<SpriteRenderer>();

                    if (elem.blendMode != "BlendMode.NORMAL")
                    {
                        Debug.Log(curr_history + " " + elem.blendMode);
                    }

                    foreach (var layerEffect in m_effects.layerEffects)
                    {
                        if (elem.blendMode == layerEffect.name)
                        {
                            renderer.material = layerEffect.material;
                        }
                    }
                }
            }

            for (int i = 0; i < node.transform.childCount; i++)
            {
                ApplyLayerEffect(node.transform.GetChild(i).gameObject, effect, curr_history);
            }
        }

        void Extract()
        {
            EditorUtility.DisplayProgressBar("Import", "Extracting tool", 1.0f);
            Debug.Log("extract start");

            using (Process process = new Process())
            {
                if (SystemInfo.operatingSystem.StartsWith("Mac"))
                {
                    process.StartInfo.FileName = "sh";
                    process.StartInfo.Arguments =
                        "Assets/MYTYKit/CmdTools/LayerTool/mac.sh";
                }else
                {
                    var path = "Assets\\MYTYKit\\CmdTools\\LayerTool\\";
                    var sourceStr = "";
                    for(char surfix = 'a'; surfix < 'g'; surfix++)
                    {
                        sourceStr += path + "Windows.zip.a" + surfix +"+";
                    }

                    sourceStr += path + "Windows.zip.ag";

                    var targetStr = path + "Windows.zip";

                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments =
                        "/c copy /b " + sourceStr + " " + targetStr;
                    
                    process.StartInfo.CreateNoWindow = true;
                }
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                
                process.Start(); 
                process.WaitForExit();
                
            }
            
            using (Process process = new Process())
            {
                if (SystemInfo.operatingSystem.StartsWith("Mac"))
                {
                    process.StartInfo.FileName = "ditto";
                    process.StartInfo.Arguments =
                        "-x -k Assets/MYTYKit/CmdTools/LayerTool/macOS.zip Assets/MYTYKit/CmdTools/LayerTool/.extracted/";
                }
                else
                {
                    process.StartInfo.FileName = "Assets/MYTYKit/CmdTools/LayerTool/7za.exe";
                    process.StartInfo.Arguments =
                        "x Assets/MYTYKit/CmdTools/LayerTool/Windows.zip -oAssets/MYTYKit/CmdTools/LayerTool/.extracted/";
                    process.StartInfo.CreateNoWindow = true;
                }

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                process.WaitForExit();
            }

            EditorUtility.ClearProgressBar();
        }

        private void EnsureFileMode()
        {
            if (!SystemInfo.operatingSystem.StartsWith("Mac")) return;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "chmod";
                process.StartInfo.Arguments = "755 " + m_layerToolPath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                process.WaitForExit();
            }
        }
    }
}
