using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    private float speed;
    private Rigidbody rb;
    private int count;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        speed = 10;
    }
    /* void Update() //called before rendering a frame
     {

     }*/

    void FixedUpdate() //called before physics calculation made
    {
        //grabs input from player through the keyboard
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        rb.AddForce(movement * speed);
    }

}
