using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Mathematics;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace yaqmv
{
	internal static partial class ModelConvertor
	{
		public static void BuildComponentList(ModelAsset asset, ref List<Vector2> UVList, ref Dictionary<int, int> backfaceMap, ref int[] indices)
		{
			var UVList2 = new List<Vector2>();  // onseam offsets to add at the end of the UV list
			int i;
			int j = asset.VertexCount;

			Vector2 n;
			for (i = 0; i < asset.VertexCount; i++)
			{
				(int px, int py) = asset.verts[i].position;
				n.X = (px + 0.5f) / asset.SkinWidth;
				n.Y = (py + 0.5f) / asset.SkinHeight;
				UVList.Add(n);
				if (asset.verts[i].onseam)
				{
					n.X = (px + 0.5f) / asset.SkinWidth + 0.5f;
					n.Y = (py + 0.5f) / asset.SkinHeight;
					UVList2.Add(n);
					backfaceMap[i] = j++;
				}
				else
					backfaceMap[i] = i;
			}
			UVList.AddRange(UVList2);

			i = 0;
			foreach (var tri in asset.tris)
			{
				if (tri.frontface)
				{
					indices[i++] = tri.indices.Item1;
					indices[i++] = tri.indices.Item2;
					indices[i++] = tri.indices.Item3;
				}
				else
				{
					indices[i++] = backfaceMap[tri.indices.Item1];
					indices[i++] = backfaceMap[tri.indices.Item2];
					indices[i++] = backfaceMap[tri.indices.Item3];
				}
			}
		}

		public static Model ConvertSkinMesh(ModelAsset asset)
		{
			var backfaceMap = new Dictionary<int, int>();   // remember what vertex index corresponds to what offseam UV index
			var UVList = new List<Vector2>();  // flat list of UV coordinates
			int[] indices = new int[asset.TriangleCount * 3];
			int i;

			BuildComponentList(asset, ref UVList, ref backfaceMap, ref indices);

			var positions = new List<Vector3>();

			for (i = 0; i < UVList.Count; i++)
			{
				positions.Add(new Vector3(UVList[i].X, UVList[i].Y, 0));
			}
			return new Model(indices, positions);
		}

		public static Model ConvertAnimatedMesh(ModelAsset asset)
		{
			var backfaceMap = new Dictionary<int, int>();   // remember what vertex index corresponds to what offseam UV index
			var UVList = new List<Vector2>();  // flat list of UV coordinates
			int[] indices = new int[asset.TriangleCount * 3];
			int i;

			BuildComponentList(asset, ref UVList, ref backfaceMap, ref indices);

			var positions = new List<Vector3>();
			var normals = new List<Vector3>();

			foreach (var frame in asset.frames)
			{
				var poslist = new List<Vector3>(Enumerable.Repeat(new Vector3(), UVList.Count));
				var normlist = new List<Vector3>(Enumerable.Repeat(new Vector3(), UVList.Count));
				for (i = 0; i < asset.VertexCount; i++)
				{
					poslist[i] = frame.positions[i].UncompressedOrigin(asset);
					normlist[i] = _normalTable[frame.positions[i].normalCompressed];
					if (backfaceMap[i] != i)
					{
						poslist[backfaceMap[i]] = poslist[i];
						normlist[backfaceMap[i]] = normlist[i];
					}
				}
				positions.AddRange(poslist);
				normals.AddRange(normlist);
			}

			return new Model(asset.TotalFrameCount, indices, UVList, positions, normals);
		}
	}
}
