using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Astrolabe.Scripts.Tools
{
	public class RoundedQuadMesh : MonoBehaviour
	{
		[FormerlySerializedAs("RoundEdges")] public float roundEdges = 0f;
		[FormerlySerializedAs("RoundTopLeft")] public float roundTopLeft = 0.0f;
		[FormerlySerializedAs("RoundTopRight")] public float roundTopRight = 0.0f;
		[FormerlySerializedAs("RoundBottomLeft")] public float roundBottomLeft = 0.0f;
		[FormerlySerializedAs("RoundBottomRight")] public float roundBottomRight = 0.0f;

		[FormerlySerializedAs("UsePercentage")] public bool usePercentage = true;

		public Rect rect = new(-0.5f, -0.5f, 1f, 1f);
		[FormerlySerializedAs("Scale")] public float scale = 1f;
		[FormerlySerializedAs("CornerVertexCount")] public int cornerVertexCount = 8;
		[FormerlySerializedAs("CreateUV")] public bool createUV = true;
		[FormerlySerializedAs("FlipBackFaceUV")] public bool flipBackFaceUV = false;
		[FormerlySerializedAs("DoubleSided")] public bool doubleSided = false;
		//public bool AutoUpdate = true;

		public bool isDirty = false;

		private MeshFilter _mMeshFilter;
		private Mesh _mMesh;
		private Vector3[] _mVertices;
		private Vector3[] _mNormals;
		private Vector2[] _mUV;
		private int[] _mTriangles;

		public void ChangeCornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
		{
			roundTopLeft = topLeft;
			roundTopRight = topRight;
			roundBottomLeft = bottomLeft;
			roundBottomRight = bottomRight;

			isDirty = true;
		}

		public void ChangeDimension(float width, float height)
		{
			rect = new Rect(-width / 2, -height / 2, width, height);
			isDirty = true;
		}

		private void Start()
		{
			_mMeshFilter = GetComponent<MeshFilter>();
			if (_mMeshFilter == null)
			{
				_mMeshFilter = gameObject.AddComponent<MeshFilter>();
			}

			if (GetComponent<MeshRenderer>() == null)
			{
				gameObject.AddComponent<MeshRenderer>();
			}

			_mMesh = new Mesh();
			_mMeshFilter.sharedMesh = _mMesh;

			UpdateMesh();
		}

		public Mesh UpdateMesh()
		{
			if (cornerVertexCount < 2)
			{
				cornerVertexCount = 2;
			}

			var sides = doubleSided ? 2 : 1;
			var vCount = cornerVertexCount * 4 * sides + sides; //+sides for center vertices
			var triCount = cornerVertexCount * 4 * sides;
			if (_mVertices == null || _mVertices.Length != vCount)
			{
				_mVertices = new Vector3[vCount];
				_mNormals = new Vector3[vCount];
			}

			if (_mTriangles == null || _mTriangles.Length != triCount * 3)
			{
				_mTriangles = new int[triCount * 3];
			}

			if (createUV && (_mUV == null || _mUV.Length != vCount))
			{
				_mUV = new Vector2[vCount];
			}

			var count = cornerVertexCount * 4;

			if (createUV)
			{
				_mUV[0] = Vector2.one * 0.5f;
				if (doubleSided)
				{
					_mUV[count + 1] = _mUV[0];
				}
			}

			var tl = Mathf.Max(0, roundTopLeft + roundEdges);
			var tr = Mathf.Max(0, roundTopRight + roundEdges);
			var bl = Mathf.Max(0, roundBottomLeft + roundEdges);
			var br = Mathf.Max(0, roundBottomRight + roundEdges);
			var f = Mathf.PI * 0.5f / (cornerVertexCount - 1);
			var a1 = 1f;
			var a2 = 1f;
			var x = 1f;
			var y = 1f;
			var rs = Vector2.one;

			if (usePercentage)
			{
				rs = new Vector2(rect.width, rect.height) * 0.5f;
				if (rect.width > rect.height)
				{
					a1 = rect.height / rect.width;
				}
				else
				{
					a2 = rect.width / rect.height;
				}

				tl = Mathf.Clamp01(tl);
				tr = Mathf.Clamp01(tr);
				bl = Mathf.Clamp01(bl);
				br = Mathf.Clamp01(br);
			}
			else
			{
				x = rect.width * 0.5f;
				y = rect.height * 0.5f;

				if (tl + tr > rect.width)
				{
					var b = rect.width / (tl + tr);
					tl *= b;
					tr *= b;
				}

				if (bl + br > rect.width)
				{
					var b = rect.width / (bl + br);
					bl *= b;
					br *= b;
				}

				if (tl + bl > rect.height)
				{
					var b = rect.height / (tl + bl);
					tl *= b;
					bl *= b;
				}

				if (tr + br > rect.height)
				{
					var b = rect.height / (tr + br);
					tr *= b;
					br *= b;
				}
			}

			_mVertices[0] = rect.center * scale;

			if (doubleSided)
			{
				_mVertices[count + 1] = rect.center * scale;
			}

			for (var i = 0; i < cornerVertexCount; i++)
			{
				var s = Mathf.Sin((float)i * f);
				var c = Mathf.Cos((float)i * f);
				Vector2 v1 = new Vector3(-x + (1f - c) * tl * a1, y - (1f - s) * tl * a2);
				Vector2 v2 = new Vector3(x - (1f - s) * tr * a1, y - (1f - c) * tr * a2);
				Vector2 v3 = new Vector3(x - (1f - c) * br * a1, -y + (1f - s) * br * a2);
				Vector2 v4 = new Vector3(-x + (1f - s) * bl * a1, -y + (1f - c) * bl * a2);

				_mVertices[1 + i] = (Vector2.Scale(v1, rs) + rect.center) * scale;
				_mVertices[1 + cornerVertexCount + i] = (Vector2.Scale(v2, rs) + rect.center) * scale;
				_mVertices[1 + cornerVertexCount * 2 + i] = (Vector2.Scale(v3, rs) + rect.center) * scale;
				_mVertices[1 + cornerVertexCount * 3 + i] = (Vector2.Scale(v4, rs) + rect.center) * scale;
				if (createUV)
				{
					if (!usePercentage)
					{
						var adj = new Vector2(2f / rect.width, 2f / rect.height);
						v1 = Vector2.Scale(v1, adj);
						v2 = Vector2.Scale(v2, adj);
						v3 = Vector2.Scale(v3, adj);
						v4 = Vector2.Scale(v4, adj);
					}

					var uv = v1 * 0.5f + Vector2.one * 0.5f;
					var uv2 = v2 * 0.5f + Vector2.one * 0.5f;
					var uv3 = v3 * 0.5f + Vector2.one * 0.5f;
					var uv4 = v4 * 0.5f + Vector2.one * 0.5f;
					//Inverting y coordinates for MRTK3 shader
					/*uv.y = 1 - uv.y;
				uv2.y = 1 - uv2.y;
				uv3.y = 1 - uv3.y;
				uv4.y = 1 - uv4.y;*/

					_mUV[1 + i] = uv;
					_mUV[1 + cornerVertexCount * 1 + i] = uv2;
					_mUV[1 + cornerVertexCount * 2 + i] = uv3;
					_mUV[1 + cornerVertexCount * 3 + i] = uv4;
				}

				if (doubleSided)
				{
					_mVertices[1 + cornerVertexCount * 8 - i] = _mVertices[1 + i];
					_mVertices[1 + cornerVertexCount * 7 - i] = _mVertices[1 + cornerVertexCount + i];
					_mVertices[1 + cornerVertexCount * 6 - i] = _mVertices[1 + cornerVertexCount * 2 + i];
					_mVertices[1 + cornerVertexCount * 5 - i] = _mVertices[1 + cornerVertexCount * 3 + i];
					if (createUV)
					{
						var uv = v1 * 0.5f + Vector2.one * 0.5f;
						var uv2 = v2 * 0.5f + Vector2.one * 0.5f;
						var uv3 = v3 * 0.5f + Vector2.one * 0.5f;
						var uv4 = v4 * 0.5f + Vector2.one * 0.5f;
						//Inverting y coordinates for MRTK3 shader
						/*uv.y = 1 - uv.y;
					uv2.y = 1 - uv2.y;
					uv3.y = 1 - uv3.y;
					uv4.y = 1 - uv4.y;*/

						_mUV[1 + cornerVertexCount * 8 - i] = uv;
						_mUV[1 + cornerVertexCount * 7 - i] = uv2;
						_mUV[1 + cornerVertexCount * 6 - i] = uv3;
						_mUV[1 + cornerVertexCount * 5 - i] = uv4;
					}
				}
			}

			for (var i = 0; i < count + 1; i++)
			{
				_mNormals[i] = -Vector3.forward;
				if (doubleSided)
				{
					_mNormals[count + 1 + i] = Vector3.forward;
					if (flipBackFaceUV)
					{
						var uv = _mUV[count + 1 + i];
						uv.x = 1f - uv.x;
						_mUV[count + 1 + i] = uv;
					}
				}
			}

			for (var i = 0; i < count; i++)
			{
				_mTriangles[i * 3] = 0;
				_mTriangles[i * 3 + 1] = i + 1;
				_mTriangles[i * 3 + 2] = i + 2;
				if (doubleSided)
				{
					_mTriangles[(count + i) * 3] = count + 1;
					_mTriangles[(count + i) * 3 + 1] = count + 1 + i + 1;
					_mTriangles[(count + i) * 3 + 2] = count + 1 + i + 2;
				}
			}

			_mTriangles[count * 3 - 1] = 1;
			if (doubleSided)
			{
				_mTriangles[_mTriangles.Length - 1] = count + 1 + 1;
			}

			_mMesh.Clear();
			_mMesh.vertices = _mVertices;
			_mMesh.normals = _mNormals;
			if (createUV)
			{
				_mMesh.uv = _mUV;
			}

			_mMesh.triangles = _mTriangles;
			return _mMesh;
		}

		private void Update()
		{
			if (isDirty)
			{
				isDirty = false;
				UpdateMesh();
			}
		}
	}
}