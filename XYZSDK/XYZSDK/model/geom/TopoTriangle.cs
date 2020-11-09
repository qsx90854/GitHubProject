using System;
using System.Collections.Generic;

namespace XYZware_SLS.model.geom
{
    public class TopoTriangle
    {
        //public static bool debugIntersections = false;
        //public static double epsilonZero = 1e-7;
        //public static double epsilonZeroMinus = -1e-7;
        //public static double epsilonOneMinus = 1 - 1e-7;
        //public static double epsilonOnePlus = 1 + 1e-7;
        public TopoVertex[] vertices = new TopoVertex[3];
        //public TopoEdge[] edges = new TopoEdge[3];
        //public RHBoundingBox boundingBox = new RHBoundingBox();
        public RHVector3 normal;
        //public bool bad = false;
        //public bool hasIntersections = false;
        //public int algHelper;
        //public int shell = -1;

        //<Carson(Taipei)><12-14-2018><Remove - indices of triangle vertex>
        //public int[] indices = new int[3];
        //<><><>

        //--- MODEL_SLA
        public TopoTriangle(TopoVertex v1, TopoVertex v2, TopoVertex v3)
        {
            vertices[0] = v1;
            vertices[1] = v2;
            vertices[2] = v3;
            RecomputeNormal();
        }
        //---

        public TopoTriangle(TopoModel model,TopoVertex v1, TopoVertex v2, TopoVertex v3, double nx, double ny, double nz)
        {
            vertices[0] = v1;
            vertices[1] = v2;
            vertices[2] = v3;
            normal = new RHVector3(nx, ny, nz);
        }

        public TopoTriangle(TopoModel model, TopoVertex v1, TopoVertex v2, TopoVertex v3)
        {
            vertices[0] = v1;
            vertices[1] = v2;
            vertices[2] = v3;
            RecomputeNormal();
        }

        //<Carson(Taipei)><11-23-2018><Modified>
        public TopoTriangle(TopoVertex v1, TopoVertex v2, TopoVertex v3, double nx, double ny, double nz, int index1, int index2, int index3)
        {
            vertices[0] = v1;
            vertices[1] = v2;
            vertices[2] = v3;
            normal = new RHVector3(nx, ny, nz);
            //indices[0] = index1 - 1;
            //indices[1] = index2 - 1;
            //indices[2] = index3 - 1;
        }
        //<><><>

        public void Unlink(TopoModel model)
        {
            //edges[0].disconnectFace(this,model);
            //edges[1].disconnectFace(this,model);
            //edges[2].disconnectFace(this,model);
            //vertices[0].disconnectFace(this);
            //vertices[1].disconnectFace(this);
            //vertices[2].disconnectFace(this);
        }

        public void FlipDirection()
        {
            normal.Scale(-1);
            TopoVertex v = vertices[0];
            vertices[0] = vertices[1];
            vertices[1] = v;
            //TopoEdge e = edges[1];
            //edges[1] = edges[2];
            //edges[2] = e;
        }

        public void RecomputeNormal()
        {
            try
            {
                RHVector3 d1 = vertices[1].pos.Subtract(vertices[0].pos);
                RHVector3 d2 = vertices[2].pos.Subtract(vertices[1].pos);
                normal = d1.CrossProduct(d2);
                normal.NormalizeSafe();
            }
            catch(System.NullReferenceException)
            { }
        }

        public int VertexIndexFor(TopoVertex test)
        {
            if (test == vertices[0]) return 0;
            if (test == vertices[1]) return 1;
            if (test == vertices[2]) return 2;
            return -1;
        }

        public double SignedVolume()
        {
            return vertices[0].pos.ScalarProduct(vertices[1].pos.CrossProduct(vertices[2].pos)) / 6.0;
        }

        public double Area()
        {
            RHVector3 d1 = vertices[1].pos.Subtract(vertices[0].pos);
            RHVector3 d2 = vertices[2].pos.Subtract(vertices[1].pos);
            return 0.5* d1.CrossProduct(d2).Length;
        }

        public TopoEdge EdgeWithVertices(TopoVertex v1, TopoVertex v2)
        {
            /*foreach (TopoEdge e in edges)
            {
                if ((e.v1 == v1 && e.v2 == v2) || (e.v2 == v1 && e.v1 == v2))
                    return e;
            }*/
            return null;
        }

        public bool IsDegenerated()
        {
            if (vertices[0] == vertices[1] || vertices[1] == vertices[2] || vertices[2] == vertices[0]) 
                return true;
            return false;
        }

        /// <summary>
        /// Checks if all vertices are colinear preventing a normal computation. If point are coliniear the center vertex is
        /// moved in the direction of the edge to allow normal computations.
        /// </summary>
        /// <returns></returns>
        public bool CheckIfColinear()
        {
            RHVector3 d1 = vertices[1].pos.Subtract(vertices[0].pos);
            RHVector3 d2 = vertices[2].pos.Subtract(vertices[1].pos);
            double angle = d1.Angle(d2);
            if (angle > 0.001 && angle<Math.PI-0.001) return false;
            return true;
        }

        public int NumberOfSharedVertices(TopoTriangle tri)
        {
            int sameVertices = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (vertices[i] == tri.vertices[j])
                    {
                        sameVertices++;
                        break;
                    }
                }
            }
            return sameVertices;
        }

        public bool SameNormalOrientation(TopoTriangle test)
        {
            for (int i = 0; i < 3; i++)
            {
                for(int j =0;j<3;j++) {
                    if (vertices[i] == test.vertices[j] && vertices[(i + 1) % 3] == test.vertices[(j + 2) % 3])
                        return true;
                }
            }
            return false;
        }

        public RHVector3 Center
        {
            get
            {
                RHVector3 c = vertices[0].pos.Add(vertices[1].pos).Add(vertices[2].pos);
                c.Scale(1.0 / 3.0);
                return c;
            }
        }

        //--- MODEL_SLA	// milton
        // 實作論文的方法 - Fast, Minimum Storage RayTriangle Intersection
        // t => delta, 如果有打到物體,t>0,否則t<0
        public bool IntersectsLineTest(RHVector3 orig, RHVector3 dir, out double t, out double u, out double v)
        {
            t = u = v = 0;
            //Debug.WriteLine("IntersectsLine orig " + orig + " dir " + dir );
            //Debug.WriteLine("IntersectsLine ver1 " + vertices[0].pos + " ver2 " + vertices[1].pos + " ver3 " + vertices[2].pos);
            RHVector3 vert0 = vertices[0].pos;
            /*  find vectors for two edges sharing vert0 
                SUB(edge1, vert1, vert0)
                SUB(edge2, vert2, vert0) */
            RHVector3 edge1 = vertices[1].pos.Subtract(vert0);
            RHVector3 edge2 = vertices[2].pos.Subtract(vert0);
            /* begin calculating determinant - also used to calculate U parameter 
            CROSS(pvec, dir, edge2)*/
            RHVector3 pvec = dir.CrossProduct(edge2);

            /* if determinant is near zero, ray lies in plane of triangle 
            det = DOT(edge1, pvec)*/
            double det = edge1.ScalarProduct(pvec);
            //Debug.WriteLine("IntersectsLine det " + det);
            /* define TEST_CULL if culling is desired 
            if (det < EPSILON)return 0*/
            if (det < 0.000001) return false;
            /* calculate distance from vert0 to ray origin 
            SUB(tvec, orig, vert0)*/
            RHVector3 tvec = orig.Subtract(vert0);
            //Debug.WriteLine("IntersectsLine tvec " + tvec);
            /* calculate U parameter and test bounds 
            *u = DOT(tvec, pvec)*/
            u = tvec.ScalarProduct(pvec);
            /*if (*u < 0.0 || *u > det) return 0;*/
            //Debug.WriteLine("IntersectsLine u " + u);
            if (u < 0 || u > det) return false;
            /* prepare to test V parameter 
            CROSS(qvec, tvec, edge1)*/
            RHVector3 qvec = tvec.CrossProduct(edge1);
            /* calculate V parameter and test bounds 
            *v = DOT(dir, qvec)*/
            v = dir.ScalarProduct(qvec);
            //Debug.WriteLine("IntersectsLine v " + v);
            /*if (*v < 0.0 || *u + *v > det) return 0*/
            if (v < 0 || (u + v) > det) return false;
            /* calculate t, scale parameters, ray intersects triangle 
             *t = DOT(edge2, qvec)
             inv_det = 1.0 / det
             *t *= inv_det
             *u *= inv_det
             *v *= inv_det*/
            double inv_det = 1.0 / det;
            t = edge2.ScalarProduct(qvec);
            t *= inv_det;
            u *= inv_det;
            v *= inv_det;
            return true;
        }
        //---

        public double DistanceToPlane(RHVector3 pos)
        {
            double d = vertices[0].pos.ScalarProduct(normal);
            return pos.ScalarProduct(normal)-d;
        }

        private void DominantAxis(out int d1, out int d2)
        {
            double n1 = Math.Abs(normal.x);
            double n2 = Math.Abs(normal.y);
            double n3 = Math.Abs(normal.z);
            if (n1 > n2 && n1 > n3)
            {
                d1 = 1;
                d2 = 2;
            }
            else if (n2 > n3)
            {
                d1 = 0;
                d2 = 2;
            }
            else
            {
                d1 = 0;
                d2 = 1;
            }
        }
    }

    public class TopoTriangleDistance : IComparer<TopoTriangleDistance>, IComparable<TopoTriangleDistance>
    {
        public double distance;
        public TopoTriangle triangle;

        public TopoTriangleDistance(double dist, TopoTriangle tri)
        {
            triangle = tri;
            distance = dist;
        }

        public int Compare(TopoTriangleDistance td1, TopoTriangleDistance td2)
        {
            return -td1.distance.CompareTo(td2.distance);
        }

        public int CompareTo(TopoTriangleDistance td)
        {
            return -distance.CompareTo(td.distance);
        }
    }
}
