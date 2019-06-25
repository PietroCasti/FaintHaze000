using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionWasps : MonoBehaviour
{
    //private Transform[] enemyPositions;

    public EnemyEpsilon epsilon_reference;

    public void ReduceEpsilonDetection()
    {
        epsilon_reference.StartCoroutine("ReduceDetection");
        Destroy(gameObject);
    }
    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy")
        {
            //Distinzione tra nemico Alpha (diminuisce il raggio dei Raycast) e il nemico Epsilon (diminuisce la grandezza del collider)

        }
    }*/

}
