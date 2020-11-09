using System;
using System.Collections;
using System.Collections.Generic;

namespace XYZware_SLS.model.geom
{
    public class TopoVertexStorage
    {
        private const int listCapacity = 0x800;
        //public const int maxVerticesPerNode = 50;
        //TopoVertexStorage left = null, right = null;
        //TopoVertexStorageLeaf leaf = null;
        private List<List<TopoVertex>> v = new List<List<TopoVertex>>();
        Hashtable hash = new Hashtable();

        //int splitDimension = -1;
        private int count = 0;
        //double splitPosition = 0;

        //public bool IsLeaf
        //{
        //    get { return leaf != null; }
        //}

        public int Count
        {
            get { return count; }
        }

        public void Clear()
        {
            //left = right = null;
            //leaf = null;
            v.Clear();
            hash.Clear();
            count = 0;
        }

        //public void ChangeCoordinates(TopoVertex vertex, RHVector3 newPos)
        //{
        //    Remove(vertex);
        //    vertex.pos = new RHVector3(newPos);
        //    Add(vertex);
        //}

        //public void Add(TopoVertex vertex)
        //{
        //    Add(vertex, 0);
        //}

        //<Carson(Taipei)><12-14-2018><Modified>
        public void Add(TopoVertex vertex, bool checkHash = true)
        {
            if (checkHash)
            {
                Int64 temp = Convert.ToInt64(Math.Floor(vertex.pos.x * 100000)) * 5915587277 + Convert.ToInt64(Math.Floor(vertex.pos.y * 100000)) * 1500450271 + Convert.ToInt64(Math.Floor(vertex.pos.z * 100000)) * 3267000013;
                if (hash[temp] == null)
                {
                    if ((count & 0x7FF) == 0)
                        v.Add(new List<TopoVertex>(listCapacity));
                    hash.Add(temp, count);
                    v[v.Count - 1].Add(vertex);
                    count++;
                }
            }
            else
            {
                if ((count & 0x7FF) == 0)
                    v.Add(new List<TopoVertex>(listCapacity));
                v[v.Count - 1].Add(vertex);
                count++;
            }
        }
        //<><><>

        //public void Add(TopoVertex vertex,int level)
        //{        
        //    Int64 temp = Convert.ToInt64(Math.Floor(vertex.pos.x * 100000)) * 5915587277 + Convert.ToInt64(Math.Floor(vertex.pos.y * 100000)) * 1500450271 + Convert.ToInt64(Math.Floor(vertex.pos.z * 100000)) * 3267000013;
        //    if (hash[temp] == null)
        //    {
        //        hash.Add(temp, count);
        //        v.Add(vertex);
        //        count++;
        //    }
        //}

        public TopoVertex SearchPoint(RHVector3 vertex)
        {
            Int64 temp = Convert.ToInt64(Math.Floor(vertex.x * 100000)) * 5915587277 + Convert.ToInt64(Math.Floor(vertex.y * 100000)) * 1500450271 + Convert.ToInt64(Math.Floor(vertex.z * 100000)) * 3267000013;
            if (hash[temp] != null)
            {
                int idx = Convert.ToInt32(hash[temp]);
                int listidx = (idx >> 11);
                int idxinlist = (idx & 0x7FF);
                return v[listidx][idxinlist];
            }
            else return null;
        }

        //public void Remove(TopoVertex vertex)
        //{
        //    if (leaf == null && left == null) return;
        //    if (RemoveTraverse(vertex)) count--;
        //}

        //private bool RemoveTraverse(TopoVertex vertex)
        //{
        //    if (IsLeaf)
        //    {
        //        if (leaf.vertices.Remove(vertex))
        //            return true;
        //        else
        //            return false; // should not happen
        //    }
        //    if (vertex.pos[splitDimension] < splitPosition)
        //        return left.RemoveTraverse(vertex);
        //    else
        //        return right.RemoveTraverse(vertex);
        //}

        public System.Collections.IEnumerator GetEnumerator()
        {
            //if (left != null)
            //{
            //    foreach (TopoVertex v in left)
            //        yield return v;
            //    foreach (TopoVertex v in right)
            //        yield return v;
            //}
            //if (leaf!=null)
            //{
            //    foreach (TopoVertex v in leaf.vertices)
            //        yield return v;
            //}

            foreach (List<TopoVertex> l in v)
                foreach (TopoVertex vert in l)
                    yield return vert;
        }

        //public HashSet<TopoVertex> SearchBox(RHBoundingBox box)
        //{
        //    HashSet<TopoVertex> set = new HashSet<TopoVertex>();
        //    if(leaf!=null || left!=null)
        //        SearchBoxTraverse(box,set);
        //    return set;
        //}

        //private void SearchBoxTraverse(RHBoundingBox box,HashSet<TopoVertex> set) {
        //    if (IsLeaf)
        //    {
        //        foreach (TopoVertex v in leaf.vertices)
        //        {
        //            if (box.ContainsPoint(v.pos))
        //                set.Add(v);
        //        }
        //        return;
        //    }
        //}
    }

    //public class TopoVertexStorageLeaf
    //{
    //    public RHBoundingBox box = new RHBoundingBox();
    //    public List<TopoVertex> vertices = new List<TopoVertex>();

    //    public void Add(TopoVertex vertex)
    //    {
    //        vertices.Add(vertex);
    //        box.Add(vertex.pos);
    //    }

    //    public int LargestDimension()
    //    {
    //        RHVector3 size = box.Size;
    //        if (size.x > size.y && size.x > size.z) return 0;
    //        if (size.y > size.z) return 1;
    //        return 2;
    //    }

    //    public TopoVertex SearchPoint(RHVector3 vertex)
    //    {
    //        foreach (TopoVertex v in vertices)
    //        {
    //            if (vertex.Distance(v.pos) < TopoModel.epsilon)
    //                return v;
    //        }
    //        return null;
    //    }
    //}
}
