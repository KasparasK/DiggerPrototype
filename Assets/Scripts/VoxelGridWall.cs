﻿using System;
using UnityEngine;
using System.Collections.Generic;

public class VoxelGridWall : MonoBehaviour
{

    private Mesh mesh;

    private List<Vector3> vertices, normals;
    private List<int> triangles;

    private int[] xEdgesMin, xEdgesMax;
    private int yEdgeMin, yEdgeMax;
    private float bottom;
   public float top;// bottom - sienos storis
    private int matID;
    public void Initialize(int resolution, int matID,Action callback)
    {
        bottom = VoxelMap.zSize;
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        this.matID = matID;
        mesh.name = "VoxelGridWall Mesh";
        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        triangles = new List<int>();
        xEdgesMin = new int[resolution];
        xEdgesMax = new int[resolution];
        callback();
    }

    Vector2[] GenerateUVs()
    {
        Vector2[] uvs = new Vector2[vertices.Count];
        Vector2 color = new Vector2(((16f * (float)matID) - 8f) / 80f, 0.5f);

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = color;
        }


        return uvs;
    }

    public void Clear()
    {
        vertices.Clear();
        normals.Clear();
        triangles.Clear();
        mesh.Clear();
        

    }

    public void Apply()
    {
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = GenerateUVs();

        mesh.RecalculateNormals();

    }

    public void CacheXEdge(int i, Voxel voxel)
    {
        xEdgesMax[i] = vertices.Count;
        Vector3 v = voxel.XEdgePoint;
        v.z = bottom;
        vertices.Add(v);
        v.z = top;
        vertices.Add(v);

        Vector3 n = voxel.xNormal;
        normals.Add(n);
        normals.Add(n);
    }

    public void CacheYEdge(Voxel voxel)
    {
        yEdgeMax = vertices.Count;
        Vector3 v = voxel.YEdgePoint;
        v.z = bottom;
        vertices.Add(v);
        v.z = top;
        vertices.Add(v);

        Vector3 n = voxel.yNormal;
        normals.Add(n);
        normals.Add(n);
    }

    public void PrepareCacheForNextCell()
    {
        yEdgeMin = yEdgeMax;
    }

    public void PrepareCacheForNextRow()
    {
        int[] swap = xEdgesMin;
        xEdgesMin = xEdgesMax;
        xEdgesMax = swap;
    }
    public void AddACAB(int i)
    {
        AddSection(yEdgeMin, xEdgesMin[i]);
    }

    public void AddACAB(int i, Vector2 extraVertex)
    {
        AddSection(yEdgeMin, xEdgesMin[i], extraVertex);
    }

    public void AddABAC(int i)
    {
        AddSection(xEdgesMin[i], yEdgeMin);
    }

    public void AddABAC(int i, Vector2 extraVertex)
    {
        AddSection(xEdgesMin[i], yEdgeMin, extraVertex);
    }

    public void AddABBD(int i)
    {
        AddSection(xEdgesMin[i], yEdgeMax);
    }

    public void AddABBD(int i, Vector2 extraVertex)
    {
        AddSection(xEdgesMin[i], yEdgeMax, extraVertex);
    }

    public void AddABCD(int i)
    {
        AddSection(xEdgesMin[i], xEdgesMax[i]);
    }

    public void AddABCD(int i, Vector2 extraVertex)
    {
        AddSection(xEdgesMin[i], xEdgesMax[i], extraVertex);
    }

    public void AddACBD(int i)
    {
        AddSection(yEdgeMin, yEdgeMax); // i virsu ziurinti siena - kampai
    }

    public void AddACBD(int i, Vector2 extraVertex)
    {
        AddSection(yEdgeMin, yEdgeMax, extraVertex);
    }

    public void AddACCD(int i)
    {
        AddSection(yEdgeMin, xEdgesMax[i]); //i virsu ziurinti siena kairys kampas?
    }

    public void AddACCD(int i, Vector2 extraVertex)
    {
       AddSection(yEdgeMin, xEdgesMax[i], extraVertex);
    }
    //----
    public void AddBDAB(int i)
    {
        AddSection(yEdgeMax, xEdgesMin[i]);
    }

    public void AddBDAB(int i, Vector2 extraVertex)
    {
        AddSection(yEdgeMax, xEdgesMin[i], extraVertex);
    }

    public void AddBDAC(int i)
    {
        AddSection(yEdgeMax, yEdgeMin);
    }

    public void AddBDAC(int i, Vector2 extraVertex)
    {
        AddSection(yEdgeMax, yEdgeMin, extraVertex);
    }

    public void AddBDCD(int i)
    {
        AddSection(yEdgeMax, xEdgesMax[i]);
    }

    public void AddBDCD(int i, Vector2 extraVertex)
    {
        AddSection(yEdgeMax, xEdgesMax[i], extraVertex);
    }

    public void AddCDAB(int i)
    {
        AddSection(xEdgesMax[i], xEdgesMin[i]);
    }

    public void AddCDAB(int i, Vector2 extraVertex)
    {
        AddSection(xEdgesMax[i], xEdgesMin[i], extraVertex);
    }

    public void AddCDAC(int i)
    {
        AddSection(xEdgesMax[i], yEdgeMin);
    }

    public void AddCDAC(int i, Vector2 extraVertex)
    {
        AddSection(xEdgesMax[i], yEdgeMin, extraVertex);
    }

    public void AddCDBD(int i)
    {
       AddSection(xEdgesMax[i], yEdgeMax);
    }

    public void AddCDBD(int i, Vector2 extraVertex)
    {
      AddSection(xEdgesMax[i], yEdgeMax, extraVertex);
    }
    private void AddSection(int a, int b)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(b + 1);
        triangles.Add(a);
        triangles.Add(b + 1);
        triangles.Add(a + 1);
    }

    private void AddSection(int a, int b, Vector3 extraPoint)
    {
        int p = vertices.Count;
        extraPoint.z = bottom;
        vertices.Add(extraPoint);
        extraPoint.z = top;
        vertices.Add(extraPoint);
        Vector3 n = normals[a];
        normals.Add(n);
        normals.Add(n);
        AddSection(a, p);
        p = vertices.Count;
        extraPoint.z = bottom;
        vertices.Add(extraPoint);
        extraPoint.z = top;
        vertices.Add(extraPoint);
        n = normals[b];
        normals.Add(n);
        normals.Add(n);
        AddSection(p, b);
    }
}
