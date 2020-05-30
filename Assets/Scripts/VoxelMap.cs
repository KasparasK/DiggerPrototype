using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ClickState
{
    NONE,
    DOWN,
    UP,
    HOLD
}
public class VoxelMap : MonoBehaviour
{

    #region GUI

    private static string[] fillTypeNames = { "Filled", "Empty" };

    private static string[] radiusNames = { "0", "1", "2", "3", "4", "5" };

    private static string[] stencilNames = { "Square", "Circle" };

    public static int fillTypeIndex = 1, radiusIndex = 4, stencilIndex = 1;

    /* private void OnGUI()
     {
         GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
         GUILayout.Label("Fill Type");
         fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames, 2);
         GUILayout.Label("Radius");
         radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 6);
         GUILayout.Label("Stencil");
         stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames, 2);
         GUILayout.EndArea();
     }*/
    #endregion

    private List<VoxelLayer> layers;
    public CameraControler cameraControler;
    public VoxelLayer voxelLayerPrefab;
    public Material material;
    public Transform[] stencilVisualizations;
    private float halfSize;
    private float voxelSize;

    private VoxelStencil[] stencils = {
        new VoxelStencil(),
        new VoxelStencilCircle()
    };

    private int currentActiveID;
    private Vector3 scale;
    private Vector3 gizmoCenter;
    private VoxelLayer activeLayer;
    private ClickState clickState;

    private const int layersCount = 3;
    void Awake()
    {
        layers = new List<VoxelLayer>();
        System.Random randomSequence = new System.Random();

        LayerData data = new LayerData(randomSequence);

        ResetActiveLayers();
        clickState = ClickState.NONE;
        // cameraControler.Initialze(new Vector2(data.chunkResolutionX * data.chunkSize / 2, data.chunkResolutionY));
        halfSize = data.chunkSize * 0.5f;
        voxelSize = data.chunkSize / data.voxelResolution;

        float zSize = 0.5f;
     
        for (int i = 0; i < layersCount; i++)
        {
            data = new LayerData(randomSequence) {zOffset = zSize * i, isLast = i == layersCount - 1};
            
            layers.Add(Instantiate(voxelLayerPrefab) as VoxelLayer);

            layers[i].Init(data, i);
        }

    }

    private void Update()
    {


        if (Application.isEditor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ResetActiveLayers();
                clickState = ClickState.DOWN;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ResetActiveLayers();
                clickState = ClickState.UP;
            }
            else if (Input.GetMouseButton(0))
                clickState = ClickState.HOLD;
            else
                clickState = ClickState.NONE;
        }
        else
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    ResetActiveLayers();
                    clickState = ClickState.DOWN;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    ResetActiveLayers();
                    clickState = ClickState.UP;
                }
                else
                    clickState = ClickState.HOLD;

            }
            else
                clickState = ClickState.NONE;
        }

        foreach (var layer in layers)
        {
            if (layer.GetFillPercent() <= 40)
                layer.ClearAll();
        }
    }
    private void FixedUpdate()
    {
        if (Application.isEditor)
        {
            PCcontrolsCylinder();
        }
        else
        {
            MobileControls();
        }
    }
    void MobileControls()
    {
        if (clickState != ClickState.NONE || clickState != ClickState.UP)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            scale = Vector3.one * ((radiusIndex + 0.5f) * voxelSize * 2f);
            RaycastHit singleRayHitInfo;

            if (Physics.Raycast(ray, out singleRayHitInfo))
            {
                Vector3 singleRayCenter = transform.InverseTransformPoint(singleRayHitInfo.point);
                gizmoCenter = singleRayCenter;
                Vector3 center = singleRayCenter;
                //   Debug.DrawRay(ray.origin, ray.direction * 20, Color.red);

                center.x += halfSize;
                center.y += halfSize;

                if (clickState == ClickState.HOLD)
                {
                    CylinderCast(touch.position);

                    VoxelStencil activeStencil = stencils[stencilIndex];
                    activeStencil.Initialize(fillTypeIndex == 0, (radiusIndex + 0.5f) * voxelSize);
                    activeStencil.SetCenter(center.x, center.y);

                    activeLayer.EditVoxels(center, activeStencil);

                }
            }
        }
    }
    void PCcontrolsCylinder()
    {
        Transform visualization = stencilVisualizations[stencilIndex];
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        scale = Vector3.one * ((radiusIndex + 0.5f) * voxelSize * 2f);
        RaycastHit singleRayHitInfo;

        if (Physics.Raycast(ray, out singleRayHitInfo))
        {
            Vector3 singleRayCenter = transform.InverseTransformPoint(singleRayHitInfo.point);
            gizmoCenter = singleRayCenter;
            Vector3 center = singleRayCenter;
            Debug.DrawRay(ray.origin, ray.direction * 20, Color.red);
            #region used for stencil visualization calculations
            center.x += halfSize;
            center.y += halfSize;
            #endregion

            if (clickState == ClickState.HOLD)
            {
                CylinderCast(Input.mousePosition);

                VoxelStencil activeStencil = stencils[stencilIndex];
                activeStencil.Initialize(fillTypeIndex == 0, (radiusIndex + 0.5f) * voxelSize);
                activeStencil.SetCenter(center.x, center.y);

                activeLayer.EditVoxels(center, activeStencil);
            }

            #region used for stencil visualization calculations

            center.x -= halfSize;
            center.y -= halfSize;
            center.z -= 0.15f;
            visualization.localPosition = center;
            visualization.localScale = new Vector3(scale.x, 0.05f, scale.z);

            #endregion

            visualization.gameObject.SetActive(true);
        }
        else
        {
            visualization.gameObject.SetActive(false);
            //Debug.Log(hitInfo.collider.gameObject.name);
        }
    }
    void CylinderCast(Vector3 clickPos)
    {
        Vector3 mouse = clickPos;
        mouse.z = 10;
        mouse = Camera.main.ScreenToWorldPoint(mouse);
        mouse.z *= -1;
        Vector3 dir = transform.forward * 20;
        Ray ray = new Ray(mouse, dir);
        RaycastHit hit;

        Debug.DrawRay(mouse, dir, Color.blue);
        if (Physics.Raycast(ray, out hit))
        {
            AddToActiveLayers(ExtractVoxelLayer(hit));
        }
        for (int i = 0; i < 16; i++)
        {
            double x = mouse.x + scale.x / 2 * Mathf.Cos(i * 22.5f * Mathf.PI / 180);
            double y = mouse.y + scale.x / 2 * Mathf.Sin(i * 45 * Mathf.PI / 180);

            Vector3 start = new Vector3((float)x, (float)y, mouse.z);

            ray = new Ray(start, dir);
            if (Physics.Raycast(ray, out hit))
            {
                AddToActiveLayers(ExtractVoxelLayer(hit));
            }

            Debug.DrawRay(start, dir, Color.blue);
        }
    }
    void AddToActiveLayers(VoxelLayer layerToTest)
    {
        if (layerToTest != null)
        {
            if (layerToTest.ID < currentActiveID)
            {
                activeLayer = layerToTest;
                // if (currentActiveID == layersCount)
                currentActiveID = layerToTest.ID;
            }
        }
    }
    VoxelLayer ExtractVoxelLayer(RaycastHit hit)
    {
        GameObject obj = hit.collider.gameObject;
        if (obj.tag == "layer")
        {
            VoxelGridSurface surf = obj.GetComponent<VoxelGridSurface>();

            VoxelLayer foundLayer = surf != null
                ? obj.transform.parent.parent.GetComponent<VoxelLayer>()
                : obj.GetComponent<VoxelLayer>();
            if (foundLayer != null)
            {
                return foundLayer;

            }
        }
        return null;
    }

    void ResetActiveLayers()
    {
        // activeLayers.Clear();
        currentActiveID = layersCount;
    }
    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gizmoCenter, scale.x / 2);
    }

}


public class LayerData
{
    public LayerData(System.Random randomSequence)
    {
        matID = randomSequence.Next(1, diffColor + 1);
      
        
    }

    private const int diffColor = 5;
    public bool isLast;
    public float zOffset;
    public int matID;
    public float chunkSize = 2f;

    public int voxelResolution = 15;

    public int chunkResolutionX = 4;
    public int chunkResolutionY = 6;
}