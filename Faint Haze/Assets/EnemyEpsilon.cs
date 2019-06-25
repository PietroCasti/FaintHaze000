using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEpsilon : MonoBehaviour
{
    //Reference al collider dell'oggetto. Usato per accedere al raggio di detezione del player, per poterlo modificare in seguito
    //all'effetto di un'interazione ambientale.
    private CircleCollider2D detection;

    //Valori numeri utilizzati nella modifica del raggio di detezione.
    public float radius_initial;
    public float radius_modified;

    //Valori numerici di supporto.
    public float reduction_duration;

    private void Awake()
    {
        detection = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        radius_initial = 1.67f;
        radius_modified = 1.0f;

        reduction_duration = 5.0f;
    }

    IEnumerator ReduceDetection()
    {
        detection.radius = radius_modified;

        yield return new WaitForSeconds(reduction_duration);

        detection.radius = radius_initial;
    }

}
