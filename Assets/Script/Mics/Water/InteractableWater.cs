using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(EdgeCollider2D))]
[RequireComponent(typeof(WaterTriggerHandler))]

public class InteractableWater : MonoBehaviour
{
    [Header("Spring")]
    [SerializeField] private float springConstant = 1.4f;
    [SerializeField] private float damping = 1.1f;
    [SerializeField] private float spread = 6.5f;
    [SerializeField, Range(1,10)] private int wavePropogationInteration = 8;
    [SerializeField, Range(0f,20f)] private float speedMult = 5.5f;

    [Header("Force")]
    public float forceMultiplier = 0.2f;
    [Range(1f, 50f)] public float maxForce = 5f;

    [Header("Collision")]
    [SerializeField, Range(1, 10)] private float playerCollisionRadiusMult = 4.15f;


    [Header("MeshGen")]
    [Range(2,500)] public int numOfXVerticies = 70;
    public float width = 10f,
                 height = 4f;
    public Material waterMaterial;

    private const int numberOfYVertices = 2;

    [Header("Gizmo")]
    public Color gizmoColor = Color.cyan;

    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Vector3[] vertices;
    private int[] topVerticesIndex;

    private EdgeCollider2D coll;
    private class WaterPoint
    {
        public float velocity, pos, targetHeight;
    }

    private List<WaterPoint> waterPoints = new List<WaterPoint>();

    private void Reset()
    {
        coll = GetComponent<EdgeCollider2D>();
        coll.isTrigger = true;
    }

    private void Start()
    {
        coll = GetComponent<EdgeCollider2D>();

        GenerateMesh();

        CreateWaterPoints();
    }

    private void FixedUpdate()
    {
        //update all spring pos
        for (int i = 1; i < waterPoints.Count - 1; i++) 
        {
            WaterPoint point = waterPoints[i];

            float x = point.pos - point.targetHeight;
            float acceleration = springConstant * x - damping * point.velocity;
            point.pos += point.velocity * speedMult * Time.fixedDeltaTime;
            vertices[topVerticesIndex[i]].y = point.pos;
            point.velocity += acceleration * speedMult * Time.fixedDeltaTime;
        }

        //wave propogation
        for(int j = 0;j < wavePropogationInteration - 1; j++)
            for(int i = 1; i < waterPoints.Count -1; i++)
            {
                float leftDelta =   spread * 
                                    (waterPoints[i].pos - waterPoints[i-1].pos) * 
                                    speedMult * Time.fixedDeltaTime;
                waterPoints[i-1].velocity += leftDelta;

                float rightDelta =  spread *
                                    (waterPoints[i].pos - waterPoints[i + 1].pos) * 
                                    speedMult * Time.fixedDeltaTime;
                waterPoints[i - 1].velocity += rightDelta;
            }
        //update mesh
        mesh.vertices = vertices;
    }

    public void Splash(Collider2D collison, float force)
    {
        float radius = collison.bounds.extents.x * playerCollisionRadiusMult;
        Vector2 center = collison.transform.position;

        for(int i = 0; i < waterPoints.Count; i++)
        {
            Vector2 vertexWorldPos = transform.TransformPoint(vertices[topVerticesIndex[i]]);

            if(IsPointInsideCircle(vertexWorldPos, center, radius))
            {
                waterPoints[i].velocity = force;
            }
        }
    }

    private bool IsPointInsideCircle(Vector2 point,Vector2 center,float radius)
    {
        float distanceSquared = (point - center).sqrMagnitude;
        return distanceSquared <= radius * radius;
    }

    [ContextMenu("ResetEdgeCollider2D")]
    public void ResetEdgeCollider2D()
    {
        coll = GetComponent<EdgeCollider2D>();

        Vector2[] newPoints = new Vector2[2];

        Vector2 firstPoints = new Vector2(vertices[topVerticesIndex[0]].x, vertices[topVerticesIndex[0]].y);
        newPoints[0] = firstPoints;

        Vector2 secondPoints = new Vector2(vertices[topVerticesIndex[topVerticesIndex.Length -1]].x,
                                            vertices[topVerticesIndex[topVerticesIndex.Length - 1]].y);
        newPoints[1] = secondPoints;


        coll.offset = Vector2.zero;
        coll.points = newPoints;
    }
    [ContextMenu("GenerateMesh")]
    public void GenerateMesh()
    {
        mesh = new Mesh();

        //add vertices
        vertices = new Vector3[numOfXVerticies * numberOfYVertices];
        topVerticesIndex = new int[numOfXVerticies];
        for(int y = 0; y < numberOfYVertices; y++)
        {
            for(int x = 0; x < numOfXVerticies; x++)
            {
                float xPos = (x / (float)(numOfXVerticies -1 )) * width - width/2 ;
                float yPos = (y / (float)(numberOfYVertices - 1)) * height - height / 2;
                vertices[y * numOfXVerticies + x] = new Vector3(xPos, yPos,0f);

                if (y == numberOfYVertices - 1)
                {
                    topVerticesIndex[x] = y * numOfXVerticies + x;


                }
            }
        }

        //triagles
        int[] triagles = new int[(numOfXVerticies - 1) * (numberOfYVertices - 1) * 6];
        int index = 0;

        for (int y = 0; y < numberOfYVertices - 1; y++)
        {
            for (int x = 0; x < numOfXVerticies - 1; x++)
            {
                int bottomLeft = y * numOfXVerticies + x,
                    bottomRight = bottomLeft + 1,
                    topLeft = bottomLeft + numOfXVerticies,
                    topRight = topLeft + 1;

                //1st triagle
                triagles[index++] = bottomLeft;
                triagles[index++] = topLeft;
                triagles[index++] = bottomRight;
                //2nd triagle
                triagles[index++] = bottomRight;
                triagles[index++] = topLeft;
                triagles[index++] = topRight;
            }
        }

        //UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        for(int i  = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2((vertices[i].x + width/2) / width,(vertices[i].y + height / 2) / height);
        }

        if(meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        meshRenderer.material = waterMaterial;

        mesh.vertices = vertices;
        mesh.triangles = triagles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private void CreateWaterPoints()
    {
        waterPoints.Clear();

        for(int i = 0; i < topVerticesIndex.Length; i++)
        {
            waterPoints.Add(new WaterPoint
            {
                pos = vertices[topVerticesIndex[i]].y,
                    targetHeight = vertices[topVerticesIndex[i]].y,
            });
        }
    }
}

//[CustomEditor(typeof(InteracableWaterEditor))]
//public class InteracableWaterEditor : Editor
//{

//}
