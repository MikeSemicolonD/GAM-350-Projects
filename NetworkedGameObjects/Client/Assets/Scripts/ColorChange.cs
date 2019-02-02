using System;
using UnityEngine;

[Serializable]
public class ColorChange {

    private MeshRenderer targetMeshRenderer;
    private Material baseMat;

    public void PassMeshRenderer(MeshRenderer meshRend)
    {
        targetMeshRenderer = meshRend;
        baseMat = targetMeshRenderer.material;
    }

    public void AssignColor(float r, float g, float b)
    {
        targetMeshRenderer.material = new Material(Shader.Find("Unlit/Color"));
        targetMeshRenderer.material.color = new Color(r, g, b, 1);
    }
}
