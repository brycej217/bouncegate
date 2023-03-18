using UnityEngine;
using System.Collections.Generic;

public class PaddleSpawner : MonoBehaviour
{
    public GameObject paddle;
    [SerializeField]
    private GameObject menu;

    private Rigidbody2D ball;
    
    private LineRenderer lr;
    private EdgeCollider2D ec;
    private Rigidbody2D rb;

    private float baseDistance = 6;
    private float distance;
    private float distanceThresh = 10f;
    private GameObject newPad;
    private Vector3 startPos;

    private bool padInProg;
    private bool focused;

    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Rigidbody2D>();
        distance = baseDistance;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (padInProg)
        {
            KillPad();
        }
    }

    private void Update()
    {
        if (ball.velocity.magnitude > distanceThresh)
        {
            distance = baseDistance + ball.velocity.magnitude / distanceThresh;
            Mathf.Clamp(distance, baseDistance, 8f);
        }

        if (menu.activeInHierarchy)
        {
            if (padInProg)
            {
                KillPad();
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartPad();
        }
        if (Input.GetMouseButton(0))
        {
            GenPad();
        }
        if (Input.GetMouseButtonUp(0))
        {
            EndPad();
        }
    }

    private void StartPad()
    {
        //create new paddle
        padInProg = true;
        newPad = Instantiate(paddle, Vector3.zero, Quaternion.identity);

        //get references to its components
        lr = newPad.GetComponent<LineRenderer>();
        ec = newPad.GetComponent<EdgeCollider2D>();
        rb = newPad.GetComponent<Rigidbody2D>();

        //set position count and first position
        lr.positionCount = 2;
        startPos = MousePos();
        lr.SetPosition(0, startPos);

        //change color
        SetColor(0.1f);
    }

    private void GenPad()
    {
        if (!padInProg)
        {
            return;
        }


        Vector3 lineDir = MousePos() - startPos;

        if (lineDir.magnitude > distance)
        {
            lr.SetPosition(1, startPos + lineDir.normalized * distance);
        }
        else
        {
            lr.SetPosition(1, MousePos());
        }
    }

    private void EndPad()
    {
        if (!padInProg)
        {
            return;
        }

        Vector3 endPos = MousePos();
        Vector3 lineDir = endPos - startPos;

        newPad.GetComponent<Paddle>().direction = lineDir; 

        if (lineDir.magnitude > distance)
        {
            lr.SetPosition(1, startPos + lineDir.normalized * distance);
        }
        else
        {
            lr.SetPosition(1, MousePos());
        }

        if (lineDir.magnitude < 0.5)
        {
            KillPad();
        }

        //change color
        SetColor(1f);

        //set collider
        SetEdgeCollider();

        //set rigidbody
        rb.centerOfMass = Vector3.Lerp(startPos, endPos, 0.5f);

        newPad.GetComponent<Paddle>().alive = true;
        padInProg = false;
        newPad = null;
    }

    private void SetEdgeCollider()
    {
        List<Vector2> edges = new();

        for (int i = 0; i < lr.positionCount; i++)
        {
            Vector3 point = lr.GetPosition(i);
            edges.Add(new Vector2(point.x, point.y));
        }

        ec.SetPoints(edges);
        ec.enabled = true;
    }

    private Vector3 MousePos()
    {
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 worldPos = new Vector3(screenPos.x, screenPos.y, 0);
        return worldPos;
    }

    private void SetColor(float alpha)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lr.colorGradient = gradient;
    }

    private void KillPad()
    {
        Destroy(newPad);
        padInProg = false;
    }
}
