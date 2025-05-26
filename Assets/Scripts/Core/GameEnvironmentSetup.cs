using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MetaBalance.Core
{
    /// <summary>
    /// Sets up the 2.5D game environment with URP post-processing
    /// </summary>
    public class GameEnvironmentSetup : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float cameraHeight = 10f;
        [SerializeField] private float cameraAngle = 45f;
        [SerializeField] private Vector3 tableCenterPosition = Vector3.zero;
        
        [Header("Environment")]
        [SerializeField] private GameObject tableModel;
        [SerializeField] private Light mainLight;
        [SerializeField] private Light fillLight;
        [SerializeField] private Material tableMaterial;
        
        [Header("Table Dimensions")]
        [SerializeField] private float tableWidth = 10f;
        [SerializeField] private float tableLength = 15f;
        [SerializeField] private float tableHeight = 0.5f;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject tableGlowEffect;
        [SerializeField] private ParticleSystem ambientParticles;
        
        [Header("Post Processing (URP)")]
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private VolumeProfile postProcessProfile;
        [SerializeField] private bool enableBloom = true;
        [SerializeField] private bool enableColorGrading = true;
        [SerializeField] private bool enableVignette = true;
        [SerializeField] private bool enableDepthOfField = false;
        
        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
                
            SetupCamera();
            SetupTable();
            SetupLighting();
            SetupPostProcessing();
        }
        
        private void SetupCamera()
        {
            // Position camera at an angle to create the 2.5D view
            Vector3 cameraPosition = tableCenterPosition;
            cameraPosition.y = cameraHeight;
            cameraPosition.z = -cameraHeight;
            
            mainCamera.transform.position = cameraPosition;
            mainCamera.transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
            
            // Set camera to perspective for 3D elements but with a high field of view
            mainCamera.orthographic = false;
            mainCamera.fieldOfView = 50f;
            
            // Add post-processing if using URP
            #if UNITY_POST_PROCESSING_STACK_V2
            // Set up post-processing here
            #endif
        }
        
        private void SetupTable()
        {
            if (tableModel != null)
            {
                // Position the table model
                tableModel.transform.position = tableCenterPosition;
                
                // Scale the table based on the specified dimensions
                tableModel.transform.localScale = new Vector3(tableWidth, tableHeight, tableLength);
                
                // Apply material if provided
                if (tableMaterial != null)
                {
                    Renderer tableRenderer = tableModel.GetComponent<Renderer>();
                    if (tableRenderer != null)
                    {
                        tableRenderer.material = tableMaterial;
                    }
                }
                
                // Add ambient particles if provided
                if (ambientParticles != null)
                {
                    ambientParticles.transform.position = tableCenterPosition + new Vector3(0, 2, 0);
                    ambientParticles.Play();
                }
                
                // Add glow effect if provided
                if (tableGlowEffect != null)
                {
                    GameObject glow = Instantiate(tableGlowEffect, tableCenterPosition, Quaternion.identity);
                    glow.transform.SetParent(tableModel.transform);
                }
            }
            else
            {
                // Create a simple table if no model is provided
                GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
                table.name = "GameTable";
                table.transform.position = tableCenterPosition;
                table.transform.localScale = new Vector3(tableWidth, tableHeight, tableLength);
                
                // Apply material if provided
                if (tableMaterial != null)
                {
                    Renderer tableRenderer = table.GetComponent<Renderer>();
                    tableRenderer.material = tableMaterial;
                }
                
                // Parent to this object
                table.transform.SetParent(transform);
            }
        }
        
        private void SetupLighting()
        {
            // Main directional light
            if (mainLight != null)
            {
                mainLight.type = LightType.Directional;
                mainLight.intensity = 1.2f;
                mainLight.color = new Color(1f, 0.96f, 0.89f); // Warm white
                mainLight.transform.rotation = Quaternion.Euler(50f, 30f, 0f);
                
                // Add shadows
                mainLight.shadows = LightShadows.Soft;
                mainLight.shadowStrength = 0.7f;
            }
            else
            {
                // Create a default main light if none provided
                GameObject lightObj = new GameObject("MainLight");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.96f, 0.89f); // Warm white
                lightObj.transform.rotation = Quaternion.Euler(50f, 30f, 0f);
                light.shadows = LightShadows.Soft;
                
                // Parent to this object
                lightObj.transform.SetParent(transform);
            }
            
            // Fill light for softer shadows
            if (fillLight != null)
            {
                fillLight.type = LightType.Directional;
                fillLight.intensity = 0.5f;
                fillLight.color = new Color(0.8f, 0.85f, 1f); // Slightly blue
                fillLight.transform.rotation = Quaternion.Euler(30f, -60f, 0f);
                fillLight.shadows = LightShadows.None; // No shadows for fill light
            }
            else
            {
                // Create a default fill light if none provided
                GameObject lightObj = new GameObject("FillLight");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 0.5f;
                light.color = new Color(0.8f, 0.85f, 1f); // Slightly blue
                lightObj.transform.rotation = Quaternion.Euler(30f, -60f, 0f);
                light.shadows = LightShadows.None;
                
                // Parent to this object
                lightObj.transform.SetParent(transform);
            }
            
            // Add ambient lighting
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.0f;
            RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.2f);
        }
        
        private void SetupPostProcessing()
        {
            // Ensure the camera has URP additional camera data
            var cameraData = mainCamera.GetUniversalAdditionalCameraData();
            if (cameraData == null)
            {
                Debug.LogWarning("Camera does not have Universal Additional Camera Data component. Post-processing may not work properly.");
                return;
            }
            
            // Enable post-processing on the camera
            cameraData.renderPostProcessing = true;
            
            // Create post-processing volume if not provided
            if (postProcessVolume == null)
            {
                GameObject volumeObj = new GameObject("Post Process Volume");
                volumeObj.transform.SetParent(transform);
                postProcessVolume = volumeObj.AddComponent<Volume>();
                postProcessVolume.isGlobal = true;
                postProcessVolume.priority = 1;
            }
            
            // Create volume profile if not provided
            if (postProcessProfile == null)
            {
                postProcessProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                postProcessVolume.profile = postProcessProfile;
                
                // Add and configure post-processing effects
                SetupBloom();
                SetupColorGrading();
                SetupVignette();
                SetupDepthOfField();
            }
            else
            {
                // Use provided profile
                postProcessVolume.profile = postProcessProfile;
            }
        }
        
        private void SetupBloom()
        {
            if (!enableBloom) return;
            
            if (!postProcessProfile.TryGet<Bloom>(out var bloom))
            {
                bloom = postProcessProfile.Add<Bloom>(false);
            }
            
            // Configure bloom for the game's aesthetic
            bloom.intensity.value = 0.3f;
            bloom.threshold.value = 1.0f;
            bloom.scatter.value = 0.7f;
            bloom.clamp.value = 65472f;
            bloom.tint.value = new Color(1f, 1f, 1f, 1f);
            bloom.highQualityFiltering.value = false;
            bloom.skipIterations.value = 1;
            bloom.dirtTexture.value = null;
            bloom.dirtIntensity.value = 0f;
            bloom.active = true;
        }
        
        private void SetupColorGrading()
        {
            if (!enableColorGrading) return;
            
            if (!postProcessProfile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                colorAdjustments = postProcessProfile.Add<ColorAdjustments>(false);
            }
            
            // Configure color grading for a sci-fi/tech aesthetic
            colorAdjustments.postExposure.value = 0.1f;
            colorAdjustments.contrast.value = 5f;
            colorAdjustments.colorFilter.value = new Color(0.9f, 0.95f, 1.0f, 1f); // Slightly cool tint
            colorAdjustments.hueShift.value = 0f;
            colorAdjustments.saturation.value = -5f; // Slightly desaturated
            colorAdjustments.active = true;
            
            // Add white balance for fine-tuning
            if (!postProcessProfile.TryGet<WhiteBalance>(out var whiteBalance))
            {
                whiteBalance = postProcessProfile.Add<WhiteBalance>(false);
            }
            
            whiteBalance.temperature.value = -5f; // Cooler temperature
            whiteBalance.tint.value = 0f;
            whiteBalance.active = true;
            
            // Add split toning for shadows/highlights
            if (!postProcessProfile.TryGet<SplitToning>(out var splitToning))
            {
                splitToning = postProcessProfile.Add<SplitToning>(false);
            }
            
            splitToning.shadows.value = new Color(0.2f, 0.3f, 0.5f, 1f); // Blue shadows
            splitToning.highlights.value = new Color(0.9f, 0.9f, 1.0f, 1f); // Neutral highlights
            splitToning.balance.value = 0f;
            splitToning.active = true;
        }
        
        private void SetupVignette()
        {
            if (!enableVignette) return;
            
            if (!postProcessProfile.TryGet<Vignette>(out var vignette))
            {
                vignette = postProcessProfile.Add<Vignette>(false);
            }
            
            // Subtle vignette to focus attention on the game table
            vignette.color.value = new Color(0.1f, 0.1f, 0.2f, 1f);
            vignette.center.value = new Vector2(0.5f, 0.45f); // Slightly higher center
            vignette.intensity.value = 0.25f;
            vignette.smoothness.value = 0.4f;
            vignette.roundness.value = 1f;
            vignette.rounded.value = false;
            vignette.active = true;
        }
        
        private void SetupDepthOfField()
        {
            if (!enableDepthOfField) return;
            
            if (!postProcessProfile.TryGet<DepthOfField>(out var depthOfField))
            {
                depthOfField = postProcessProfile.Add<DepthOfField>(false);
            }
            
            // Subtle depth of field to enhance the 2.5D effect
            depthOfField.mode.value = DepthOfFieldMode.Gaussian;
            depthOfField.gaussianStart.value = 8f;
            depthOfField.gaussianEnd.value = 15f;
            depthOfField.gaussianMaxRadius.value = 1f;
            depthOfField.highQualitySampling.value = false;
            depthOfField.active = true;
        }
        
        public Vector3 GetTableCenter()
        {
            return tableCenterPosition;
        }
        
        public Vector3 GetTableDimensions()
        {
            return new Vector3(tableWidth, tableHeight, tableLength);
        }
    }
}