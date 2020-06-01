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

    public Renderer sword;
    public Renderer sword1;
    public Renderer sword2;
    public Renderer sword3;

    public Camera cam;

    private List<VoxelLayer> layers;
    public CameraControler cameraControler;
    public VoxelLayer voxelLayerPrefab;

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
    public static float zSize = 0.5f;
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

        for (int i = 0; i < layersCount; i++)
        {
            data = new LayerData(randomSequence) {zOffset = zSize * i, isLast = i == layersCount - 1};
            
            layers.Add(Instantiate(voxelLayerPrefab) as VoxelLayer);

            layers[i].Init(data, i);
        }

        FrustumTest();
    }


    void FrustumTest()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        // Create a "Plane" GameObject aligned to each of the calculated planes
        for (int i = 0; i < 6; ++i)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            p.name = "Plane " + i.ToString();
            p.transform.position = -planes[i].normal * planes[i].distance;
            p.transform.rotation = Quaternion.FromToRotation(Vector3.up, planes[i].normal);
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
        /*
    Debug.Log(IsFullyVisible(sword, cam) == true?"visible":"0 not visible");
    Debug.Log(IsFullyVisible(sword1, cam) == true ? "visible" : "1 not visible");
    Debug.Log(IsFullyVisible(sword2, cam) == true ? "visible" : "2 not visible");
    Debug.Log(IsFullyVisible(sword3, cam) == true ? "visible" : "3 not visible");
    */

    }
    void MobileControls()
    {
        if (clickState != ClickState.NONE || clickState != ClickState.UP)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = cam.ScreenPointToRay(touch.position);
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
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        scale = Vector3.one*2;// * ((radiusIndex + 0.5f) * voxelSize * 2f);
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
                activeStencil.Initialize(fillTypeIndex == 0, 1);
                activeStencil.SetCenter(center.x, center.y);

                activeLayer.EditVoxels(center, activeStencil);
            }

            #region used for stencil visualization calculations

            center.x -= halfSize;
            center.y -= halfSize;
            center.z -= 0.30f;
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
        mouse = cam.ScreenToWorldPoint(mouse);
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
    public static bool IsFullyVisible(Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

        Bounds bounds = renderer.bounds;
        Vector3 size = bounds.size;
        Vector3 min = bounds.min;

        //Calculate the 8 points on the corners of the bounding box
        List<Vector3> boundsCorners = new List<Vector3>(8) {
            min,
            min + new Vector3(0, 0, size.z),
            min + new Vector3(size.x, 0, size.z),
            min + new Vector3(size.x, 0, 0),
        };
        for (int i = 0; i < 4; i++)
            boundsCorners.Add(boundsCorners[i] + size.y * Vector3.back);

        //Check each plane on every one of the 8 bounds' corners
        for (int p = 0; p < planes.Length; p++)
        {
            for (int i = 0; i < boundsCorners.Count; i++)
            {
                if (planes[p].GetSide(boundsCorners[i]) == false)
                    return false;
            }
        }
        return true;
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

    public int voxelResolution = 10;

    public int chunkResolutionX = 4;
    public int chunkResolutionY = 6;
}