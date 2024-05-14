using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using System.Runtime.Remoting;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace yaqmv
{
	internal class ModelAsset
	{
		internal ModelAsset(string mdlpath)
		{
			using (var mdlfile = new BinaryReader(File.OpenRead(mdlpath), Encoding.ASCII))
			{
				header = new Header(mdlfile);
				float group;

				// TODO test Enumerable.Repeat for stability on these
				skins = new Skin[header.skincount];
				for (int i = 0; i < header.skincount; i++)
				{
					group = mdlfile.ReadInt32();
					if (group != 0)
					{
						// TODO handle skingroup accordingly
					}
					skins[i] = new Skin(mdlfile, header);
				}

				verts = new Vertex[header.vertexcount];
				for (int i = 0; i < header.vertexcount; i++)
					verts[i] = new Vertex(mdlfile, header);

				tris = new Triangle[header.trianglecount];
				for (int i = 0; i < header.trianglecount; i++)
					tris[i] = new Triangle(mdlfile, header);

				frames = new Frame[header.framecount];
				for (int i = 0; i < header.framecount; i++)
				{
					group = mdlfile.ReadInt32();
					if (group != 0)
					{
						// TODO handle framegroup accordingly
					}
					frames[i] = new Frame(mdlfile, header);
				}
				Debug.Assert(mdlfile.BaseStream.Position == mdlfile.BaseStream.Length, "didn't read the entire file for some reason");
			}
		}

		internal struct Header
		{
			internal char[] ident;
			internal int version;
			internal Vector3 scale;
			internal Vector3 origin;
			internal float radius;
			internal Vector3 eyepos;
			internal int skincount;
			internal int skinwidth;
			internal int skinheight;
			internal int vertexcount;
			internal int trianglecount;
			internal int framecount;
			internal int synctype;
			internal int flags;
			internal float average_size;

			internal Header(BinaryReader mdl)
			{
				ident = mdl.ReadChars(4);
				version = mdl.ReadInt32();
				scale = new Vector3(mdl.ReadSingle(), mdl.ReadSingle(), mdl.ReadSingle());
				origin = new Vector3(mdl.ReadSingle(), mdl.ReadSingle(), mdl.ReadSingle());
				radius = mdl.ReadSingle();
				eyepos = new Vector3(mdl.ReadSingle(), mdl.ReadSingle(), mdl.ReadSingle());
				skincount = mdl.ReadInt32();
				skinwidth = mdl.ReadInt32();
				skinheight = mdl.ReadInt32();
				vertexcount = mdl.ReadInt32();
				trianglecount = mdl.ReadInt32();
				framecount = mdl.ReadInt32();
				synctype = mdl.ReadInt32();
				flags = mdl.ReadInt32();
				average_size = mdl.ReadSingle();

				if (!ident.SequenceEqual("IDPO"))
					throw new Exception("Ident does not match IDPO");

				if (version != 6)
					throw new Exception("mdl file is not version 6");

				if (skinwidth <= 0)
					throw new Exception("mdl file has invalid skinwidth");

				if (skinheight <= 0)
					throw new Exception("mdl file has invalid skinheight");
			}
		};
		internal class Skin
		{
			public byte[] indices;
			public byte[] pixels;
			public Skin(BinaryReader mdl, Header h)
			{
				indices = mdl.ReadBytes(h.skinwidth * h.skinheight);
				pixels = new byte[h.skinwidth * h.skinheight * 4];
				int j = 0;
				for (int i = 0; i < h.skinwidth * h.skinheight; i++)
				{
					var color = Palette.Color(indices[i]);
					pixels[j++] = color.Item1;
					pixels[j++] = color.Item2;
					pixels[j++] = color.Item3;
					pixels[j++] = color.Item4;
				}
			}
		}
		internal class Vertex
		{
			public Vector2 position;   // from file
			public bool onseam;
			public Vertex(BinaryReader mdl, Header h)
			{
				onseam = mdl.ReadInt32() > 0;
				position = new Vector2(mdl.ReadInt32(), mdl.ReadInt32());
			}
		}
		internal class Triangle
		{
			public (int, int, int) indices;
			public bool frontface;
			public Triangle(BinaryReader mdl, Header h)
			{
				frontface = mdl.ReadInt32() > 0;
				indices = (mdl.ReadInt32(), mdl.ReadInt32(), mdl.ReadInt32());
			}
		}
		internal class Coord
		{
			public (byte, byte, byte) origin_compressed;
			public byte normal_compressed;
			public Coord(BinaryReader mdl)
			{ 
				origin_compressed = (mdl.ReadByte(), mdl.ReadByte(), mdl.ReadByte());
				normal_compressed = mdl.ReadByte();
			}
			public Vector3 UncompressedOrigin(ModelAsset mdl)
			{
				return new Vector3(origin_compressed.Item1 * mdl.Scale[0] + mdl.Origin[0],
					origin_compressed.Item2 * mdl.Scale[1] + mdl.Origin[1],
					origin_compressed.Item3 * mdl.Scale[2] + mdl.Origin[2]);
			}
		}
		internal class Frame
		{
			public Coord mins, maxs;
			public string name;
			public Coord[] positions;

			public Frame(BinaryReader mdl, Header h)
			{
				mins = new Coord(mdl);
				maxs = new Coord(mdl);
				name = new string(mdl.ReadChars(16));
				name = name.Substring(0, name.IndexOf('\0'));
				positions = new Coord[h.vertexcount];
				for (int i = 0; i < h.vertexcount; i++)
				{
					positions[i] = new Coord(mdl);
				}
			}
		}

		Header header;
		internal Skin[] skins;
		internal Vertex[] verts;
		internal Triangle[] tris;
		internal Frame[] frames;

		internal int SkinCount { get { return header.skincount; } }
		internal int SkinWidth { get { return header.skinwidth; } }
		internal int SkinHeight { get { return header.skinheight; } }
		internal int VertexCount { get { return header.vertexcount; } }
		internal int TriangleCount { get { return header.trianglecount; } }
		internal int FrameCount { get { return header.framecount; } }
		internal Vector3 Scale { get { return header.scale; } }
		internal Vector3 Origin { get { return header.origin; } }

		internal Vector3 CenterOfFrame(int frame)
		{
			Vector3 mins = frames[frame].mins.UncompressedOrigin(this);
			Vector3 maxs = frames[frame].maxs.UncompressedOrigin(this);
			return (mins + maxs) * 0.5f;
		}
		internal float RadiusOfFrame(int frame)
		{
			Vector3 mins = frames[frame].mins.UncompressedOrigin(this);
			Vector3 maxs = frames[frame].maxs.UncompressedOrigin(this);
			return (maxs - mins).Length;
		}
	}
}
