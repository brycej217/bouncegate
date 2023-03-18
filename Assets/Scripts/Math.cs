using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Math : MonoBehaviour
{
    public static Vector2 RandomVector()
    {
        Vector2 randVector = new Vector2();
        randVector.x = Random.Range(-99999999, 99999999);
        randVector.y = Random.Range(-99999999, 99999999);
        if (randVector.x == 0)
        {
            randVector.x = 1;
        }
        if (randVector.y == 0)
        {
            randVector.y = 1;
        }
        randVector.Normalize();
        return randVector;
    }
}
