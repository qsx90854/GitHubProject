﻿using GLNKG;

namespace XYZware_SLS.model.geom
{
    public class TopoVertex
    {
        public RHVector3 pos;
        public int id;

        public TopoVertex(int _id, RHVector3 _pos)
        {
            id = _id;
            pos = _pos;
        }

        public TopoVertex(int _id,RHVector3 _pos,Matrix4 trans)
        {
            id = _id;
            pos = new RHVector3(
                _pos.x*trans.Column0.X+_pos.y*trans.Column0.Y+_pos.z*trans.Column0.Z+trans.Column0.W,
                _pos.x*trans.Column1.X+_pos.y*trans.Column1.Y+_pos.z*trans.Column1.Z+trans.Column1.W,
                _pos.x*trans.Column2.X+_pos.y*trans.Column2.Y+_pos.z*trans.Column2.Z+trans.Column2.W
            );
        } 

        public double distance(TopoVertex vertex)
        {
            return pos.Distance(vertex.pos);
        }

        public double distance(RHVector3 vertex)
        {
            return pos.Distance(vertex);
        }
    }
}
