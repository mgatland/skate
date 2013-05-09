//#pragma strict
// Generate planar uv coordinates

private var scale = 1;

//flat and angled surfaces are drawn as if projected from above
//ignore Y
function noYUV (tris,i,vertices):Vector2 {
	return Vector2 ((vertices[tris[i]].z)/scale, (vertices[tris[i]].x)/scale);
}

//vertical surfaces when x is not increasing.
function noXUV (tris,i,vertices):Vector2 {
	return Vector2 ((vertices[tris[i]].z)/scale, (vertices[tris[i]].y)/scale);
}

//vertical surfaces when z is not increasing.
function noZUV (tris,i,vertices):Vector2 {
	return Vector2 ((vertices[tris[i]].y)/scale, (vertices[tris[i]].x)/scale);
}

function Start () {
    var mesh : Mesh = GetComponent(MeshFilter).mesh;
    var vertices : Vector3[]  = mesh.vertices;
    var uvs : Vector2[] = new Vector2[vertices.Length];
    
    //for (var i = 0 ; i < uvs.Length; i++)
	//the 2d coordinates need to increase regardless of which pair of 3d coordinates are increasing
	//x+y -> u+v (z has no effect)
	//x+z -> u+v (y has no effect)
	//y+z -> u+v (x has no effect)
		//print(mesh.vertices[i] + " " + mesh.uv[i]);
    //    uvs[i] = Vector2 ((vertices[i].x+vertices[i].y), (vertices[i].z));
		//where y and z are increasing... is this even solvable?

	var tris : int[] = mesh.triangles;
	//print("tries length: " + tris.Length);
	//print("uv length: " + uvs.length);
	for (i = 0; i < tris.Length; i+=3) {
		var firstVert = vertices[tris[i]];
		var secondVert = vertices[tris[i+1]];
		var thirdVert = vertices[tris[i+2]];
		var xChange : boolean = (firstVert.x != secondVert.x || firstVert.x != thirdVert.x);
		var yChange : boolean = (firstVert.y != secondVert.y || firstVert.y != thirdVert.y);
		var zChange : boolean = (firstVert.z != secondVert.z || firstVert.z != thirdVert.z);
		if (!xChange) {
			uvs[tris[i]] = noXUV(tris,i,vertices);
			uvs[tris[i+1]] = noXUV(tris,i+1,vertices);
			uvs[tris[i+2]] = noXUV(tris,i+2,vertices);
		} else if (!zChange) {
			uvs[tris[i]] = noZUV(tris,i,vertices);
			uvs[tris[i+1]] = noZUV(tris,i+1,vertices);
			uvs[tris[i+2]] = noZUV(tris,i+2,vertices);
		} else { //default to ignoring Y.
			uvs[tris[i]] = noYUV(tris,i,vertices);
			uvs[tris[i+1]] = noYUV(tris,i+1,vertices);
			uvs[tris[i+2]] = noYUV(tris,i+2,vertices);
		}
	}
	
	mesh.uv = uvs;
}