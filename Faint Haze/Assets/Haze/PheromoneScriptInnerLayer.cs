using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneScriptInnerLayer : MonoBehaviour

{
    public List<EnemyMovement> enemyScripts = new List<EnemyMovement>();
    //private EnemyMovement EnemyScript;

    // Inner Layer Cambia lo stato del nemico da "Attratto" a "Stordito", facendolo fermare per il tempo rimanente della boccietta

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == ("Enemy"))
        {
            if (collision.GetComponent<EnemyMovement>())
                enemyScripts.Add(collision.GetComponent<EnemyMovement>());


            for (int i = 0; i < enemyScripts.Count; i++)
            {
                if (enemyScripts[i].ActiveStatus == EnemyMovement.Status.Attratto)
                {
                    enemyScripts[i].pheromonePosition = new Vector2(transform.parent.position.x, enemyScripts[i].transform.position.y);
                    enemyScripts[i].ActiveStatus = EnemyMovement.Status.Stordito;
                }

            }
        }
    }

    private void OnDestroy()
    {
        
        for (int i = 0; i < enemyScripts.Count; i++)
        {
            

            if (enemyScripts[i].ActiveStatus == EnemyMovement.Status.Stordito)
            {
                
                enemyScripts[i].ActiveStatus = EnemyMovement.Status.Ritorno;
            }
        }
    }
}
