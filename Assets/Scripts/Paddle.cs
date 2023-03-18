using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    private Ball ball;

    private LineRenderer lr;
    private EdgeCollider2D ec;
    private Rigidbody2D rb;
    private AudioSource audioSource;

    public Vector3 direction;

    private ParticleSystem ps;

    public bool alive = false;

    public float life = 3f;
    private float timePassed = 0f;

    private bool speed;
    private bool slow;
    private GameManager GM;

    private void Awake()
    {
        GM = GameObject.Find("/GameManager").GetComponent<GameManager>();  
    }

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        ec = GetComponent<EdgeCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        ps = transform.GetChild(0).GetComponent<ParticleSystem>();
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
    }

    private void Update()
    {
        if (timePassed > life)
        {
            Destroy(gameObject);
        }

        if (alive)
        {
            timePassed += Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            if(!GM.GameOn) {
                GM.StartGame(); 
            }
            GameManager.BallBounce(direction);
            gameObject.layer = LayerMask.NameToLayer("Gate");

            //add force to paddle
            Rigidbody2D brb = collision.gameObject.GetComponent<Rigidbody2D>();
            rb.AddForce(-brb.velocity * 2500f, ForceMode2D.Force);

            ec.isTrigger = true;

            int type = Random.Range(0, 2);

            if (type == 0)
            {
                speed = true;
                SetColor(Color.red);
                ps.startColor = Color.red;
                audioSource.pitch = 1f;
            }
            else
            {
                slow = true;
                SetColor(Color.blue);
                ps.startColor = Color.blue;
                audioSource.pitch = 0.5f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            Rigidbody2D brb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (speed)
            {
                brb.velocity *= 1.125f;
                GameManager.SpeedField();
            }
            else if (slow)
            {
                GameManager.SlowField();
            }

            //audio
            audioSource.Play();

            //particle system
            ps.transform.position = brb.transform.position;
            ps.Play();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Vector3 dir = collision.GetComponent<Wall>().direction;
            rb.velocity = Vector3.Reflect(rb.velocity, dir);
        }
    }

    private void SetColor(Color color)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.75f, 0.0f), new GradientAlphaKey(0.75f, 1.0f) }
        );
        lr.colorGradient = gradient;
    }
}
