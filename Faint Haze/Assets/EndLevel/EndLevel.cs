using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using UnityEngine;

public class EndLevel : MonoBehaviour
{

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("TriggerEnter");

        if (collision.tag == "Player")
        {
            Debug.Log("ChecktagPlayer");

            if (Input.GetKeyDown(KeyCode.Z))
            {

                Debug.Log(collision.GetComponent<Haze>());
                animator.SetTrigger("Door");
                Destroy(collision.GetComponent<Haze>());
            }
            
        }
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("SceneEpsilon");
    }



}
