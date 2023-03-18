using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Ball : MonoBehaviour
{
    public Rigidbody2D rb;

    private AudioSource audioSource;
    public ParticleSystem ps;
    private TrailRenderer tr;

    private float minSpeed = 5f;
    public float maxSpeed = 25f;
    private Transform SpriteTransform;
    private bool frozen = false;

    private void Awake()
    {
        SpriteTransform = GameObject.Find("/Ball/Circle").GetComponent<Transform>();
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        tr = GetComponent<TrailRenderer>();

        rb.velocity = new Vector2(RandomStartVector(),RandomStartVector()); //add force in random direction
    }
    private float RandomStartVector() {
        return Random.Range(2, minSpeed) * RandomBinary();
    }

    private int RandomBinary() {
        int rando = Random.Range(0, 2);
        if(rando == 0) {
            return -1;
        }
        else {
            return 1; 
        }
    }
    private void FixedUpdate()
    {
        SpeedControl();

        //change rotation
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (Mathf.Approximately(rb.velocity.magnitude, maxSpeed) || rb.velocity.magnitude > maxSpeed)
        {
            tr.enabled = true;
        }
        else
        {
            tr.enabled = false;
        }
    }

    private void SpeedControl()
    {
        if (Mathf.Approximately(rb.velocity.magnitude, maxSpeed) || rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        if (rb.velocity.magnitude <= minSpeed)
        {
            rb.velocity = rb.velocity.normalized * minSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //camera shake
        float magnitude = rb.velocity.magnitude / 5f;
        CameraShaker.Instance.ShakeOnce(magnitude, magnitude, 0.1f, 0.1f);

        StartCoroutine(Squash());

        //paddle audio
        float pitch = rb.velocity.magnitude / 5f;
        Mathf.Clamp(pitch, 0.5f, 4f);
        audioSource.pitch = pitch;

        audioSource.Play();

        //timeFreeze if maxspeed
        if (rb.velocity.magnitude >= maxSpeed)
        {
            if (!frozen)
            {
                StartCoroutine(TimeFreeze());
            }
        }

        ps.Play();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            collision.transform.parent.GetComponent<AudioSource>().Play();
            GameManager.WallHit(collision.gameObject.GetComponent<Wall>().direction);
            StartCoroutine(Squash());
            ps.Play();
        }
    }
    private IEnumerator TimeFreeze()
    {
        frozen = true; 
        float prevScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.15f);
        Time.timeScale = prevScale;
        frozen = false;
    }

    IEnumerator Squash() { 
        for(float i = SpriteTransform.localScale.y ; i < 1f + ClampedVelocity(); i += .1f) {
            Vector3 temp = new Vector3(1, i, 1); 
            SpriteTransform.localScale = temp;
            yield return new WaitForSecondsRealtime(.01f);
        }

        for(float i = SpriteTransform.localScale.y ; i > 1f; i -= .1f) {
            Vector3 temp = new Vector3(1, i, 1); 
            SpriteTransform.localScale = temp;
            yield return new WaitForSecondsRealtime(.01f);
        }
    }

    private float ClampedVelocity() {

        float speed = rb.velocity.magnitude;
        return Mathf.Clamp(speed/10, .25f, 1f);
    }
}
