using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage=5;
    public Transform targetPosition;
    
    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    void Update()
    {
        
    }

}
