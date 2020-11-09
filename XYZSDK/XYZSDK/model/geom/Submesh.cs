using GLNKG;
using GLNKG.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace XYZware_SLS.model.geom
{
    //public class SubmeshEdge
    //{
    //    public int vertex1, vertex2;
    //    public int color;
    //    public SubmeshEdge(int v1, int v2, int col)
    //    {
    //        vertex1 = v1;
    //        vertex2 = v2;
    //        color = col;
    //    }
    //}

    //public class SubmeshTriangle
    //{
    //    public int vertex1, vertex2, vertex3;
    //    public int color;
    //    //--- MODEL_SLA	// milton
    //    public int[] colors = new int[3]; // for vertex1, vertext2, vertex3 respectively
    //    //---

    //    public SubmeshTriangle(int v1, int v2, int v3, int col)
    //    {
    //        vertex1 = v1;
    //        vertex2 = v2;
    //        vertex3 = v3;
    //        color = col;
    //        //--- MODEL_SLA	// milton
    //        colors[0] = colors[1] = colors[2] = col;
    //        //---
    //    }

    //    public void Normal(Submesh mesh, out float nx, out float ny, out float nz)
    //    {
    //        Vector3 v0 = mesh.vertices[vertex1];
    //        Vector3 v1 = mesh.vertices[vertex2];
    //        Vector3 v2 = mesh.vertices[vertex3];
    //        float a1 = v1.X - v0.X;
    //        float a2 = v1.Y - v0.Y;
    //        float a3 = v1.Z - v0.Z;
    //        float b1 = v2.X - v1.X;
    //        float b2 = v2.Y - v1.Y;
    //        float b3 = v2.Z - v1.Z;
    //        nx = a2 * b3 - a3 * b2;
    //        ny = a3 * b1 - a1 * b3;
    //        nz = a1 * b2 - a2 * b1;
    //        float length = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
    //        if (length == 0)
    //        {
    //            nx = ny = 0;
    //            nz = 1;
    //        }
    //        else
    //        {
    //            nx /= length;
    //            ny /= length;
    //            nz /= length;
    //        }
    //    }
    //}
    
    public class Submesh
    {
        public const int MESHCOLOR_FRONTBACK = -1;
        //public const int MESHCOLOR_ERRORFACE = -2;
        public const int MESHCOLOR_ERROREDGE = -3;
        public const int MESHCOLOR_OUTSIDE = -4;
        public const int MESHCOLOR_EDGE_LOOP = -5;
        public const int MESHCOLOR_CUT_EDGE = -6;
        public const int MESHCOLOR_NORMAL = -7;
        public const int MESHCOLOR_EDGE = -8;
        public const int MESHCOLOR_BACK = -9;
        //--- MODEL_SLA	// milton
        public const int MESHCOLOR_TRANS_BLUE = -10;
        public const int MESHCOLOR_PINK = -11;
        //---

        //public List<Vector3> vertices = new List<Vector3>();
        //public List<SubmeshEdge> edges = new List<SubmeshEdge>();
        //public List<SubmeshTriangle> triangles = new List<SubmeshTriangle>();
        //public List<SubmeshTriangle> trianglesError = new List<SubmeshTriangle>();
        public bool selected = false;
        public int extruder = 0;
        public int modelID = 0;
        //<Carson(Taipei)><04-09-2019><Added>
        public int color;
        //<><><>
        public int tmpvar_nTriangles = 0;
        public int tmpvar_nEdges = 0;
        public Dictionary<Vector3, int> tmpvar_vertDict;
        public List<Vector3> tmpvar_vertList;
        public List<int> tmpvar_vertCommTriList;
        public Dictionary<Vector3, HashSet<Vector3>> tmpvar_normalDict;
        public List<Vector3> tmpvar_normalList;
        public List<int> colorList;

        public float[] glVertices = null;
        public int[] glColors = null;
        public int[] glEdges = null;
        public int[] glTriangles = null;
        //public int[] glTrianglesError = null;
        public int[] glBuffer = null;
        public float[] glNormals = null;

        public int dplistTriangles = 0;
        public int dplistEdges = 0;

        int a;

        //<Carson(Taipei)><04-09-2019><Removed>
        //public Color[] colors = {Color.FromArgb(255,151,101,46), Color.FromArgb(255,98,8,38),Color.FromArgb(255,0,195,114), Color.FromArgb(255,26,42,96), Color.Yellow, 
        //                        Color.Green,  Color.Blue, Color.Indigo, Color.FromArgb(255,140,246,228), Color.Black, 
        //                        Color.DarkOliveGreen,Color.FromArgb(255,209,0,174), Color.FromArgb(255,108,28,88), Color.Gray, Color.Maroon,
        //                        Color.FromArgb(255,233,100,5), Color.FromArgb(255,0,81,255), Color.FromArgb(255,164,145,62), Color.FromArgb(255,0,165,83), Color.FromArgb(255,193,69,15),
        //                        Color.FromArgb(255,37,60,50), Color.FromArgb(255,36,43,69), Color.FromArgb(255,38,14,40), Color.FromArgb(255,168,86,122), Color.FromArgb(255,176,130,92),
        //                        Color.FromArgb(255,100,67,54),  Color.FromArgb(255,139,231,31), Color.FromArgb(255,183,210,114), Color.FromArgb(255,237,78,69), Color.FromArgb(255,255,0,53)};
        //<><><>
        public void Clear()
        {
            //vertices.Clear();
            //edges.Clear();
            //triangles.Clear();
            //trianglesError.Clear();

            glVertices = null;
            glColors = null;
            glEdges = null;
            glTriangles = null;
            //glTrianglesError = null;
            glNormals = null;

            ClearGL();
            //<Carson(Taipei)><03-27-2019><Removed>
            //if (!Main.main.isPasteModel && !Main.main.isRemove)
            //if (Main.memory.NextValue() > 90)
            //    GC.Collect();
            //<><><>
        }

        public static int ColorToRgba32(Color c)
        {
            return (int)((c.A << 24) | (c.B << 16) | (c.G << 8) | c.R);
        }

        //<Carson(Taipei)><03-22-2019><Modified>
        public int ConvertColorIndex(int idx)
        {
            if (idx >= 0)
                return 255 << 24 | idx;
            switch (idx)
            {
                case MESHCOLOR_FRONTBACK:
                    //<Carson(Taipei)><04-09-2019><Modified>
                    return ColorToRgba32(Color.LightYellow);
                    //if (modelID < colors.Length)
                    //{
                    //    Main.main.threedview.ui.modelsList.mInfo.Color = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(colors[modelID].A, colors[modelID].R, colors[modelID].G, colors[modelID].B));
                    //    Main.main.threedview.ui.modelsList.mInfo.TempColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(colors[modelID].A, colors[modelID].R, colors[modelID].G, colors[modelID].B));

                    //    return ColorToRgba32(colors[modelID]);
                    //}
                    //else
                    //{
                    //    a = modelID / 30;
                    //    Main.main.threedview.ui.modelsList.mInfo.Color = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(colors[(modelID - (a * 30))].A, colors[(modelID - (a * 30))].R, colors[(modelID - (a * 30))].G, colors[(modelID - (a * 30))].B));
                    //    Main.main.threedview.ui.modelsList.mInfo.TempColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(colors[(modelID - (a * 30))].A, colors[(modelID - (a * 30))].R, colors[(modelID - (a * 30))].G, colors[(modelID - (a * 30))].B));
                    //    return ColorToRgba32(colors[(modelID - (a * 30))]);
                    //}
                    //<><><>
                    //return ColorToRgba32(Color.FromArgb(255, 151, 101, 46));
                case MESHCOLOR_BACK:
                    return ColorToRgba32(Main.threeDSettings.insideFaces.BackColor);
                //case MESHCOLOR_ERRORFACE:
                    //return ColorToRgba32(Main.threeDSettings.errorModel.BackColor);
                case MESHCOLOR_ERROREDGE:
                    return ColorToRgba32(Main.threeDSettings.errorModelEdge.BackColor);
                case MESHCOLOR_OUTSIDE:
                    return ColorToRgba32(Main.threeDSettings.outsidePrintbed.BackColor);
                case MESHCOLOR_EDGE_LOOP:
                    return ColorToRgba32(Main.threeDSettings.edges.BackColor);
                case MESHCOLOR_CUT_EDGE:
                    return ColorToRgba32(Main.threeDSettings.cutFaces.BackColor);
                case MESHCOLOR_NORMAL:
                    return ColorToRgba32(Color.Blue);
                case MESHCOLOR_EDGE:
                    return ColorToRgba32(Main.threeDSettings.edges.BackColor);
                //--- MODEL_SLA // milton
                case MESHCOLOR_TRANS_BLUE:
                    return ColorToRgba32(Color.FromArgb(128, 0, 0, 255));
                case MESHCOLOR_PINK:
                    //<Carson(Taipei)><04-09-2019><Modified>
                    //if (modelID < colors.Length)
                    //{
                    //    return ColorToRgba32(colors[modelID]);
                    //}
                    //else
                    //{
                    //    a = modelID / 30;
                    //    return ColorToRgba32(colors[(modelID - (a * 30))]);
                    //}
                    return ColorToRgba32(Color.FromArgb(255,151,101,46));
                    //<><><>
                //---
            }
            return ColorToRgba32(Color.Wheat);
        }
        //<><><>

        /// <summary>
        /// Remove unneded temporary data
        /// </summary>
        public void Compress()
        {
            Compress(false, 0);
        }

        public void Compress(bool override_color, int color)
        {
            //glVertices = new float[3 * vertices.Count];
            //glNormals = new float[3 * vertices.Count];
            //glColors = new int[vertices.Count];
            //glEdges = new int[edges.Count * 2];
            //glTriangles = new int[triangles.Count * 3];
            //glTrianglesError = new int[trianglesError.Count * 3];
            //UpdateDrawLists();
            UpdateColors(override_color, color);
            //vertices.Clear();
        }

        //Carson(Taipei)><11-12-2018><Added - Simplify the process for function of Compress>
        public void Compress(int color)
        {
            int c = ConvertColorIndex(color);
            UpdateColors(true, c);
        }
        //<><><>

        //public int VertexId(RHVector3 v)
        //{
        //    int pos = vertices.Count;
        //    vertices.Add(new Vector3((float)v.x, (float)v.y, (float)v.z));
        //    return pos;
        //}

        //public int VertexId(double x, double y, double z)
        //{
        //    int pos = vertices.Count;
        //    vertices.Add(new Vector3((float)x, (float)y, (float)z));
        //    return pos;
        //}


        private Vector3 calculateNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            float a1 = v1.X - v0.X;
            float a2 = v1.Y - v0.Y;
            float a3 = v1.Z - v0.Z;
            float b1 = v2.X - v1.X;
            float b2 = v2.Y - v1.Y;
            float b3 = v2.Z - v1.Z;
            float nx = a2 * b3 - a3 * b2;
            float ny = a3 * b1 - a1 * b3;
            float nz = a1 * b2 - a2 * b1;
            float length = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (length == 0)
            {
                nx = ny = 0;
                nz = 1;
            }
            else
            {
                nx /= length;
                ny /= length;
                nz /= length;
            }

            return new Vector3(nx, ny, nz);
        }

        public void StartFillingMesh(int nofTriangles, int nofEdges)
        {
            Clear();

            tmpvar_vertDict = new Dictionary<Vector3, int>();
            tmpvar_vertList = new List<Vector3>();
            tmpvar_normalList = new List<Vector3>();
            tmpvar_normalDict = new Dictionary<Vector3, HashSet<Vector3>>();
            tmpvar_vertCommTriList = new List<int>();
            colorList = new List<int>();

            glTriangles = new int[nofTriangles * 3];
            glEdges = new int[nofEdges * 2];
            tmpvar_nTriangles = 0;
            tmpvar_nEdges = 0;
        }

        //<Carson(Taipei)><03-22-2019><Modified>
        public void EndFillingMesh()
        {
            glVertices = new float[tmpvar_vertList.Count * 3];
            int idx = 0;
            foreach (Vector3 v in tmpvar_vertList)
            {
                glVertices[idx++] = v.X;
                glVertices[idx++] = v.Y;
                glVertices[idx++] = v.Z;
            }

            glNormals = new float[tmpvar_normalList.Count * 3];
            idx = 0;
            foreach (Vector3 v in tmpvar_normalList)
            {
                glNormals[idx++] = v.X;
                glNormals[idx++] = v.Y;
                glNormals[idx++] = v.Z;
            }
            //<Carson(Taipei)><03-27-2019><Modified>
            idx = 0;
            if (tmpvar_normalList.Count > 0)
            {
                glNormals = new float[tmpvar_normalList.Count * 3];
                foreach (Vector3 v in tmpvar_normalList)
                {
                    glNormals[idx++] = v.X;
                    glNormals[idx++] = v.Y;
                    glNormals[idx++] = v.Z;
                }
            }
            else if (tmpvar_normalDict.Count > 0)
            {
                glNormals = new float[tmpvar_normalDict.Count * 3];
                foreach (Vector3 v in tmpvar_vertList)
                {
                    Vector3 n = new Vector3();
                    foreach (Vector3 nv in tmpvar_normalDict[v])
                    {
                        n += nv;
                    }
                    n.Normalize();
                    glNormals[idx++] = n.X;
                    glNormals[idx++] = n.Y;
                    glNormals[idx++] = n.Z;
                }
            }
            //<><><>
            glColors = new int[colorList.Count];
            foreach (int c in colorList)
            {
                glColors[idx++] = c;
            }

            tmpvar_vertList = null;
            tmpvar_vertDict = null;
            tmpvar_normalList = null;
            tmpvar_normalDict = null;
            tmpvar_vertCommTriList = null;
        }
        //<><><>

        public void AddEdge(RHVector3 v1, RHVector3 v2, int color)
        {
            Vector3[] vv = new Vector3[2];
            vv[0].X = (float)v1.x; vv[0].Y = (float)v1.y; vv[0].Z = (float)v1.z;
            vv[1].X = (float)v2.x; vv[1].Y = (float)v2.y; vv[1].Z = (float)v2.z;

            int[] nv = new int[2];
            for (int i = 0; i < 2; ++i)
            {
                if (!tmpvar_vertDict.ContainsKey(vv[i]))
                {
                    tmpvar_vertDict.Add(vv[i], tmpvar_vertList.Count);
                    nv[i] = tmpvar_vertList.Count;
                    tmpvar_vertList.Add(vv[i]);
                    colorList.Add(color);
                    tmpvar_normalList.Add(new Vector3());
                    tmpvar_vertCommTriList.Add(0);

                }
                else
                {
                    nv[i] = tmpvar_vertDict[vv[i]];
                    colorList[nv[i]] = color;
                }
            }

            glEdges[tmpvar_nEdges * 2] = nv[0];
            glEdges[tmpvar_nEdges * 2 + 1] = nv[1];
            tmpvar_nEdges = tmpvar_nEdges + 1;
        }

        public void AddTriangle(RHVector3 v1, RHVector3 v2, RHVector3 v3, int color)
        {
            Vector3[] vv = new Vector3[3];
            vv[0].X = (float)v1.x; vv[0].Y = (float)v1.y; vv[0].Z = (float)v1.z;
            vv[1].X = (float)v2.x; vv[1].Y = (float)v2.y; vv[1].Z = (float)v2.z;
            vv[2].X = (float)v3.x; vv[2].Y = (float)v3.y; vv[2].Z = (float)v3.z;
            Vector3 normal = calculateNormal(vv[0], vv[1], vv[2]);

            int[] nv = new int[3];
            for (int i = 0; i < 3; ++i)
            {
                if (!tmpvar_vertDict.ContainsKey(vv[i]))
                {
                    tmpvar_vertDict.Add(vv[i], tmpvar_vertList.Count);
                    nv[i] = tmpvar_vertList.Count;
                    tmpvar_vertList.Add(vv[i]);
                    colorList.Add(color);
                    tmpvar_normalList.Add(normal);
                    tmpvar_vertCommTriList.Add(1);

                }
                else
                {
                    nv[i] = tmpvar_vertDict[vv[i]];
                    colorList[nv[i]] = color;
                    tmpvar_normalList[nv[i]] =
                        (tmpvar_normalList[nv[i]] * tmpvar_vertCommTriList[nv[i]] + normal) / (tmpvar_vertCommTriList[nv[i]] + 1);
                    ++(tmpvar_vertCommTriList[nv[i]]);
                }
            }
            glTriangles[tmpvar_nTriangles * 3] = nv[0];
            glTriangles[tmpvar_nTriangles * 3 + 1] = nv[1];
            glTriangles[tmpvar_nTriangles * 3 + 2] = nv[2];
            tmpvar_nTriangles = tmpvar_nTriangles + 1;
        }

        //<Carson(Taipei)><04-09-2019><Modified - smooth shading>
        public void AddTriangleForSmooth(RHVector3 v1, RHVector3 v2, RHVector3 v3)
        {
            Vector3[] vv = new Vector3[3] { v1.asVector3(), v2.asVector3(), v3.asVector3() };
            Vector3 normal = calculateNormal(vv[0], vv[1], vv[2]);
            int nv;

            for (int i = 0; i < 3; ++i)
            {
                if (!tmpvar_vertDict.ContainsKey(vv[i]))
                {
                    nv = tmpvar_vertList.Count;
                    tmpvar_vertList.Add(vv[i]);
                    tmpvar_vertDict.Add(vv[i], nv);
                    tmpvar_normalDict.Add(vv[i], new HashSet<Vector3>());
                }
                else
                {
                    nv = tmpvar_vertDict[vv[i]];
                }
                tmpvar_normalDict[vv[i]].Add(normal);
                glTriangles[tmpvar_nTriangles * 3 + i] = nv;
            }
            tmpvar_nTriangles++;
        }
        //<><><>

        //<Carson(Taipei)><04-09-2019><Modifed - flat shading>
        public void AddTriangleForFlat(RHVector3 v1, RHVector3 v2, RHVector3 v3)
        {
            Vector3[] vv = new Vector3[3] { v1.asVector3(), v2.asVector3(), v3.asVector3() };
            Vector3 normal = calculateNormal(vv[0], vv[1], vv[2]);

            for (int i = 0; i < 3; ++i)
            {
                glTriangles[tmpvar_nTriangles * 3 + i] = tmpvar_vertList.Count;
                tmpvar_normalList.Add(normal);
                tmpvar_vertList.Add(vv[i]);
            }
            tmpvar_nTriangles++;
        }
        //<><><>

        private void ClearGL()
        {
            try
            {
                if (glBuffer != null)
                {
                    GL.DeleteBuffers(glBuffer.Length, glBuffer);
                    glBuffer = null;
                }

                if (dplistTriangles != 0)
                {
                    GL.DeleteLists(dplistTriangles, 1);
                    dplistTriangles = 0;
                }

                if (dplistEdges != 0)
                {
                    GL.DeleteLists(dplistEdges, 1);
                    dplistEdges = 0;
                }
            }
            catch { }
        }

        public void UpdateColors(bool override_color, int color)
        {
            glColors = new int[colorList.Count];
            int idx = 0;
            foreach (int c in colorList)
            {
                if (!override_color)
                {
                    glColors[idx++] = ConvertColorIndex(c);
                }
                else
                    glColors[idx++] = color;
            }

            if (glBuffer != null)
            {
                // Bind current context to Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, glBuffer[2]);
                // Send data to buffer
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(glColors.Length * sizeof(int)), glColors, BufferUsageHint.StaticDraw);
                // Validate that the buffer is the correct size
                int bufferSize;
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (glColors.Length * sizeof(int) != bufferSize)
                    throw new ApplicationException("Vertex array not uploaded correctly");
                // Clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        //<Carson(Taipei)><04-09-2019><Modified>
        public void UpdateColors(bool bindBuffer)
        {
            glColors = new int[glTriangles.Length];
            for (int i = 0; i < glColors.Length; i++)
            {
                glColors[i] = color;
            }
            if (bindBuffer)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, glBuffer[2]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(glColors.Length * sizeof(int)), glColors, BufferUsageHint.StaticDraw);
            }
        }
        //<><><>

        //public void UpdateDrawLists()
        //{
        //    int idx = 0;
        //    foreach (SubmeshTriangle t in triangles)
        //    {
        //        int n1 = 3 * t.vertex1;
        //        int n2 = 3 * t.vertex2;
        //        int n3 = 3 * t.vertex3;
        //        Vector3 v1 = vertices[t.vertex1];
        //        Vector3 v2 = vertices[t.vertex2];
        //        Vector3 v3 = vertices[t.vertex3];
        //        t.Normal(this, out glNormals[n1], out glNormals[n1 + 1], out glNormals[n1 + 2]);
        //        glNormals[n2] = glNormals[n3] = glNormals[n1];
        //        glNormals[n2 + 1] = glNormals[n3 + 1] = glNormals[n1 + 1];
        //        glNormals[n2 + 2] = glNormals[n3 + 2] = glNormals[n1 + 2];
        //        glVertices[n1++] = v1.X;
        //        glVertices[n1++] = v1.Y;
        //        glVertices[n1] = v1.Z;
        //        glVertices[n2++] = v2.X;
        //        glVertices[n2++] = v2.Y;
        //        glVertices[n2] = v2.Z;
        //        glVertices[n3++] = v3.X;
        //        glVertices[n3++] = v3.Y;
        //        glVertices[n3] = v3.Z;
        //        glTriangles[idx++] = t.vertex1;
        //        glTriangles[idx++] = t.vertex2;
        //        glTriangles[idx++] = t.vertex3;
        //    }
        //    idx = 0;
        //    foreach (SubmeshTriangle t in trianglesError)
        //    {
        //        int n1 = 3 * t.vertex1;
        //        int n2 = 3 * t.vertex2;
        //        int n3 = 3 * t.vertex3;
        //        Vector3 v1 = vertices[t.vertex1];
        //        Vector3 v2 = vertices[t.vertex2];
        //        Vector3 v3 = vertices[t.vertex3];
        //        t.Normal(this, out glNormals[n1], out glNormals[n1 + 1], out glNormals[n1 + 2]);
        //        glNormals[n2] = glNormals[n3] = glNormals[n1];
        //        glNormals[n2 + 1] = glNormals[n3 + 1] = glNormals[n1 + 1];
        //        glNormals[n2 + 2] = glNormals[n3 + 2] = glNormals[n1 + 2];
        //        glVertices[n1++] = v1.X;
        //        glVertices[n1++] = v1.Y;
        //        glVertices[n1] = v1.Z;
        //        glVertices[n2++] = v2.X;
        //        glVertices[n2++] = v2.Y;
        //        glVertices[n2] = v2.Z;
        //        glVertices[n3++] = v3.X;
        //        glVertices[n3++] = v3.Y;
        //        glVertices[n3] = v3.Z;
        //        glTrianglesError[idx++] = t.vertex1;
        //        glTrianglesError[idx++] = t.vertex2;
        //        glTrianglesError[idx++] = t.vertex3;
        //    }
        //    idx = 0;
        //    foreach (SubmeshEdge e in edges)
        //    {
        //        int n1 = 3 * e.vertex1;
        //        int n2 = 3 * e.vertex2;
        //        Vector3 v1 = vertices[e.vertex1];
        //        Vector3 v2 = vertices[e.vertex2];
        //        glNormals[n1] = glNormals[n2] = 0;
        //        glNormals[n1 + 1] = glNormals[n2 + 1] = 0;
        //        glNormals[n1 + 2] = glNormals[n2 + 2] = 1;
        //        glVertices[n1++] = v1.X;
        //        glVertices[n1++] = v1.Y;
        //        glVertices[n1] = v1.Z;
        //        glVertices[n2++] = v2.X;
        //        glVertices[n2++] = v2.Y;
        //        glVertices[n2] = v2.Z;
        //        glEdges[idx++] = e.vertex1;
        //        glEdges[idx++] = e.vertex2;
        //    }
        //}

        public GLNKG.Graphics.Color4 convertColor(Color col)
        {
            return new GLNKG.Graphics.Color4(col.R, col.G, col.B, col.A);
        }

        public void Draw(int method, Vector3 edgetrans, bool forceFaces = false)
        {
            //<Carson(Taipei)><04-11-2019><Modified>
            //GL.ShadeModel(ShadingModel.Flat);
            //GL.LightModel(LightModelParameter.LightModelTwoSide, 1);
            GL.Material(MaterialFace.Back, MaterialParameter.AmbientAndDiffuse, convertColor(Main.threeDSettings.insideFaces.BackColor));
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new GLNKG.Graphics.Color4(0, 0, 0, 0));
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, 50f);

            //GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Normalize);
            //GL.LineWidth(1f);
            //GL.DepthFunc(DepthFunction.Less);
            if (method == 2)
            {
                if (glBuffer == null)
                {
                    glBuffer = new int[5];
                    GL.GenBuffers(5, glBuffer);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, glBuffer[0]);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(glVertices.Length * sizeof(float)), glVertices, BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, glBuffer[1]);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(glNormals.Length * sizeof(float)), glNormals, BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, glBuffer[2]);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(glColors.Length * sizeof(int)), glColors, BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, glBuffer[3]);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(glTriangles.Length * sizeof(int)), glTriangles, BufferUsageHint.StaticDraw);
                    //GL.BindBuffer(BufferTarget.ElementArrayBuffer, glBuffer[4]);
                    //GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(glEdges.Length * sizeof(int)), glEdges, BufferUsageHint.StaticDraw);
                    //GL.BindBuffer(BufferTarget.ElementArrayBuffer, glBuffer[5]);
                    //GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(glTrianglesError.Length * sizeof(int)), glTrianglesError, BufferUsageHint.StaticDraw);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, glBuffer[0]);
                GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
                GL.EnableClientState(ArrayCap.VertexArray);

                GL.BindBuffer(BufferTarget.ArrayBuffer, glBuffer[1]);
                GL.NormalPointer(NormalPointerType.Float, 0, 0);
                GL.EnableClientState(ArrayCap.NormalArray);

                GL.BindBuffer(BufferTarget.ArrayBuffer, glBuffer[2]);
                GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(int), IntPtr.Zero);

                GL.EnableClientState(ArrayCap.ColorArray);
                GL.Enable(EnableCap.ColorMaterial);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, glBuffer[3]);

                if (Main.threeDSettings.ShowFaces || forceFaces)
                {
                    GL.DrawElements(BeginMode.Triangles, glTriangles.Length, DrawElementsType.UnsignedInt, 0);
                }

                GL.LightModel(LightModelParameter.LightModelTwoSide, 0);
                //GL.BindBuffer(BufferTarget.ElementArrayBuffer, glBuffer[5]);
                //GL.DrawElements(BeginMode.Triangles, glTrianglesError.Length, DrawElementsType.UnsignedInt, 0);
               // GL.Disable(EnableCap.ColorMaterial);
                GL.DisableClientState(ArrayCap.NormalArray);
                //GL.Disable(EnableCap.Lighting);
                GL.DepthFunc(DepthFunction.Lequal);
                //GL.BindBuffer(BufferTarget.ElementArrayBuffer, glBuffer[4]);
                //GL.PushMatrix();
                //GL.Translate(edgetrans);
                //GL.DrawElements(BeginMode.Lines, glEdges.Length, DrawElementsType.UnsignedInt, 0);
                //GL.PopMatrix();
                GL.Enable(EnableCap.Lighting);
                //GL.DisableClientState(ArrayCap.ColorArray);
                GL.DisableClientState(ArrayCap.VertexArray);
                GL.DisableClientState(ArrayCap.ColorArray);
                GL.Disable(EnableCap.ColorMaterial);
                GL.Disable(EnableCap.Normalize);
            }
            //<><><>
            else if (method == 1)
            {
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.VertexPointer(3, VertexPointerType.Float, 0, glVertices);
                GL.EnableClientState(ArrayCap.NormalArray);
                GL.NormalPointer(NormalPointerType.Float, 0, glNormals);
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.Enable(EnableCap.ColorMaterial);
                GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, glColors);
                GL.EnableClientState(ArrayCap.ColorArray);
                if (Main.threeDSettings.ShowFaces || forceFaces)
                    GL.DrawElements(BeginMode.Triangles, glTriangles.Length, DrawElementsType.UnsignedInt, glTriangles);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.AmbientAndDiffuse, convertColor(Main.threeDSettings.errorModel.BackColor));
                GL.LightModel(LightModelParameter.LightModelTwoSide, 0);
                //GL.DrawElements(BeginMode.Triangles, glTrianglesError.Length, DrawElementsType.UnsignedInt, glTrianglesError);
                GL.DepthFunc(DepthFunction.Lequal);
                GL.Disable(EnableCap.Lighting);
                GL.PushMatrix();
                GL.Translate(edgetrans);
                GL.DrawElements(BeginMode.Lines, glEdges.Length, DrawElementsType.UnsignedInt, glEdges);
                GL.PopMatrix();
                GL.Enable(EnableCap.Lighting);
                GL.Disable(EnableCap.ColorMaterial);
                GL.DisableClientState(ArrayCap.ColorArray);
                GL.DisableClientState(ArrayCap.VertexArray);
                GL.DisableClientState(ArrayCap.NormalArray);

            }
            else if (method == 0)
            {
                int n;
                if (dplistTriangles == 0)
                {
                    dplistTriangles = GL.GenLists(1);
                    GL.NewList(dplistTriangles, ListMode.Compile);

                    //GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);
                    //GL.Enable(EnableCap.ColorMaterial);
                    GL.Begin(BeginMode.Triangles);
                    n = glTriangles.Length;
                    if (n <= 100)
                    {
                        for (int i = 0; i < n; ++i)
                        {
                            int p = glTriangles[i] * 3;
                            //int col = glColors[glTriangles[i]];
                            //GL.Color4(new GLNKG.Graphics.Color4((byte)(col & 255), (byte)((col >> 8) & 255), (byte)((col >> 16) & 255), (byte)(col >> 24)));
                            //GL.Normal3(glNormals[p], glNormals[p + 1], glNormals[p + 2]);
                            GL.Vertex3(glVertices[p], glVertices[p + 1], glVertices[p + 2]);
                        }
                    }
                    else
                    {
                        //In every 3 triangles, only the last triangle will be drawn
                        for (int i = 6; i < n; i += 9)
                        {
                            int p = glTriangles[i] * 3;
                            GL.Vertex3(glVertices[p], glVertices[p + 1], glVertices[p + 2]);
                            p = glTriangles[i + 1] * 3;
                            GL.Vertex3(glVertices[p], glVertices[p + 1], glVertices[p + 2]);
                            p = glTriangles[i + 2] * 3;
                            GL.Vertex3(glVertices[p], glVertices[p + 1], glVertices[p + 2]);
                        }
                    }
                    GL.End();

                    GL.EndList();
                }
                //if (Main.threeDSettings.ShowFaces || forceFaces)
                GL.CallList(dplistTriangles);

                //GL.LightModel(LightModelParameter.LightModelTwoSide, 0);
                //GL.Begin(BeginMode.Triangles);
                //n = glTrianglesError.Length;
                //for (int i = 0; i < n; i++)
                //{
                //    int p = glTrianglesError[i] * 3;
                //    int col = glColors[glTrianglesError[i]];
                //    GL.Color4(new GLNKG.Graphics.Color4((byte)(col & 255), (byte)((col >> 8) & 255), (byte)((col >> 16) & 255), (byte)(col >> 24)));
                //    GL.Normal3(glNormals[p], glNormals[p + 1], glNormals[p + 2]);
                //    GL.Vertex3(glVertices[p], glVertices[p + 1], glVertices[p + 2]);
                //}
                //GL.End();
                //GL.Disable(EnableCap.Lighting);

                n = glEdges.Length;
                if (n != 0)
                {
                    GL.PushMatrix();
                    GL.Translate(edgetrans);

                    if (dplistEdges == 0)
                    {
                        dplistEdges = GL.GenLists(1);
                        GL.NewList(dplistEdges, ListMode.Compile);

                        GL.Begin(BeginMode.Lines);
                        for (int i = 0; i < n; i++)
                        {
                            int p = glEdges[i] * 3;
                            //int col = glColors[glEdges[i]];
                            //GL.Color4(new GLNKG.Graphics.Color4((byte)(col & 255), (byte)((col >> 8) & 255), (byte)((col >> 16) & 255), (byte)(col >> 24)));
                            //GL.Normal3(glNormals[p], glNormals[p + 1], glNormals[p + 2]);
                            GL.Vertex3(glVertices[p], glVertices[p + 1], glVertices[p + 2]);
                        }
                        GL.End();

                        GL.EndList();
                    }

                    GL.PopMatrix();
                }

                //GL.Enable(EnableCap.Lighting);
                //GL.Disable(EnableCap.ColorMaterial);
            }
        }
    }
}
