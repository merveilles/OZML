using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

// To do: 
// * Proper run-time triangulation
// * Run-time recalculation of normals taking smoothing groups into account
// * Splitting up meshes exceeding 16 bit index buffer limit into multiple meshes
// * Multiple materials support
 
public class ObjImporter 
{
	public static List<Mesh> CreateMesh(string file)
    {
		int capacity = 9999999;

		Vector3[] vertices = new Vector3[capacity];
		Vector2[] uv = new Vector2[capacity];
		Vector3[] normals = new Vector3[capacity];
		int[] triangles = new int[capacity];
		Vector4[] faces = new Vector4[capacity];

		int vertexCount = 0;
		int uvCount = 0;
		int normalCount = 0;
		int triangleCount = 0;
		int faceCount = 0;
		
		Vector3 temp = new Vector3();
		int tempCounter;
		
		int dynamicFaceCount = 0;
		int dynamicTriangleCount = 0;
		List<int> triangleCounts = new List<int>();
		List<int> faceCounts = new List<int>();

		int indexBufferLimit = 65532;
		float maxVertices = indexBufferLimit;
		
		bool isMax = false;
		
		using (StringReader reader = new StringReader(file))
		{
            char[] splitIdentifier = { ' ' };
            char[] splitIdentifier2 = { '/' };
            string[] brokenString;
            string[] brokenBrokenString;
			
			string currentText;
		    while ((currentText = reader.ReadLine()) != null)
		    {
				if (currentText != null)
				{
					if(!isMax)
					{
						isMax = currentText.Contains("  "); // double space indicates max format
					}	

					currentText = currentText.Replace("  ", " ");
				}
				
				if(dynamicFaceCount > indexBufferLimit) 
				{
					triangleCounts.Add(dynamicTriangleCount);
					faceCounts.Add(dynamicFaceCount);
					dynamicTriangleCount = 0;
					dynamicFaceCount = 0;
					//break;
				}
				
                brokenString = currentText.Split(splitIdentifier, 50);
                switch (brokenString[0])
                {
                    case "v":
                        vertices[vertexCount] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]), System.Convert.ToSingle(brokenString[3]));
                        vertexCount++;
						break;
                    case "vt":
						uv[uvCount] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
						uvCount++;
                        break;
                    case "vn":
						normals[normalCount] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]), System.Convert.ToSingle(brokenString[3]));
						normalCount++;
                        break;
                    case "f":
                        tempCounter = 1;
						List<int> intTemp = new List<int>();
                        while (tempCounter < brokenString.Length && ("" + brokenString[tempCounter]).Length > 0)
                        {
                            brokenBrokenString = brokenString[tempCounter].Split(splitIdentifier2, 3);    //Separate the face into individual components (vert, uv, normal)
                            
							temp.x = System.Convert.ToInt32(brokenBrokenString[0]);
							temp.y = System.Convert.ToInt32(brokenBrokenString[1]);
							if(brokenBrokenString.Length > 2) temp.z = System.Convert.ToInt32(brokenBrokenString[2]);
							else temp.z = 0.0f;
						
							faces[faceCount] = temp;
							intTemp.Add( faceCount );
                            
							faceCount++;
							tempCounter++;
							dynamicFaceCount++;
                        }
					
                        /*tempCounter = 0;
                        while (tempCounter < intTemp.Count / 3)     //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
                        {
							triangles[triangleCount] = intTemp[tempCounter];
							triangleCount++;
							dynamicTriangleCount++;
							triangles[triangleCount] = intTemp[tempCounter + 1];
							triangleCount++;
							dynamicTriangleCount++;
							triangles[triangleCount] = intTemp[tempCounter + 2];
							triangleCount++;
							dynamicTriangleCount++;

                            tempCounter+=3;
                        }*/
					
						for( int i = 1; i < intTemp.Count - 1; ++i ) 
						{
							triangles[triangleCount] = intTemp[0];
							triangleCount++;
							dynamicTriangleCount++;
							triangles[triangleCount] = intTemp[i];
							triangleCount++;
							dynamicTriangleCount++;
							triangles[triangleCount] = intTemp[i+1];
							triangleCount++;
							dynamicTriangleCount++;
						}
                        break;
					default:
						break;
                }
            }
        }
		
		triangleCounts.Add(dynamicTriangleCount);
		faceCounts.Add(dynamicFaceCount);
		
		float vertexQuotient = faceCount/maxVertices;
		int submeshCount = (int)Mathf.Ceil(vertexQuotient);
		
		List<Mesh> meshList = new List<Mesh>();

		for(var n = 0; n < 1; n++)
		{
			//int fx = n < submeshCount - 1 ? indexBufferLimit : (faceCount - indexBufferLimit*(submeshCount - 1));
			//int tx = n < submeshCount - 1 ? indexBufferLimit : (triangleCount - indexBufferLimit*(submeshCount - 1));
			int fx = faceCounts[n];
			int tx = triangleCounts[n];
		
			Vector3[] newVerts = new Vector3[fx];
	        Vector2[] newUVs = new Vector2[fx];
	        Vector3[] newNormals = new Vector3[fx];
	        int[] newTriangles = new int[tx];
			
			var fIndexStart = indexBufferLimit*n;
			
			int index = 0;
			for(int i = fIndexStart; i < fIndexStart + fx; i++)
			{
				temp = faces[i];												
									newVerts[index] = vertices[(int)temp.x - 1];
	            if (temp.y >= 1) 	newUVs[index] = uv[(int)temp.y - 1];
				if(normalCount != 0)
				{
					if (temp.z >= 1) 	
					{
						newNormals[index] = normals[(int)temp.z - 1];
					}
				}
				
				index++;
			}

			Mesh mesh = new Mesh();
			mesh.vertices = newVerts;     
	        mesh.uv = newUVs;        
	        if(normalCount != 0) mesh.normals = newNormals;
			System.Array.Copy(triangles, fIndexStart, newTriangles, 0, tx);
			mesh.triangles = newTriangles;
			
			//if(normalCount == 0) 
			//{
				//mesh.RecalculateNormals();
				
				// If true, this indicates the mesh is flat shaded. Meshes currently need to be exported with 'Normals' checked.
				
				/*Vector3[] meshNormals = mesh.normals;
				Vector3[] smoothNormals = new Vector3[fx];
				Vector3[] meshVertices = mesh.vertices;

				int firstVertex = 0;
				for (int i = 1; i < fx - 1; i += 2) 
				{
					if ( meshVertices[i] == meshVertices[i+1] ) 
					{
						Vector3 averageNormal = ( meshNormals[i] + meshNormals[i+1] )/2;
						smoothNormals[i] = averageNormal;
						smoothNormals[i+1] = averageNormal;
					}
					else 
					{
						Vector3 averageNormal = ( meshNormals[firstVertex] + meshNormals[i] )/2;
						smoothNormals[firstVertex] = averageNormal;
						smoothNormals[i] = averageNormal;
						firstVertex = i+1;
					}
				}

				mesh.normals = smoothNormals;*/
			//}
			
			if( normalCount > 0 )
			{
				Vector3[] meshNormals = mesh.normals;
				Vector3[] meshVertices = mesh.vertices;
				int[] meshTriangles = mesh.triangles;
				
		        for( int i = 0; i < meshNormals.Length; i++ ) 
					meshNormals[i] = Vector3.zero;
				
				int f = 0;
		        for( int i = 0; i < meshTriangles.Length / 3; i++ ) 
				{
					Vector3 trinormal = CalcNormal( 
						meshVertices[meshTriangles[f]], 
						meshVertices[meshTriangles[f+1]], 
						meshVertices[meshTriangles[f+2]] 
					);
					
					meshNormals[meshTriangles[f]] = trinormal;
					meshNormals[meshTriangles[f+1]] = trinormal;
					meshNormals[meshTriangles[f+2]] = trinormal;
					
					f+=3;
				}
				
		        for( int i = 0; i < meshNormals.Length; i++ ) 
					meshNormals[i] = Vector3.Normalize( meshNormals[i] );
				mesh.normals = meshNormals;
				
				calculateMeshTangents(mesh);
			}
			
			mesh.RecalculateBounds();
			
			meshList.Add(mesh);
		}
		
		return meshList;
    }
	
	public static Vector3 CalcNormal( Vector3 a, Vector3 b, Vector3 c )
	{
		return Vector3.Cross( a, b ) + Vector3.Cross( b, c ) + Vector3.Cross( c, a );
	}

	public static void calculateMeshTangents(Mesh mesh)
	{
	    //speed up math by copying the mesh arrays
	    int[] triangles = mesh.triangles;
	    Vector3[] vertices = mesh.vertices;
	    Vector2[] uv = mesh.uv;
	    Vector3[] normals = mesh.normals;
	
	    //variable definitions
	    int triangleCount = triangles.Length;
	    int vertexCount = vertices.Length;
	
	    Vector3[] tan1 = new Vector3[vertexCount];
	    Vector3[] tan2 = new Vector3[vertexCount];
	
	    Vector4[] tangents = new Vector4[vertexCount];
	
	    for (long a = 0; a < triangleCount; a += 3)
	    {
	        long i1 = triangles[a + 0];
	        long i2 = triangles[a + 1];
	        long i3 = triangles[a + 2];
	
	        Vector3 v1 = vertices[i1];
	        Vector3 v2 = vertices[i2];
	        Vector3 v3 = vertices[i3];
	
	        Vector2 w1 = uv[i1];
	        Vector2 w2 = uv[i2];
	        Vector2 w3 = uv[i3];
	
	        float x1 = v2.x - v1.x;
	        float x2 = v3.x - v1.x;
	        float y1 = v2.y - v1.y;
	        float y2 = v3.y - v1.y;
	        float z1 = v2.z - v1.z;
	        float z2 = v3.z - v1.z;
	
	        float s1 = w2.x - w1.x;
	        float s2 = w3.x - w1.x;
	        float t1 = w2.y - w1.y;
	        float t2 = w3.y - w1.y;
	
	        float r = 1.0f / (s1 * t2 - s2 * t1);
	
	        Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
	        Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
	
	        tan1[i1] += sdir;
	        tan1[i2] += sdir;
	        tan1[i3] += sdir;
	
	        tan2[i1] += tdir;
	        tan2[i2] += tdir;
	        tan2[i3] += tdir;
	    }
	
	
	    for (long a = 0; a < vertexCount; ++a)
	    {
	        Vector3 n = normals[a];
	        Vector3 t = tan1[a];
	
	        //Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
	        //tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
	        Vector3.OrthoNormalize(ref n, ref t);
	        tangents[a].x = t.x;
	        tangents[a].y = t.y;
	        tangents[a].z = t.z;
	
	        tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
	    }
	
	    mesh.tangents = tangents;
	}
}