using System.Collections;
using UnityEngine;
using UnityEngine.UIElements; // Required for modern UI Toolkit components

public class UIToolkitTelemetry : MonoBehaviour
{
    private UIDocument uiDocument;
    
    // UI Toolkit Element References matched to your specific UXML names
    private Label depthLabel;
    private Label pressureLabel;
    private Label temperatureLabel;
    private Label statusValueLabel;


    [Header("Material Warning Framework")]
    public GameObject industrialLODPipelineObject;

    private IndustrialLODPipeline lodPipeline;
    private Coroutine telemetryCoroutine;
    private Renderer[] submarineRenderers;
    private Material[] submarineMaterials;
    private Color[] originalColors;
    // Local Mock Data Variables calculated inside the streamer
    private float currentDepth = 0f;
    private float hullPressure = 1.0f;
    private float engineTemp = 45f;

    void OnEnable()
    {
        // 1. Locate the master UI Document component on the GameObject
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        // 2. Query the exact custom element names you defined inside your UXML file
        var root = uiDocument.rootVisualElement;
        depthLabel = root.Q<Label>("DepthLabel");
        pressureLabel = root.Q<Label>("PressureLabel");
        temperatureLabel = root.Q<Label>("TemperatureLabel");
        statusValueLabel = root.Q<Label>("StatusValue");

        if (industrialLODPipelineObject == null)
        {
            Debug.LogWarning("Industrial LOD Pipeline Object is not assigned.");
            return;
        }

        lodPipeline = industrialLODPipelineObject.GetComponent<IndustrialLODPipeline>();
        if (lodPipeline == null)
        {
            Debug.LogWarning("Industrial LOD Pipeline component was not found on the assigned object.");
            return;
        }

        lodPipeline.LodsReady += StartTelemetry;
        if (lodPipeline.IsReady)
        {
            StartTelemetry();
        }
    }

    private void OnDisable()
    {
        if (lodPipeline != null)
        {
            lodPipeline.LodsReady -= StartTelemetry;
        }

        if (telemetryCoroutine != null)
        {
            StopCoroutine(telemetryCoroutine);
            telemetryCoroutine = null;
        }
    }

    private void StartTelemetry()
    {
        if (telemetryCoroutine != null)
        {
            return;
        }

        CacheSubmarineMaterials();
        telemetryCoroutine = StartCoroutine(StreamTelemetryFeed());
    }

    private IEnumerator StreamTelemetryFeed()
    {
        while (true)
        {
            // The simulation calculations acting as your mock data pipeline source
            currentDepth += Random.Range(0.5f, 2.0f);
            hullPressure = 1.0f + (currentDepth * 0.09f);
            engineTemp = Mathf.Clamp(engineTemp + Random.Range(-1f, 1.5f), 40f, 95f);

            // Inject the runtime data strings into your custom UXML label parameters
            if (depthLabel != null) depthLabel.text = $"Depth: {currentDepth:F1} meters";
            if (pressureLabel != null) pressureLabel.text = $"Hull Pressure: {hullPressure:F2} ATM";
            if (temperatureLabel != null) temperatureLabel.text = $"Core Temp: {engineTemp:F1} °C";

            // Process warning states reactively based on thermal thresholds
            if (engineTemp >= 85f)
            {
                // CRITICAL OVERHEAT PHASE
                if (statusValueLabel != null)
                {
                    statusValueLabel.text = "CRITICAL (OVERHEAT)";
                        // Remove the previous state class
                    statusValueLabel.RemoveFromClassList("status-operational");
                    statusValueLabel.RemoveFromClassList("status-warning");
                    statusValueLabel.RemoveFromClassList("status-critical");

                    statusValueLabel.AddToClassList("status-critical");
                    
                }

                // Make the physical submarine mesh blink red alongside the panel alert
                float blink = Mathf.PingPong(Time.time * 2f, 1f);
                UpdateSubmarineMaterialAlert(critical: true);
            }
            else if (engineTemp >= 70f)
            {
                // WARNING PHASE
                if (statusValueLabel != null)
                {
                    statusValueLabel.text = "WARNING";
                    statusValueLabel.RemoveFromClassList("status-operational");
                    statusValueLabel.RemoveFromClassList("status-warning");
                    statusValueLabel.RemoveFromClassList("status-critical");
                    statusValueLabel.AddToClassList("status-warning");
                }
                UpdateSubmarineMaterialAlert(critical: false);
            }
            else
            {
                // STABLE OPERATIONAL PHASE
                if (statusValueLabel != null)
                {
                    statusValueLabel.text = "OPERATIONAL";
                    statusValueLabel.RemoveFromClassList("status-operational");
                    statusValueLabel.RemoveFromClassList("status-warning");
                    statusValueLabel.RemoveFromClassList("status-critical");
                    statusValueLabel.AddToClassList("status-operational");
                }
                UpdateSubmarineMaterialAlert(critical: false);
            }

            // Sync/poll sensor intervals every half-second matching industrial IoT setups
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void CacheSubmarineMaterials()
    {
        if (industrialLODPipelineObject == null)
        {
            Debug.LogWarning("Industrial LOD Pipeline Object is not assigned.");
            return;
        }

        // Include renderers on the parent and all child LOD meshes.
        submarineRenderers =
            industrialLODPipelineObject.GetComponentsInChildren<Renderer>(true);

        var materials = new System.Collections.Generic.List<Material>();

        foreach (Renderer renderer in submarineRenderers)
        {
            // .materials gives us the instantiated materials used by this renderer.
            foreach (Material material in renderer.materials)
            {
                if (material != null && !materials.Contains(material))
                {
                    materials.Add(material);
                }
            }
        }

        submarineMaterials = materials.ToArray();
        originalColors = new Color[submarineMaterials.Length];

        for (int i = 0; i < submarineMaterials.Length; i++)
        {
            originalColors[i] = submarineMaterials[i].color;
        }
    }

    private void UpdateSubmarineMaterialAlert(bool critical)
{
    if (submarineMaterials == null || originalColors == null)
        return;

    float blink = Mathf.PingPong(Time.time * 2f, 1f);

    for (int i = 0; i < submarineMaterials.Length; i++)
    {
        Material material = submarineMaterials[i];

        if (material == null)
            continue;

        if (critical)
        {
            material.color = Color.Lerp(
                originalColors[i],
                Color.red,
                blink
            );
        }
        else
        {
            material.color = originalColors[i];
        }
    }
}
}
