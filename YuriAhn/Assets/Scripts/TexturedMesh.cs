// This sample code demonstrates how to create a texture using Perlin noise.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturedMesh : MonoBehaviour {

	public int grid_size = 85;
	public float scale = 10;
    public float height_scalar = 1;
    public float plant_scale = 0.1f;

    private Vector3[] verts;  // the vertices of the mesh
    private int[] tris;       // the triangles of the mesh (triplets of integer references to vertices)
    private Vector3[] normals;

    private int ntris = 0;    // the number of triangles that have been created so far
    private int nplants = 0;
    private int nplant_grids = 0;
    private int max_plants = 10;

    private Dictionary <(int,int), float> height_map;
    GameObject s;

    // Create a quad that is textured

    public GameObject createPatch(int start_x, int start_z)
    {
        // call the routine that makes a mesh (a cube) from scratch
        Mesh my_mesh = CreateMyMesh2();

        // create a new GameObject and give it a MeshFilter and a MeshRenderer
        s = new GameObject("Textured Mesh");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        // associate my mesh with this object
        s.GetComponent<MeshFilter>().mesh = my_mesh;

        //update the heights of the vertices
        updateHeight(start_x, start_z);

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();
        rend.material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        // create a texture
        Texture2D texture = make_a_texture(start_x, start_z);

        // attach the texture to the mesh
        Renderer renderer = s.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        // create plants in random locations (within the grassy regions)
        create_plants(start_x, start_z);

        return s;
    }

    void updateHeight(int start_x, int start_z)
    {
        height_map = new Dictionary<(int, int), float>();

        int vert_index = 0;
        for (int i = start_x; i < start_x+grid_size+1; i++)
            for (int j = start_z; j < start_z+grid_size+1; j++)
            {
                float x = i * scale / (float)grid_size;
                float z = j * scale / (float)grid_size;
                float h = height(x, z, height_scalar);

                height_map.Add((i, j), h);

                Vector3 vert = verts[vert_index];
                verts[vert_index] = new Vector3(vert.x, h, vert.z);
                vert_index += 1;

            }


        s.GetComponent<MeshFilter>().mesh.vertices = verts;
        s.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }

	// create a texture using Perlin noise
	Texture2D make_a_texture(int start_x, int start_z) {

		// create the texture and an array of colors that will be copied into the texture
		Texture2D texture = new Texture2D (grid_size, grid_size);
		Color[] colors = new Color[grid_size * grid_size];

        // create the Perlin noise pattern in "colors"
     
        for (int i = start_x; i < start_x + grid_size; i++)
        {
            for (int j = start_z; j < start_z + grid_size; j++)
            {

                float h = height_map[(i, j)];

                Color color = calculateColor(h);
                colors[(j - start_z) * grid_size + (i - start_x)] = color;
         
            }
        
        }

        // copy the colors into the texture
        texture.SetPixels(colors);

		// do texture-y stuff, probably including making the mipmap levels
		texture.Apply();

		// return the texture
		return (texture);
	}

    void create_plants(int start_x, int start_z)
    {
        GameObject plant_grid = new GameObject("plant_grid");
        plant_grid.name = nplant_grids.ToString("plant grid 0");

        for (int i = 0; i < max_plants; i++)
        {

            // generate random coordinates within the ranges
            int x = Mathf.RoundToInt(Random.Range(start_x, start_x + grid_size));
            int z = Mathf.RoundToInt(Random.Range(start_z, start_z + grid_size));

            // get the height of this coordinate
            float h = height_map[(x, z)];

            // if the height is in the grassy region (0.93 > h > 0.6), then generate a plant
            if (h > 0.6 && h < 0.93)
            {
                GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                plant.transform.position = new Vector3((float)x/(float)grid_size, h, (float)z/(float)grid_size);  // move this object to a new location
                plant.transform.localScale = new Vector3(plant_scale, plant_scale, plant_scale);  // shrink the object

                // give the plant a name
                plant.name = nplants.ToString("plant 0");

                // change the color of the plant
                Renderer rend = plant.GetComponent<Renderer>();
                rend.material.color = new Color((Random.Range(30,80))/255.0f, (Random.Range(100,120))/255.0f, (Random.Range(30, 80)) / 255.0f, 1.0f);

                plant.transform.SetParent(plant_grid.transform, false);
                nplants++;
            }

        }
        nplant_grids++;

    }

    Mesh CreateMyMesh2()
    {

        // create a mesh object
        Mesh mesh = new Mesh();

        ntris = 0; //zero out number of triangles for each mesh
        int squares = grid_size;

        // vertices 
        int num_verts = (squares+1)*(squares+1);
        verts = new Vector3[num_verts];
        normals = new Vector3[num_verts];

        int vert_num = 0;
        for (float x = 0; x < squares+1; x++)
        {
            for (float z = 0; z < squares+1;  z++)
            {
                verts[vert_num] = new Vector3(x/(float) squares, 0 , z/(float)squares);
                vert_num += 1;
            }
        }

        // create uv coordinates

        Vector2[] uv = new Vector2[verts.Length];

        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2(verts[i].x, verts[i].z);
        }
        

        int num_tris = squares*squares*2;
        tris = new int[num_tris * 3];  // need 3 vertices per triangle

        for (int i = 0; i < num_verts-squares-2; i+=1)
        {
           
            if (i != squares && (i - squares) % (squares + 1) != 0)
            {
                //Debug.Log(i +" "+ (i + 1)+ " "+((i + 1) + (squares + 1))+ " "+(i + (squares + 1)));
                MakeQuad(i, i + 1, (i + 1) + (squares + 1), i + (squares + 1));
                
            }
        }

        // save the vertices and triangles in the mesh object
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uv;  // save the uv texture coordinates
        mesh.normals = normals;

        //mesh.RecalculateNormals();  // automatically calculate the vertex normals
   
        return (mesh);
    }

    // make a triangle from three vertex indices (clockwise order)
    void MakeTri(int i1, int i2, int i3)
    {
        int index = ntris * 3;  // figure out the base index for storing triangle indices
        ntris++;

        //Debug.Log("index: " + index+", ntris: " +ntris+", tris length: "+tris.Length);
        tris[index] = i1;
        tris[index + 1] = i2;
        tris[index + 2] = i3;
    }

    // make a quadrilateral from four vertex indices (clockwise order)
    void MakeQuad(int i1, int i2, int i3, int i4)
    {
        MakeTri(i1, i2, i3);
        MakeTri(i1, i3, i4);

        Vector3[] vertices = new Vector3[] { verts[i1], verts[i2], verts[i3], verts[i4] };
        int[] indicies = new int[] { i1, i2, i3, i4 };
        calculate_normals(vertices, indicies); //calculate surface normals of vertices
    }

    // calculates surface normals of vertices 
    void calculate_normals(Vector3[] vertices, int[] indicies)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 edge1 = new Vector3();
            Vector3 edge2 = new Vector3();
            if (i == vertices.Length -1)
            {
                edge1 = vertices[i] - vertices[0];
                edge2 = vertices[0]- vertices[i];

            } else
            {
                edge1 = vertices[i] - vertices[i + 1];
                edge2 = vertices[i + 1] - vertices[i];
            }

            Vector3 cross_prod = Vector3.Cross(edge1, edge2);
            for (int j = 0; j < vertices.Length; j++)
            {
                normals[indicies[j]] += cross_prod;
            }
        }

        for (int i = 0; i < indicies.Length; i++)
        {
            normals[indicies[i]].Normalize(); 
        }
    }

    Color calculateColor(float t)
    {
        if (t < 0.5)
        {
            //mid water (medium blue)
            return new Color(0.239f, 0.50f, 0.960f, 1.0f);
        }
        else if (t < 0.6)
        {
            //shallow water (light blue)
            return new Color(0.258f, 0.619f, 0.960f, 1.0f);
        } else if (t > 0.95)
        {
            // snowy tips (light yellowish white)
            return new Color(0.949f, 0.949f, 0.721f, 1.0f);
        } else
        {
            // grassy areas (light green)
            return new Color(0.352f, 0.670f, 0.392f, 1.0f);
        }
    }

    float height(float x, float z, float height_scalar)
    {
        float height = Mathf.PerlinNoise(x, z) + (0.5f) * Mathf.PerlinNoise(2 * x, 2 * z) + (0.25f) * Mathf.PerlinNoise(4 * x, 4 * z) + (0.125f)* Mathf.PerlinNoise(8*x,8*z);
        return height*height_scalar;
    }

	// Update is called once per frame
	void Update () {

	}
		
}
