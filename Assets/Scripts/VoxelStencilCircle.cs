﻿using UnityEngine;

public class VoxelStencilCircle : VoxelStencil
{

    private float sqrRadius;

    public override void Initialize(bool fillType, float radius)
    {
        base.Initialize(fillType, radius);
        sqrRadius = radius * radius;
    }

    public override void Apply(Voxel voxel, ref int activeVoxelsCount)
    {
        float x = voxel.position.x - centerX;
        float y = voxel.position.y - centerY;
        if (x * x + y * y <= sqrRadius)
        {
            if (voxel.state != fillType)
            {
                activeVoxelsCount += fillType == true ? 1 : -1;
                voxel.state = fillType;
            }
        }
    }

    protected override void FindHorizontalCrossing(Voxel xMin, Voxel xMax)
    {
        float y2 = xMin.position.y - centerY;
        y2 *= y2;
        if (xMin.state == fillType)
        {
            float x = xMin.position.x - centerX;
            if (x * x + y2 <= sqrRadius)
            {
                x = centerX + Mathf.Sqrt(sqrRadius - y2);
                if (xMin.xEdge == float.MinValue || xMin.xEdge < x)
                {
                    xMin.xEdge = x;
                    xMin.xNormal = ComputeNormal(x, xMin.position.y, xMax);
                }
                else
                {
                    ValidateHorizontalNormal(xMin, xMax);
                }
            }
        }
        else if (xMax.state == fillType)
        {
            float x = xMax.position.x - centerX;
            if (x * x + y2 <= sqrRadius)
            {
                x = centerX - Mathf.Sqrt(sqrRadius - y2);
                if (xMin.xEdge == float.MinValue || xMin.xEdge > x)
                {
                    xMin.xEdge = x;
                    xMin.xNormal = ComputeNormal(x, xMin.position.y, xMin);
                }
                else
                {
                    ValidateHorizontalNormal(xMin, xMax);
                }
            }
        }
    }

    protected override void FindVerticalCrossing(Voxel yMin, Voxel yMax)
    {
        float x2 = yMin.position.x - centerX;
        x2 *= x2;
        if (yMin.state == fillType)
        {
            float y = yMin.position.y - centerY;
            if (y * y + x2 <= sqrRadius)
            {
                y = centerY + Mathf.Sqrt(sqrRadius - x2);
                if (yMin.yEdge == float.MinValue || yMin.yEdge < y)
                {
                    yMin.yEdge = y;
                    yMin.yNormal = ComputeNormal(yMin.position.x, y, yMax);
                }
                else
                {
                    ValidateVerticalNormal(yMin, yMax);
                }
            }
        }
        else if (yMax.state == fillType)
        {
            float y = yMax.position.y - centerY;
            if (y * y + x2 <= sqrRadius)
            {
                y = centerY - Mathf.Sqrt(sqrRadius - x2);
                if (yMin.yEdge == float.MinValue || yMin.yEdge > y)
                {
                    yMin.yEdge = y;
                    yMin.yNormal = ComputeNormal(yMin.position.x, y, yMin);
                }
                else
                {
                    ValidateVerticalNormal(yMin, yMax);
                }
            }
        }
    }

    private Vector3 ComputeNormal(float x, float y, Voxel other)
    {
        if (fillType == true && other.state == false)
        {
            return new Vector2(x - centerX, y - centerY).normalized;
        }
        else
        {
            return new Vector2(centerX - x, centerY - y).normalized;
        }
    }
}