using OpenTK.Mathematics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace yaqmv
{
	public class ModelAsset : INotifyPropertyChanged
	{
		internal string filePath;

		Header header;
		internal List<Skin> skins;
		internal Vertex[] verts;
		internal Triangle[] tris;
		internal Frame[] frames;
		internal Anim[] anims;
		internal ObservableCollection<string> AnimNames = [];

		internal int SkinCount { get { return header.skincount; } }
		internal int SkinWidth { get { return header.skinwidth; } }
		internal int SkinHeight { get { return header.skinheight; } }
		internal int VertexCount { get { return header.vertexcount; } }
		internal int TriangleCount { get { return header.trianglecount; } }
		internal int FrameCount { get { return header.framecount; } }
		internal int TotalFrameCount { get; }
		internal Vector3 Scale { get { return header.scale; } }
		internal Vector3 Origin { get { return header.origin; } }

		public class SkinLayoutChangedArgs
		{
			public int Skin { get; set; }
			public int Frame { get; set; }
		}

		public event EventHandler<SkinLayoutChangedArgs> SkinLayoutChanged;
		private void NotifySkinLayoutChanged(int skin, int frame)
		{
			SkinLayoutChanged?.Invoke(this, new SkinLayoutChangedArgs() { Skin = skin, Frame = frame });
		}

		private bool _isLoaded;
		public bool IsLoaded { get => _isLoaded;
			private set {
				if (_isLoaded == value) return; 
				_isLoaded = value;
				NotifyPropertyChanged(nameof(IsLoaded));
			} 
		}
		private bool _isModified;
		public bool IsModified
		{
			get => _isModified;
			set
			{
				if (_isModified == value) return;
				_isModified = value;
				NotifyPropertyChanged(nameof(IsModified));
			}
		}

		public ModelAsset()
		{
			filePath = "";
			header = new Header();
			skins = [];
			verts = [];
			tris = [];
			frames = [new Frame()];
			anims = [new Anim()];
			TotalFrameCount = 0;

			AnimNames.Clear();
			NotifySkinLayoutChanged(0,0);

			IsLoaded = false;
			IsModified = false;
		}

		internal ModelAsset(string mdlpath)
		{
			filePath = mdlpath;

			using (var mdlfile = new BinaryReader(File.OpenRead(mdlpath), Encoding.ASCII))
			{
				float group;
				int i, s;

				header = new Header(mdlfile);

				Debug.WriteLine("READING");
				Debug.WriteLine("skins: " + mdlfile.BaseStream.Position);
				skins = [];
				for (i = 0; i < header.skincount; i++)
				{
					group = mdlfile.ReadInt32();
					if (group != 0)
					{
						int skinFrameCount = mdlfile.ReadInt32();
						skins.Add(new Skin());
						for (s = 0; s < skinFrameCount; s++)
						{
							skins[i].durations.Add(mdlfile.ReadSingle());

							// should we wish to convert the 'durations' listed in the file to actual 
							// durations - in the mdl spec they're actually the total time at which
							// that frame ends relative to start
							//if (s > 0)
							//	skins[i].durations[s] -= skins[i].durations[s - 1];
							// but they're also misintepreted and ignored by every version of quake
						}
						for (s = 0; s < skinFrameCount; s++)
						{
							skins[i].images.Add(new Image(mdlfile, header));
						}
					}
					else
						skins.Add(new Skin(new Image(mdlfile, header)));
				}

				Debug.WriteLine("verts: " + mdlfile.BaseStream.Position);
				verts = new Vertex[header.vertexcount];
				for (i = 0; i < header.vertexcount; i++)
					verts[i] = new Vertex(mdlfile, header);

				Debug.WriteLine("tris: " + mdlfile.BaseStream.Position);
				tris = new Triangle[header.trianglecount];
				for (i = 0; i < header.trianglecount; i++)
					tris[i] = new Triangle(mdlfile, header);

				Debug.WriteLine("frames: " + mdlfile.BaseStream.Position);
				List<Frame> framelist = new(header.framecount);
				List<Anim> animlist = [];
				for (i = 0; i < header.framecount; i++)
				{
					group = mdlfile.ReadInt32();
					if (group != 0)
					{
						Debug.WriteLine("\tframegroup: " + mdlfile.BaseStream.Position);
						animlist.Add(new Anim("_temp_framegroup", framelist.Count, true, framelist.Count + mdlfile.ReadInt32()-1));
						var anim = animlist.Last();

						anim.mins = new Coord(mdlfile);
						anim.maxs = new Coord(mdlfile);

						List<float> durations = new(anim.frameCount);
						for (int j = 0; j < anim.frameCount; j++)
							durations.Add(mdlfile.ReadSingle());
						for (int j = 0; j < anim.frameCount; j++)
							framelist.Add(new Frame(mdlfile, header));

						anim.name = framelist.Last().NamePrefix;
						anim.SetDurations(durations);
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

				frames = [.. framelist]; //framelist.ToArray();
				anims = [.. animlist]; //animlist.ToArray();
				TotalFrameCount = framelist.Count;

				AnimNames.Clear();
				foreach (var anim in anims)
					AnimNames.Add(anim.name);
				NotifySkinLayoutChanged(0,0);

				Debug.WriteLine("done reading at: " + mdlfile.BaseStream.Position);
				Debug.Assert(mdlfile.BaseStream.Position == mdlfile.BaseStream.Length, "didn't read the entire file for some reason");

			}
			IsLoaded = true;
		}

		internal void Write()
		{
			Write(filePath);
		}

		internal void Write(string mdlpath)
		{
			using (var mdlOut = new BinaryWriter(File.OpenWrite(mdlpath), Encoding.ASCII))
			{
				header.Write(mdlOut);

				Debug.WriteLine("WRITING");
				Debug.WriteLine("skins: " + mdlOut.BaseStream.Position);

				Debug.Assert(skins.Count == header.skincount, "Skin count mismatch!");
				foreach (Skin sk in skins)
				{
					if (sk.images.Count > 1)
					{
						mdlOut.Write(1);   // needs to be four bytes, 'true' would be 1
						mdlOut.Write(sk.images.Count);
						foreach (float f in sk.durations)
						{
							mdlOut.Write(f);
						}
					}
					else
					{
						mdlOut.Write(0);
					}
					foreach (Image i in sk.images)
					{
						i.Write(mdlOut);
					}
				}

				Debug.WriteLine("verts: " + mdlOut.BaseStream.Position);
				Debug.Assert(verts.Length == header.vertexcount, "Vertex count mismatch!");
				foreach (Vertex v in verts)
				{
					v.Write(mdlOut);
				}

				Debug.WriteLine("tris: " + mdlOut.BaseStream.Position);
				Debug.Assert(tris.Length == header.trianglecount, "Triangle count mismatch!");
				foreach (Triangle t in tris)
				{
					t.Write(mdlOut);
				}

				Debug.WriteLine("frames: " + mdlOut.BaseStream.Position);
				foreach (Anim a in anims)
				{
					a.Write(mdlOut);
					for (int i = a.first; i <= a.last; ++i)
					{
						if (!a.isGroup)
							mdlOut.Write(0);
						frames[i].Write(mdlOut);
					}
				}

				Debug.WriteLine("done writing at: " + mdlOut.BaseStream.Position);
			}
		}

		internal void ReplaceImage(Bitmap bmp, int skin = 0, int frame = 0)
		{
			Debug.Assert(skin < skins.Count);
			Debug.Assert(frame < skins[skin].images.Count);
			Image img = new(bmp);
			skins[skin].images[frame] = img;

			IsModified = true;
		}

		internal void AddSkin(Bitmap bmp)
		{
			Image img = new(bmp);
			skins.Add(new Skin(img));
			header.skincount++;
			NotifySkinLayoutChanged(skins.Count - 1, 0);

			IsModified = true;
		}
		internal void DeleteSkin(int skin = 0)
		{
			Debug.Assert(skins.Count > 1);
			Debug.Assert(skin < skins.Count);
			skins[skin].Dispose();
			skins.RemoveAt(skin);
			header.skincount--;
			NotifySkinLayoutChanged(skin, 0);

			IsModified = true;
		}

		internal void AddSkinframe(Bitmap bmp, int skin = 0)
		{
			Debug.Assert(skin < skins.Count);
			skins[skin].images.Add(new Image(bmp));

			// no quake port respects this value, including the originals, but we should specify it somehow ...
			skins[skin].durations.Add(0.1f);
			NotifySkinLayoutChanged(skin, skins[skin].images.Count - 1);

			IsModified = true;
		}
		internal void DeleteSkinframe(int skin = 0, int frame = 0)
		{
			Debug.Assert(skin < skins.Count);

			Debug.Assert(skins[skin].images.Count > 1);
			Debug.Assert(frame < skins[skin].images.Count);

			skins[skin].images[frame].Dispose();
			skins[skin].images.RemoveAt(frame);
			skins[skin].durations.RemoveAt(frame);
			NotifySkinLayoutChanged(skin, frame);

			IsModified = true;
		}

		public Bitmap ExportSkinImage(int skin = 0, int frame = 0)
		{
			Debug.Assert(skin < skins.Count);
			Debug.Assert(frame < skins[skin].images.Count);
			// the last parameter expects full color input to match to the palette
			// itself, and if we give it that we run afoul of quake's duplicated
			// palette indices and write a skin that isn't 1:1 what's in the .mdl
			//Bitmap bmp = new Bitmap(SkinWidth, SkinHeight, SkinWidth, PixelFormat.Format8bppIndexed, skins[skin].images[frame].indices[0]);
			Bitmap bmp = new(SkinWidth, SkinHeight, PixelFormat.Format8bppIndexed);
			ColorPalette pal = bmp.Palette;
			for (int i = 0; i < 256; i++)
			{
				pal.Entries[i] = Palette.ColorSystem(i);
			}
			bmp.Palette = pal;

			skins[skin].images[frame].Export(ref bmp);
			return bmp;
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
				ident = [ 'l','o','l','.' ];
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
			public void Write(BinaryWriter mdlOut)
			{
				if (!ident.SequenceEqual("IDPO"))
					throw new Exception("Ident does not match IDPO");

				mdlOut.Write(ident);
				mdlOut.Write(version);

				mdlOut.Write(scale[0]);
				mdlOut.Write(scale[1]);
				mdlOut.Write(scale[2]);

				mdlOut.Write(origin[0]);
				mdlOut.Write(origin[1]);
				mdlOut.Write(origin[2]);

				mdlOut.Write(radius);

				mdlOut.Write(eyepos[0]);
				mdlOut.Write(eyepos[1]);
				mdlOut.Write(eyepos[2]);

				mdlOut.Write(skincount);
				mdlOut.Write(skinwidth);
				mdlOut.Write(skinheight);

				mdlOut.Write(vertexcount);
				mdlOut.Write(trianglecount);
				mdlOut.Write(framecount);
				mdlOut.Write(synctype);
				mdlOut.Write(flags);
				mdlOut.Write(average_size);
			}
		};

		internal class Skin : IDisposable
		{
			public List<Image> images;
			public List<float> durations;
			public Skin(Image i)
			{
				images = [i];
				durations = [0.1f];
			}
			public Skin()
			{
				images = [];
				durations = [];
			}
			private bool _disposed;
			~Skin() { if (!_disposed) Debug.WriteLine("Undisposed skin"); }
			public void Dispose()
			{
				if (_disposed) return;
				foreach (Image img in images) img.Dispose();
				GC.SuppressFinalize(this);
				_disposed = true;
			}
		}

		internal class Image : IDisposable
		{
			public Texture Tex;
			internal byte[] indices;

			public Image(BinaryReader mdl, Header h)
			{
				indices = mdl.ReadBytes(h.skinwidth * h.skinheight);
				GenTexture(h.skinwidth, h.skinheight);
			}

			public Image(Bitmap bmp)
			{
				int size = bmp.Width * bmp.Height;
				byte[] bytedata = new byte[size];

				BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
				Marshal.Copy(data.Scan0, bytedata, 0, size);
				bmp.UnlockBits(data);

				indices = bytedata;
				GenTexture(bmp.Width, bmp.Height);
			}

			public void Export(ref Bitmap bmp)
			{
				BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
				Marshal.Copy(indices, 0, data.Scan0, bmp.Width * bmp.Height);
				bmp.UnlockBits(data);
			}

			public void Write(BinaryWriter mdlOut)
			{
				mdlOut.Write(indices);
			}

			private void GenTexture(int w, int h)
			{
				byte[] pixels;
				pixels = new byte[w * h * 4];
				int j = 0;
				for (int i = 0; i < w * h; i++)
				{
					var color = Palette.Color(indices[i]);
					pixels[j++] = color.Item1;
					pixels[j++] = color.Item2;
					pixels[j++] = color.Item3;
					pixels[j++] = color.Item4;
				}
				Tex = new Texture(w, h, pixels);
			}

			private bool _disposed;
			~Image() { if (!_disposed) Debug.WriteLine("Undisposed image"); }
			public void Dispose()
			{
				if (_disposed) return;
				Tex.Dispose();
				GC.SuppressFinalize(this);
				_disposed = true;
			}
		}
		internal class Vertex
		{
			public (int,int) position;   // from file
			public bool onseam;
			public Vertex(BinaryReader mdl, Header h)
			{
				onseam = mdl.ReadInt32() > 0;
				position = (mdl.ReadInt32(), mdl.ReadInt32());
			}
			public void Write(BinaryWriter mdlOut)
			{
				mdlOut.Write(onseam ? 1 : 0);
				mdlOut.Write(position.Item1);
				mdlOut.Write(position.Item2);
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
			public void Write(BinaryWriter mdlOut)
			{
				mdlOut.Write(frontface ? 1 : 0);
				mdlOut.Write(indices.Item1);
				mdlOut.Write(indices.Item2);
				mdlOut.Write(indices.Item3);
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
			public void Write(BinaryWriter mdlOut)
			{
				mdlOut.Write(originCompressed.Item1);
				mdlOut.Write(originCompressed.Item2);
				mdlOut.Write(originCompressed.Item3);
				mdlOut.Write(normalCompressed);
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
			public float[] durations;	// groups only

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
				mins = new Coord();
				maxs = new Coord();
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
			public void SetDurations(List<float> dur)
			{
				durations = [.. dur]; //dur.ToArray();
			}

			public void Write(BinaryWriter mdlOut)
			{
				if (isGroup)
				{
					mdlOut.Write(1);	// 4 byte bool
					mdlOut.Write(frameCount);
					mins.Write(mdlOut);
					maxs.Write(mdlOut);
					foreach (float f in durations)
						mdlOut.Write(f);
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
				positions = [];
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
			public void Write(BinaryWriter mdlOut)
			{
				mins.Write(mdlOut);
				maxs.Write(mdlOut);
				mdlOut.Write(Encoding.ASCII.GetBytes(name));
				for (int i = 16; i > name.Length; --i)
				{
					byte zero = 0;
					mdlOut.Write(zero);
				}
				foreach (var p in positions)
				{
					p.Write(mdlOut);
				}
			}
			public string NamePrefix { get { return name.TrimEnd(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9' ]); } }
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








		public event PropertyChangedEventHandler? PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
