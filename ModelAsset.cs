using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Xaml;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace yaqmv
{
	internal class ModelAsset
	{
		Header header;
		internal Skin[] skins;
		internal Vertex[] verts;
		internal Triangle[] tris;
		internal Frame[] frames;
		internal Anim[] anims;
		internal ObservableCollection<string> AnimNames = new ObservableCollection<string>();
		internal ObservableCollection<string> SkinNames = new ObservableCollection<string>();

		internal int SkinCount { get { return header.skincount; } }
		internal int SkinWidth { get { return header.skinwidth; } }
		internal int SkinHeight { get { return header.skinheight; } }
		internal int VertexCount { get { return header.vertexcount; } }
		internal int TriangleCount { get { return header.trianglecount; } }
		internal int FrameCount { get { return header.framecount; } }
		private int TotalFrameCount { get; }
		internal Vector3 Scale { get { return header.scale; } }
		internal Vector3 Origin { get { return header.origin; } }

		public ModelAsset()
		{
			header = new Header();
			skins = Array.Empty<Skin>();
			verts = Array.Empty<Vertex>();
			tris = Array.Empty<Triangle>();
			frames = [new Frame()];
			anims = [new Anim()];
			TotalFrameCount = 0;

			AnimNames.Clear();
			SkinNames.Clear();
		}

		internal ModelAsset(string mdlpath)
		{
			using (var mdlfile = new BinaryReader(File.OpenRead(mdlpath), Encoding.ASCII))
			{
				float group;
				int i;

				header = new Header(mdlfile);

				// TODO test Enumerable.Repeat for stability on these
				skins = new Skin[header.skincount];
				for (i = 0; i < header.skincount; i++)
				{
					group = mdlfile.ReadInt32();
					if (group != 0)
					{
						// TODO handle skingroup accordingly
					}
					skins[i] = new Skin(mdlfile, header);
				}

				verts = new Vertex[header.vertexcount];
				for (i = 0; i < header.vertexcount; i++)
					verts[i] = new Vertex(mdlfile, header);

				tris = new Triangle[header.trianglecount];
				for (i = 0; i < header.trianglecount; i++)
					tris[i] = new Triangle(mdlfile, header);

				List<Frame> framelist = new List<Frame>(header.framecount);
				List<Anim> animlist = new List<Anim>();
				for (i = 0; i < header.framecount; i++)
				{
					group = mdlfile.ReadInt32();
					if (group != 0)
					{
						animlist.Add(new Anim("_temp_framegroup", framelist.Count, true, framelist.Count + mdlfile.ReadInt32()-1));
						var anim = animlist.Last();

						anim.mins = new Coord(mdlfile);
						anim.maxs = new Coord(mdlfile);

						List<float> durations = new List<float>(anim.frameCount);
						for (int j = 0; j < anim.frameCount; j++)
							durations.Add(mdlfile.ReadSingle());
						for (int j = 0; j < anim.frameCount; j++)
							framelist.Add(new Frame(mdlfile, header));

						anim.name = framelist.Last().NamePrefix;
					}
					else
					{
						framelist.Add(new Frame(mdlfile, header));
						if (animlist.Count > 0 && framelist.Last().NamePrefix == animlist.Last().name)
						{
							animlist.Last().frameCount++;
							animlist.Last().last++;
						}
						else
						{
							animlist.Add(new Anim(framelist.Last().NamePrefix, framelist.Count-1));
						}
					}

				}
				frames = framelist.ToArray();
				anims = animlist.ToArray();
				TotalFrameCount = framelist.Count;

				AnimNames.Clear();
				foreach (var anim in anims)
					AnimNames.Add(anim.name);

				SkinNames.Clear();
				i = 0;
				foreach (var skin in skins)
					SkinNames.Add("Skin " + (i++).ToString());

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
			public Header()
			{
				ident = new char[]{ 'l','o','l','.' };
				version = 0;
				scale = new Vector3(0);
				origin = new Vector3(0);
				radius = 0;
				eyepos = new Vector3(0);
				skincount = 0;
				skinwidth = 0;
				skinheight = 0;
				vertexcount = 0;
				trianglecount = 0;
				framecount = 0;
				synctype = 0;
				flags = 0;
				average_size = 0;
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
			public (byte, byte, byte) originCompressed;
			public byte normalCompressed;
			public Coord()
			{
				originCompressed = (0, 0, 0);
				normalCompressed = 0;
			}
			public Coord(BinaryReader mdl)
			{ 
				originCompressed = (mdl.ReadByte(), mdl.ReadByte(), mdl.ReadByte());
				normalCompressed = mdl.ReadByte();
			}
			public Vector3 UncompressedOrigin(ModelAsset mdl)
			{
				return new Vector3(originCompressed.Item1 * mdl.Scale[0] + mdl.Origin[0],
					originCompressed.Item2 * mdl.Scale[1] + mdl.Origin[1],
					originCompressed.Item3 * mdl.Scale[2] + mdl.Origin[2]);
			}
		}
		internal class Anim
		{
			// anims are either actual framegroups or just frames named
			// with the same prefix that we spiritually group together
			public string name;
			public Coord mins, maxs;
			public int frameCount;
			public int first, last;
			public bool isGroup;

			public Anim()
			{
				name = "";
				mins = new Coord();
				maxs = new Coord();
				frameCount = 0;
				first = last = 0;
				isGroup = false;
			}
			public Anim(string prefix, int startframe, bool grp = false, int endframe = -1)
			{
				name = prefix;
				first = startframe;
				isGroup = grp;
				if (grp)
				{
					last = endframe;
					frameCount = endframe - startframe + 1;
				}
				else
				{
					last = first;
					frameCount = 1;
				}
			}
		}

		internal class Frame
		{
			public Coord mins, maxs;
			public string name;
			public Coord[] positions;
			public float duration;

			public Frame()
			{
				mins = new Coord();
				maxs = new Coord();
				name = "";
				positions = new Coord[] { };
			}
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
			public string NamePrefix { get { return name.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }); } }
		}

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
