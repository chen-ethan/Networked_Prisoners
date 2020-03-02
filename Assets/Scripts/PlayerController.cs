using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float health;
    // Start is called before the first frame update
    void Start()
    {
        transform.position+= new Vector3(
            Random.Range(-4f,4f),
            Random.Range(-4f,4f),
            0f);
            health = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
