using GLNKG;
using System;

namespace XYZware_SLS.model.geom
{
    public class RHVector3
    {
        public double x = 0, y = 0, z = 0;

        public RHVector3(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public RHVector3(RHVector3 orig)
        {
            x = orig.x;
            y = orig.y;
            z = orig.z;
        }

        public RHVector3(Vector3 orig)
        {
            x = orig.X;
            y = orig.Y;
            z = orig.Z;
        }

        public RHVector3(Vector4 orig)
        {
            x = orig.X/orig.W;
            y = orig.Y / orig.W;
            z = orig.Z / orig.W;
        }

        public Vector4 asVector4()
        {
            return new Vector4((float)x, (float)y, (float)z, 1);
        }

        public Vector3 asVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }

        public double Length
        {
            get
            {
                 return Math.Sqrt(x * x + y * y + z * z);    // 模長 => 向量的大小
            }
        }

        public void NormalizeSafe()
        {
            double l = Length;
            if (l == 0)
            {
                x = y = 0;
                z = 0;
            }
            else
            {
                if (l < 1)   // 在計算單位向量的時候,考慮長度,如果長度小於1,將當作1(移到中心點的意思)  - Steven
                {
                    l = 1;
                }

                x /= l;
                y /= l;
                z /= l;
            }
        }

        public void Shrink(double factorx, double factory, double factorz)
        {
            x *= factorx;
            y *= factory;
            z *= factorz;
        }

        public void StoreMinimum(RHVector3 vec)
        {
            x = Math.Min(x, vec.x);
            y = Math.Min(y, vec.y);
            z = Math.Min(z, vec.z);
        }

        public void StoreMaximum(RHVector3 vec)
        {
            x = Math.Max(x, vec.x);
            y = Math.Max(y, vec.y);
            z = Math.Max(z, vec.z);
        }

        public double Distance(RHVector3 point)
        {
            double dx = point.x - x;
            double dy = point.y - y;
            double dz = point.z - z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public void Scale(double factor)
        {
            x *= factor;
            y *= factor;
            z *= factor;
        }

        private RHVector3 size;
        private RHVector3 ResizeObject
        {
            get
            {
                size = new RHVector3(x, y, z);
                return new RHVector3(x, y, z);
            }
        }

        public double ScalarProduct(RHVector3 vector)
        {
            //try
            //{
                return x * vector.x + y * vector.y + z * vector.z;
            //}
            //catch(System.NullReferenceException)
            //{
            //    return x;
            //}
        }

        public double AngleForNormalizedVectors(RHVector3 direction)
        {
            return Math.Acos(ScalarProduct(direction));
        }

        public double Angle(RHVector3 direction)
        {
            return Math.Acos(ScalarProduct(direction)/(Length*direction.Length));
        }

        public RHVector3 Subtract(RHVector3 vector)
        {
            //try
            //{
                return new RHVector3(x - vector.x, y - vector.y, z - vector.z);
            //}
            //catch(System.OutOfMemoryException)
            //{
            //    throw new System.OutOfMemoryException();
            //    System.Windows.Forms.MessageBox.Show("Error(" + "Load file failed" + "): " + "Load file failed.");
            //    GC.Collect();
            //}
        }

        public RHVector3 Add(RHVector3 vector)
        {
            return new RHVector3(x + vector.x, y + vector.y, z + vector.z);
        }

        public void SubtractInternal(RHVector3 vector)
        {
            x -= vector.x;
            y -= vector.y;
            z -= vector.z;
        }

        public void AddInternal(RHVector3 vector)
        {
            x += vector.x;
            y += vector.y;
            z += vector.z;
        }

        public RHVector3 CrossProduct(RHVector3 vector)
        {
            //try
            //{
                return new RHVector3(
                    y * vector.z - z * vector.y,
                    z * vector.x - x * vector.z,
                    x * vector.y - y * vector.x);
            //}
            //catch(System.OutOfMemoryException)
            //{
            //    return null;
            //    //System.Windows.Forms.MessageBox.Show("Error(" + "Load file failed" + "): " + "Load file failed.");
            //    GC.Collect();
            //}
        }

        public double this[int dimension]
        {
            get
            {
                if (dimension == 0) return x;
                else if (dimension == 1) return y;
                else return z;
            }
            set
            {
                if (dimension == 0) x = value;
                else if (dimension == 1) y = value;
                else z = value;
            }
        }

        //<Carson(Taipei)><02-21-2019><Added>
        public RHVector3 Transform(Matrix4 mat)
        {
            Vector3 v = Vector3.Transform(asVector3(), mat);
            return new RHVector3(v.X, v.Y, v.Z);
        }
        //<><><>

        //--- MODEL_SLA
        public override bool Equals(object obj)
        {
            RHVector3 compare = obj as RHVector3;
            if (x == compare.x && y == compare.y && z == compare.z)
                return true;
            else
                return false;
        }
        //---

        public override string ToString()
        {
            return "(" + x.ToString() + ";" + y.ToString() + ";" + z.ToString() + ")";
        }

        public Vector3 ToVector3()
        {
            return new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), Convert.ToSingle(z));
        }

        public double Total_Surface_Area()
        {
            return 2.0 * (x * y + y * z + z * x);
        }
    }
}
