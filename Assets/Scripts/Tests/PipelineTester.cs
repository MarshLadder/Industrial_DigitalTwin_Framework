using UnityEngine;

public class PipelineTester : MonoBehaviour
{
    public IndustrialLODPipeline pipeline;
    // Paste the absolute path to your downloaded GrabCAD .step file here
    public string testStepFilePath = @"C:\Users\Tony\Downloads\nereus11-autonomous-xluuv-carrier-and-underwater-survey-system-1.snapshot.2\NEREUS11_XLUUV.step"; 

    void Start()
    {
        if (pipeline != null)
        {
            pipeline.ProcessIndustrialAsset(testStepFilePath);
        }
    }
}
