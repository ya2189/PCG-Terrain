// This sample code demonstrates how to create geometry "on demand" based on camera motion.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotionUpdated : MonoBehaviour {


    int max_chunk = -1;   // the number of chunks that we've made
    int chunk_size = 85;

    List<Vector2> generated_chunks;
    Vector2 prev_chunk;
    Vector2 curr_chunk;

	Camera MainCamera;
    GameObject patch;

    void Start () {

        generated_chunks = new List<Vector2>();
        prev_chunk = new Vector2(0, 0);
        curr_chunk = new Vector2(0, 0);

        patch = GameObject.Find("Patch");

		// cache the main camera
		MainCamera = Camera.main;
    }
	
	// Move the camera, and maybe create a new plane
	void Update () {

       
        // get the horizontal and verticle controls (arrows, or WASD keys)
        float dx = Input.GetAxis ("Horizontal");
		float dz = Input.GetAxis ("Vertical");

		//Debug.LogFormat ("dx dz: {0} {1}", dx, dz);

		// sensitivity factors for translate and rotate
		float translate_factor = 0.1f;
		float rotate_factor = 3.0f;

		// translate forward or backwards
		MainCamera.transform.Translate (0, 0, dz * translate_factor);

		// rotate left or right
		MainCamera.transform.Rotate (0, dx * rotate_factor, 0);

		// grab the main camera position
		Vector3 cam_pos = MainCamera.transform.position;

        prev_chunk = curr_chunk;
        curr_chunk = new Vector2(Mathf.FloorToInt(cam_pos.x), Mathf.FloorToInt(cam_pos.z ));
        if (prev_chunk != curr_chunk && !generated_chunks.Contains(curr_chunk))
        {
   
            generated_chunks.Add(curr_chunk);
            create_new_chunk(curr_chunk);
        }

	}
  
    // create a new chunk
    void create_new_chunk(Vector2 curr_chunk)
    {
        int index = max_chunk + 1;
        // create a new chunk
        GameObject s = patch.GetComponent<TexturedMesh>().createPatch((int)curr_chunk.x * chunk_size, (int)curr_chunk.y*chunk_size);
        s.name = index.ToString("Patch 0");  // give this chunk a name

        // move plane to proper location in x and z location
        s.transform.position = new Vector3(curr_chunk.x, 0.0f, curr_chunk.y);

        max_chunk++;

    }
}

