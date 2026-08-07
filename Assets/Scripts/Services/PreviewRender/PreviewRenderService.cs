using MultiPlayerTemplate.Services.Layer;
using UnityEngine;
using VContainer;

namespace MultiPlayerTemplate.Services
{
    public class PreviewRenderService : MonoBehaviour, IPreviewRenderService
    {
        [Header("Setup")]
        [SerializeField] private Camera previewCamera;
        [SerializeField] private Light previewLight;
        [SerializeField] private Transform previewRoot;
        [SerializeField] private Vector3 servicePosition = new Vector3(5000, 5000, 5000);

        private ILayerService _layerService;
        private RenderTexture _renderTexture;

        [Inject]
        public void Construct(ILayerService layerService)
        {
            _layerService = layerService;
            if (_layerService == null)
            {
                Debug.LogError("[PreviewRenderService] ILayerService dependency is null!");
            }
        }

        private void Awake()
        {
            transform.position = servicePosition;
        }

        private void InitializeComponents()
        {
            if (previewCamera == null)
            {
                GameObject camGo = new GameObject("PreviewCamera");
                camGo.transform.SetParent(transform);
                camGo.transform.localPosition = new Vector3(0, 0, -5);
                previewCamera = camGo.AddComponent<Camera>();
                previewCamera.clearFlags = CameraClearFlags.SolidColor;
                previewCamera.backgroundColor = new Color(0, 0, 0, 0);
                previewCamera.nearClipPlane = 0.01f;
                previewCamera.farClipPlane = 100f;
                previewCamera.cullingMask = _layerService.GetLayerMask(LayerName.Preview);
                previewCamera.enabled = false;
            }

            if (previewLight == null)
            {
                GameObject lightGo = new GameObject("PreviewLight");
                lightGo.transform.SetParent(transform);
                lightGo.transform.localRotation = Quaternion.Euler(50, -30, 0);
                previewLight = lightGo.AddComponent<Light>();
                previewLight.type = LightType.Directional;
                previewLight.intensity = 1.0f;
            }

            if (previewRoot == null)
            {
                GameObject rootGo = new GameObject("PreviewRoot");
                rootGo.transform.SetParent(transform);
                rootGo.transform.localPosition = Vector3.zero;
                previewRoot = rootGo.transform;
            }
        }

        // public void SetupPreview(RawImage targetImage, AnimatorComponent animatorComponent)
        // {
        //     if (previewCamera == null) InitializeComponents();
        //     if (targetImage == null)
        //     {
        //         Debug.LogError("[PreviewRenderService] targetImage is null in SetupPreview!");
        //         return;
        //     }
        //     if (animatorComponent == null)
        //     {
        //         Debug.LogError("[PreviewRenderService] animatorComponent is null in SetupPreview!");
        //         return;
        //     }
        //
        //     // Clear previous preview objects
        //     ClearPreview();
        //
        //     // Create or resize RenderTexture if needed
        //     Rect rect = targetImage.rectTransform.rect;
        //     int width = Mathf.RoundToInt(rect.width);
        //     int height = Mathf.RoundToInt(rect.height);
        //     
        //     // Ensure valid dimensions
        //     if (width <= 0) width = 256;
        //     if (height <= 0) height = 256;
        //
        //     if (_renderTexture == null || _renderTexture.width != width || _renderTexture.height != height)
        //     {
        //         if (_renderTexture != null)
        //         {
        //             _renderTexture.Release();
        //             Destroy(_renderTexture);
        //         }
        //         _renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        //         _renderTexture.Create();
        //     }
        //
        //     targetImage.texture = _renderTexture;
        //     previewCamera.targetTexture = _renderTexture;
        //     previewCamera.enabled = true;
        //
        //     AnimatorComponent animatorComponentInstance = Instantiate(animatorComponent, previewRoot);
        //     _layerService.SetLayer(animatorComponentInstance.gameObject, LayerName.Preview);
        //     
        //     Transform previewTarget = animatorComponentInstance.transform;
        //     animatorComponentInstance.SetGrounded(true);
        //
        //     Renderer[] renderers = previewTarget.GetComponentsInChildren<Renderer>();
        //     if (renderers == null || renderers.Length == 0) return;
        //
        //     Bounds combinedBounds = new Bounds();
        //     bool boundsInitialized = false;
        //     System.Collections.Generic.List<GameObject> newObjects = new System.Collections.Generic.List<GameObject>();
        //     newObjects.Add(animatorComponentInstance.gameObject);
        //
        //     foreach (Renderer renderer in renderers)
        //     {
        //         if (renderer == null || !renderer.enabled) continue;
        //         
        //         // Update bounds
        //         if (!boundsInitialized)
        //         {
        //             combinedBounds = renderer.bounds;
        //             boundsInitialized = true;
        //         }
        //         else
        //         {
        //             combinedBounds.Encapsulate(renderer.bounds);
        //         }
        //     }
        //
        //     if (boundsInitialized)
        //     {
        //         // Center the preview objects relative to previewRoot
        //         Vector3 worldCenter = combinedBounds.center;
        //         foreach (GameObject obj in newObjects)
        //         {
        //             // Calculate local offset from world center and apply it to preview root position
        //             obj.transform.position = previewRoot.position + (obj.transform.position - worldCenter);
        //         }
        //         
        //         FrameBounds(new Bounds(previewRoot.position, combinedBounds.size));
        //     }
        // }

        private void FrameBounds(Bounds bounds)
        {
            float distance = bounds.size.magnitude * 1.5f;
            if (distance < 1f) distance = 1f;
            
            previewCamera.transform.position = previewRoot.position + new Vector3(0, bounds.size.y * 0.2f, -distance);
            previewCamera.transform.LookAt(previewRoot.position);
        }

        public void ClearPreview()
        {
            if (previewCamera != null)
            {
                previewCamera.enabled = false;
            }

            if (previewRoot != null)
            {
                foreach (Transform child in previewRoot)
                {
                    if (child != null) Destroy(child.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (previewCamera != null)
            {
                previewCamera.targetTexture = null;
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }
    }
}
