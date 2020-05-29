using UnityEngine;

public class VoxelLayer : MonoBehaviour
{

    float chunkSize = 2f;

    int voxelResolution = 8;

    int chunkResolutionX = 2;
    int chunkResolutionY = 2;

    private int callbacksCount;

    public VoxelGrid voxelGridPrefab;

    private VoxelGrid[] chunks;

    private float voxelSize;
    private float halfSize;

    private int activeVoxelsCount;

    private int totalVoxelsCount;

    public float maxFeatureAngle = 135f;

    public int ID;
    public void Init(LayerData data,int ID)
    {
        this.ID = ID; 
        chunkSize = data.chunkSize;
        voxelResolution = data.voxelResolution;
        chunkResolutionX = data.chunkResolutionX;
        chunkResolutionY = data.chunkResolutionY;

        callbacksCount = 0;
        halfSize = chunkSize * 0.5f;

        voxelSize = chunkSize / voxelResolution;
        int chunksCount = chunkResolutionX * chunkResolutionY;
        chunks = new VoxelGrid[chunksCount];

        activeVoxelsCount = totalVoxelsCount = chunksCount * voxelResolution * voxelResolution;
        

        for (int i = 0, y = 0; y < chunkResolutionY; y++)
        {
            for (int x = 0; x < chunkResolutionX; x++, i++)
            {
                CreateChunk(i, x, y, data.zOffset,data.matID,data.isLast);

            }
        }

        if (data.isLast)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(chunkResolutionX * chunkSize, chunkResolutionY * chunkSize);
            box.center = new Vector3((chunkResolutionX * chunkSize) / 2 - 1, (chunkResolutionY * chunkSize) / 2 - 1, data.zOffset);
        }
        InitCallback();

    }

    public float GetFillPercent()
    {
        float fill = (float)activeVoxelsCount / (float)totalVoxelsCount * 100f;
        return fill;
    }

    public void ClearAll()
    {
        foreach (var chunk in chunks)
        {
            chunk.ClearAll();
        }

        foreach (var chunk in chunks)
        {
            chunk.Refresh();
        }
    }

    public void EditVoxels(Vector2 center, VoxelStencil stencilToUse)
    {
       
        int xStart = (int)((stencilToUse.XStart - voxelSize) / chunkSize);
        if (xStart < 0)
        {
            xStart = 0;
        }
        int xEnd = (int)((stencilToUse.XEnd + voxelSize) / chunkSize);
        if (xEnd >= chunkResolutionX)
        {
            xEnd = chunkResolutionX - 1;
        }
        int yStart = (int)((stencilToUse.YStart - voxelSize) / chunkSize);
        if (yStart < 0)
        {
            yStart = 0;
        }
        int yEnd = (int)((stencilToUse.YEnd + voxelSize) / chunkSize);
        if (yEnd >= chunkResolutionY)
        {
            yEnd = chunkResolutionY - 1;
        }

        for (int y = yEnd; y >= yStart; y--)
        {
            int i = y * chunkResolutionX  + xEnd;
            for (int x = xEnd; x >= xStart; x--, i--)
            {
                stencilToUse.SetCenter(center.x - x * chunkSize, center.y - y * chunkSize);
                chunks[i].Apply(stencilToUse, ref activeVoxelsCount);
            }
        }
    }

    private void CreateChunk(int i, int x, int y,float z, int matID, bool isLast)
    {
        VoxelGrid chunk = Instantiate(voxelGridPrefab) as VoxelGrid;
        chunk.Initialize(voxelResolution, chunkSize, maxFeatureAngle, matID, isLast, InitCallback);
        chunk.transform.parent = transform;
        chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize, z);
        chunks[i] = chunk;
        if (x > 0)
        {
            chunks[i - 1].xNeighbor = chunk;
        }
        if (y > 0)
        {
            chunks[i - chunkResolutionX].yNeighbor = chunk;
            if (x > 0)
            {
                chunks[i - chunkResolutionX - 1].xyNeighbor = chunk;
            }
        }

    }

    void InitCallback()
    {
        callbacksCount++;
        if(callbacksCount == chunkResolutionX*chunkResolutionY+1)
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].Refresh(
                   // CutStart
                    );
            }

    }

    void CutStart()
    {
     /*   Vector2 center = new Vector2(chunkResolutionX*chunkSize/2,chunkResolutionY*chunkSize);

         VoxelStencil activeStencil = stencils[1];
         activeStencil.Initialize(false, (4 + 0.5f) * voxelSize);
         activeStencil.SetCenter(center.x, center.y);

        EditVoxels(center, activeStencil);
        */
    }


}