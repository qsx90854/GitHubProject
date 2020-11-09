using GLNKG;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using XYZware_SLS.view;
using XYZware_SLS.view.utils;

namespace XYZware_SLS.model.geom
{
    // milton
    public class Layer
    {
        private Double zLowBound = 0;// inclusive
        private Double span = 5;
        public List<Cube> cubeList = null;

        public Layer(Double zLow, Double span)
        {
            cubeList = new List<Cube>();
            this.zLowBound = zLow;
            //this.zUppBound = zUpp;
            this.span = span;
        }

        ~Layer() { Clean(); }

        public void Clean()
        {
            if (null != cubeList)
            {
                foreach (Cube cube in cubeList)
                    cube.Clean();
                cubeList.Clear();
            }
            //cubeList = null;
        }

        public Double ZLowBound
        {
            get { return zLowBound; }
        }
    }

    public class Cube
    {
        private Double xLowBound = 0;// inclusive
        private Double yLowBound = 0;// inclusive
        private Double zLowBound = 0;// exclusive
        private Double span = 5;
        private int groupIdx = -1;

        public List<XYZTuple<int, int>> verIdxTriIdxList = null;

        public Cube(Double xLow, Double yLow, Double zLow, Double span)
        {
            verIdxTriIdxList = new List<XYZTuple<int, int>>();
            this.xLowBound = xLow;
            //this.xUppBound = xUpp;
            this.yLowBound = yLow;
            this.zLowBound = zLow;
            this.span = span;
        }

        ~Cube() { Clean(); }

        public void Clean()
        {
            if (null != verIdxTriIdxList)
                verIdxTriIdxList.Clear();
            //verIdxTriIdxList = null;
        }
        public int GroupIdx
        {
            get { return groupIdx; }
            set { groupIdx = value; }
        }

        public Double XLowBound
        {
            get { return xLowBound; }
        }
        public Double YLowBound
        {
            get { return yLowBound; }
        }
        public Double ZLowBound
        {
            get { return zLowBound; }
        }
        public Double Span
        {
            get { return span; }
        }
    }

    public class MeshData
    {
        public int nRefObject = 0;
        public TopoVertexStorage vertices = new TopoVertexStorage();
        public TopoTriangleStorage triangles = new TopoTriangleStorage();
        public LinkedList<TopoEdge> edges = new LinkedList<TopoEdge>();
        public TopoTriangleStorage[] split_triangles = new TopoTriangleStorage[4] { new TopoTriangleStorage(), 
                                                                                    new TopoTriangleStorage(), 
                                                                                    new TopoTriangleStorage(),
                                                                                    new TopoTriangleStorage() };
        public HashSet<TopoTriangle> intersectingTriangles = new HashSet<TopoTriangle>();
    }

    public class TopoModel
    {
        public const bool debugRepair = false;
        public const float epsilon = 0.001f;
        public MeshData meshdata = new MeshData();
        public TopoVertexStorage vertices { get { return meshdata.vertices; } }
        public TopoTriangleStorage triangles { get { return meshdata.triangles; } }
        public LinkedList<TopoEdge> edges { get { return meshdata.edges; } }
        public RHBoundingBox boundingBox = new RHBoundingBox();
        //<Frank (Taipei)> <first half of 2018> --SECTION START--
        //The coordinate value of vertices may be calibrated, but the member 'boundingBox' don't change when vertices coordinate value
        //changes (it seems that the very original coordinate value information is still useful in some situation), add another bounding box - bbox,
        //this bounding box will change when vertices coordinate value changes
        public RHBoundingBox bbox = new RHBoundingBox();
        //<Frank (Taipei)> <first half of 2018> --SECTION END--
        public HashSet<TopoTriangle> intersectingTriangles { get { return meshdata.intersectingTriangles; } }
        public int badEdges = 0;
        public int badTriangles = 0;
        public int shells = 0;
        public int updatedNormals = 0;
        public int loopEdges = 0;
        public int manyShardEdges = 0;
        public bool manifold = false;
        public bool normalsOriented = false;
        public bool intersectionsUpToDate = false;
        public InfoProgressPanel ipp = null;
        [DllImport(@".\x86\TestDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void modelRepair(string filename);
        [DllImport(@".\x64\TestDll64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void modelRepair64(string filename);

        public TopoTriangleStorage[] split_triangles { get { return meshdata.split_triangles; } }

        public bool IsLargeModel = false;
        public int DegeneratedTrianglesCount = 0;

        public void StartAction(string name)
        {
            if (ipp == null) return;
            ipp.Action = Trans.T(name);
            ipp.Progress = 0;
        }

        public void Progress(double prg)
        {
            prg *= 100.0;
            if (ipp == null) return;
            if (prg < 0) prg = 0;
            if (prg > 100) prg = 100;
            ipp.Progress = (int)prg;
        }

        public bool IsActionStopped()
        {
            if (ipp == null) return false;
            return ipp.IsKilled;
        }

        public void clear()
        {
            vertices.Clear();
            triangles.Clear();
            foreach (TopoTriangleStorage ts in split_triangles)
                ts.Clear();
            edges.Clear();
            boundingBox.Clear();
            intersectionsUpToDate = false;
            //<Carson(Taipei)><03-27-2019><Removed>
            //if (!Main.main.isRemove)
            //if (Main.memory.NextValue() > 90)
            //    GC.Collect();
            //<><><>
        }

        public TopoModel Copy()
        {
            TopoModel newModel = new TopoModel();
            //int nOld = vertices.Count;
            //int i = 0;
            //List<TopoVertex> vcopy = new List<TopoVertex>(vertices.Count);
            //foreach (TopoVertex v in vertices)
            //{
            //    v.id = i++;
            //    //v.pos.x -= boundingBox.Center.x;
            //    //v.pos.y -= boundingBox.Center.y;
            //    //v.pos.z -= boundingBox.Center.z;
            //    //v.pos.z -= boundingBox.zMin;
            //    TopoVertex newVert = new TopoVertex(v.id, v.pos);
            //    newModel.addVertex(newVert);
            //    vcopy.Add(newVert);
            //}

            //foreach (TopoTriangle t in triangles)
            //{
            //    //TopoTriangle triangle = new TopoTriangle(newModel, vcopy[t.vertices[0].id], vcopy[t.vertices[1].id], vcopy[t.vertices[2].id], t.normal.x, t.normal.y, t.normal.z);
            //    //newModel.triangles.Add(triangle);

            //    RHVector3 normal = new RHVector3(t.normal.x, t.normal.y, t.normal.z);
            //    RHVector3 p1 = new RHVector3(t.vertices[0].pos.x, t.vertices[0].pos.y, t.vertices[0].pos.z);
            //    RHVector3 p2 = new RHVector3(t.vertices[1].pos.x, t.vertices[1].pos.y, t.vertices[1].pos.z);
            //    RHVector3 p3 = new RHVector3(t.vertices[2].pos.x, t.vertices[2].pos.y, t.vertices[2].pos.z);
            //    normal.NormalizeSafe();

            //    //addTriangle(p1, p2, p3, normal);
            //    addTriangle(p1, p2, p3, normal, 0);
            //}

            //SYDNY 04/02/2018
            //for (i = 0; i < 4; ++i)
            //{
            //    TopoTriangleStorage st = split_triangles[i];

            //    foreach (TopoTriangle t in st.triangles)
            //    {
            //        RHVector3 normal = new RHVector3(0, 0, 0);
            //        RHVector3 p1 = new RHVector3(t.vertices[0].pos.x, t.vertices[0].pos.y, t.vertices[0].pos.z);
            //        RHVector3 p2 = new RHVector3(t.vertices[1].pos.x, t.vertices[1].pos.y, t.vertices[1].pos.z);
            //        RHVector3 p3 = new RHVector3(t.vertices[2].pos.x, t.vertices[2].pos.y, t.vertices[2].pos.z);
            //        normal.NormalizeSafe();

            //        newModel.addTriangle(p1, p2, p3, normal, 0);
            //    }
            //}

            newModel.meshdata = meshdata;
            newModel.IsLargeModel = IsLargeModel;
            newModel.boundingBox.Add(boundingBox);
            newModel.bbox.Add(bbox);

            UpdateVertexNumbers();
            newModel.UpdateVertexNumbers();
            newModel.badEdges = 0;
            newModel.badTriangles = badTriangles;
            newModel.shells = shells;
            newModel.updatedNormals = updatedNormals;
            newModel.loopEdges = loopEdges;
            newModel.manyShardEdges = 0;
            newModel.manifold = manifold;
            newModel.normalsOriented = normalsOriented;
            return newModel;
        }

        public void Merge(TopoModel model, Matrix4 trans)
        {
            try
            {
                int nOld = vertices.Count;
                int i = 0;
                List<TopoVertex> vcopy = new List<TopoVertex>(model.vertices.Count);
                foreach (TopoVertex v in model.vertices)
                {
                    v.id = i++;
                    TopoVertex newVert = new TopoVertex(v.id, v.pos, trans);
                    addVertex(newVert);
                    vcopy.Add(newVert);
                }

                //<GELO><04-01-2016><Used split_triangles in the Merge>
                //foreach (TopoTriangle t in model.triangles)
                //{
                //    TopoTriangle triangle = new TopoTriangle(this, vcopy[t.vertices[0].id], vcopy[t.vertices[1].id], vcopy[t.vertices[2].id]);
                //    triangle.RecomputeNormal();
                //    triangles.Add(triangle);
                //}

                if (IsLargeModel)
                {
                    foreach (TopoTriangle t in model.split_triangles[0])
                    {
                        TopoTriangle triangle = new TopoTriangle(this, vcopy[t.vertices[0].id], vcopy[t.vertices[1].id], vcopy[t.vertices[2].id]);
                        triangle.RecomputeNormal();
                        split_triangles[0].Add(triangle);
                    }
                }
                else
                {
                    for (int ii = 0; ii <= 3; ii++)
                    {
                        foreach (TopoTriangle t in model.split_triangles[ii])
                        {
                            TopoTriangle triangle = new TopoTriangle(this, vcopy[t.vertices[0].id], vcopy[t.vertices[1].id], vcopy[t.vertices[2].id]);
                            triangle.RecomputeNormal();
                            split_triangles[ii].Add(triangle);
                        }
                    }
                }

                //<><><>

                //RemoveUnusedDatastructures();
                intersectionsUpToDate = false;
            }
            catch (System.OutOfMemoryException)
            {
                GC.Collect();
            }
        }

        //<Carson(Taipei)><12-14-2018><Modified>
        public void addVertex(TopoVertex v, bool findPoint = true)
        {
            vertices.Add(v, findPoint);
            boundingBox.Add(v.pos);
            //<Frank (Taipei)> <first half of 2018>
            //Also update another bounding box - bbox
            bbox.Add(v.pos);
        }    
        //<><><>

        public TopoVertex findVertexOrNull(RHVector3 pos)
        {
            return vertices.SearchPoint(pos);
            /*  foreach (TopoVertex v in vertices)
              {
                  if (v.distance(pos) < epsilon)
                      return v;
              }
              return null;*/
        }

        public TopoVertex addVertex(RHVector3 pos)
        {
            TopoVertex newVertex = findVertexOrNull(pos);
            if (newVertex == null)
            {
                //<Carson(Taipei)><12-14-2018><Modified - id from 0 start>
                //newVertex = new TopoVertex(vertices.Count + 1, pos);
                newVertex = new TopoVertex(vertices.Count, pos);
                //<><><>
                addVertex(newVertex);
            }
            return newVertex;
        }

        public void UpdateVertexNumbers()
        {
            //<Carson(Taipei)><12-19-2018><Modified - id from 0 start>
            int i = 0;//int i = 1;
            //<><><>
            foreach (TopoVertex v in vertices)
            {
                v.id = i++;
            }
        }

        /*public TopoEdge getOrCreateEdgeBetween(TopoVertex v1, TopoVertex v2)
        {
            foreach (TopoTriangle t in v1.connectedFacesList)
            {
                for (int i = 0; i < 3; i++)
                {
                    if ((v1 == t.vertices[i] && v2 == t.vertices[(i + 1) % 3]) || (v2 == t.vertices[i] && v1 == t.vertices[(i + 1) % 3]))
                        return t.edges[i];
                }
            }
            //foreach (TopoEdge edge in edges)
            //{
            //    if (edge.isBuildOf(v1, v2))
            //        return edge;
            //}
            TopoEdge newEdge = new TopoEdge(v1, v2);
            edges.AddLast(newEdge);
            return newEdge;
        }*/

        public void UpdateIntersectingTriangles()
        {
            /*if (intersectionsUpToDate) return;
            intersectingTriangles.Clear();
            HashSet<TopoTriangle> candidates;
            int counter = 0,n = triangles.Count;
            StartAction("L_INTERSECTION_TESTS");
            foreach (TopoTriangle test in triangles)
            {
                test.hasIntersections = false;
            }
            foreach (TopoTriangle test in triangles)
            {
                counter++;
                Progress((double)counter / n);
                if (counter % 600 == 0)
                {
                    Application.DoEvents();
                    if (IsActionStopped()) return;
                }
                if (test.hasIntersections) continue;
                candidates = triangles.FindIntersectionCandidates(test);
                foreach (TopoTriangle candidate in candidates)
                {
                    if (test.Intersects(candidate))
                    {
                        candidate.CheckIfColinear();
                        test.Intersects(candidate);
                        if (test.hasIntersections == false)
                        {
                            test.hasIntersections = true;
                            test.bad = true;
                            intersectingTriangles.Add(test);
                           // Debug.WriteLine(test);
                        }
                        if (candidate.hasIntersections == false)
                        {
                            candidate.hasIntersections = true;
                            candidate.bad = true;
                            intersectingTriangles.Add(candidate);
                            //Debug.WriteLine(candidate);
                        }
                    }
                }
            }*/
            intersectionsUpToDate = true;
        }

        public TopoTriangle addTriangle(double p1x, double p1y, double p1z, double p2x, double p2y, double p2z,
            double p3x, double p3y, double p3z, double nx, double ny, double nz)
        {
            TopoVertex v1 = addVertex(new RHVector3(p1x, p1y, p1z));
            TopoVertex v2 = addVertex(new RHVector3(p2x, p2y, p2z));
            TopoVertex v3 = addVertex(new RHVector3(p3x, p3y, p3z));

            TopoTriangle triangle = new TopoTriangle(this, v1, v2, v3, nx, ny, nz);
            return AddTriangle(triangle);
        }

        public TopoTriangle addTriangle(RHVector3 p1, RHVector3 p2, RHVector3 p3, RHVector3 normal)
        {
            TopoVertex v1 = addVertex(p1);
            TopoVertex v2 = addVertex(p2);
            TopoVertex v3 = addVertex(p3);

            TopoTriangle triangle = new TopoTriangle(this, v1, v2, v3, normal.x, normal.y, normal.z);
            return AddTriangle(triangle);
        }

        public TopoTriangle AddTriangle(TopoTriangle triangle)
        {
            if (triangle.IsDegenerated())
                triangle.Unlink(this);
            else
                triangles.Add(triangle);
            return triangle;
        }

        //<GELO><04-01-2016><>
        public TopoTriangle addTriangle(RHVector3 p1, RHVector3 p2, RHVector3 p3, RHVector3 normal, int index)
        {
            TopoVertex v1 = addVertex(p1);
            TopoVertex v2 = addVertex(p2);
            TopoVertex v3 = addVertex(p3);

            //<Carson(Taipei)><12-14-2018><Modified - cancel index of traingle>
            TopoTriangle triangle = new TopoTriangle(this, v1, v2, v3, normal.x, normal.y, normal.z);
            //TopoTriangle triangle = new TopoTriangle(v1, v2, v3, normal.x, normal.y, normal.z, v1.id, v2.id, v3.id);
            //<><><>
            return AddTriangle(triangle, index);
        }

        public TopoTriangle AddTriangle(TopoTriangle triangle, int index)
        {
            if (triangle.IsDegenerated())
            {
                triangle.Unlink(this);
                DegeneratedTrianglesCount++;
            }
            else
            {
                //split_triangles[index].Add(triangle);
                split_triangles[0].Add(triangle, index);
            }
                
            return triangle;
        }
        public void MergeMultiList()
        {
            split_triangles[0].convertTempToHashSet();
        }
        //Tonton<11-25-2019><Added>
        public TopoTriangle addTriangle(RHVector3 p1, RHVector3 p2, RHVector3 p3)
        {
            TopoVertex v1 = addVertex(p1);
            TopoVertex v2 = addVertex(p2);
            TopoVertex v3 = addVertex(p3);

            TopoTriangle triangle = new TopoTriangle(this, v1, v2, v3);
            return AddTriangle(triangle, 0);
        }

        public int CountAllTriangles()
        {
            return split_triangles[0].Count + split_triangles[1].Count + split_triangles[2].Count;
        }
        //<><><>

        public void removeTriangle(TopoTriangle triangle)
        {
            triangle.Unlink(this);
            triangles.Remove(triangle);
        }

        public void UpdateNormals()
        {
            /*
            CountShells();
            StartAction("L_FIXING_NORMALS");
            ResetTriangleMarker();
            updatedNormals = 0;
            int count = 0;
            foreach (TopoTriangle triangle in triangles)
            {
                count++;
                Progress((double)count / triangles.Count);
                if ((count % 2500) == 0)
                    Application.DoEvents();
                if (triangle.algHelper == 0)
                {
                    int testShell = triangle.shell;
                    triangle.RecomputeNormal();
                    RHVector3 lineStart = triangle.Center;
                    RHVector3 lineDirection = triangle.normal;
                    int hits = 0;
                    double delta;
                    foreach (TopoTriangle test in triangles)
                    {
                        if (test != triangle && test.IntersectsLine(lineStart, lineDirection, out delta))
                        {
                           // Debug.WriteLine(test);
                           // Debug.WriteLine(triangle);
                            if (delta > 0) hits++;
                        }
                    }
                    if ((hits & 1) == 1)
                    {
                        triangle.FlipDirection();
                        updatedNormals++;
                    }
                    FloodFillNormals(triangle);
                    if (SignedShellVolume(testShell) < 0)
                    {
                        foreach (TopoTriangle flip in triangles)
                        {
                            triangle.FlipDirection();
                        }
                    }
                }
                else
                    triangle.RecomputeNormal();
            }
            */
        }

        /*private void FloodFillNormals(TopoTriangle good)
        {
            good.algHelper = 1;
            HashSet<TopoTriangle> front = new HashSet<TopoTriangle>();
            front.Add(good);
            HashSet<TopoTriangle> newFront = new HashSet<TopoTriangle>();
            int i;
            int cnt = 0;
            while (front.Count > 0)
            {
                foreach (TopoTriangle t in front)
                {
                    cnt++;
                    if((cnt % 2000) == 0)
                        Application.DoEvents();
                    for (i = 0; i < 3; i++)
                    {
                        foreach (TopoTriangle test in t.edges[i].faces)
                        {
                            if (t != test && test.algHelper == 0)
                            {
                                test.algHelper = 1;
                                newFront.Add(test);
                                if (!t.SameNormalOrientation(test))
                                {
                                    test.FlipDirection();
                                    updatedNormals++;
                                }
                            }
                        }
                    }
                }
                front = newFront;
                newFront = new HashSet<TopoTriangle>();
            }
        }*/

        public bool CheckNormals()
        {
            /*
            CountShells();
            ResetTriangleMarker();
            normalsOriented = true;
            foreach (TopoTriangle triangle in triangles)
            {
                if (triangle.algHelper == 0)
                {
                    int testShell = triangle.shell;
                    triangle.RecomputeNormal();
                    RHVector3 lineStart = triangle.Center;
                    RHVector3 lineDirection = triangle.normal;
                    int hits = 0;
                    double delta;
                    foreach (TopoTriangle test in triangles)
                    {
                        if (test != triangle && test.IntersectsLine(lineStart, lineDirection, out delta))
                        {
                            if (delta > 0) hits++;
                        }
                    }
                    if ((hits & 1) == 1)
                    {
                        normalsOriented = false;
                        return false;
                    }
                    if (SignedShellVolume(testShell) < 0)
                    {
                        normalsOriented = false;
                        return false;
                    }
                    if (!FloodFillCheckNormals(triangle))
                    {
                        normalsOriented = false;
                        return false;
                    }
                }
            }*/
            return true;
        }

        public double Surface()
        {
            double surface = 0;
            //<Carson(Taipei)><05-20-2019><Modified>
            //foreach (TopoTriangle t in triangles)
            //foreach (TopoTriangleStorage ts in split_triangles)
            foreach (TopoTriangle t in split_triangles[0])
            {
                surface += t.Area();
            }
            //<><><>
            return surface;
        }

        //<Carson(Taipei)><05-29-2019><Modified>
        public double Surface(Matrix4 trans)
        {
            double surface = 0;
            trans.Row3 = new Vector4(0, 0, 0, 1);
            foreach (TopoTriangle t in split_triangles[0])
            {
                RHVector3 v0 = new RHVector3(t.vertices[0].pos).Transform(trans);
                RHVector3 v1 = new RHVector3(t.vertices[1].pos).Transform(trans);
                RHVector3 v2 = new RHVector3(t.vertices[2].pos).Transform(trans);
                surface += 0.5 * v1.Subtract(v0).CrossProduct(v2.Subtract(v1)).Length;
            }
            return surface;
        }
        //<><><>

        public double Volume()
        {
            double volume = 0;
            //<ZSL><04-18-2016><Modified - Update computation of volume to use 'split_triangles[]' instead of 'triangles'>
            //foreach (TopoTriangle t in triangles)
            //    volume += t.SignedVolume();
            //<Carson(Taipei)><05-20-2019><Modified>
            //for (int ii = 0; ii <= 3; ii++)
            //{
            foreach (TopoTriangle t in split_triangles[0])
            {
                volume += t.SignedVolume();
            }
            //}
            //<><><>
            //<><><>
            return Math.Abs(volume);
        }

        //<Carson(Taipei)><05-29-2019><Modified>
        public double Volume(Matrix4 trans)
        {
            double volume = 0;
            trans.Row3 = new Vector4(0, 0, 0, 1);
            //trans.Row3 = new Vector4((float)Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.txt_SFwidth.Text),
            //     (float)Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.txt_SFdepth.Text),
            //     (float)Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.txt_SFheight.Text), 1);
            foreach (TopoTriangle t in split_triangles[0])
            {
                RHVector3 v0 = new RHVector3(t.vertices[0].pos).Transform(trans);
                RHVector3 v1 = new RHVector3(t.vertices[1].pos).Transform(trans);
                RHVector3 v2 = new RHVector3(t.vertices[2].pos).Transform(trans);
                volume += v0.ScalarProduct(v1.CrossProduct(v2)) / 6.0;
            }
            return Math.Abs(volume);
        }
        //<><><>

        /*public double SignedShellVolume(int shell)
        {
            double volume = 0;
            foreach (TopoTriangle t in triangles)
            {
                if(t.shell == shell)
                    volume += t.SignedVolume();
            }
            return volume;
        }

        private bool FloodFillCheckNormals(TopoTriangle good)
        {
            good.algHelper = 1;
            HashSet<TopoTriangle> front = new HashSet<TopoTriangle>();
            front.Add(good);
            HashSet<TopoTriangle> newFront = new HashSet<TopoTriangle>();
            int i;
            int cnt = 0;
            while (front.Count > 0)
            {
                foreach (TopoTriangle t in front)
                {
                    cnt++;
                    if((cnt % 2000) == 0)
                        Application.DoEvents();
                    for (i = 0; i < 3; i++)
                    {
                        foreach (TopoTriangle test in t.edges[i].faces)
                        {
                            if (t != test && test.algHelper == 0)
                            {
                                test.algHelper = 1;
                                newFront.Add(test);
                                test.RecomputeNormal();
                                if (!t.SameNormalOrientation(test))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                front = newFront;
                newFront = new HashSet<TopoTriangle>();
            }
            return true;
        }
        
        public int MarkPlanarRegions()
        {
            ResetTriangleMarker();
            int regionCounter = 0;
            foreach (TopoTriangle triangle in triangles)
            {
                if (triangle.algHelper == 0)
                {
                    FloodFillPlanarRegions(triangle, ++regionCounter);
                }
            }
            return regionCounter;
        }
        
        private void FloodFillPlanarRegions(TopoTriangle good, int marker)
        {
            good.algHelper = marker;
            HashSet<TopoTriangle> front = new HashSet<TopoTriangle>();
            front.Add(good);
            HashSet<TopoTriangle> newFront = new HashSet<TopoTriangle>();
            int i;
            while (front.Count > 0)
            {
                Application.DoEvents();
                foreach (TopoTriangle t in front)
                {
                    for (i = 0; i < 3; i++)
                    {
                        foreach (TopoTriangle test in t.edges[i].faces)
                        {
                            if (test.algHelper == 0)
                            {
                                if (t.normal.Angle(test.normal) < 1e-6)
                                {
                                    test.algHelper = 1;
                                    newFront.Add(test);
                                }
                            }
                        }
                    }
                }
                front = newFront;
                newFront = new HashSet<TopoTriangle>();
            }
        }
        
        public HashSet<TopoEdge> OpenLoopEdges()
        {
            HashSet<TopoEdge> list = new HashSet<TopoEdge>();
            foreach (TopoEdge edge in edges)
            {
                if (edge.connectedFaces == 1)
                    list.Add(edge);
            }
            return list;
        }*/

        public bool JoinTouchedOpenEdges(double limit)
        {
            /*Console.WriteLine("Open Edges:");
            foreach (TopoEdge e in OpenLoopEdges())
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("=========");*/
            /*bool changed = false;
            bool somethingChanged = false;
            RHVector3 projectedPoint = new RHVector3(0, 0, 0);
            double lambda;
            int cnt = 0;
            do
            {
                changed = false;
                HashSet<TopoEdge> list = OpenLoopEdges();
                foreach (TopoEdge edge in list)
                {
                    cnt++;
                    if ((cnt % 10) == 0)
                    {
                        Application.DoEvents();
                        if (IsActionStopped()) return true;
                    }
                    foreach (TopoEdge test in list)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (edge.ProjectPoint(test[i].pos, out lambda, projectedPoint))
                            {
                                double dist = test[i].pos.Distance(projectedPoint);
                                //Console.WriteLine("Distance:" + dist + " lambda " + lambda);
                                if (dist < limit && edge.FirstTriangle.VertexIndexFor(test[i]) == -1)
                                {
                                    //Console.WriteLine("RedLine:" + edge);
                                    edge.InsertVertex(this, test[i]);
                                    changed = true;
                                    somethingChanged = true;
                                    break;
                                }
                            }
                        }
                        if (changed) break;
                    }
                    if (changed) break;
                }
            } while (changed == true); 
            if (somethingChanged) intersectionsUpToDate = false;
            return somethingChanged;*/
            return false;
        }

        /*public bool RemoveUnusedDatastructures()
        {
            LinkedList<TopoEdge> removeEdges = new LinkedList<TopoEdge>();
            LinkedList<TopoVertex> removeVertices = new LinkedList<TopoVertex>();
            foreach (TopoEdge edge in edges)
            {
                if (edge.connectedFaces == 0)
                    removeEdges.AddLast(edge);
            }
            foreach (TopoVertex vertex in vertices)
            {
                if (vertex.connectedFaces == 0)
                    removeVertices.AddLast(vertex);
            }
            bool changed = removeEdges.Count > 0 || removeVertices.Count > 0;
            foreach (TopoEdge edge in removeEdges)
                edges.Remove(edge);
            foreach (TopoVertex vertex in removeVertices)
                vertices.Remove(vertex);
            int vertexNumber = 1;
            foreach (TopoVertex vertex in vertices)
                vertex.id = vertexNumber++;
            return changed;
        }*/

        /*public void ResetTriangleMarker()
        {
            foreach (TopoTriangle triangle in triangles)
            {
                triangle.algHelper = 0;
            }
        }
        
        public bool RemoveDegeneratedFaces()
        {
            bool changed = false;
            HashSet<TopoTriangle> deleteFaces = new HashSet<TopoTriangle>();
            int defectTriangles = 0;
            int isolatedTriangles = 0;
            int doubleTriangles = 0;
            ResetTriangleMarker();
            foreach (TopoTriangle triangle in triangles)
            {
                if (triangle.IsIsolated())
                {
                    isolatedTriangles++;
                    deleteFaces.Add(triangle);
                }
                else if (triangle.IsDegenerated())
                {
                    defectTriangles++;
                    deleteFaces.Add(triangle);
                }

                HashSet<TopoTriangle> candidates = triangles.FindIntersectionCandidates(triangle);
                if (triangle.algHelper == 0)
                    foreach (TopoTriangle candidate in triangle.vertices[0].connectedFacesList)
                    {
                        if (triangle.NumberOfSharedVertices(candidate) == 3)
                        {
                            triangle.algHelper = 1;
                            if (candidate.algHelper == 0)
                            {
                                deleteFaces.Add(candidate);
                                doubleTriangles++;
                            }
                        }
                    }
            }
            changed = deleteFaces.Count > 0;
            foreach (TopoTriangle face in deleteFaces)
            {
                face.Unlink(this);
                triangles.Remove(face);
            }
            if (defectTriangles > 0)
                RLog.info(Trans.T("L_REMOVED_DEGENERATED") + defectTriangles);
            if (isolatedTriangles > 0)
                RLog.info(Trans.T("L_REMOVED_ISOLATED") + isolatedTriangles);
            if (doubleTriangles > 0)
                RLog.info(Trans.T("L_REMOVED_DOUBLE") + doubleTriangles);
            if (changed) intersectionsUpToDate = false;
            return changed;
        }*/

        public void Analyse()
        {
            MessageBox.Show("Analyse");
            //RepairUnobtrusive();
            UpdateIntersectingTriangles();
            CheckNormals();
            manyShardEdges = 0;
            loopEdges = 0;
            foreach (TopoEdge edge in edges)
            {
                if (edge.connectedFaces < 2)
                    loopEdges++;
                else if (edge.connectedFaces > 2)
                    manyShardEdges++;
            }
            if (loopEdges + manyShardEdges == 0)
            {
                manifold = true;
            }
            else
            {
                manifold = false;
                MessageBox.Show(Trans.T("M_OBJECT_IS_NON_MANIFOLD"), Trans.T("W_OBJECT_IS_NON_MANIFOLD"));
            }
            UpdateVertexNumbers();
            /*if (false)
            {
                Debug.WriteLine("Intersecting triangles:");
                foreach (TopoTriangle t in intersectingTriangles)
                    Debug.WriteLine(t);
                Debug.WriteLine("========");
            }*/
        }

        /*public void RetestIntersectingTriangles()
        {
            foreach (TopoTriangle t in intersectingTriangles)
            {
                TopoTriangle hit = IntersectsTriangleAnyTriangle(t);
                if (hit != null)
                {
                    Debug.WriteLine(t);
                    Debug.WriteLine("Hits:" + hit);
                }
            }
        }
        
        public TopoTriangle IntersectsTriangleAnyTriangle(TopoTriangle test)
        {
            HashSet<TopoTriangle> candidates = triangles.FindIntersectionCandidates(test);
            foreach (TopoTriangle candidate in candidates)
            {
                if (test.Intersects(candidate)) return candidate;
            }
            return null;
        }*/

        public void checkEdgesOver2()
        {
            foreach (TopoEdge e in edges)
            {
                if (e.connectedFaces > 2)
                {
                    Console.WriteLine("Too many connected faces");
                    return;
                }
            }
        }

        /*public void updateBad()
        {
            badTriangles = badEdges = 0;
            foreach (TopoTriangle triangle in triangles)
            {
                triangle.bad = triangle.hasIntersections; // intersectingTriangles.Contains(triangle);
            }
            foreach (TopoEdge edge in edges)
            {
                if (edge.connectedFaces != 2)
                {
                    badEdges++;
                }
            }
            foreach (TopoTriangle triangle in triangles)
            {
                if (triangle.bad)
                    badTriangles++;
            }
        }
        
        private void FloodFillTriangles(TopoTriangle tri, int value)
        {
            tri.algHelper = value;
            HashSet<TopoTriangle> front = new HashSet<TopoTriangle>();
            front.Add(tri);
            HashSet<TopoTriangle> newFront = new HashSet<TopoTriangle>();
            int i;
            while (front.Count > 0)
            {
                foreach (TopoTriangle t in front)
                {
                    for (i = 0; i < 3; i++)
                    {
                        foreach (TopoTriangle test in t.edges[i].faces)
                        {
                            if (test.algHelper == 0)
                            {
                                test.algHelper = value;
                                newFront.Add(test);
                            }
                        }
                    }
                }
                front = newFront;
                newFront = new HashSet<TopoTriangle>();
            }
        }
        
        private void FloodFillShells(TopoTriangle tri, int value)
        {
            tri.shell = value;
            HashSet<TopoTriangle> front = new HashSet<TopoTriangle>();
            front.Add(tri);
            HashSet<TopoTriangle> newFront = new HashSet<TopoTriangle>();
            int i;
            while (front.Count > 0)
            {
                foreach (TopoTriangle t in front)
                {
                    for (i = 0; i < 3; i++)
                    {
                        foreach (TopoTriangle test in t.edges[i].faces)
                        {
                            if (test.shell == 0)
                            {
                                test.shell = value;
                                newFront.Add(test);
                            }
                        }
                    }
                }
                front = newFront;
                newFront = new HashSet<TopoTriangle>();
            }
        }
        
        public int CountShells()
        {
            foreach (TopoTriangle t in triangles)
                t.shell = 0;
            int nShells = 0;
            foreach (TopoTriangle t in triangles)
            {
                if (t.shell == 0)
                {
                    nShells++;
                    FloodFillShells(t, nShells);
                }
            }
            return nShells;
        }*/

        public List<TopoModel> SplitIntoSurfaces()
        {
            /*CountShells();
            foreach (TopoTriangle tri in triangles)
                tri.algHelper = tri.shell;

            List<TopoModel> models = new List<TopoModel>();
            Dictionary<int, TopoModel> modelMap = new Dictionary<int, TopoModel>();
            foreach (TopoTriangle tri in triangles)
            {
                int shell = tri.algHelper;
                if (modelMap.ContainsKey(shell))
                {
                    RHVector3 v1 = tri.vertices[0].pos;
                    RHVector3 v2 = tri.vertices[1].pos;
                    RHVector3 v3 = tri.vertices[2].pos;
                    RHVector3 n = tri.normal;
                    modelMap[shell].addTriangle(v1.x, v1.y, v1.z, v2.x, v2.y, v2.z, v3.x, v3.y, v3.z, n.x, n.y, n.z);
                }
                else
                {
                    List<TopoTriangleDistance> intersections = new List<TopoTriangleDistance>();
                    RHVector3 lineStart = tri.Center;
                    RHVector3 lineDirection = tri.normal;
                    double delta;
                    foreach (TopoTriangle test in triangles)
                    {
                        if(test.IntersectsLine(lineStart, lineDirection, out delta))
                        {
                            intersections.Add(new TopoTriangleDistance(delta,test));
                        }
                    }
                    intersections.Sort();
                    Stack<TopoTriangleDistance> tdStack = new Stack<TopoTriangleDistance>();
                    foreach (TopoTriangleDistance td in intersections)
                    {
                        if (td.triangle == tri)
                        {
                            TopoModel m = null;
                            if ((tdStack.Count & 2) == 0)
                            {
                                m = new TopoModel();
                                models.Add(m);
                                modelMap.Add(shell, m);
                            }
                            else
                            {
                                int trueShell = tdStack.ElementAt(tdStack.Count-1).triangle.algHelper;
                                foreach (TopoTriangle t in triangles)
                                {
                                    if (t.algHelper == shell)
                                        t.algHelper = trueShell;
                                }
                                if (modelMap.ContainsKey(trueShell))
                                    m = modelMap[trueShell];
                                else
                                {
                                    m = new TopoModel();
                                    models.Add(m);
                                    modelMap.Add(shell, m);
                                }
                            }
                            RHVector3 v1 = tri.vertices[0].pos;
                            RHVector3 v2 = tri.vertices[1].pos;
                            RHVector3 v3 = tri.vertices[2].pos;
                            RHVector3 n = tri.normal;
                            m.addTriangle(v1.x, v1.y, v1.z, v2.x, v2.y, v2.z, v3.x, v3.y, v3.z, n.x, n.y, n.z);
                            break;
                        }
                        else if (tdStack.Count > 0 && tdStack.Peek().triangle.algHelper == td.triangle.algHelper)
                        {
                            tdStack.Pop();
                        }
                        else
                        {
                            tdStack.Push(td);
                        }
                    }
                }
            }
            foreach (TopoModel m in models)
            {
                m.Analyse();
            }
            return models;*/
            return null;
        }

        public void CutMesh(Submesh mesh, RHVector3 normal, RHVector3 point, int defaultFaceColor)
        {
            TopoPlane plane = new TopoPlane(normal, point);
            bool drawEdges = Main.threeDSettings.ShowEdges;
            foreach (TopoEdge e in edges)
                e.algHelper = 0; // Mark drawn edges, so we insert them only once
            foreach (TopoTriangle t in triangles)
            {
                int side = plane.testTriangleSideFast(t);
                mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, defaultFaceColor);
                /*if (side == -1)
                {
                    mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, (t.bad ? Submesh.MESHCOLOR_ERRORFACE : defaultFaceColor));
                    if (drawEdges)
                    {
                        if (t.edges[0].algHelper == 0)
                        {
                            mesh.AddEdge(t.vertices[0].pos, t.vertices[1].pos, t.edges[0].connectedFaces == 2 ? Submesh.MESHCOLOR_EDGE : Submesh.MESHCOLOR_ERROREDGE);
                            t.edges[0].algHelper = 1;
                        }
                        if (t.edges[1].algHelper == 0)
                        {
                            mesh.AddEdge(t.vertices[1].pos, t.vertices[2].pos, t.edges[1].connectedFaces == 2 ? Submesh.MESHCOLOR_EDGE : Submesh.MESHCOLOR_ERROREDGE);
                            t.edges[1].algHelper = 1;
                        }
                        if (t.edges[2].algHelper == 0)
                        {
                            mesh.AddEdge(t.vertices[2].pos, t.vertices[0].pos, t.edges[2].connectedFaces == 2 ? Submesh.MESHCOLOR_EDGE : Submesh.MESHCOLOR_ERROREDGE);
                            t.edges[2].algHelper = 1;
                        }
                    }
                    else
                    {
                        if (t.edges[0].algHelper == 0 && t.edges[0].connectedFaces != 2)
                        {
                            mesh.AddEdge(t.vertices[0].pos, t.vertices[1].pos, Submesh.MESHCOLOR_ERROREDGE);
                            t.edges[0].algHelper = 1;
                        }
                        if (t.edges[1].algHelper == 0 && t.edges[1].connectedFaces != 2)
                        {
                            mesh.AddEdge(t.vertices[1].pos, t.vertices[2].pos, Submesh.MESHCOLOR_ERROREDGE);
                            t.edges[1].algHelper = 1;
                        }
                        if (t.edges[2].algHelper == 0 && t.edges[2].connectedFaces != 2)
                        {
                            mesh.AddEdge(t.vertices[2].pos, t.vertices[0].pos, Submesh.MESHCOLOR_ERROREDGE);
                            t.edges[2].algHelper = 1;
                        }
                    }
                }
                else if (side == 0)
                {
                    //plane.addIntersectionToSubmesh(mesh, t, drawEdges, (t.bad ? Submesh.MESHCOLOR_ERRORFACE : defaultFaceColor));
                }*/
            }
        }

        //--- MODEL_SLA	// milton
        /*
        public TopoEdge getMaxLengthEdge()
        {
            double maxEdgeLength = double.NegativeInfinity;
            TopoEdge edgeReturn = null;
            foreach(TopoTriangle tri in this.triangles.triangles){
                for (int i = 0; i < tri.vertices.Count(); i++)
                {
                    TopoVertex v1 = tri.vertices[i];
                    TopoVertex v2 = tri.vertices[(i + 1) % tri.vertices.Count()];
                    RHVector3 edge = v1.pos.Subtract(v2.pos);
                    if (edge.Length > maxEdgeLength)
                    {
                        maxEdgeLength = edge.Length;
                        edgeReturn = new TopoEdge(v1,v2);
                    }
                }
            }
            return edgeReturn;
        }*/

        public void getTriInWorld(Matrix4 trans, TopoTriangle tInObj, out TopoTriangle tInWorld)
        {
            /*Vector4 ver1 = new Vector4((float)tInObj.vertices[0].pos.x, (float)tInObj.vertices[0].pos.y, (float)tInObj.vertices[0].pos.z, 1);
            Vector4 ver2 = new Vector4((float)tInObj.vertices[1].pos.x, (float)tInObj.vertices[1].pos.y, (float)tInObj.vertices[1].pos.z, 1);
            Vector4 ver3 = new Vector4((float)tInObj.vertices[2].pos.x, (float)tInObj.vertices[2].pos.y, (float)tInObj.vertices[2].pos.z, 1);*/
            Vector4 ver1 = tInObj.vertices[0].pos.asVector4();
            Vector4 ver2 = tInObj.vertices[1].pos.asVector4();
            Vector4 ver3 = tInObj.vertices[2].pos.asVector4();
            ver1 = Vector4.Transform(ver1, trans);
            ver2 = Vector4.Transform(ver2, trans);
            ver3 = Vector4.Transform(ver3, trans);
            TopoVertex v1 = new TopoVertex(0, new RHVector3(ver1.X, ver1.Y, ver1.Z));
            TopoVertex v2 = new TopoVertex(1, new RHVector3(ver2.X, ver2.Y, ver2.Z));
            TopoVertex v3 = new TopoVertex(2, new RHVector3(ver3.X, ver3.Y, ver3.Z));
            tInWorld = new TopoTriangle(v1, v2, v3);
        }

        /*
        public void FillMeshOverhang(ref LinkedList<ThreeDModel> supModels, Matrix4 modelMx, Submesh mesh, int defaultColor, double overhangThreshhold)
        {
            TopoTriangle tInWorld = null;
            //if (prevSupPartCnt != supModels.Count) ;
            TopoEdge maxEdge = getMaxLengthEdge();
            double maxEdgeLen = 15;
            if( null!=maxEdge ){
                Vector4 v1Vec = maxEdge.v1.pos.asVector4();
                v1Vec = Vector4.Transform(v1Vec, modelMx);
                Vector4 v2Vec = maxEdge.v2.pos.asVector4();
                v2Vec = Vector4.Transform(v2Vec, modelMx);
                maxEdgeLen = new RHVector3(v1Vec.X - v2Vec.X, v1Vec.Y - v2Vec.Y, v1Vec.Z - v2Vec.Z).Length/2.7;
                //Debug.WriteLine("Max Edge length in world is " + maxEdgeLen + " v1 " + maxEdge.v1.pos + "v2 " + maxEdge.v2.pos);
            }
            foreach (TopoTriangle t in triangles)
            {
                // We can decide here the normal 
                getTriInWorld(modelMx, t, out tInWorld);
                if (tInWorld.normal.z > overhangThreshhold || tInWorld.Center.z < 2)
                    mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, defaultColor);
                else
                {
                    mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, Submesh.MESHCOLOR_OUTSIDE);
                    // Get max edge length of this triangle
                    double localMaxEdgeLen = double.NegativeInfinity;
                    for (int cIdx = 0; cIdx < tInWorld.vertices.Count(); cIdx++)
                    {
                        TopoVertex v1 = tInWorld.vertices[cIdx];
                        TopoVertex v2 = tInWorld.vertices[(cIdx + 1) % 3];
                        RHVector3 edge = v1.pos.Subtract(v2.pos);
                        if (edge.Length > localMaxEdgeLen)
                            localMaxEdgeLen = edge.Length;
                    }
                    
                    // Find the closest sup head
                    Double[] minDistList = new Double[3] { Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity };
                    SupportModel[] nearestSupHeadList = new SupportModel[3]{null, null, null};

                    foreach (ThreeDModel sup in supModels)
                    {
                        if ((int)SupportInformation.ModelType.HEAD == ((SupportModel)sup).supType)
                        {
                            for (int vIdx = 0; vIdx < tInWorld.vertices.Count(); vIdx++)
                            {
                                TopoVertex vInWor = tInWorld.vertices[vIdx];
                                RHVector3 dist = null;
                                /*if ( ((SupportModel)sup).originalModel.isFromImport )
                                {   // These come from imported supports
                                    PrintModel printSup = (PrintModel)sup;
                                    Vector4 supV0Vec4 = printSup.originalModel.vertices.v[0].pos.asVector4();
                                    supV0Vec4 = Vector4.Transform(supV0Vec4, printSup.trans);
                                    //Debug.WriteLine("Sup head v0 world pos " + supV0Vec4.X + " " + supV0Vec4.Y + " " + supV0Vec4.Z);
                                    dist = new RHVector3(supV0Vec4.X - vInWor.pos.x,
                                                         supV0Vec4.Y - vInWor.pos.y,
                                                         supV0Vec4.Z - vInWor.pos.z);
                                }
                                else//*
                                {
                                    //Debug.WriteLine("Sup head Position " +sup.Position .x + " " + sup.Position.y + " " + sup.Position.z + " t in World " + tInWorld.Center);
                                    dist = new RHVector3(sup.Position.x - vInWor.pos.x,
                                                         sup.Position.y - vInWor.pos.y,
                                                         sup.Position.z - vInWor.pos.z);
                                }

                                if (dist.Length < minDistList[vIdx])
                                {
                                    minDistList[vIdx] = dist.Length;
                                    nearestSupHeadList[vIdx] = (SupportModel)sup;
                                }
                            }
                        }
                    }

                    for (int vIdx = 0; vIdx < tInWorld.vertices.Count(); vIdx++)
                    {
                        TopoVertex vInWor = tInWorld.vertices[vIdx];
                        if (null == nearestSupHeadList[vIdx]) continue;
                        Double dist = minDistList[vIdx];
                        SupportModel supHead = nearestSupHeadList[vIdx];
                        /*
                        RHVector3 dist = new RHVector3(sup.Position.x - vInWor.pos.x,
                                                        sup.Position.y - vInWor.pos.y,
                                                        sup.Position.z - vInWor.pos.z);//*

                        double edgeLevel = maxEdgeLen;

                        /*if (localMaxEdgeLen < (maxEdgeLen / 3))
                            edgeLevel = (maxEdgeLen / 3);
                        else if (localMaxEdgeLen < (maxEdgeLen / 2))
                            edgeLevel = (maxEdgeLen / 2);
                        else
                            edgeLevel = maxEdgeLen;

                        if (dist < edgeLevel)
                        {
                            int gradient = (int)Math.Round(255 * dist / edgeLevel);
                            gradient = (gradient < 210) ? 0 : gradient;
                            //Debug.WriteLine("Gradient is " + gradient);
                            // the more distance, redder! the less distance, whiter! 
                            mesh.triangles[mesh.triangles.Count - 1].colors[vIdx] = Submesh.ColorToRgba32(Color.FromArgb(255, 255, 255 - gradient, 255 - gradient));
                        }
                    }// for (int vIdx = 0;
                }
            }//foreach (TopoTriangle t
        }*/
        //---

        // milton
        public void FillMeshCheckRAM(Matrix4 modelMx, Submesh mesh, int defaultColor)
        {
            int cnt = 0;
            TopoTriangle triWor = null;
            mesh.StartFillingMesh(triangles.Count, edges.Count);
            foreach (TopoTriangle t in triangles)
            {
                //if (0 == cnt % 4000)
                //{
                //    //ulong totalRam = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
                //    ulong availRam = new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory / 1024 / 1024;
                //    if (availRam < Main.SWMemoryRemainMin || Main.main.getCurMemoryUsed() >= Main.SWMemoryUsedLimit)
                //    {
                //        throw new System.OutOfMemoryException();
                //    }
                //}
                getTriInWorld(modelMx, t, out triWor);
                // Steven
#if DEBUG_MODE
                if ((triWor.normal.z) < ThreeDControl.OVERHANG_THRESHOLD)
                    mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, Submesh.MESHCOLOR_PINK);
                else
#endif
                    mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, defaultColor);
                cnt++;
            }
            mesh.EndFillingMesh();
        }

        public void FillMeshCheckRAM(Matrix4 modelMx, Submesh mesh, int defaultColor, int index)
        {
            int cnt = 0;
            TopoTriangle triWor = null;
            mesh.StartFillingMesh(split_triangles[index].Count, edges.Count);
            foreach (TopoTriangle t in split_triangles[index])
            {
                //if (0 == cnt % 4000)
                //{
                //    //ulong totalRam = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
                //    ulong availRam = new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory / 1024 / 1024;
                //    if (availRam < Main.SWMemoryRemainMin || Main.main.getCurMemoryUsed() >= Main.SWMemoryUsedLimit)
                //    {
                //        throw new System.OutOfMemoryException();
                //    }
                //}
                getTriInWorld(modelMx, t, out triWor);
                // Steven
#if DEBUG_MODE
                if ((triWor.normal.z) < ThreeDControl.OVERHANG_THRESHOLD)
                    mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, Submesh.MESHCOLOR_PINK);
                else
#endif
                    mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, defaultColor);
                cnt++;
            }
            mesh.EndFillingMesh();
        }

        public void FillMesh(Submesh mesh, int defaultColor)
        {
            bool drawEdges = Main.threeDSettings.ShowEdges;
            foreach (TopoTriangle t in triangles)
            {
                mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, defaultColor);
                /*mesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, (t.bad ? Submesh.MESHCOLOR_ERRORFACE : defaultColor));
                if (drawEdges)
                {
                    mesh.AddEdge(t.vertices[0].pos, t.vertices[1].pos, t.edges[0].connectedFaces == 2 ? Submesh.MESHCOLOR_EDGE : Submesh.MESHCOLOR_ERROREDGE);
                    mesh.AddEdge(t.vertices[1].pos, t.vertices[2].pos, t.edges[1].connectedFaces == 2 ? Submesh.MESHCOLOR_EDGE : Submesh.MESHCOLOR_ERROREDGE);
                    mesh.AddEdge(t.vertices[2].pos, t.vertices[0].pos, t.edges[2].connectedFaces == 2 ? Submesh.MESHCOLOR_EDGE : Submesh.MESHCOLOR_ERROREDGE);
                }
                else
                {
                    if(t.edges[0].connectedFaces != 2)
                        mesh.AddEdge(t.vertices[0].pos, t.vertices[1].pos, Submesh.MESHCOLOR_ERROREDGE);
                    if (t.edges[1].connectedFaces != 2)
                        mesh.AddEdge(t.vertices[1].pos, t.vertices[2].pos, Submesh.MESHCOLOR_ERROREDGE);
                    if (t.edges[2].connectedFaces != 2)
                        mesh.AddEdge(t.vertices[2].pos, t.vertices[0].pos, Submesh.MESHCOLOR_ERROREDGE);
                }*/
            }
        }


        //<Carson(Taipei)><04-09-2019><Modified>
        public void FillMesh(Submesh mesh)
        {
            mesh.StartFillingMesh(split_triangles[0].Count, edges.Count);
            if (/*split_triangles[0].Count > 4000 9500*/ Main.main.threedview.ui.toolbar.SmoothShading.IsChecked)
            {
                foreach (TopoTriangle t in split_triangles[0])
                {
                    mesh.AddTriangleForSmooth(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos);
                }
            }
            else
            {
                foreach (TopoTriangle t in split_triangles[0])
                {
                    mesh.AddTriangleForFlat(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos);
                }
            }
            mesh.EndFillingMesh();
            mesh.UpdateColors(false);
        }
        //<><><>

        public void exportObj(string filename, bool withNormals)
        {
            FileStream fs = File.Open(filename, FileMode.Create);
            TextWriter w = new EnglishStreamWriter(fs);
            w.WriteLine("# exported by XYZware_Nobel");
            foreach (TopoVertex v in vertices)
            {
                w.Write("v ");
                w.Write(v.pos.x);
                w.Write(" ");
                w.Write(v.pos.y);
                w.Write(" ");
                w.WriteLine(v.pos.z);
            }
            int idx = 1;
            if (withNormals)
            {
                foreach (TopoTriangle t in triangles)
                {
                    w.Write("vn ");
                    w.Write(t.normal.x);
                    w.Write(" ");
                    w.Write(t.normal.y);
                    w.Write(" ");
                    w.WriteLine(t.normal.z);
                }
            }
            foreach (TopoTriangle t in triangles)
            {
                w.Write("f ");
                w.Write(t.vertices[0].id);
                if (withNormals)
                {
                    w.Write("//");
                    w.Write(idx);
                }
                w.Write(" ");
                w.Write(t.vertices[1].id);
                if (withNormals)
                {
                    w.Write("//");
                    w.Write(idx);
                }
                w.Write(" ");
                w.Write(t.vertices[2].id);
                if (withNormals)
                {
                    w.Write("//");
                    w.Write(idx);
                }
                w.WriteLine();
            }
            w.Close();
            fs.Close();
        }

        public void exportSTL(string filename, bool binary, bool DomodelRepair)
        {
            /*
             * <GELO><For debugging only - remove later>
             */
            //binary = false;
            //<><><>

            float zoomX = (float)1.03;
            float zoomY = (float)1.03;
            float zoomZ = (float)1.0115;


            FileStream fs = File.Open(filename, FileMode.Create);
            if (binary)
            {
                BinaryWriter w = new BinaryWriter(fs);
                int i;
                for (i = 0; i < 20; i++) w.Write((int)0);
                #region "Old implementation"
                //w.Write(triangles.Count);
                //foreach (TopoTriangle t in triangles)
                //{
                //    w.Write((float)t.normal.x);
                //    w.Write((float)t.normal.y);
                //    w.Write((float)t.normal.z);
                //    for (i = 0; i < 3; i++)
                //    {
                //        w.Write((float)t.vertices[i].pos.x);
                //        w.Write((float)t.vertices[i].pos.y);
                //        w.Write((float)t.vertices[i].pos.z);
                //    }
                //    w.Write((short)0);
                //}
                #endregion

                w.Write(split_triangles[0].Count + split_triangles[1].Count + split_triangles[2].Count + split_triangles[3].Count);
                for (int ii = 0; ii < 4; ii++)
                {
                    foreach (TopoTriangle t in split_triangles[ii])
                    {
                        w.Write((float)t.normal.x);
                        w.Write((float)t.normal.y);
                        w.Write((float)t.normal.z);
                        for (i = 0; i < 3; i++)
                        {
                            if (Main.main.threedview.ui.MaterialSelection_UI.chk_AutoCalculate.IsChecked == true)
                            {
                                t.vertices[i].pos.x *= zoomX;
                                t.vertices[i].pos.y *= zoomY;
                                t.vertices[i].pos.z *= zoomZ;
                            }

                            w.Write((float)t.vertices[i].pos.x);
                            w.Write((float)t.vertices[i].pos.y);
                            w.Write((float)t.vertices[i].pos.z);

                        }
                        w.Write((short)0);
                    }
                }

                w.Close();
            }
            else
            {
                TextWriter w = new EnglishStreamWriter(fs);
                w.WriteLine("solid XYZ");

                #region "old implementation"
                //foreach (TopoTriangle t in triangles)
                //{
                //    w.Write("  facet normal ");
                //    w.Write(t.normal.x);
                //    w.Write(" ");
                //    w.Write(t.normal.y);
                //    w.Write(" ");
                //    w.WriteLine(t.normal.z);
                //    w.WriteLine("    outer loop");
                //    w.Write("      vertex ");
                //    w.Write(t.vertices[0].pos.x);
                //    w.Write(" ");
                //    w.Write(t.vertices[0].pos.y);
                //    w.Write(" ");
                //    w.WriteLine(t.vertices[0].pos.z);
                //    w.Write("      vertex ");
                //    w.Write(t.vertices[1].pos.x);
                //    w.Write(" ");
                //    w.Write(t.vertices[1].pos.y);
                //    w.Write(" ");
                //    w.WriteLine(t.vertices[1].pos.z);
                //    w.Write("      vertex ");
                //    w.Write(t.vertices[2].pos.x);
                //    w.Write(" ");
                //    w.Write(t.vertices[2].pos.y);
                //    w.Write(" ");
                //    w.WriteLine(t.vertices[2].pos.z);
                //    w.WriteLine("    endloop");
                //    w.WriteLine("  endfacet");
                //}
                #endregion


                for (int ii = 0; ii <= 3; ii++)
                {
                    foreach (TopoTriangle t in split_triangles[ii])
                    {
                        w.Write("  facet normal ");
                        w.Write(t.normal.x);
                        w.Write(" ");
                        w.Write(t.normal.y);
                        w.Write(" ");
                        w.WriteLine(t.normal.z);
                        w.WriteLine("    outer loop");
                        w.Write("      vertex ");
                        w.Write(t.vertices[0].pos.x);
                        w.Write(" ");
                        w.Write(t.vertices[0].pos.y);
                        w.Write(" ");
                        w.WriteLine(t.vertices[0].pos.z);
                        w.Write("      vertex ");
                        w.Write(t.vertices[1].pos.x);
                        w.Write(" ");
                        w.Write(t.vertices[1].pos.y);
                        w.Write(" ");
                        w.WriteLine(t.vertices[1].pos.z);
                        w.Write("      vertex ");
                        w.Write(t.vertices[2].pos.x);
                        w.Write(" ");
                        w.Write(t.vertices[2].pos.y);
                        w.Write(" ");
                        w.WriteLine(t.vertices[2].pos.z);
                        w.WriteLine("    endloop");
                        w.WriteLine("  endfacet");

                    }
                }

                w.WriteLine("endsolid XYZware_SLS");
                w.Close();
            }
            if (DomodelRepair)
            {
                try
                {
                    if (Main.main.OS32bit == 1)
                    {
                        modelRepair(filename);
                    }
                    else
                    {
                        modelRepair64(filename);
                    }
                }
                catch { }
            }
            fs.Close();
        }

        // added function which can export/saved a 3mf model. <Darwin 09/24/2018> [Start] <In Progress>
        #region Export3MF
        //public void export3MF(string filename, bool binary, bool DomodelRepair)
        //{
        //    FileStream fs = File.Open(filename, FileMode.Create);
        //    if (binary)
        //    {
        //        BinaryWriter w = new BinaryWriter(fs);
        //        int i;
        //        for (i = 0; i < 20; i++) w.Write((int)0);

        //        w.Write(split_triangles[0].Count + split_triangles[1].Count + split_triangles[2].Count + split_triangles[3].Count);
        //        for (int ii = 0; ii < 4; ii++)
        //        {
        //            foreach (TopoTriangle t in split_triangles[ii])
        //            {
        //                w.Write((float)t.normal.x);
        //                w.Write((float)t.normal.y);
        //                w.Write((float)t.normal.z);
        //                for (i = 0; i < 3; i++)
        //                {
        //                    w.Write((float)t.vertices[i].pos.x);
        //                    w.Write((float)t.vertices[i].pos.y);
        //                    w.Write((float)t.vertices[i].pos.z);
        //                }
        //                w.Write((short)0);
        //            }
        //        }

        //        w.Close();
        //    }
        //    else
        //    {
        //        TextWriter w = new EnglishStreamWriter(fs);
        //        w.WriteLine("solid XYZ");

        //        for (int ii = 0; ii <= 3; ii++)
        //        {
        //            foreach (TopoTriangle t in split_triangles[ii])
        //            {
        //                w.Write("  facet normal ");
        //                w.Write(t.normal.x);
        //                w.Write(" ");
        //                w.Write(t.normal.y);
        //                w.Write(" ");
        //                w.WriteLine(t.normal.z);
        //                w.WriteLine("    outer loop");
        //                w.Write("      vertex ");
        //                w.Write(t.vertices[0].pos.x);
        //                w.Write(" ");
        //                w.Write(t.vertices[0].pos.y);
        //                w.Write(" ");
        //                w.WriteLine(t.vertices[0].pos.z);
        //                w.Write("      vertex ");
        //                w.Write(t.vertices[1].pos.x);
        //                w.Write(" ");
        //                w.Write(t.vertices[1].pos.y);
        //                w.Write(" ");
        //                w.WriteLine(t.vertices[1].pos.z);
        //                w.Write("      vertex ");
        //                w.Write(t.vertices[2].pos.x);
        //                w.Write(" ");
        //                w.Write(t.vertices[2].pos.y);
        //                w.Write(" ");
        //                w.WriteLine(t.vertices[2].pos.z);
        //                w.WriteLine("    endloop");
        //                w.WriteLine("  endfacet");
        //            }
        //        }

        //        w.WriteLine("endsolid XYZware_SLS");
        //        w.Close();
        //    }
        //    if (DomodelRepair)
        //    {
        //        try
        //        {
        //            if (Main.main.OS32bit == 1)
        //            {
        //                modelRepair(filename);
        //            }
        //            else
        //            {
        //                modelRepair64(filename);
        //            }
        //        }
        //        catch { }
        //    }
        //    fs.Close();
        //}
        #endregion
        //<><><>[End]


        private RHVector3 extractVector(string s)
        {
            RHVector3 v = new RHVector3(0, 0, 0);
            s = s.Trim().Replace("  ", " ");
            int p = s.IndexOf(' ');
            if (p < 0) throw new Exception("Format error");
            double.TryParse(s.Substring(0, p), NumberStyles.Float, XYZLib.XYZ.Model.GCode.format, out v.x);
            s = s.Substring(p).Trim();
            p = s.IndexOf(' ');
            if (p < 0) throw new Exception("Format error");
            double.TryParse(s.Substring(0, p), NumberStyles.Float, XYZLib.XYZ.Model.GCode.format, out v.y);
            s = s.Substring(p).Trim();
            double.TryParse(s, NumberStyles.Float, XYZLib.XYZ.Model.GCode.format, out v.z);
            return v;
        }

        private void ReadArray(Stream stream, byte[] data)
        {
            int offset = 0;
            int remaining = data.Length;
            while (remaining > 0)
            {
                int read = stream.Read(data, offset, remaining);
                if (read <= 0)
                    throw new EndOfStreamException
                        (String.Format("End of stream reached with {0} bytes left to read", remaining));
                remaining -= read;
                offset += read;
            }
        }

        public bool importObj(string filename)
        {
            clear();
            bool error = false;
            try
            {
                string[] text = System.IO.File.ReadAllText(filename).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                int vPos = 0;
                int vnPos = 0;
                int count = 0, countMax = text.Length;
                List<TopoVertex> vList = new List<TopoVertex>();
                List<RHVector3> vnList = new List<RHVector3>();
                foreach (string currentLine in text)
                {
                    count++;
                    //Progress((double)count / countMax);
                    if (count % 2000 == 0)
                    {
                        Application.DoEvents();
                        if (IsActionStopped()) return true;
                    }
                    string line = currentLine.Trim().ToLower();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    int p = line.IndexOf(" ");
                    if (p < 0) continue;
                    string cmd = line.Substring(0, p);
                    if (cmd == "v")
                    {
                        RHVector3 vert = extractVector(line.Substring(p + 1));
                        vList.Add(addVertex(vert));
                        vPos++;
                    }
                    else if (cmd == "vn")
                    {
                        RHVector3 vert = extractVector(line.Substring(p + 1));
                        vnList.Add(vert);
                        vnPos++;
                    }
                    else if (cmd == "f")
                    {
                        line = line.Substring(p + 1).Replace("  ", " ");
                        string[] parts = line.Split(new char[] { ' ' });
                        int[] vidx = new int[parts.Length];
                        int[] nidx = new int[parts.Length];
                        RHVector3 norm = null;
                        p = 0;
                        foreach (string part in parts)
                        {
                            string[] sp = part.Split('/');
                            if (sp.Length > 0)
                            {
                                int.TryParse(sp[0], out vidx[p]);
                                if (vidx[p] < 0) vidx[p] += vPos;
                                else vidx[p]--;
                                if (vidx[p] < 0 || vidx[p] >= vPos)
                                {
                                    error = true;
                                    break;
                                }
                            }
                            if (sp.Length > 2 && norm == null)
                            {
                                int.TryParse(sp[0], out nidx[p]);
                                if (nidx[p] < 0) nidx[p] += vnPos;
                                else nidx[p]--;
                                if (nidx[p] >= 0 && nidx[p] < vnPos)
                                    norm = vnList[nidx[p]];
                            }
                            p++;
                        }
                        for (int i = 2; i < parts.Length; i++)
                        {
                            TopoTriangle triangle = new TopoTriangle(this, vList[vidx[0]], vList[vidx[1]], vList[vidx[2]]);
                            if (norm != null && norm.ScalarProduct(triangle.normal) < 0)
                                triangle.FlipDirection();
                            AddTriangle(triangle);
                        }
                    }
                    if (error) break;
                }
            }
            catch
            {
                error = true;
            }
            if (error)
            {
                clear();
            }
            return error;
        }

        private void importSTLAscii(string filename)
        {
            try
            {
                Stream stream = new FileStream(filename, FileMode.Open);
                long chars = stream.Length;
                long est = chars / 1024;
                stream.Close();

                string text = System.IO.File.ReadAllText(filename);

                #region "After File.ReadAllText()"
                int lastP = 0, p, pend, normal, outer, vertex, vertex2;
                int count = 0, max = text.Length;

                while ((p = text.IndexOf("facet", lastP)) > 0)
                {
                    count++;

                    if (count % 4000 == 0)
                    {
                        Main.main.threedview.ui.BusyWindow.busyProgressbar.Value = ((double)lastP / max) * 100.0;
                        Application.DoEvents();
                        //if (IsActionStopped()) return;
                        if (Main.main.threedview.ui.BusyWindow.killed) return;
                    }

                    pend = text.IndexOf("endfacet", p + 5);
                    normal = text.IndexOf("normal", p) + 6;
                    outer = text.IndexOf("outer loop", normal);
                    RHVector3 normalVect = extractVector(text.Substring(normal, outer - normal));
                    normalVect.NormalizeSafe();
                    outer += 10;
                    vertex = text.IndexOf("vertex", outer) + 6;
                    vertex2 = text.IndexOf("vertex", vertex);
                    RHVector3 p1 = extractVector(text.Substring(vertex, vertex2 - vertex));
                    vertex2 += 7;
                    vertex = text.IndexOf("vertex", vertex2);
                    RHVector3 p2 = extractVector(text.Substring(vertex2, vertex - vertex2));
                    vertex += 7;
                    vertex2 = text.IndexOf("endloop", vertex);
                    RHVector3 p3 = extractVector(text.Substring(vertex, vertex2 - vertex));
                    lastP = pend + 8;
                    addTriangle(p1, p2, p3, normalVect);
                }
                #endregion
            }
            catch (OutOfMemoryException out_of_mem_ex)
            {
                System.Diagnostics.Debug.WriteLine("Could not allocate memory for the string.");
            }
        }

        private void importSTLAscii2(string filename)
        {
            IsLargeModel = false;

            Stream stream = new FileStream(filename, FileMode.Open);
            long chars = stream.Length;
            long est = chars / 1024;
            stream.Close();

            string outer_loop = "outer loop";
            string end_loop = "endloop";
            int vertex_count = 0;
            RHVector3[] facet = new RHVector3[3];
            int facet_count = 0;
            int total_facet_count = 0;
            int slice = 0;
            int index = 0;

            using (StreamReader sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    if (total_facet_count % 10000 == 0)
                    {
                        Main.main.threedview.ui.BusyWindow.busyProgressbar.Value = 0.0;
                        Application.DoEvents();
                        //if (IsActionStopped()) return;
                        if (Main.main.threedview.ui.BusyWindow.killed) return;
                    }

                    if (sr.ReadLine().ToLower().Trim() == outer_loop)
                    {
                        total_facet_count++;
                    }
                }
            }

            //<Carson(Taipei)><01-22-2019><Removed - Cancel split model to reduce memory usage. All models are "IsLargeModel = false" now.>
            //if (total_facet_count >= 8000) IsLargeModel = true;
            //<><><>

            slice = total_facet_count / 4;

            using (StreamReader sr = new StreamReader(filename))
            {
                string tmp = "";

                while (!sr.EndOfStream)
                {
                    if (sr.ReadLine().ToLower().Trim() == outer_loop)
                    {
                        index = facet_count / slice;
                        if (index > 3) index = 3;
                        facet_count++;

                        if (facet_count % 10000 == 0)
                        {
                            Main.main.threedview.ui.BusyWindow.busyProgressbar.Value = ((double)facet_count / (double)total_facet_count) * 100.0;
                            Application.DoEvents();
                            //if (IsActionStopped()) return;
                            if (Main.main.threedview.ui.BusyWindow.killed) return;
                        }

                        tmp = sr.ReadLine();

                        while (tmp != end_loop)
                        {
                            facet[vertex_count] = new RHVector3(stringVertexToVector3(tmp));
                            vertex_count++;
                            tmp = sr.ReadLine().Trim();
                        }

                        Vector3 vn = Vector3.Cross(facet[0].ToVector3() - facet[1].ToVector3(), facet[2].ToVector3() - facet[0].ToVector3());
                        vn.Normalize();
                        RHVector3 normal = new RHVector3(Convert.ToDouble(vn.X), Convert.ToDouble(vn.Y), Convert.ToDouble(vn.Z));

                        vertex_count = 0;

                        //Mark Add at 2020 09 18 
                        addTriangle(facet[0], facet[1], facet[2], normal, index);
                        //--Mark Hide at 2020 09 18
                        //if (IsLargeModel)
                        //    addTriangle(facet[0], facet[1], facet[2], normal, index);
                        //else
                        //    addTriangle(facet[0], facet[1], facet[2], normal, 0);
                        //-----

                        //addTriangle(facet[0], facet[1], facet[2], normal);



                    }
                }
                MergeMultiList();//Mark add at 2020 09 18
            }

            //System.Diagnostics.Debug.WriteLine(string.Format("Total facet count : {0}", total_facet_count));
            //for(int ii = 0; ii < 4; ii++)
            //{
            //    System.Diagnostics.Debug.WriteLine(string.Format("TriangleStorage[{0}] count : {1}", ii, split_triangles[ii].Count));
            //}
        }

        RHVector3 stringVertexToVector3(string _string_vertex)
        {
            string[] vertex = _string_vertex.Replace("vertex", "").Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            return new RHVector3(Convert.ToDouble(vertex[0]), Convert.ToDouble(vertex[1]), Convert.ToDouble(vertex[2]));
        }

        // milton
        public void importArr(ref byte[] stlArr)
        {
            MemoryStream stream = new MemoryStream();

            stream.Write(stlArr, 0, stlArr.Length);
            stream.Position = 0;

            byte[] header = new byte[80];
            ReadArray(stream, header);
            BinaryReader r = new BinaryReader(stream);
            int nTri = r.ReadInt32();

            try
            {
                for (int i = 0; i < nTri; i++)
                {
                    if (i > 0 && i % 4000 == 0)
                    {
                        Main.main.threedview.ui.BusyWindow.busyProgressbar.Value = ((double)i / nTri) * 100.0;
                        Application.DoEvents();
                        if (IsActionStopped()) return;
                        if (Main.main.threedview.ui.BusyWindow.killed) return;
                    }
                    //timer.Start();
                    RHVector3 normal = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    RHVector3 p1 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    RHVector3 p2 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    RHVector3 p3 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    normal.NormalizeSafe();
                    addTriangle(p1, p2, p3, normal);
                    //timer.Stop();
                    r.ReadUInt16();
                }
                r.Close();
                stream.Close();
                //showTime("addTriangle(p1, p2, p3, normal)");
                //stopWatch.Reset();
            }
            catch
            {
                //MessageBox.Show(Trans.T("M_LOAD_STL_FILE_ERROR"), Trans.T("W_LOAD_STL_FILE_ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                //Tonton <04-01-19> Modify messagebox for incompatible format
                Main.main.threedview.ui.FileFormat.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void importSTLWOCatch(string filename)
        {
            StartAction("L_LOADING...");
            clear();
            FileStream f = null;
            BinaryReader r = null;

            int total_facet_count = 0;

            try
            {
                //if (Path.GetExtension(filename) == ".3mf")
                //{
                //    load3mf(filename);
                //    return;
                //}

                bool isAscii = isTrueASCII(filename);
                f = File.OpenRead(filename);
                byte[] header = new byte[80];
                ReadArray(f, header);
                r = new BinaryReader(f);
                int nTri = r.ReadInt32();
                if ((f.Length != 84 + nTri * 50) && isAscii)
                {
                    r.Close();
                    f.Close();
                    //importSTLAscii(filename);
                    importSTLAscii2(filename);
                }
                else
                {
                    //<Carson(Taipei)><01-22-2019><Removed - Cancel split model to reduce memory usage. All models are "IsLargeModel = false" now.>
                    //if (nTri > 8000)
                    //{
                    //    IsLargeModel = true;
                    //}
                    //<><><>

                    int slice = nTri / 4;
                    int index = 0;

                    for (int i = 0; i < nTri; i++)
                    {
                        index = i / slice;
                        if (index > 3) index = 3;
                        //<Carson(Taipei)><01-22-2019><Modified>
                        //--- MODEL_SLA	// milton
                        if (i > 0 && i % 10000 == 0)
                        {
                            //if (Main.main.objectPlacement.totalFacetCount3mf > 0)
                            //{
                            //    Main.main.objectPlacement.facetCounter3mf += 10000;
                            //    Main.main.threedview.ui.BusyWindow.busyProgressbar.Value = ((double)Main.main.objectPlacement.facetCounter3mf / (double)Main.main.objectPlacement.totalFacetCount3mf) * 100.0;                                
                            //}                                
                            //else
                            Main.main.threedview.ui.BusyWindow.busyProgressbar.Value = ((double)i / nTri) * 100.0;

                            Application.DoEvents();
                            //if (IsActionStopped()) return;
                            if (Main.main.threedview.ui.BusyWindow.killed) return;

                            //ulong totalRam = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
                            //ulong availRam = new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory / 1024 / 1024;
                            //if (availRam < Main.SWMemoryRemainMin || Main.main.getCurMemoryUsed() >= Main.SWMemoryUsedLimit)
                            //{
                            //    throw new System.OutOfMemoryException();
                            //}
                        }
                        //---
                        //<><><>

                        RHVector3 normal = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        RHVector3 p1 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        RHVector3 p2 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        RHVector3 p3 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        normal.NormalizeSafe();

                        ////addTriangle(p1, p2, p3, normal);
                        
                        //if (IsLargeModel)
                        //    addTriangle(p1, p2, p3, normal, index);
                        //else
                        //    addTriangle(p1, p2, p3, normal, 0);

                        
                        addTriangle(p1, p2, p3, normal, index);
                        
                        


                        // Console.WriteLine("STL {0} triangle", i);
                        //Debug.WriteLine(i.ToString());
                        r.ReadUInt16();
                        //if(index > 4000000 && index%100000 == 0)
                        //{
                        //    split_trianglesTrimExcess();
                        //    Console.WriteLine("STL {0} triangle", i);
                        //}
                            
                        //if (r.BaseStream.Position == r.BaseStream.Length)
                        //{
                            
                        //    Console.WriteLine("END FILE");
                        //}

                    }

                }
            } // let the upper methods catch the exception
            finally
            {
                MergeMultiList();

                if (r != null)
                    r.Close();

                if (f != null)
                    f.Close();
            }
            
            
        }

        //void load3mf(string filePath)
        //{
        //    try
        //    {
        //        var existingZipFile = filePath;
        //        var targetDirectory = Main.main.Temp_folder;

        //        if (Directory.Exists(Main.main.Temp_folder + @"\3D"))
        //        {
        //            deleteAll(Main.main.Temp_folder + @"\3D");
        //        }

        //        string exePath = System.Windows.Forms.Application.StartupPath;

        //        if (Main.main.OS32bit == 1)
        //            SevenZipExtractor.SetLibraryPath(Path.Combine(exePath, @"x86\x867z.dll"));
        //        else
        //            SevenZipExtractor.SetLibraryPath(Path.Combine(exePath, @"x64\647z.dll"));

        //        using (SevenZipExtractor zip = new SevenZipExtractor(existingZipFile))
        //        {
        //            zip.ExtractFiles(targetDirectory, @"3D\3dmodel.model");
        //        }
               

        //        if (File.Exists(targetDirectory + @"\3D\3dmodel.model"))
        //        {
        //            List<TopoVertex> vListTemp = new List<TopoVertex>();
        //            XmlReader myReader = XmlReader.Create(targetDirectory + @"\3D\3dmodel.model");
        //            int ttt = myReader.Depth;
                    
        //            FileInfo info = new FileInfo(filePath);
        //            int lastP = 0;
        //            long max = info.Length;

        //            while (myReader.Read())
        //            {
        //                lastP += 20;
        //                Application.DoEvents();
        //                //System.Threading.Thread.Sleep(1);
        //                if (IsActionStopped()) return;
        //                if (Main.main.threedview.ui.BusyWindow.killed) return;

        //                Main.main.threedview.ui.BusyWindow.busyProgressbar.Value = ((double)lastP / max) * 100.0;

        //                if (myReader.NodeType == System.Xml.XmlNodeType.Element)
        //                {
        //                    //Debug.WriteLine(myReader.Name);

        //                    if (myReader.Name == "object")
        //                        vListTemp = new List<TopoVertex>();

        //                    if (myReader.Name == "vertex")
        //                    {
        //                        double tx = Convert.ToDouble(myReader.GetAttribute("x"));
        //                        double ty = Convert.ToDouble(myReader.GetAttribute("y"));
        //                        double tz = Convert.ToDouble(myReader.GetAttribute("z"));
        //                        RHVector3 vert = new RHVector3(tx, ty, tz);
        //                        vListTemp.Add(addVertex(vert));
        //                    }

        //                    if (myReader.Name == "triangle")
        //                    {
        //                        int[] vidx = new int[3];
        //                        vidx[0] = Convert.ToInt32(myReader.GetAttribute("v1"));
        //                        vidx[1] = Convert.ToInt32(myReader.GetAttribute("v2"));
        //                        vidx[2] = Convert.ToInt32(myReader.GetAttribute("v3"));
        //                        TopoTriangle triangle = new TopoTriangle(this, vListTemp[vidx[0]], vListTemp[vidx[1]], vListTemp[vidx[2]]);
        //                        //AddTriangle(triangle);
        //                        AddTriangle(triangle, 0);
        //                    }
        //                }
        //            }          
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(Trans.T("W_LOAD_STL_FILE_ERROR"), Trans.T("L_PLA_ERROR_TITLE"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        void deleteAll(string folderPath)
        {
            foreach (var item in Directory.GetFiles(folderPath))
            {
                File.Delete(item);
            }

            foreach (var item in Directory.GetDirectories(folderPath))
            {
                deleteAll(item);
                Directory.Delete(item);
            }
        }

        //<Carson(Taipei)><05-07-2019><Added>
        public void IndependentMeshData()
        {
            MeshData newMeshData = new MeshData();
            if (meshdata.nRefObject > 1) meshdata.nRefObject--;
            foreach (TopoVertex v in meshdata.vertices)
            {
                newMeshData.vertices.Add(new TopoVertex(v.id, v.pos), true);
            }
            foreach (TopoTriangle t in meshdata.split_triangles[0])
            {
                TopoVertex v1 = newMeshData.vertices.SearchPoint(t.vertices[0].pos);
                TopoVertex v2 = newMeshData.vertices.SearchPoint(t.vertices[1].pos);
                TopoVertex v3 = newMeshData.vertices.SearchPoint(t.vertices[2].pos);
                RHVector3 n = new RHVector3(t.normal);
                newMeshData.split_triangles[0].Add(new TopoTriangle(this, v1, v2, v3, n.x, n.y, n.z));
            }
            newMeshData.nRefObject = 1;
            meshdata = newMeshData;
        }
        //<><><>

        public void importSTL(string filename)
        {
            StartAction("L_LOADING...");
            clear();
            try
            {
                bool isAscii = isTrueASCII(filename);
                FileStream f = File.OpenRead(filename);
                byte[] header = new byte[80];
                ReadArray(f, header);
                BinaryReader r = new BinaryReader(f);
                int nTri = r.ReadInt32();
                if ((f.Length != 84 + nTri * 50) && isAscii)
                {
                    r.Close();
                    f.Close();
                    importSTLAscii(filename);
                }
                else
                {
                    for (int i = 0; i < nTri; i++)
                    {
                        //--- MODEL_SLA	// milton
                        if (i > 0 && i % 4000 == 0)
                        {
                            Main.main.threedview.ui.BusyWindow.busyProgressbar.Value = ((double)i / nTri) * 100.0;
                            Application.DoEvents();
                            if (IsActionStopped()) return;
                            if (Main.main.threedview.ui.BusyWindow.killed) return;
                        }
                        //---

                        //timer.Start();
                        RHVector3 normal = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        RHVector3 p1 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        RHVector3 p2 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        RHVector3 p3 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        normal.NormalizeSafe();
                        addTriangle(p1, p2, p3, normal);
                        //timer.Stop();
                        r.ReadUInt16();
                    }
                    r.Close();
                    f.Close();
                    //showTime("addTriangle(p1, p2, p3, normal)");
                    //stopWatch.Reset();
                }
            }
            catch
            {
                //MessageBox.Show(Trans.T("M_LOAD_STL_FILE_ERROR"), Trans.T("W_LOAD_STL_FILE_ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Tonton <04-01-19> Modify messagebox for incompatible format
                Main.main.threedview.ui.FileFormat.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private bool isTrueASCII(string filename)
        {
            // read first 256 bytes to check the STL type
            bool isAscii = true;
            long fileSize = new FileInfo(filename).Length;
            BinaryReader r = new BinaryReader(File.Open(filename, FileMode.Open));
            r.BaseStream.Position = 0;
            // look for control characters in first 256(or file size) bytes
            byte[] bytes = new byte[(int)((fileSize < 256L) ? fileSize : 256L)];
            bytes = r.ReadBytes(bytes.Length);
            for (int i = 0; i < bytes.Length; ++i)
            {
                if (bytes[i] > 127)
                {
                    isAscii = false; // this is a binary file
                    break;
                }
            }
            r.Close();
            r = null;

            return isAscii;
        }
    }
}
