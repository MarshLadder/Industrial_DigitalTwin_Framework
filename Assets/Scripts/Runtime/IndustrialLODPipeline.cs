using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using GLTFast;

public class IndustrialLODPipeline : MonoBehaviour
{
    public bool IsReady { get; private set; }
    public event Action LodsReady;

    private string GetConverterPath()
    {
        // Dynamically tracks the Step2Gltf executable bundled inside your project deployment
        return Path.Combine(Application.streamingAssetsPath, "Binaries", "Windows", "Step2Gltf.exe");
    }

    public async void ProcessIndustrialAsset(string incomingCADPath)
    {
        IsReady = false;
        string exePath = GetConverterPath();
        string outputFolder = Path.Combine(Application.persistentDataPath, "GeneratedGLTF");
        Directory.CreateDirectory(outputFolder);
        string assetName = Path.GetFileNameWithoutExtension(incomingCADPath);

        // Keep each CAD asset's generated LODs in persistent storage for reuse.
        string highPolyPath = Path.Combine(outputFolder, $"{assetName}_LOD0.glb");
        string medPolyPath  = Path.Combine(outputFolder, $"{assetName}_LOD1.glb");
        string lowPolyPath  = Path.Combine(outputFolder, $"{assetName}_LOD2.glb");

        UnityEngine.Debug.Log("⚡ Kicking off multi-threaded industrial triangulation pipeline...");

        // Run the 3 heavy C++ processes completely in parallel on background system threads!
        // This prevents the Unity XR runtime framework from freezing while computing geometry.
        Task<bool> taskLOD0 = Task.Run(() => RunTessellationWorker(exePath, incomingCADPath, highPolyPath, 0.02f)); // Smooth close-up
        Task<bool> taskLOD1 = Task.Run(() => RunTessellationWorker(exePath, incomingCADPath, medPolyPath,  0.20f)); // Mid distance
        Task<bool> taskLOD2 = Task.Run(() => RunTessellationWorker(exePath, incomingCADPath, lowPolyPath,  0.80f)); // Far away blocky variant

        // Wait smoothly until all three generation tasks output their files
        await Task.WhenAll(taskLOD0, taskLOD1, taskLOD2);

        if (taskLOD0.Result && taskLOD1.Result && taskLOD2.Result)
        {
            UnityEngine.Debug.Log("🎉 All 3 LOD variants generated successfully. Assembling Digital Twin...");
            await ConstructLODGroupObject(highPolyPath, medPolyPath, lowPolyPath);
            IsReady = true;
            LodsReady?.Invoke();
        }
        else
        {
            UnityEngine.Debug.LogError("🚨 Pipeline failure: One or more tessellation steps crashed.");
        }
    }

    private bool RunTessellationWorker(string exePath, string inputPath, string outputPath, float linearDeflection)
    {
        if (File.Exists(outputPath))
        {
            UnityEngine.Debug.Log($"Using existing glTF: {outputPath}");
            return true;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = exePath;
        // Pass your modern command line arguments straight into the C++ main function parameters
        startInfo.Arguments = $"--linear={linearDeflection} \"{inputPath}\" \"{outputPath}\"";
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;

        using (Process process = Process.Start(startInfo))
        {
            process.WaitForExit();
            return process.ExitCode == 0;
        }
    }

    private async Task ConstructLODGroupObject(string path0, string path1, string path2)
    {
        // 1. Establish the main structural anchor GameObject for your factory component
        GameObject assetRoot = new GameObject("DigitalTwin_Asset_Root");
        assetRoot.transform.SetParent(this.transform);
        LODGroup lodGroup = assetRoot.AddComponent<LODGroup>();

        LOD[] lodSettings = new LOD[3];

        // 2. Asynchronously stream the 3 distinct file models into memory using glTFast libraries
        GameObject goLOD0 = await LoadGltfAssetAsync(path0, "LOD0_High", assetRoot.transform);
        GameObject goLOD1 = await LoadGltfAssetAsync(path1, "LOD1_Med",  assetRoot.transform);
        GameObject goLOD2 = await LoadGltfAssetAsync(path2, "LOD2_Low",  assetRoot.transform);

        // 3. Package the render references and assign their camera visibility screen-height ratios
        lodSettings[0] = new LOD(0.90f, goLOD0.GetComponentsInChildren<Renderer>()); // Visible when object takes > 90% of screen height
        lodSettings[1] = new LOD(0.85f, goLOD1.GetComponentsInChildren<Renderer>()); // Visible when object takes 85% - 90% of screen height
        lodSettings[2] = new LOD(0.80f, goLOD2.GetComponentsInChildren<Renderer>()); // Visible when object takes 80% - 85% of screen height

        // 4. Force the native engine lifecycle to control the visibility layers automatically
        lodGroup.SetLODs(lodSettings);
        lodGroup.RecalculateBounds();
        
        UnityEngine.Debug.Log("🚀 Digital Twin is live and completely optimized for high-performance XR!");
    }

    private async Task<GameObject> LoadGltfAssetAsync(string filePath, string name, Transform parent)
    {
        GameObject wrapper = new GameObject(name);
        wrapper.transform.SetParent(parent);

        var gltfBoundsAsset = wrapper.AddComponent<GltfBoundsAsset>();
        
        // Stream the geometric buffers from local disk space asynchronously
        bool success = await gltfBoundsAsset.Load(new System.Uri(filePath).AbsoluteUri);
        
        if (!success) UnityEngine.Debug.LogError($"Failed to load asset stream: {name}");
        return wrapper;
    }
}
