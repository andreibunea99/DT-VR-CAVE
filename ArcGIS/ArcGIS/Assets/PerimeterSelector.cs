using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerimeterSelector : MonoBehaviour
{
    private List<Vector2> worldPoints = new List<Vector2>();
    private List<Vector2> screenPoints = new List<Vector2>();
    public GameObject markerPrefab;  // prefab for marking the points
    public Material lineMaterial;  // material for lines from the parameter
    private LineRenderer lineRenderer;
    public Material fillMaterial;
    public Vector3 videoScreenOffset = new Vector3(20000, 500, 0);


    // Start is called before the first frame update
    void Start()
    {
        // Initialize the line renderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Selected");
            Vector2 screenPos = Input.mousePosition;

            // Convert mouse coordinates to normalized coordinates (0 to 1)
            Vector2 normalizedPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);

            Debug.Log("Normalized pos: " + normalizedPos + " scale: " + gameObject.transform.localScale + "Position: " + gameObject.transform.localPosition + " Result: " + normalizedPos * gameObject.transform.localScale);

            // Scale normalized coordinates to the video screen size
            Vector2 scaledPos = new Vector2(normalizedPos.x * gameObject.transform.localScale.x, normalizedPos.y * gameObject.transform.localScale.y);

            // Convert to world position and apply the video screen offset
            Vector3 worldPos = new Vector3(scaledPos.x + videoScreenOffset.x, scaledPos.y + videoScreenOffset.y, videoScreenOffset.z + 10f);  // Adjust z as necessary
           
            // Add the point to the list
            screenPoints.Add(screenPos);
            worldPoints.Add(worldPos);
            GameObject marker = Instantiate(markerPrefab, worldPos, Quaternion.identity);
            marker.transform.localScale = new Vector3(500f, 500f, 100f); // Adjust the values as needed


            Debug.Log(worldPoints.Count);

            // Draw the line
            if (worldPoints.Count > 1)
            {
                lineRenderer.positionCount = worldPoints.Count;
                lineRenderer.SetPositions(worldPoints.ConvertAll(p => (Vector3)p).ToArray());

            }

            // Check if parameter is complete
            if (worldPoints.Count == 4)
            {
                CompleteSelection();
            }
        }
    }

    void CompleteSelection()
    {
        // Close the parameter shape
        lineRenderer.positionCount = 5;
        lineRenderer.SetPosition(4, lineRenderer.GetPosition(0));

        // When completed, fill the parameter with a colour with opacity 0.5
        FillPerimeter();

        StartPoseEstimation();

    }


    void FillPerimeter()
    {
        // Create game object to render the filled polygon
        GameObject fillObject = new GameObject("PerimeterFill");
        fillObject.transform.position = Vector3.zero;

        // Add mesh renderer and mesh filter to display the filled area
        MeshFilter meshFilter = fillObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = fillObject.AddComponent<MeshRenderer>();

        // Create the mesh
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[worldPoints.Count];
        for (int i = 0; i < worldPoints.Count; i++)
        {
            vertices[i] = worldPoints[i];
        }

        mesh.vertices = vertices;
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
        meshFilter.mesh = mesh;
        meshRenderer.material = fillMaterial;
    }


    void StartPoseEstimation()

    {
        Debug.Log("The parameter has been saved!");
        // Log the points for debugging
        Debug.Log("World Points:");
        foreach (var point in worldPoints)
        {
            Debug.Log(point);
        }

        Debug.Log("Screen Points:");
        foreach (var point in screenPoints)
        {
            Debug.Log(point);
        }

        // TODO Sort the screen points so is upper left, upper right, lower right, lower left. Lets call this sceneSquare
        // TODO Create the fundamental matrix that maps this coordinates to a square that starts from (0, 1, 0), (1, 1, 0), (1, 0, 0), (0, 0, 0). Lets call this worldSquare
        // TODO This fundamental matrix should help me determine the position inside the real world square of screen points from the video screen
    }
}
