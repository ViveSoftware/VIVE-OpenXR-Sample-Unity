using UnityEngine;

public static class MeshGenerationHelper
{
    public static Mesh GenerateQuadMesh(Vector3[] vertices)
    {
        Mesh quadMesh = new Mesh();
        quadMesh.vertices = vertices;

        //Create array that represents vertices of the triangles
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;

        quadMesh.triangles = triangles;
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++, i++)
            {
                uv[i] = new Vector2((float)x, (float)y);
                tangents[i] = tangent;
            }
        }
        quadMesh.uv = uv;
        quadMesh.tangents = tangents;
        quadMesh.RecalculateNormals();

        return quadMesh;
    }


}