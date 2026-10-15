// using System.Diagnostics;
// using System.IO;
// using System.Threading.Tasks;
// using UnityEngine;

// public class CADIngestionEngine : MonoBehaviour
// {
//     private string GetConverterPath()
//     {
//         // Dynamically locate your bundled background worker path inside the app installer
//         return Path.Combine(Application.streamingAssetsPath, "Binaries", "Windows", "Step2Gltf.exe");
//     }

//     public async void ProcessAndLoadCAD(string userSelectedCADPath)
//     {
//         string exePath = GetConverterPath();
        
//         // Output path inside temporary app cache directory
//         string outputGltfPath = Path.Combine(Application.temporaryCachePath, "runtime_twin_output.gltf");

//         // Clean old cached files if they exist
//         if (File.Exists(outputGltfPath)) File.Delete(outputGltfPath);

//         UnityEngine.Debug.Log("Executing Open CASCADE Tessellation Pipeline...");

//         // 1. Run the heavy C++ triangulation calculation on a background system thread
//         // We pass a linear deflection tolerance value of 0.1 for balanced mesh density
//         bool conversionSuccess = await Task.Run(() => RunTessellationWorker(exePath, userSelectedCADPath, outputGltfPath, 0.1f));

//         // 2. If the background process finished successfully, stream the mesh into the scene
//         if (conversionSuccess && File.Exists(outputGltfPath))
//         {
//             UnityEngine.Debug.Log("Tessellation Complete! Streaming glTF architecture into Unity hierarchy...");
//             LoadMeshIntoScene(outputGltfPath);
//         }
//         else
//         {
//             UnityEngine.Debug.LogError("CAD Translation Pipeline Failure. Check file integrity.");
//         }
//     }

//     private bool RunTessellationWorker(string exePath, string inputPath, string outputPath, float linearDeflection)
//     {
//         ProcessStartInfo startInfo = new ProcessStartInfo
//         {
//             FileName = exePath,

//             // Pass the absolute file strings directly as arguments into your C++ main method
//             Arguments = $"--linear={linearDeflection} \"{inputPath}\" \"{outputPath}\"",
//             CreateNoWindow = true,
//             UseShellExecute = false
//         };

//         using (Process process = Process.Start(startInfo))
//         {
//             process.WaitForExit();
//             return process.ExitCode == 0;
//         }
//     }

//     private void LoadMeshIntoScene(string gltfFilePath)
//     {
//         // Instantiates the generated glTF assembly tree programmatically using com.unity.cloud.gltfast
//         var gltfComponent = gameObject.AddComponent<GLTFast.GltfAsset>();
//         gltfComponent.Url = gltfFilePath;
//     }
// }
