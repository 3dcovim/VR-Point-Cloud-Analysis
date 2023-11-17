// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx
#define IMPORTER_EXTENDED
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pcx
{
    [ScriptedImporter(1, "ply")]
    class PlyImporter : ScriptedImporter
    {
        #region ScriptedImporter implementation

        public enum ContainerType { Mesh, ComputeBuffer, Texture }

        [SerializeField] ContainerType _containerType = ContainerType.ComputeBuffer;

        private bool importNormals = false;

        public override void OnImportAsset(AssetImportContext context)
        {

                // ComputeBuffer container
                // Create a prefab with PointCloudRenderer.
                var gameObject = new GameObject();
                var data = ImportAsPointCloudData(context.assetPath);



                var renderer = gameObject.AddComponent<PointCloudRenderer>();
                renderer.sourceData = data;

#if IMPORTER_EXTENDED
                var data_extended = ImportAsPointCloudDataExtended(context.assetPath);
                renderer.sourceDataExt = data_extended;
                renderer.sourceDataProps = data_extended.propsCount;
                var pointCloudScript = gameObject.AddComponent<PointCloud>();
#endif

                context.AddObjectToAsset("prefab", gameObject);
                if (data != null) context.AddObjectToAsset("data", data);
                if (data_extended != null) context.AddObjectToAsset("data_extended", data_extended);


                context.SetMainObject(gameObject);
        }

        #endregion

        #region Internal utilities

        static Material GetDefaultMaterial()
        {
            // Via package manager
            var path_upm = "Packages/jp.keijiro.pcx/Editor/Default Point.mat";
            // Via project asset database
            var path_prj = "Assets/Pcx/Editor/Default Point.mat";
            return AssetDatabase.LoadAssetAtPath<Material>(path_upm) ??
                   AssetDatabase.LoadAssetAtPath<Material>(path_prj);
        }

        #endregion

        #region Internal data structure

        enum DataProperty
        {
            Invalid,
            R8, G8, B8, A8,
            R16, G16, B16, A16,
            SingleX, SingleY, SingleZ,
            DoubleX, DoubleY, DoubleZ,
            Data8, Data16, Data32, Data64
        }

        static int GetPropertySize(DataProperty p)
        {
            switch (p)
            {
                case DataProperty.R8: return 1;
                case DataProperty.G8: return 1;
                case DataProperty.B8: return 1;
                case DataProperty.A8: return 1;
                case DataProperty.R16: return 2;
                case DataProperty.G16: return 2;
                case DataProperty.B16: return 2;
                case DataProperty.A16: return 2;
                case DataProperty.SingleX: return 4;
                case DataProperty.SingleY: return 4;
                case DataProperty.SingleZ: return 4;
                case DataProperty.DoubleX: return 8;
                case DataProperty.DoubleY: return 8;
                case DataProperty.DoubleZ: return 8;
                case DataProperty.Data8: return 1;
                case DataProperty.Data16: return 2;
                case DataProperty.Data32: return 4;
                case DataProperty.Data64: return 8;
            }
            return 0;
        }

        class DataHeader
        {
            public List<DataProperty> properties = new List<DataProperty>();
            public int vertexCount = -1;
            public int propertyCount = 0;   //AGO23
        }

        class DataBody
        {
            public List<Vector3> vertices;
            public List<Color32> colors;

            public DataBody(int vertexCount)
            {
                vertices = new List<Vector3>(vertexCount);
                colors = new List<Color32>(vertexCount);
            }

            public void AddPoint(
                float x, float y, float z,
                byte r, byte g, byte b, byte a
            )
            {
                vertices.Add(new Vector3(x, y, z));
                colors.Add(new Color32(r, g, b, a));
            }
        }


#if IMPORTER_EXTENDED
        enum DataPropertyExtended
        {
            Invalid,
            R8, G8, B8, A8,
            R16, G16, B16, A16,
            SingleX, SingleY, SingleZ,
            DoubleX, DoubleY, DoubleZ,
            Data8, Data16, Data32, Data64,
            SingleNx, SingleNy, SingleNz,
            SingleP //AGO23
            //           Singlesi,Singlest,Singlesc,Singlescv,Singlesr,Singless, SingleP  //AGO23
        }


        static int GetPropertySizeExtended(DataPropertyExtended p)
        {
            switch (p)
            {
                case DataPropertyExtended.R8: return 1;
                case DataPropertyExtended.G8: return 1;
                case DataPropertyExtended.B8: return 1;
                case DataPropertyExtended.A8: return 1;
                case DataPropertyExtended.R16: return 2;
                case DataPropertyExtended.G16: return 2;
                case DataPropertyExtended.B16: return 2;
                case DataPropertyExtended.A16: return 2;
                case DataPropertyExtended.SingleX: return 4;
                case DataPropertyExtended.SingleY: return 4;
                case DataPropertyExtended.SingleZ: return 4;
                case DataPropertyExtended.DoubleX: return 8;
                case DataPropertyExtended.DoubleY: return 8;
                case DataPropertyExtended.DoubleZ: return 8;
                case DataPropertyExtended.Data8: return 1;
                case DataPropertyExtended.Data16: return 2;
                case DataPropertyExtended.Data32: return 4;
                case DataPropertyExtended.Data64: return 8;
                case DataPropertyExtended.SingleNx: return 4;
                case DataPropertyExtended.SingleNy: return 4;
                case DataPropertyExtended.SingleNz: return 4;
                //               case DataPropertyExtended.Singlesi: return 4; //// AGO23
                //               case DataPropertyExtended.Singlest: return 4; //// AGO23
                //               case DataPropertyExtended.Singlesc: return 4; //// AGO23
                //               case DataPropertyExtended.Singlescv: return 4; //// AGO23
                //               case DataPropertyExtended.Singlesr: return 4; //// AGO23
                //               case DataPropertyExtended.Singless: return 4;//// AGO23
                case DataPropertyExtended.SingleP: return 4;  //// AGO23

            }
            return 0;
        }




        class DataHeaderExtended
        {
            public List<DataPropertyExtended> properties = new List<DataPropertyExtended>();
            public int vertexCount = -1;
            public int propertyCount = 0;  //AGO23
        }

        class DataBodyExtended
        {
            public List<Vector3> vertices_ext;
            public List<Color32> colors_ext;
            public List<Vector3> normals;
            //            public List<float> scalar_intensities; //AGO23
            //            public List<float> scalar_times; //AGO23
            //            public List<float> scalar_confidences; //AGO23
            //            public List<float> scalar_curvatures; //AGO23
            //            public List<float> scalar_ranges; //AGO23
            //            public List<float> scalar_sorStdDevs; //AGO23
            // public List<float> property_value;//AGO23
            public List<List<float>> property_s;//AGO23




            public DataBodyExtended(int vertexCount, int propertyCount) //AGO23
            {
                vertices_ext = new List<Vector3>(vertexCount);
                colors_ext = new List<Color32>(vertexCount);
                normals = new List<Vector3>(vertexCount);
                //                scalar_intensities = new List<float>(vertexCount); //AGO23
                //                scalar_times = new List<float>(vertexCount); //AGO23
                //                scalar_confidences = new List<float>(vertexCount); //AGO23
                //                scalar_curvatures = new List<float>(vertexCount); //AGO23
                //                scalar_ranges = new List<float>(vertexCount); //AGO23
                //                scalar_sorStdDevs = new List<float>(vertexCount); //AGO23
                property_s = new List<List<float>>(propertyCount);//AGO23
                for (int indexal = 0; indexal < propertyCount; indexal++) //AGO23
                {//AGO23
                    property_s.Add(new List<float>(vertexCount));//AGO23

                }//AGO23

                //  numbers[0].Add(2);  // Add the integer '2' to the List<int> at index '0' of numbers.

            }

            public void AddPointExtended(
                float x, float y, float z,
                byte r, byte g, byte b, byte a,
                float nx, float ny, float nz,
                //                float si, float st, float sc, //AGO23
                //                float scv, float sr, float ss, //AGO23
                System.Single[] prop_package //AGO23
            )
            {
                vertices_ext.Add(new Vector3(x, y, z));
                colors_ext.Add(new Color32(r, g, b, a));
                normals.Add(new Vector3(nx, ny, nz));
                //                scalar_intensities.Add(si); //AGO23
                //                scalar_times.Add(st); //AGO23
                //                scalar_confidences.Add(sc); //AGO23
                //                scalar_curvatures.Add(scv); //AGO23
                //                scalar_ranges.Add(sr); //AGO23
                //                scalar_sorStdDevs.Add(ss); //AGO23
                for (int indexal = 0; indexal < prop_package.Length; indexal++) //AGO23
                {//AGO23
                    property_s[indexal].Add(prop_package[indexal]);//AGO23

                }//AGO23



            }
        }
#endif

        #endregion

        #region Reader implementation

        Mesh ImportAsMesh(string path)
        {
            try
            {
                var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var header = ReadDataHeader(new StreamReader(stream));
                var body = ReadDataBody(header, new BinaryReader(stream));

                var mesh = new Mesh();
                mesh.name = Path.GetFileNameWithoutExtension(path);

                mesh.indexFormat = header.vertexCount > 65535 ?
                    IndexFormat.UInt32 : IndexFormat.UInt16;

                mesh.SetVertices(body.vertices);
                mesh.SetColors(body.colors);

                mesh.SetIndices(
                    Enumerable.Range(0, header.vertexCount).ToArray(),
                    MeshTopology.Points, 0
                );

                mesh.UploadMeshData(true);
                return mesh;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }



        PointCloudData ImportAsPointCloudData(string path)
        {
            try
            {
                var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var header = ReadDataHeader(new StreamReader(stream));
                var body = ReadDataBody(header, new BinaryReader(stream));
                var data = ScriptableObject.CreateInstance<PointCloudData>();
                data.Initialize(body.vertices, body.colors);
                data.name = Path.GetFileNameWithoutExtension(path);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }




#if IMPORTER_EXTENDED
        PointCloudDataExtended ImportAsPointCloudDataExtended(string path)
        {
            try
            {
                var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var header = ReadDataHeaderExtended(new StreamReader(stream));
                var body = ReadDataBodyExtended(header, new BinaryReader(stream));
                var data = ScriptableObject.CreateInstance<PointCloudDataExtended>();
                data.propsCount = body.property_s.Count;
                data.importNormal = importNormals;
                //           data.Initialize(body.vertices_ext, body.colors_ext, body.normals,body.scalar_intensities,body.scalar_times,body.scalar_confidences,body.scalar_curvatures, body.scalar_ranges,body.scalar_sorStdDevs); //AGO23
                data.Initialize(body.vertices_ext, body.colors_ext, body.normals, body.property_s);//AGO23

                data.name = Path.GetFileNameWithoutExtension(path);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }
#endif

        BakedPointCloud ImportAsBakedPointCloud(string path)
        {
            try
            {
                var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var header = ReadDataHeader(new StreamReader(stream));
                var body = ReadDataBody(header, new BinaryReader(stream));
                var data = ScriptableObject.CreateInstance<BakedPointCloud>();
                data.Initialize(body.vertices, body.colors);
                data.name = Path.GetFileNameWithoutExtension(path);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }



        DataHeader ReadDataHeader(StreamReader reader)
        {
            var data = new DataHeader();
            var readCount = 0;

            // Magic number line ("ply")
            var line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "ply")
                throw new ArgumentException("Magic number ('ply') mismatch.");

            // Data format: check if it's binary/little endian.
            line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "format binary_little_endian 1.0")
                throw new ArgumentException(
                    "Invalid data format ('" + line + "'). " +
                    "Should be binary/little endian.");

            // Read header contents.
            for (var skip = false; ;)
            {
                // Read a line and split it with white space.
                line = reader.ReadLine();
                readCount += line.Length + 1;
                if (line == "end_header") break;
                var col = line.Split();

                // Element declaration (unskippable)
                if (col[0] == "element")
                {
                    if (col[1] == "vertex")
                    {
                        data.vertexCount = Convert.ToInt32(col[2]);
                        skip = false;
                    }
                    else
                    {
                        // Don't read elements other than vertices.
                        skip = true;
                    }
                }

                if (skip) continue;

                // Property declaration line
                if (col[0] == "property")
                {
                    var prop = DataProperty.Invalid;

                    // Parse the property name entry.
                    switch (col[2])
                    {
                        case "red": prop = DataProperty.R8; break;
                        case "green": prop = DataProperty.G8; break;
                        case "blue": prop = DataProperty.B8; break;
                        case "alpha": prop = DataProperty.A8; break;
                        case "x": prop = DataProperty.SingleX; break;
                        case "y": prop = DataProperty.SingleY; break;
                        case "z": prop = DataProperty.SingleZ; break;
                    }

                    // Check the property type.
                    if (col[1] == "char" || col[1] == "uchar" ||
                        col[1] == "int8" || col[1] == "uint8")
                    {
                        if (prop == DataProperty.Invalid)
                            prop = DataProperty.Data8;
                        else if (GetPropertySize(prop) != 1)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "short" || col[1] == "ushort" ||
                             col[1] == "int16" || col[1] == "uint16")
                    {
                        switch (prop)
                        {
                            case DataProperty.Invalid: prop = DataProperty.Data16; break;
                            case DataProperty.R8: prop = DataProperty.R16; break;
                            case DataProperty.G8: prop = DataProperty.G16; break;
                            case DataProperty.B8: prop = DataProperty.B16; break;
                            case DataProperty.A8: prop = DataProperty.A16; break;
                        }
                        if (GetPropertySize(prop) != 2)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "int" || col[1] == "uint" || col[1] == "float" ||
                             col[1] == "int32" || col[1] == "uint32" || col[1] == "float32")
                    {
                        if (prop == DataProperty.Invalid)
                            prop = DataProperty.Data32;
                        else if (GetPropertySize(prop) != 4)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "int64" || col[1] == "uint64" ||
                             col[1] == "double" || col[1] == "float64")
                    {
                        switch (prop)
                        {
                            case DataProperty.Invalid: prop = DataProperty.Data64; break;
                            case DataProperty.SingleX: prop = DataProperty.DoubleX; break;
                            case DataProperty.SingleY: prop = DataProperty.DoubleY; break;
                            case DataProperty.SingleZ: prop = DataProperty.DoubleZ; break;
                        }
                        if (GetPropertySize(prop) != 8)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported property type ('" + line + "').");
                    }

                    data.properties.Add(prop);
                }
            }

            // Rewind the stream back to the exact position of the reader.
            reader.BaseStream.Position = readCount;

            return data;
        }

        DataBody ReadDataBody(DataHeader header, BinaryReader reader)
        {
            var data = new DataBody(header.vertexCount);

            float x = 0, y = 0, z = 0;
            Byte r = 255, g = 255, b = 255, a = 255;

            for (var i = 0; i < header.vertexCount; i++)
            {
                foreach (var prop in header.properties)
                {
                    switch (prop)
                    {
                        case DataProperty.R8: r = reader.ReadByte(); break;
                        case DataProperty.G8: g = reader.ReadByte(); break;
                        case DataProperty.B8: b = reader.ReadByte(); break;
                        case DataProperty.A8: a = reader.ReadByte(); break;

                        case DataProperty.R16: r = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataProperty.G16: g = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataProperty.B16: b = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataProperty.A16: a = (byte)(reader.ReadUInt16() >> 8); break;

                        case DataProperty.SingleX: x = reader.ReadSingle(); break;
                        case DataProperty.SingleY: y = reader.ReadSingle(); break;
                        case DataProperty.SingleZ: z = reader.ReadSingle(); break;

                        case DataProperty.DoubleX: x = (float)reader.ReadDouble(); break;
                        case DataProperty.DoubleY: y = (float)reader.ReadDouble(); break;
                        case DataProperty.DoubleZ: z = (float)reader.ReadDouble(); break;

                        case DataProperty.Data8: reader.ReadByte(); break;
                        case DataProperty.Data16: reader.BaseStream.Position += 2; break;
                        case DataProperty.Data32: reader.BaseStream.Position += 4; break;
                        case DataProperty.Data64: reader.BaseStream.Position += 8; break;
                    }
                }

                data.AddPoint(x, y, z, r, g, b, a);
            }

            return data;
        }









#if IMPORTER_EXTENDED

        DataHeaderExtended ReadDataHeaderExtended(StreamReader reader)
        {
            var data = new DataHeaderExtended();
            var readCount = 0;

            // Magic number line ("ply")
            var line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "ply")
                throw new ArgumentException("Magic number ('ply') mismatch.");

            // Data format: check if it's binary/little endian.
            line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "format binary_little_endian 1.0")
                throw new ArgumentException(
                    "Invalid data format ('" + line + "'). " +
                    "Should be binary/little endian.");

            // Read header contents.
            for (var skip = false; ;)
            {
                // Read a line and split it with white space.
                line = reader.ReadLine();
                readCount += line.Length + 1;
                if (line == "end_header") break;
                var col = line.Split();

                // Element declaration (unskippable)
                if (col[0] == "element")
                {
                    if (col[1] == "vertex")
                    {
                        data.vertexCount = Convert.ToInt32(col[2]);
                        skip = false;
                    }
                    else
                    {
                        // Don't read elements other than vertices.
                        skip = true;
                    }
                }

                if (skip) continue;

                // Property declaration line
                if (col[0] == "property")
                {
                    var prop = DataPropertyExtended.Invalid;

                    // Parse the property name entry.
                    switch (col[2])
                    {
                        case "red": prop = DataPropertyExtended.R8; break;
                        case "green": prop = DataPropertyExtended.G8; break;
                        case "blue": prop = DataPropertyExtended.B8; break;
                        case "alpha": prop = DataPropertyExtended.A8; break;
                        case "x": prop = DataPropertyExtended.SingleX; break;
                        case "y": prop = DataPropertyExtended.SingleY; break;
                        case "z": prop = DataPropertyExtended.SingleZ; break;
                        case "nx": prop = DataPropertyExtended.SingleNx; break;
                        case "ny": prop = DataPropertyExtended.SingleNy; break;
                        case "nz": prop = DataPropertyExtended.SingleNz; break;
                        //                   case "scalar_intensity":prop = DataPropertyExtended.Singlesi; break;  //AGO23
                        //                   case "scalar_time":prop = DataPropertyExtended.Singlest;break; //AGO23
                        //                   case "scalar_confidence":prop = DataPropertyExtended.Singlesc;break; //AGO23
                        //                   case "scalar_curvature":prop = DataPropertyExtended.Singlescv;break; //AGO23
                        //                   case "scalar_range":prop = DataPropertyExtended.Singlesr;break; //AGO23
                        //                   case "scalar_sorStdDev":prop = DataPropertyExtended.Singless;break; //AGO23
                        default: prop = DataPropertyExtended.SingleP; data.propertyCount++; break; //AGO23
                    }




                    // Check the property type.
                    if (col[1] == "char" || col[1] == "uchar" ||
                        col[1] == "int8" || col[1] == "uint8")
                    {
                        if (prop == DataPropertyExtended.Invalid)
                            prop = DataPropertyExtended.Data8;
                        else if (GetPropertySizeExtended(prop) != 1)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "short" || col[1] == "ushort" ||
                             col[1] == "int16" || col[1] == "uint16")
                    {
                        switch (prop)
                        {
                            case DataPropertyExtended.Invalid: prop = DataPropertyExtended.Data16; break;
                            case DataPropertyExtended.R8: prop = DataPropertyExtended.R16; break;
                            case DataPropertyExtended.G8: prop = DataPropertyExtended.G16; break;
                            case DataPropertyExtended.B8: prop = DataPropertyExtended.B16; break;
                            case DataPropertyExtended.A8: prop = DataPropertyExtended.A16; break;
                        }
                        if (GetPropertySizeExtended(prop) != 2)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "int" || col[1] == "uint" || col[1] == "float" ||
                             col[1] == "int32" || col[1] == "uint32" || col[1] == "float32")
                    {
                        if (prop == DataPropertyExtended.Invalid)
                            prop = DataPropertyExtended.Data32;
                        else if (GetPropertySizeExtended(prop) != 4)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else if (col[1] == "int64" || col[1] == "uint64" ||
                             col[1] == "double" || col[1] == "float64")
                    {
                        switch (prop)
                        {
                            case DataPropertyExtended.Invalid: prop = DataPropertyExtended.Data64; break;
                            case DataPropertyExtended.SingleX: prop = DataPropertyExtended.DoubleX; break;
                            case DataPropertyExtended.SingleY: prop = DataPropertyExtended.DoubleY; break;
                            case DataPropertyExtended.SingleZ: prop = DataPropertyExtended.DoubleZ; break;
                        }
                        if (GetPropertySizeExtended(prop) != 8)
                            throw new ArgumentException("Invalid property type ('" + line + "').");
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported property type ('" + line + "').");
                    }

                    data.properties.Add(prop);
                }
            }

            // Rewind the stream back to the exact position of the reader.
            reader.BaseStream.Position = readCount;

            return data;
        }

        DataBodyExtended ReadDataBodyExtended(DataHeaderExtended header, BinaryReader reader)
        {
            var data = new DataBodyExtended(header.vertexCount, header.propertyCount);

            float x = 0, y = 0, z = 0;
            Byte r = 255, g = 255, b = 255, a = 255;
            float nx = 0, ny = 0, nz = 0; //AGO23
                                          //float si = 0, sc = 0, st = 0, scv = 0, sr = 0, ss = 0, sp = 0; //AGO23
            System.Single[] prop_package;//AGO23


            for (var i = 0; i < header.vertexCount; i++)
            {
                prop_package = new System.Single[header.propertyCount];//AGO23
                int prop_counter = 0;//AGO23
                foreach (var prop in header.properties)
                {
                    switch (prop)
                    {
                        case DataPropertyExtended.R8: r = reader.ReadByte(); break;
                        case DataPropertyExtended.G8: g = reader.ReadByte(); break;
                        case DataPropertyExtended.B8: b = reader.ReadByte(); break;
                        case DataPropertyExtended.A8: a = reader.ReadByte(); break;

                        case DataPropertyExtended.R16: r = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataPropertyExtended.G16: g = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataPropertyExtended.B16: b = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataPropertyExtended.A16: a = (byte)(reader.ReadUInt16() >> 8); break;

                        case DataPropertyExtended.SingleX: x = reader.ReadSingle(); break;
                        case DataPropertyExtended.SingleY: y = reader.ReadSingle(); break;
                        case DataPropertyExtended.SingleZ: z = reader.ReadSingle(); break;

                        case DataPropertyExtended.DoubleX: x = (float)reader.ReadDouble(); break;
                        case DataPropertyExtended.DoubleY: y = (float)reader.ReadDouble(); break;
                        case DataPropertyExtended.DoubleZ: z = (float)reader.ReadDouble(); break;

                        case DataPropertyExtended.Data8: reader.ReadByte(); break;
                        case DataPropertyExtended.Data16: reader.BaseStream.Position += 2; break;
                        case DataPropertyExtended.Data32: reader.BaseStream.Position += 4; break;
                        case DataPropertyExtended.Data64: reader.BaseStream.Position += 8; break;

                        case DataPropertyExtended.SingleNx: nx = reader.ReadSingle(); break;
                        case DataPropertyExtended.SingleNy: ny = reader.ReadSingle(); break;
                        case DataPropertyExtended.SingleNz: nz = reader.ReadSingle(); break;
                        //                       case DataPropertyExtended.Singlesi: si = reader.ReadSingle(); break;   //AGO23
                        //                       case DataPropertyExtended.Singlest: st = reader.ReadSingle(); break;  //AGO23
                        //                       case DataPropertyExtended.Singlesc: sc = reader.ReadSingle(); break;  //AGO23
                        //                       case DataPropertyExtended.Singlescv:    scv = reader.ReadSingle(); break;  //AGO23
                        //                       case DataPropertyExtended.Singlesr: sr = reader.ReadSingle(); break;  //AGO23
                        //                       case DataPropertyExtended.Singless: ss = reader.ReadSingle(); break;  //AGO23
                        //case DataPropertyExtended.SingleP: sp = reader.ReadSingle(); break;  //AGO23
                        case DataPropertyExtended.SingleP: prop_package[prop_counter] = reader.ReadSingle(); prop_counter = (prop_counter >= header.propertyCount) ? 0 : (prop_counter + 1); break;  //AGO23



                    }
                }

                //               data.AddPointExtended(x, y, z, r, g, b, a, nx, ny, nz, si, st, sc, scv, sr, ss, prop_package);  //AGO23
                data.AddPointExtended(x, y, z, r, g, b, a, nx, ny, nz, prop_package);  //AGO23
            }

            return data;
        }
#endif

    }

    #endregion
}
