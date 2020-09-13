using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestMath : MonoBehaviour
{
    private VectorPair mouseInput;

    private Square exampleCell;

    private System.Collections.Generic.List<VectorPair> wallsAndNormals = new List<VectorPair>();

    [SerializeField]
    private TextMeshPro label = null;

    [SerializeField]
    private LineRenderer lineRenderer1 = null;
    [SerializeField]
    private LineRenderer lineRenderer2 = null;

    private LineRenderer[] linePaths = null;
    [SerializeField]
    private int numberInstance = 10;
    [SerializeField]
    private GameObject linePrefab = null;


    [SerializeField]
    private Transform cross = null;

    public void Start()
    {
        this.exampleCell.Center = Vector2.zero;
        this.exampleCell.Size = 3;
        this.exampleCell.Refresh();
        this.wallsAndNormals.Clear();
        this.wallsAndNormals.Add(this.exampleCell.North);
        this.wallsAndNormals.Add(this.exampleCell.East);
        this.wallsAndNormals.Add(this.exampleCell.South);
        this.wallsAndNormals.Add(this.exampleCell.West);

        this.linePaths = new LineRenderer[this.numberInstance];
        for (int index = 0; index < this.numberInstance; ++index)
        {
            GameObject go = GameObject.Instantiate(this.linePrefab);
            this.linePaths[index] = go.GetComponent<LineRenderer>();
            this.linePaths[index].enabled = false;
        }

        this.mouseInput.V1 = new Vector2(1.6273f, 4.0960f);
        this.mouseInput.V2 = new Vector2(-6.2694f, -14.2791f);
        this.ComputePath();
    }

    private void ComputePath()
    {
        if (this.mouseInput.Magnitude() <= float.Epsilon)
        {
            return;
        }

        float remainingPath = 20;
        Vector2 position = this.mouseInput.V1;
        Vector2 direction = (this.mouseInput.V2 - this.mouseInput.V1).normalized;
        Vector2 attemptedDestination = direction * remainingPath + position;

        this.linePaths[1].enabled = true;
        this.linePaths[1].positionCount = 2;
        this.linePaths[1].SetPosition(0, position);
        this.linePaths[1].SetPosition(1, position + direction);
        this.linePaths[1].startColor = Color.yellow;
        this.linePaths[1].endColor = Color.yellow;
        
        int index = 0;
        this.linePaths[0].SetPosition(index++, position);
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        int bounceIndex = 0;
        while (remainingPath > float.Epsilon)
        {
            stringBuilder.AppendLine($"Bounce {bounceIndex++}");
            Vector2 newPos = Vector2.zero;
            bool bounced = false;
            Vector2 newDirection = Vector2.zero;
            float minimumSquareMagnitude = float.MaxValue;
            for(int wallIndex = 0; wallIndex < this.wallsAndNormals.Count; ++wallIndex)
            {
                VectorPair wall = this.wallsAndNormals[wallIndex];

                Vector2 p;
                if (LinesIntersect(position, attemptedDestination, wall.V1, wall.V2, out p))
                {
                    float squareMagnitude = Vector2.SqrMagnitude(p - position);
                    if (squareMagnitude < minimumSquareMagnitude && squareMagnitude > float.Epsilon)
                    {
                        newDirection = direction - 2 * (Vector2.Dot(wall.Normale, direction) * wall.Normale);
                        bounced = true;
                        minimumSquareMagnitude = squareMagnitude;
                        newPos = p;
                        //stringBuilder.AppendLine($"{ wallIndex } Intersection with {wall.Normale}, pos {position} new pos {p}, direction { direction }, new direction {newDirection}, normale {wall.Normale}, squareMagnitude { squareMagnitude }, minSquareMagnitude { minimumSquareMagnitude }.");
                        stringBuilder.AppendLine($"({position.x.ToString("0.0000")}, {position.y.ToString("0.0000")}), ({attemptedDestination.x.ToString("0.0000")}, {attemptedDestination.y.ToString("0.0000")})");
                    }
                }
            }

            if (!bounced)
            {
                newPos = attemptedDestination;
            }
            else
            {
                direction = newDirection;
            }

            this.linePaths[0].positionCount = index + 1;
            this.linePaths[0].SetPosition(index++, newPos);

            remainingPath -= Mathf.Max(Vector2.Distance(position, newPos), 0f);
            position = newPos;
            attemptedDestination = direction * remainingPath + position;

            if (index > 100 || !bounced)
            {
                break;
            }
        }

        this.label.text = stringBuilder.ToString();
        Debug.Log(stringBuilder.ToString());
        this.linePaths[0].enabled = true;
    }

    private void Update()
    {
        bool changed = false;
        if (Input.GetMouseButtonDown(0))
        {
            this.mouseInput.V1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            changed = true;
        }
        else if (Input.GetMouseButton(0))
        {
            this.mouseInput.V2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            changed = true;
        }
        

        if (changed)
        {
            this.ComputePath();
        }

        this.lineRenderer1.positionCount = this.exampleCell.Corners.Length;
        this.lineRenderer1.SetPositions(this.exampleCell.Corners);
    }

    // Determines if the lines AB and CD intersect.
    static bool LinesIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 intersection)
    {
        Vector2 delta = c - a;
        Vector2 ab = b - a;
        Vector2 cd = d - c;

        float deltaCrossAB = delta.x * ab.y - delta.y * ab.x;
        float deltaCrossCD = delta.x * cd.y - delta.y * cd.x;
        float ABCrossCD = ab.x * cd.y - ab.y * cd.x;

        if (deltaCrossAB == 0f)
        {
            // Lines are collinear, and so intersect if they have any overlap
            // In my case, when extremities overlap I should not care.
            intersection = a;
            return false;
        }

        if (ABCrossCD == 0f)
        {
            intersection = Vector2.zero;
            return false; // Lines are parallel.
        }

        float rxsr = 1f / ABCrossCD;
        float t = deltaCrossCD * rxsr;
        float u = deltaCrossAB * rxsr;

        intersection = a + t * ab;

        return (intersection != a) && (t > float.Epsilon) && (t < 1f) && (u > float.Epsilon) && (u < 1f);
    }
    
    public struct VectorPair
    {
        public Vector2 V1;
        public Vector2 V2;
        public Vector2 Normale;

        public float Magnitude()
        {
            return (this.V1 - this.V2).magnitude;
        }

        public void ComputeNormale()
        {
            this.Normale = (this.V2 - this.V1).normalized;
            float temp = this.Normale.x;
            this.Normale.x = this.Normale.y;
            this.Normale.y = temp;
        }
    }

    public struct Square
    {
        public Vector2 Center;
        public float Size;

        public VectorPair North;
        public VectorPair East;
        public VectorPair South;
        public VectorPair West;

        public Vector3[] Corners;

        public void Refresh()
        {
            float halfSize = this.Size / 2;
            this.North.V1 = this.Center + new Vector2(-halfSize, halfSize);
            this.North.V2 = this.Center + new Vector2(halfSize, halfSize);
            this.East.V1 = this.Center + new Vector2(halfSize, halfSize);
            this.East.V2 = this.Center + new Vector2(halfSize, -halfSize);
            this.South.V1 = this.Center + new Vector2(halfSize, -halfSize);
            this.South.V2 = this.Center + new Vector2(-halfSize, -halfSize);
            this.West.V1 = this.Center + new Vector2(-halfSize, -halfSize);
            this.West.V2 = this.Center + new Vector2(-halfSize, halfSize);

            this.North.ComputeNormale();
            this.East.ComputeNormale();
            this.South.ComputeNormale();
            this.West.ComputeNormale();

            this.Corners = new Vector3[] { this.North.V1, this.East.V1, this.South.V1, this.West.V1, this.North.V1 };
        }
    }
}
