using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneScriptOuterLayer : MonoBehaviour
{
   // private EnemyMovement EnemyScript;
    public List<EnemyMovement> enemyScripts = new List<EnemyMovement>();

    //Outer layer cambia lo stato del nemico da "Ronda" ad "Attratto", facendolo avvicinare al punto centrale dei feromoni

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == ("Enemy"))
        {
            if (collision.GetComponent<EnemyMovement>()) 
                enemyScripts.Add(collision.GetComponent<EnemyMovement>());


            for (int i = 0; i<enemyScripts.Count; i++)
            {
                if (enemyScripts[i].ActiveStatus == EnemyMovement.Status.Ronda)
                {
                    enemyScripts[i].pheromonePosition = new Vector2(transform.parent.position.x, enemyScripts[i].transform.position.y);
                    enemyScripts[i].ActiveStatus = EnemyMovement.Status.Attratto;
                }

            }
            Debug.Log("OuterLayerCheck");
            /*EnemyScript = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyMovement>();
            if (EnemyScript.ActiveStatus == EnemyMovement.Status.Ronda) 
            {
                EnemyScript.pheromonePosition = new Vector2(transform.parent.position.x, EnemyScript.transform.position.y);
                EnemyScript.ActiveStatus = EnemyMovement.Status.Attratto;
            }*/
        }


        
    }
}
