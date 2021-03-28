using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace STL
{
    public class STLImporter
    {
        //WARNING: This STL Importer has issues importing ASCII Files on certain computers running Windows 10.
        public string STLHeader { get; private set; }
        public STLFormat STLType { get; private set; }
        public uint TriangleCount { get; private set; }
        public Triangle[] AllTriangles { get; private set; }

        public STLImporter(string TargetFile)
        {
            // Verify That The File Exists
            if (!File.Exists(TargetFile))
                throw new System.IO.FileNotFoundException("Target File Does Not Exist!", "Error!");

            // Load The File Into The Memory as ASCII
            string[] allLinesASCII = File.ReadAllLines(TargetFile);

            // Detect if STL File is ASCII or Binary
            bool ASCII = isAscii(allLinesASCII);

            // Insert Comment Here
            if (ASCII)
            {
                STLType = STLFormat.ASCII;
                AllTriangles = ASCIISTLOpen(allLinesASCII);
            }
            else
            {
                STLType = STLFormat.Binary;
                AllTriangles = BinarySTLOpen(TargetFile);
            }

        }

        Triangle[] BinarySTLOpen(string TargetFile)
        {
            List<Triangle> Triangles = new List<Triangle>();

            byte[] fileBytes = File.ReadAllBytes(TargetFile);
            byte[] header = new byte[80];

            for (int b = 0; b < 80; b++)
                header[b] = fileBytes[b];

            STLHeader = System.Text.Encoding.UTF8.GetString(header);

            uint NumberOfTriangles = System.BitConverter.ToUInt32(fileBytes, 80);
            TriangleCount = NumberOfTriangles;

            for (int i = 0; i < NumberOfTriangles; i++)
            {
                // Read The Normal Vector
                float normalI = System.BitConverter.ToSingle(fileBytes, 84 + i * 50);
                float normalJ = System.BitConverter.ToSingle(fileBytes, (1 * 4) + 84 + i * 50);
                float normalK = System.BitConverter.ToSingle(fileBytes, (2 * 4) + 84 + i * 50);

                // Read The XYZ Positions of The First Vertex
                float vertex1x = System.BitConverter.ToSingle(fileBytes, 3 * 4 + 84 + i * 50);
                float vertex1y = System.BitConverter.ToSingle(fileBytes, 4 * 4 + 84 + i * 50);
                float vertex1z = System.BitConverter.ToSingle(fileBytes, 5 * 4 + 84 + i * 50);

                // Read The XYZ Positions of The Second Vertex
                float vertex2x = System.BitConverter.ToSingle(fileBytes, 6 * 4 + 84 + i * 50);
                float vertex2y = System.BitConverter.ToSingle(fileBytes, 7 * 4 + 84 + i * 50);
                float vertex2z = System.BitConverter.ToSingle(fileBytes, 8 * 4 + 84 + i * 50);

                // Read The XYZ Positions of The Third Vertex
                float vertex3x = System.BitConverter.ToSingle(fileBytes, 9 * 4 + 84 + i * 50);
                float vertex3y = System.BitConverter.ToSingle(fileBytes, 10 * 4 + 84 + i * 50);
                float vertex3z = System.BitConverter.ToSingle(fileBytes, 11 * 4 + 84 + i * 50);

                // Read The Attribute Byte Count
                int Attribs = System.BitConverter.ToInt16(fileBytes, 12 * 4 + 84 + i * 50);

                // Create a Triangle
                Triangle T = new Triangle();

                // Save all the Data Into Said Triangle
                T.normals = new Vector3(normalI, normalK, normalJ);
                T.vertex1 = new Vector3(vertex1x, vertex1z, vertex1y);
                T.vertex2 = new Vector3(vertex2x, vertex2z, vertex2y);//Possible Error?
                T.vertex3 = new Vector3(vertex3x, vertex3z, vertex3y);

                // Add The Triangle
                Triangles.Add(T);
            }

            return Triangles.ToArray();
        }

        Triangle[] ASCIISTLOpen(string[] ASCIILines)
        {
            STLHeader = ASCIILines[0].Replace("solid ", "");

            uint tCount = 0;
            List<Triangle> Triangles = new List<Triangle>();

            foreach (string s in ASCIILines)
                if (s.Contains("facet normal"))
                    tCount++;

            TriangleCount = tCount;

            for (int i = 0; i < tCount * 7; i += 7)
            {
                string n = ASCIILines[i + 1].Trim().Replace("facet normal", "").Replace("  ", " ");

                // Read The Normal Vector
                float normalI = float.Parse(n.Split(' ')[1]);
                float normalJ = float.Parse(n.Split(' ')[2]);
                float normalK = float.Parse(n.Split(' ')[3]);

                string v1 = ASCIILines[i + 3].Split('x')[1].Replace("  ", " ");


                // Read The XYZ Positions of The First Vertex
                float vertex1x = float.Parse(v1.Split(' ')[1]);
                float vertex1y = float.Parse(v1.Split(' ')[2]);
                float vertex1z = float.Parse(v1.Split(' ')[3]);

                string v2 = ASCIILines[i + 4].Split('x')[1].Replace("  ", " ");

                // Read The XYZ Positions of The Second Vertex
                float vertex2x = float.Parse(v2.Split(' ')[1]);
                float vertex2y = float.Parse(v2.Split(' ')[2]);
                float vertex2z = float.Parse(v2.Split(' ')[3]);

                string v3 = ASCIILines[i + 5].Split('x')[1].Replace("  ", " ");

                // Read The XYZ Positions of The Third Vertex
                float vertex3x = float.Parse(v3.Split(' ')[1]);
                float vertex3y = float.Parse(v3.Split(' ')[2]);
                float vertex3z = float.Parse(v3.Split(' ')[3]);

                // Create a Triangle
                Triangle T = new Triangle();

                // Save all the Data Into Said Triangle
                T.normals = new Vector3(normalI, normalK, normalJ);
                T.vertex1 = new Vector3(vertex1x, vertex1z, vertex1y);
                T.vertex2 = new Vector3(vertex2x, vertex2z, vertex2y);
                T.vertex3 = new Vector3(vertex3x, vertex3z, vertex3y);

                // Add The Triangle
                Triangles.Add(T);
            }

            return Triangles.ToArray();
        }

        bool isAscii(string[] Lines)
        {
            string[] Keywords = new string[] { "facet", "solid", "outer", "loop", "vertex", "endloop", "endfacet" };
            int Det = 0;

            foreach (string s in Lines)
            {
                foreach (string ss in Keywords)
                {
                    if (s.Contains(ss))
                    {
                        Det++;
                    }
                }
            }

            if (Det > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public enum STLFormat
        {
            ASCII,
            Binary
        }

        public static float[] AverageUpFaceNormalsAndOutputVertexBuffer(Triangle[] Input, float CutoffAngle)
        {
            Vector3[] VERTEX_DATA = new Vector3[Input.Length * 3];
            Vector3[] VERTEX_NORMALS = new Vector3[Input.Length * 3];
            int[] N_COUNT = new int[Input.Length * 3];

            for (int i = 0; i < Input.Length; i++)
            {
                VERTEX_DATA[i * 3] = Input[i].vertex1;
                VERTEX_DATA[i * 3 + 1] = Input[i].vertex2;
                VERTEX_DATA[i * 3 + 2] = Input[i].vertex3;
            }

            CutoffAngle *= (float)(Math.PI / 180f);
            CutoffAngle = (float)Math.Cos(CutoffAngle);

            for (int i = 0; i < VERTEX_DATA.Length; i++)
            {
                for (int j = 0; j < VERTEX_DATA.Length; j++)
                {
                    if (Vector3.Compare(VERTEX_DATA[j], VERTEX_DATA[i]) && Vector3.Dot(Input[i / 3].normals, Input[j / 3].normals) > CutoffAngle)
                    {
                        VERTEX_NORMALS[i] += Input[j / 3].normals;
                        N_COUNT[i]++;
                    }
                }
            }

            for (int i = 0; i < N_COUNT.Length; i++)
            {
                if (N_COUNT[i] != 0)
                    VERTEX_NORMALS[i] /= N_COUNT[i];
            }

            float[] Output = new float[VERTEX_DATA.Length * 6];

            for (int i = 0; i < VERTEX_DATA.Length; i++)
            {
                Output[i * 6 + 0] = VERTEX_DATA[i].x;
                Output[i * 6 + 1] = VERTEX_DATA[i].y;
                Output[i * 6 + 2] = VERTEX_DATA[i].z;
                Output[i * 6 + 3] = VERTEX_NORMALS[i].x;
                Output[i * 6 + 4] = VERTEX_NORMALS[i].y;
                Output[i * 6 + 5] = VERTEX_NORMALS[i].z;

            }

            return Output;
        }
    }

    public class Triangle
    {
        public Vector3 normals;
        public Vector3 vertex1;
        public Vector3 vertex2;
        public Vector3 vertex3;

        public void Scale(float value)
        {
            vertex1 *= value;
            vertex2 *= value;
            vertex3 *= value;

        }

    }
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        /// <summary>
        /// Creates a new Vector3
        /// </summary>
        /// <param name="posX">X Value</param>
        /// <param name="posY">Y Value</param>
        /// <param name="posZ">Z Value</param>
        public Vector3(float posX, float posY, float posZ)
        {
            x = posX;
            y = posY;
            z = posZ;
        }

        /// <summary>
        /// Calculates the 3 dimensional distance between point A and Point B
        /// </summary>
        /// <param name="From">Point A</param>
        /// <param name="To">Point B</param>
        /// <returns></returns>
        public static float Distance(Vector3 From, Vector3 To)
        {
            return (float)Math.Sqrt(Math.Pow(From.x - To.x, 2) + Math.Pow(From.y - To.y, 2) + Math.Pow(From.z - To.z, 2));
        }
        /// <summary>
        /// Adds two Vector3 together
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector3 operator +(Vector3 A, Vector3 B)
        {
            return new Vector3(A.x + B.x, A.y + B.y, A.z + B.z);
        }
        /// <summary>
        /// Substacts Vector B from Vector A
        /// </summary>
        /// <param name="A">Vector A</param>
        /// <param name="B">Vector B</param>
        /// <returns></returns>
        public static Vector3 operator -(Vector3 A, Vector3 B)
        {
            return new Vector3(A.x - B.x, A.y - B.y, A.z - B.z);
        }

        public static Vector3 operator -(float A, Vector3 B)
        {
            return new Vector3(A - B.x, A - B.y, A - B.z);
        }

        public static Vector3 operator -(Vector3 A, float B)
        {
            return new Vector3(A.x - B, A.y - B, A.z - B);
        }

        public static bool Compare(Vector3 A, Vector3 B)
        {
            return (A.x == B.x && A.y == B.y && A.z == B.z);
        }

        public static Vector3 operator *(Vector3 A, Vector3 B)
        {
            return new Vector3(A.x * B.x, A.y * B.y, A.z * B.z);
        }

        public static Vector3 operator *(Vector3 A, float B)
        {
            return new Vector3(A.x * B, A.y * B, A.z * B);
        }

        public static Vector3 operator *(float A, Vector3 B)
        {
            return new Vector3(A * B.x, A * B.y, A * B.z);
        }

        public static bool operator >(Vector3 A, float B)
        {
            return A.x > B & A.y > B & A.z > B;
        }

        public static bool operator <(Vector3 A, float B)
        {
            return A.x < B & A.y < B & A.z < B;
        }

        public void Clamp01()
        {
            if (x < 0) x = 0;
            if (x > 1) x = 1;

            if (y < 0) y = 0;
            if (y > 1) y = 1;

            if (z < 0) z = 0;
            if (z > 1) z = 1;
        }

        public Vector3 Abs()
        {
            return new Vector3(Math.Abs(x), Math.Abs(y), Math.Abs(z));
        }

        public static Vector3 LerpAngle(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(Lerp1D(a.x, b.x, t), Lerp1D(a.y, b.y, t), Lerp1D(a.z, b.z, t));
        }

        static float Lerp1D(float a, float b, float t)
        {
            float val = Repeat(b - a, 360);
            if (val > 180f)
                val -= 360f;

            return a + val * Clamp01(t);
        }

        static float Repeat(float t, float length)
        {
            return Clamp(t - (float)Math.Floor(t / length) * length, 0f, length);
        }

        public Vector3 Repeat(float length)
        {
            float x1 = Clamp(x - (float)Math.Floor(x / length) * length, 0f, length);
            float y1 = Clamp(y - (float)Math.Floor(y / length) * length, 0f, length);
            float z1 = Clamp(z - (float)Math.Floor(z / length) * length, 0f, length);

            if (x1 > 180f) x1 -= 360f;
            if (y1 > 180f) y1 -= 360f;
            if (z1 > 180f) z1 -= 360f;

            return new Vector3(x1, y1, z1);
        }

        static float Clamp(float v, float min, float max)
        {
            if (v > max) return max;
            else if (v < min) return min;
            else return v;
        }

        static float Clamp01(float v)
        {
            if (v < 0) return 0;
            if (v > 1) return 1;
            else return v;
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            if (t > 1) t = 1;
            else if (t < 0) t = 0;
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static Vector3 operator -(Vector3 A)
        {
            return new Vector3(-A.x, -A.y, -A.z);
        }

        public static Vector3 operator /(Vector3 a, float d)
        {
            return new Vector3(a.x / d, a.y / d, a.z / d);
        }

        public static float Magnitude(Vector3 vector)
        {
            return (float)Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }


        const float EPSILON = 10E-4f;
        public bool isApproximately(Vector3 CompareTo)
        {
            return Math.Abs(CompareTo.x - x) < EPSILON && Math.Abs(CompareTo.y - y) < EPSILON && Math.Abs(CompareTo.z - z) < EPSILON;
        }

        /// <summary>
        /// Returns a string in the format of "Vector3 X: " + X + ", Y: " + Y + ", Z: " + Z
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "X: " + x.ToString() + ", Y: " + y.ToString() + ", Z:" + z.ToString();
        }

        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
        {
            return -2f * Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static float Dot(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            float num = Magnitude(value);
            if (num > 1E-05f)
            {
                return value / num;
            }
            return new Vector3(0, 0, 0);
        }

        public static Vector3 Max(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
        }

        public static Vector3 Sin(Vector3 AngleDegrees)
        {
            return new Vector3((float)Math.Sin(AngleDegrees.x * (Math.PI / 180f)), (float)Math.Sin(AngleDegrees.y * (Math.PI / 180f)), (float)Math.Sin(AngleDegrees.z * (Math.PI / 180f)));
        }

        public static Vector3 Cos(Vector3 AngleDegrees)
        {
            return new Vector3((float)Math.Cos(AngleDegrees.x * (Math.PI / 180f)), (float)Math.Cos(AngleDegrees.y * (Math.PI / 180f)), (float)Math.Cos(AngleDegrees.z * (Math.PI / 180f)));
        }
    }


}
