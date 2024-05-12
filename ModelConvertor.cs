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
		public static Model Convert(ModelAsset asset, Shader shader)
		{
			var backfacemap = new Dictionary<int, int>();   // remember what vertex index corresponds to what offseam UV index
			var UVList = new List<Vector2>();  // flat list of UV coordinates
			var UVList2 = new List<Vector2>();  // onseam offsets to add at the end of the UV list

			int i;
			int j = asset.VertexCount;
			Vector2 n;
			for (i = 0; i < asset.VertexCount; i++)
			{
				n.X = (asset.verts[i].position.X + 0.5f) / asset.SkinWidth;
				n.Y = (asset.verts[i].position.Y + 0.5f) / asset.SkinHeight;
				UVList.Add(n);
				if (asset.verts[i].onseam)
				{
					n.X = (asset.verts[i].position.X + 0.5f) / asset.SkinWidth + 0.5f;
					n.Y = (asset.verts[i].position.Y + 0.5f) / asset.SkinHeight;
					UVList2.Add(n);
					backfacemap[i] = j++;
				}
				else
					backfacemap[i] = i;
			}
			UVList.AddRange(UVList2);
			
			int[] indices = new int[asset.TriangleCount * 3];
			j = 0;
			foreach(var tri in asset.tris)
			{
				if (tri.frontface)
				{
					indices[j++] = tri.indices.Item1;
					indices[j++] = tri.indices.Item2;
					indices[j++] = tri.indices.Item3;
				}
				else
				{
					indices[j++] = backfacemap[tri.indices.Item1];
					indices[j++] = backfacemap[tri.indices.Item2];
					indices[j++] = backfacemap[tri.indices.Item3];
				}
			}
			Debug.Assert(asset.TriangleCount * 3 == j);
			var positions = new List<Vector3>();
			var normals = new List<Vector3>();

			j = 0;
			foreach (var frame in asset.frames)
			{
				var poslist = new List<Vector3>(Enumerable.Repeat(new Vector3(), UVList.Count));
				var normlist = new List<Vector3>(Enumerable.Repeat(new Vector3(), UVList.Count));
				for (i = 0; i < asset.VertexCount; i++)
				{
					var orgc = frame.positions[i].origin_compressed;
					var normc = frame.positions[i].normal_compressed;
					poslist[i] = new Vector3(orgc.Item1 * asset.Scale[0] + asset.Origin[0],
											 orgc.Item2 * asset.Scale[1] + asset.Origin[1],
											 orgc.Item3 * asset.Scale[2] + asset.Origin[2]);
					normlist[i] = normaltable[normc];
					if (backfacemap[i] != i)
					{
						poslist[backfacemap[i]] = poslist[i];
						normlist[backfacemap[i]] = normlist[i];
					}
				}
				positions.AddRange(poslist);
				normals.AddRange(normlist);
			}

			return new Model(asset.FrameCount, indices, UVList, positions, normals, shader);
		}
	}
}
