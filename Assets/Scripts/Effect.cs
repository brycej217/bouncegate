using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    private float duration;
    private ParticleSystem ps;

    private void Start()
    {
        ps = transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
        duration = ps.main.duration;
        StartCoroutine(Duration());
    }

    private IEnumerator Duration()
    {
        float timePassed = 0f;

        while (timePassed > duration)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
