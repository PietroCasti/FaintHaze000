using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float health;
    public float speed;

    private Rigidbody2D rb2d;
    private Vector2 velocity;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        health = 10;
        speed = 5;
    }

    private void FixedUpdate()
    {
        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
        velocity = move.normalized * speed;
        rb2d.MovePosition(rb2d.position + velocity * Time.fixedDeltaTime);
    }
}
