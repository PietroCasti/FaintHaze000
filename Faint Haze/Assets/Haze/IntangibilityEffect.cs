using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntangibilityEffect : MonoBehaviour
{
    //Il seguente script altera l'aspetto dell'oggetto in cui si nasconde il personaggio, aggiungendo trasparenza? al fine di
    //far vedere la posizione esatta al suo interno.

    public GameObject intangibility_object;

    public enum Height { Full, Half }
    public Height object_height;

    public Material mat_intangible;
    private Material mat_original;

    private void Awake()
    {
        mat_original = intangibility_object.GetComponent<MeshRenderer>().material;
    }

    public void ChangeVision()
    {
        intangibility_object.GetComponent<MeshRenderer>().material = mat_intangible;
    }

    public void ChangeBack()
    {
        intangibility_object.GetComponent<MeshRenderer>().material = mat_original;
    }

    public virtual void PushAway(GameObject target)
    {

        Debug.Log("PUSH!");
        if (target.GetComponent<BoxCollider2D>().bounds.center.x < intangibility_object.GetComponent<BoxCollider2D>().bounds.center.x)
        {
            target.transform.position = Vector2.Lerp(target.transform.position, target.transform.position + new Vector3(-3, 0, 0), 0.75f);
        }
        else
        {
            target.transform.position = Vector2.Lerp(target.transform.position, target.transform.position + new Vector3(3, 0, 0), 0.75f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (collision.GetComponent<Haze>().isIntangible == true)
            {
                ChangeVision();
                collision.GetComponent<Haze>().isStuck = true;
                if (object_height == Height.Full)
                {
                    collision.GetComponent<Haze>().stealth_status = Haze.Visibility.Hidden;
                }
                else
                {
                    collision.GetComponent<Haze>().stealth_status = Haze.Visibility.Caution;
                }

            }

        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (collision.GetComponent<Haze>().isIntangible == false && collision.GetComponent<Haze>().isStuck == true)
            {
                collision.GetComponent<Haze>().isStuck = false;
                collision.GetComponent<Haze>().stealth_status = Haze.Visibility.Visible;
                PushAway(collision.gameObject);
            }

            if (collision.GetComponent<Haze>().isIntangible == true && collision.GetComponent<Haze>().isStuck == false)
            {
                ChangeVision();
                collision.GetComponent<Haze>().isStuck = true;
                if (object_height == Height.Full)
                {
                    collision.GetComponent<Haze>().stealth_status = Haze.Visibility.Hidden;
                }
                else
                {
                    collision.GetComponent<Haze>().stealth_status = Haze.Visibility.Caution;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            ChangeBack();
            collision.GetComponent<Haze>().isStuck = false;
            collision.GetComponent<Haze>().stealth_status = Haze.Visibility.Visible;
        }
    }
}
