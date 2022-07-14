using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Timers;
using System;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

[Serializable]
public class LayerEffectInfo
{
    public string layerPath;
    public string blendMode;
}

public class ExecuteCmdTool
{
    private string layerToolPath = "Assets/MYTYKit/CmdTools/LayerTool/.extracted/macOS/export_layer_effect_x86/export_layer_effect_x86";

    private class LayerToolArg
    {
        public string path;
        public GameObject go;
        public string output;
    }

    private static LayerEffectList effects;

    public ExecuteCmdTool()
    {

    }

    public void ExecuteLayerTool(string psbPath, GameObject rootNode)
    {

#if UNITY_EDITOR
        var worker = new BackgroundWorker();
        if (effects == null) effects = AssetDatabase.LoadAssetAtPath<LayerEffectList>("Assets/MYTYKit/LayerEffect/LayerEffectList.asset");


        Debug.Log("processor " + SystemInfo.processorType);
        Debug.Log("os : " + SystemInfo.operatingSystem);

        
        if (SystemInfo.operatingSystem.StartsWith("Mac"))
        {
            if (SystemInfo.processorType.StartsWith("Apple"))
            {
                layerToolPath = "Assets/MYTYKit/CmdTools/LayerTool/.extracted/macOS/export_layer_effect/export_layer_effect";
            }

        }
        else
        {
            layerToolPath = "Assets/MYTYKit/CmdTools/LayerTool/.extracted/Windows/export_layer_effect_win/export_layer_effect_win.exe";
        }

        if (!File.Exists(layerToolPath))
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

            Debug.Log("args : " + psbPath);
            Debug.Log("selected tool path : " + layerToolPath);

            using Process process = new Process();
            process.StartInfo.FileName = layerToolPath;
            process.StartInfo.Arguments = psbPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
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
#endif

    }

    public static void ApplyLayerEffect(GameObject node, LayerEffectInfo[] effect, string history)
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
                foreach (var layerEffect in effects.layerEffects)
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

    private void Extract()
    {
#if UNITY_EDITOR
        EditorUtility.DisplayProgressBar("Import", "Extracting tool", 1.0f);
        Debug.Log("extract start");

        using (Process process = new Process())
        {
            if (SystemInfo.operatingSystem.StartsWith("Mac"))
            {
                process.StartInfo.FileName = "ditto";
                process.StartInfo.Arguments = "-x -k Assets/MYTYKit/CmdTools/LayerTool/macOS.zip Assets/MYTYKit/CmdTools/LayerTool/.extracted/";
            }else
            {
                process.StartInfo.FileName = "Assets/MYTYKit/CmdTools/LayerTool/7za.exe";
                process.StartInfo.Arguments = "x Assets/MYTYKit/CmdTools/LayerTool/Windows.zip -oAssets/MYTYKit/CmdTools/LayerTool/.extracted/";
            }
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            process.WaitForExit();
        }

        EditorUtility.ClearProgressBar();
#endif
    }

    private void EnsureFileMode()
    {
        if (!SystemInfo.operatingSystem.StartsWith("Mac")) return;
        
        using (Process process = new Process())
        {
            process.StartInfo.FileName = "chmod";
            process.StartInfo.Arguments = "755 "+layerToolPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            process.WaitForExit();
        }
    }
}
