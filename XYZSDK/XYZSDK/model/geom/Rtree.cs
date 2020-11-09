using System;
using System.Collections.Generic;
using GLNKG;

namespace XYZware_SLS.model.geom
{
    public class RTree<T>
    {
        int maxNodeEntries;
        int minNodeEntries;
        Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();
        const int ENTRY_STATUS_ASSIGNED = 0;
        const int ENTRY_STATUS_UNASSIGNED = 1;
        byte[] entryStatus = null;
        byte[] initialEntryStatus = null;
        Stack<int> parents = new Stack<int>();
        Stack<int> parentsEntry = new Stack<int>();
        int treeHeight = 1;
        int rootNodeId = 0;
        int highestUsedNodeId = 0;
        Stack<int> deletedNodeIds = new Stack<int>();
        Dictionary<int, T> items = new Dictionary<int, T>();
        volatile int idcounter = int.MinValue;
        delegate void intproc(int x);

        public RTree()
        {
            Init();
        }

        public RTree(int MaxNodeEntries, int MinNodeEntries)
        {
            minNodeEntries = MinNodeEntries;
            maxNodeEntries = MaxNodeEntries;
            Init();
        }

        void Init()
        {
            if (maxNodeEntries < 2)
            {
                maxNodeEntries = 10;
            }

            if (minNodeEntries < 1 || minNodeEntries > maxNodeEntries / 2)
            {
                minNodeEntries = maxNodeEntries / 2;
            }

            entryStatus = new byte[maxNodeEntries];
            initialEntryStatus = new byte[maxNodeEntries];

            for (int i = 0; i < maxNodeEntries; i++)
            {
                initialEntryStatus[i] = ENTRY_STATUS_UNASSIGNED;
            }

            Node root = new Node(rootNodeId, 1, maxNodeEntries);
            nodeMap.Add(rootNodeId, root);
        }

        public void Add(RBox box, T item)
        {
            idcounter++;
            int id = idcounter;
            items.Add(id, item);
            Add(box.Copy(), id, 1);
        }

        void Add(RBox box, int id, int level)
        {
            Node n = ChooseNode(box, level);
            Node newLeaf = null;
            if (n.entryCount < maxNodeEntries)
            {
                n.AddEntryNoCopy(box, id);
            }
            else
            {
                newLeaf = SplitNode(n, box, id);
            }
            Node newNode = AdjustTree(n, newLeaf);
            if (newNode != null)
            {
                int oldRootNodeId = rootNodeId;
                Node oldRoot = nodeMap[oldRootNodeId];

                rootNodeId = GetNextNodeId();
                treeHeight++;
                Node root = new Node(rootNodeId, treeHeight, maxNodeEntries);
                root.AddEntry(newNode.mbr, newNode.nodeId);
                root.AddEntry(oldRoot.mbr, oldRoot.nodeId);
                nodeMap.Add(rootNodeId, root);
            }
        }

        Node ChooseNode(RBox box, int level)
        {
            Node n = nodeMap[rootNodeId];
            parents.Clear();
            parentsEntry.Clear();
            while (true)
            {
                if (n.level == level)
                {
                    return n;
                }
                double leastEnlargement = n.GetEntry(0).Enlargement(box);
                int index = 0;
                for (int i = 1; i < n.entryCount; i++)
                {
                    RBox tempBox = n.GetEntry(i);
                    double tempEnlargement = tempBox.Enlargement(box);
                    if ((tempEnlargement < leastEnlargement) ||
                        ((tempEnlargement == leastEnlargement) &&
                         (tempBox.area < n.GetEntry(index).area)))
                    {
                        index = i;
                        leastEnlargement = tempEnlargement;
                    }
                }
                parents.Push(n.nodeId);
                parentsEntry.Push(index);
                n = nodeMap[n.ids[index]];
            }
        }

        int GetNextNodeId()
        {
            int nextNodeId = 0;
            if (deletedNodeIds.Count > 0)
            {
                nextNodeId = deletedNodeIds.Pop();
            }
            else
            {
                nextNodeId = 1 + highestUsedNodeId++;
            }
            return nextNodeId;
        }

        Node SplitNode(Node n, RBox newBox, int newId)
        {
            System.Array.Copy(initialEntryStatus, 0, entryStatus, 0, maxNodeEntries);

            Node newNode = null;
            newNode = new Node(GetNextNodeId(), n.level, maxNodeEntries);
            nodeMap.Add(newNode.nodeId, newNode);

            PickSeeds(n, newBox, newId, newNode);

            while (n.entryCount + newNode.entryCount < maxNodeEntries + 1)
            {
                if (maxNodeEntries + 1 - newNode.entryCount == minNodeEntries)
                {
                    for (int i = 0; i < maxNodeEntries; i++)
                    {
                        if (entryStatus[i] == ENTRY_STATUS_UNASSIGNED)
                        {
                            entryStatus[i] = ENTRY_STATUS_ASSIGNED;
                            n.mbr.Add(n.entries[i]);
                            n.entryCount++;
                        }
                    }
                    break;
                }
                if (maxNodeEntries + 1 - n.entryCount == minNodeEntries)
                {
                    for (int i = 0; i < maxNodeEntries; i++)
                    {
                        if (entryStatus[i] == ENTRY_STATUS_UNASSIGNED)
                        {
                            entryStatus[i] = ENTRY_STATUS_ASSIGNED;
                            newNode.AddEntryNoCopy(n.entries[i], n.ids[i]);
                            n.entries[i] = null;
                        }
                    }
                    break;
                }
                PickNext(n, newNode);
            }

            n.Reorganize(maxNodeEntries - 1);
            return newNode;
        }

        void PickSeeds(Node n, RBox newBox, int newId, Node newNode)
        {
            double maxNormalizedSeparation = 0;
            int highestLowIndex = 0;
            int lowestHighIndex = 0;

            n.mbr.Add(newBox);
            double mbrSizeX = n.mbr.max.X - n.mbr.min.X;
            double mbrSizeY = n.mbr.max.Y - n.mbr.min.Y;
            double mbrSizeZ = n.mbr.max.Z - n.mbr.min.Z;
            double tempHighestLowX = newBox.min.X;
            double tempLowestHighX = newBox.max.X;

            double tempHighestLowY = newBox.min.Y;
            double tempLowestHighY = newBox.max.Y;

            double tempHighestLowZ = newBox.min.Z;
            double tempLowestHighZ = newBox.max.Z;

            for (int i = 0; i < n.entryCount; i++)
            {
                #region Get HighestLow  or LowestHigh X
                if (n.entries[i].min.X >= tempHighestLowX)
                {
                    tempHighestLowX = n.entries[i].min.X;
                }
                else
                {
                    if (n.entries[i].max.X <= tempLowestHighX)
                    {
                        tempLowestHighX = n.entries[i].max.X;
                    }
                }
                #endregion
                #region Get HighestLow  or LowestHigh Y
                if (n.entries[i].min.Y >= tempHighestLowY)
                {
                    tempHighestLowY = n.entries[i].min.Y;
                }
                else
                {
                    if (n.entries[i].max.Y <= tempLowestHighY)
                    {
                        tempLowestHighY = n.entries[i].max.Y;
                    }
                }
                #endregion
                #region Get HighestLow  or LowestHigh Z
                if (n.entries[i].min.Z >= tempHighestLowZ)
                {
                    tempHighestLowZ = n.entries[i].min.Z;
                }
                else
                {
                    if (n.entries[i].max.Z <= tempLowestHighZ)
                    {
                        tempLowestHighZ = n.entries[i].max.Z;
                    }
                }
                #endregion
                double sX = (tempHighestLowX - tempLowestHighX) / mbrSizeX;
                double sY = (tempHighestLowY - tempLowestHighY) / mbrSizeY;
                double sZ = (tempHighestLowZ - tempLowestHighZ) / mbrSizeZ;

                double normalizedSeparation = Math.Max(Math.Max(sX, sY), sZ);
                if (normalizedSeparation > maxNormalizedSeparation)
                {
                    maxNormalizedSeparation = normalizedSeparation;
                    highestLowIndex = i;
                    lowestHighIndex = i;
                }
            }

            if (highestLowIndex == -1)
            {
                newNode.AddEntry(newBox, newId);
            }
            else
            {
                newNode.AddEntryNoCopy(n.entries[highestLowIndex], n.ids[highestLowIndex]);
                n.entries[highestLowIndex] = null;
                n.entries[highestLowIndex] = newBox;
                n.ids[highestLowIndex] = newId;
            }
            if (lowestHighIndex == -1)
            {
                lowestHighIndex = highestLowIndex;
            }

            entryStatus[lowestHighIndex] = ENTRY_STATUS_ASSIGNED;
            n.entryCount = 1;
            n.mbr.Set(n.entries[lowestHighIndex].min, n.entries[lowestHighIndex].max);
        }

        int PickNext(Node n, Node newNode)
        {
            double maxDifference = double.NegativeInfinity;
            int next = 0;
            int nextGroup = 0;

            maxDifference = double.NegativeInfinity;

            for (int i = 0; i < maxNodeEntries; i++)
            {
                if (entryStatus[i] == ENTRY_STATUS_UNASSIGNED)
                {

                    double nIncrease = n.mbr.Enlargement(n.entries[i]);
                    double newNodeIncrease = newNode.mbr.Enlargement(n.entries[i]);
                    double difference = Math.Abs(nIncrease - newNodeIncrease);

                    if (difference > maxDifference)
                    {
                        next = i;

                        if (nIncrease < newNodeIncrease)
                        {
                            nextGroup = 0;
                        }
                        else if (newNodeIncrease < nIncrease)
                        {
                            nextGroup = 1;
                        }
                        else if (n.mbr.area < newNode.mbr.area)
                        {
                            nextGroup = 0;
                        }
                        else if (newNode.mbr.area < n.mbr.area)
                        {
                            nextGroup = 1;
                        }
                        else if (newNode.entryCount < maxNodeEntries / 2)
                        {
                            nextGroup = 0;
                        }
                        else
                        {
                            nextGroup = 1;
                        }
                        maxDifference = difference;
                    }
                }
            }

            entryStatus[next] = ENTRY_STATUS_ASSIGNED;

            if (nextGroup == 0)
            {
                n.mbr.Add(n.entries[next]);
                n.entryCount++;
            }
            else
            {
                newNode.AddEntryNoCopy(n.entries[next], n.ids[next]);
                n.entries[next] = null;
            }

            return next;
        }

        Node AdjustTree(Node n, Node nn)
        {
            while (n.level != treeHeight)
            {
                Node parent = nodeMap[parents.Pop()];
                int entry = parentsEntry.Pop();

                if (!parent.entries[entry].Equal(n.mbr))
                {
                    parent.entries[entry].Set(n.mbr.min, n.mbr.max);
                    parent.mbr.Set(parent.entries[0].min, parent.entries[0].max);
                    for (int i = 1; i < parent.entryCount; i++)
                    {
                        parent.mbr.Add(parent.entries[i]);
                    }
                }

                Node newNode = null;
                if (nn != null)
                {
                    if (parent.entryCount < maxNodeEntries)
                    {
                        parent.AddEntry(nn.mbr, nn.nodeId);
                    }
                    else
                    {
                        newNode = SplitNode(parent, nn.mbr.Copy(), nn.nodeId);
                    }
                }
                n = parent;
                nn = newNode;
                parent = null;
                newNode = null;
            }

            return nn;
        }

        public List<T> Intersects(RBox box)
        {
            List<T> retval = new List<T>();
            Intersects(box, delegate(int id)
            {
                retval.Add(items[id]);
            });
            return retval;
        }

        void Intersects(RBox box, intproc v)
        {
            Node rootNode = nodeMap[rootNodeId];
            Intersects(box, v, rootNode);
        }

        void Intersects(RBox box, intproc v, Node n)
        {
            for (int i = 0; i < n.entryCount; i++)
            {
                if (box.Intersects(n.entries[i]))
                {
                    if (n.level == 1)
                    {
                        v(n.ids[i]);
                    }
                    else
                    {
                        Node childNode = nodeMap[n.ids[i]];
                        Intersects(box, v, childNode);
                    }
                }
            }
        }

        public bool MeshOverlapDetect(List<Vector3[]> mesh, List<RBox> box)
        {
            for (int i = 0; i < box.Count; i++)
            {
                List<T> ts = Intersects(box[i]); //ts應該是本地模型與傳進來的box判斷有交錯的網格(點?)集合
                foreach (Vector3[] t in ts as List<Vector3[]>)
                {
                    if (TriangleTriangleIntersection(t, mesh[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool TriangleTriangleIntersection(Vector3[] t1, Vector3[] t2)
        {
            Vector3[] segment1 = new Vector3[2];
            Vector3[] segment2 = new Vector3[2];
            if (TriangleTriangleIntersection(t2, t1, out segment1) && TriangleTriangleIntersection(t1, t2, out segment2))
            {
                Vector3 t1Normal = Vector3.Cross(t1[1] - t1[0], t1[2] - t1[0]);
                Vector3 t2Normal = Vector3.Cross(t2[1] - t2[0], t2[2] - t2[0]);
                Vector3 d = Vector3.Cross(t1Normal, t2Normal);
                d.Normalize();
                Vector3 a = 0.25f * (segment1[0] + segment1[1] + segment2[0] + segment2[1]);

                float t00 = Vector3.Dot(d, segment1[0] - a);
                float t01 = Vector3.Dot(d, segment1[1] - a);
                float t10 = Vector3.Dot(d, segment2[0] - a);
                float t11 = Vector3.Dot(d, segment2[1] - a);
                if (Math.Max(t00, t01) < Math.Min(t10, t11))
                    return false;
                if (Math.Max(t10, t11) < Math.Min(t00, t01))
                    return false;
                return true;
            }
            return false;
        }

        bool TriangleTriangleIntersection(Vector3[] t1, Vector3[] t2, out Vector3[] segment)
        {
            Vector3 nor = Vector3.Cross(t1[1] - t1[0], t1[2] - t1[0]);
            nor.Normalize();

            float[] d = new float[3];
            int positive = 0, negative = 0, zero = 0;
            for (int i = 0; i < 3; ++i)
            {
                d[i] = Vector3.Dot(t2[i] - t1[0], nor);
                d[i] = (float)Math.Round(d[i], 3);
                if (d[i] >= 0.0f)
                {
                    ++positive;
                }
                else if (d[i] < 0.0f)
                {
                    ++negative;
                }
                else
                {
                    ++zero;
                }
            }
            segment = new Vector3[2];
            if (positive > 0 && negative > 0)
            {
                if (positive == 2)  // and negative == 1
                {
                    if (d[0] < 0.0f)
                    {
                        segment[0] = (d[1] * t2[0] - d[0] * t2[1]) / (d[1] - d[0] + 0.000000001f);
                        segment[1] = (d[2] * t2[0] - d[0] * t2[2]) / (d[2] - d[0] + 0.000000001f);
                    }
                    else if (d[1] < 0.0f)
                    {
                        segment[0] = (d[0] * t2[1] - d[1] * t2[0]) / (d[0] - d[1] + 0.000000001f);
                        segment[1] = (d[2] * t2[1] - d[1] * t2[2]) / (d[2] - d[1] + 0.000000001f);
                    }
                    else  // d[2] < 0.0f
                    {
                        segment[0] = (d[0] * t2[2] - d[2] * t2[0]) / (d[0] - d[2] + 0.000000001f);
                        segment[1] = (d[1] * t2[2] - d[2] * t2[1]) / (d[1] - d[2] + 0.000000001f);
                    }
                }
                else if (negative == 2)  // and positive == 1
                {
                    if (d[0] > 0.0f)
                    {
                        segment[0] = (d[1] * t2[0] - d[0] * t2[1]) / (d[1] - d[0] + 0.000000001f);
                        segment[1] = (d[2] * t2[0] - d[0] * t2[2]) / (d[2] - d[0] + 0.000000001f);
                    }
                    else if (d[1] > 0.0f)
                    {
                        segment[0] = (d[0] * t2[1] - d[1] * t2[0]) / (d[0] - d[1] + 0.000000001f);
                        segment[1] = (d[2] * t2[1] - d[1] * t2[2]) / (d[2] - d[1] + 0.000000001f);
                    }
                    else  // d[2] > 0.0f
                    {
                        segment[0] = (d[0] * t2[2] - d[2] * t2[0]) / (d[0] - d[2] + 0.000000001f);
                        segment[1] = (d[1] * t2[2] - d[2] * t2[1]) / (d[1] - d[2] + 0.000000001f);
                    }
                }
                else  // positive == 1, negative == 1, zero == 1
                {
                    if (d[0] == 0.0f)
                    {
                        segment[0] = t2[0];
                        segment[1] = (d[2] * t2[1] - d[1] * t2[2]) / (d[2] - d[1] + 0.000000001f);
                    }
                    else if (d[1] == 0.0f)
                    {
                        segment[0] = t2[1];
                        segment[1] = (d[0] * t2[2] - d[2] * t2[0]) / (d[0] - d[2] + 0.000000001f);
                    }
                    else  // d[2] == 0.0f
                    {
                        segment[0] = t2[2];
                        segment[1] = (d[1] * t2[0] - d[0] * t2[1]) / (d[1] - d[0] + 0.000000001f);
                    }
                }
                return true;
            }
            return false;
        }

    }

    public class Node
    {
        public int nodeId = 0;
        public RBox mbr = null;
        public RBox[] entries = null;
        public int[] ids = null;
        public int level;
        public int entryCount;

        public Node(int nodeId, int level, int maxNodeEntries)
        {
            this.nodeId = nodeId;
            this.level = level;
            entries = new RBox[maxNodeEntries];
            ids = new int[maxNodeEntries];
        }

        public RBox GetEntry(int index)
        {
            if (index < entryCount)
            {
                return entries[index];
            }
            return null;
        }

        public void AddEntry(RBox box, int id)
        {
            ids[entryCount] = id;
            entries[entryCount] = box.Copy();
            entryCount++;
            if (mbr == null)
            {
                mbr = box.Copy();
            }
            else
            {
                mbr.Add(box);
            }
        }

        public void AddEntryNoCopy(RBox box, int id)
        {
            ids[entryCount] = id;
            entries[entryCount] = box;
            entryCount++;
            if (mbr == null)
            {
                mbr = box.Copy();
            }
            else
            {
                mbr.Add(box);
            }
        }

        public void Reorganize(int countdownIndex)
        {
            for (int index = 0; index < entryCount; index++)
            {
                if (entries[index] == null)
                {
                    while (entries[countdownIndex] == null && countdownIndex > index)
                    {
                        countdownIndex--;
                    }
                    entries[index] = entries[countdownIndex];
                    ids[index] = ids[countdownIndex];
                    entries[countdownIndex] = null;
                }
            }
        }
    }

    public class RBox
    {
        public Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);      
        public float area = 0;

        public RBox()
        {

        }

        public RBox(Vector3[] vs)
        {
            foreach (Vector3 v in vs)
            {
                min.X = Math.Min(v.X, min.X);
                min.Y = Math.Min(v.Y, min.Y);
                min.Z = Math.Min(v.Z, min.Z);
                max.X = Math.Max(v.X, max.X);
                max.Y = Math.Max(v.Y, max.Y);
                max.Z = Math.Max(v.Z, max.Z);
            }
            area = (max.X - min.X) * (max.Y - min.Y) * (max.Z - min.Z);
        }

        public RBox(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
            area = (max.X - min.X) * (max.Y - min.Y) * (max.Z - min.Z);
        }

        public void Add(Vector3 p)
        {
            min.X = Math.Min(p.X, min.X);
            min.Y = Math.Min(p.Y, min.Y);
            min.Z = Math.Min(p.Z, min.Z);
            max.X = Math.Max(p.X, max.X);
            max.Y = Math.Max(p.Y, max.Y);
            max.Z = Math.Max(p.Z, max.Z);
            area = (max.X - min.X) * (max.Y - min.Y) * (max.Z - min.Z);

        }

        public void Add(RBox box)
        {
            min.X = Math.Min(box.min.X, min.X);
            min.Y = Math.Min(box.min.Y, min.Y);
            min.Z = Math.Min(box.min.Z, min.Z);
            max.X = Math.Max(box.max.X, max.X);
            max.Y = Math.Max(box.max.Y, max.Y);
            max.Z = Math.Max(box.max.Z, max.Z);
            area = (max.X - min.X) * (max.Y - min.Y) * (max.Z - min.Z);
        }

        public void Set(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
            area = (max.X - min.X) * (max.Y - min.Y) * (max.Z - min.Z);
        }

        public RBox Copy()
        {
            return new RBox(min, max);
        }

        public float Enlargement(RBox box)
        {
            return (Math.Max(max.X, box.max.X) - Math.Min(min.X, box.min.X)) * 
                (Math.Max(max.Y, box.max.Y) - Math.Min(min.Y, box.min.Y)) * 
                (Math.Max(max.Z, box.max.Z) - Math.Min(min.Z, box.min.Z)) 
                - area;
        }

        public bool Equal(RBox box)
        {
            return (min.X == box.min.X && min.Y == box.min.Y && min.Z == box.min.Z && max.X == box.max.X && max.Y == box.max.Y && max.Z == box.max.Z);
        }

        public bool Intersects(RBox box)
        {
            if (max.X < box.min.X)
                return false;
            if (min.X > box.max.X)
                return false;
            if (max.Y < box.min.Y)
                return false;
            if (min.Y > box.max.Y)
                return false;
            if (max.Z < box.min.Z)
                return false;
            if (min.Z > box.max.Z)
                return false;
            return true;
        }
    }

}
