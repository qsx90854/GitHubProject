using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace XYZware_SLS.SLS.Tools
{
    public class ThreeWLImageProcessing
    {
        int currentHeight, maxHeight;
        MCvScalar mainColor;
        MCvScalar blueColor;
        Point[] frameTop, modelTop, frameBottom, frameRight, frameLeft;
        public enum ViewDir
        {
            Front,
            Back,
            Right,
            Left
        }
        Dictionary<ViewDir, Image<Bgra, byte>> viewMat;
        Mat frame, modelMask, show, model, border;
        Image<Bgra, byte> frameImg;
        System.Windows.Controls.Image showTarget;
        public bool HasTarget { get {return showTarget != null; } }
        bool IsOld3wl;

        public ThreeWLImageProcessing()
        {
            frameTop = new Point[4] { new Point(46, 33), new Point(14, 26), new Point(307, 26), new Point(275, 33) };
            frameBottom = new Point[4] { new Point(49, 263), new Point(18, 319), new Point(303, 319), new Point(272, 263) };
            mainColor = new MCvScalar(0, 0, 255, 255);//red border
            blueColor = new MCvScalar(255, 0, 0, 255);//red border
            frameImg = new Image<Bgra, byte>(Properties.Resources.frame);
            frame = frameImg.Mat;
            show = new Mat();
            model = new Mat();
            modelMask = new Mat();
            border = new Mat();
            viewMat = new Dictionary<ViewDir, Image<Bgra, byte>>();
        }

        public void SetIndicatorHeight(int layerHeight)
        {
            double ratio = layerHeight / 2300.0;

            //viewMat.Add(view, modelsImg);

            if (IsOld3wl)
            {
                frameTop = new Point[4] { new Point(36, 13), new Point(5, 5), new Point(319, 5), new Point(285, 13) };
                frameBottom = new Point[4] { new Point(40, 257), new Point(10, 314), new Point(317, 314), new Point(282, 257) };
            }
            else
            {
                frameTop = new Point[4] { new Point(46, 33), new Point(14, 26), new Point(307, 26), new Point(275, 33) };
                frameBottom = new Point[4] { new Point(49, 263), new Point(18, 319), new Point(303, 319), new Point(272, 263) };
               // frameRight = new Point[4] { new Point(44, 33), new Point(47, 363), new Point(307, 26), new Point(275, 33) };
               // frameLeft = new Point[4] { new Point(47, 263), new Point(18, 319), new Point(303, 319), new Point(272, 263) };
            }

            modelTop = new Point[4]
            { 
                GetViewPoint(frameBottom[0],frameTop[0],ratio),
                GetViewPoint(frameBottom[1],frameTop[1],ratio),
                GetViewPoint(frameBottom[2],frameTop[2],ratio),
                GetViewPoint(frameBottom[3],frameTop[3],ratio),
            };

            currentHeight = 1;
            maxHeight = layerHeight;
            ChangeViewDir(ViewDir.Front);
        }

        public void BindingTarget(System.Windows.Controls.Image source)
        {
            if (modelMask != null)
                IsOld3wl = modelMask.Size != new Size(322, 322) ? true : false;
            showTarget = source;
        }

        public void ChangeViewDir(ViewDir view)
        {
            viewMat[view].Mat.CopyTo(model);
            CvInvoke.CvtColor(model, modelMask, ColorConversion.Rgb2Gray);
            CvInvoke.Threshold(modelMask, modelMask, 250, 255, ThresholdType.BinaryInv);
            DrawPlaneAndDisplay(currentHeight);
        }

        public byte[] CropAndAddImage(ViewDir view, Bitmap image)
        {
            //Get range
            int dim = (int)(image.Height / 1.7);
            Size size = new Size(dim, dim);
            //Point location = new Point((int)(image.Width / 2 - dim / 2), (int)((image.Height / 2) - dim / 2) + (int)(frame.Rows*1.05) /2);
            Point location = new Point((int)(image.Width / 2 - dim / 2), (int)((image.Height / 2) - dim / 2) + (int)(image.Height * 0.189));
            Rectangle roi = new Rectangle(location, size);

            //Crop and resize
            Image<Bgra, byte> modelsImg = new Image<Bgra, byte>(image);
            modelsImg.ROI = roi;
            if (modelsImg.Cols != frame.Cols || modelsImg.Rows != frame.Rows)
                CvInvoke.Resize(modelsImg, modelsImg, new Size(frame.Cols, frame.Rows));

            //Add into array
            if (viewMat.ContainsKey(view))
                viewMat.Remove(view);
            viewMat.Add(view, modelsImg);

            //Combine models and frame
            Image<Bgra, byte> combineImg = new Image<Bgra, byte>(frame.Size);
            frame.CopyTo(combineImg);
            CvInvoke.CvtColor(modelsImg, modelMask, ColorConversion.Rgb2Gray);
            CvInvoke.Threshold(modelMask, modelMask, 250, 255, ThresholdType.BinaryInv);
            modelsImg.Mat.CopyTo(combineImg, modelMask);

            //output byte[]
            Bitmap bitmap = combineImg.ToBitmap();
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return combineImg.ToJpegData(100);
        }

        public byte[] AddImageToByteList(ViewDir view, Bitmap image)
        {
            if (new Image<Bgra, byte>(image).Cols != frame.Cols || new Image<Bgra, byte>(image).Rows != frame.Rows)
                CvInvoke.Resize(new Image<Bgra, byte>(image), new Image<Bgra, byte>(image), new Size(frame.Cols, frame.Rows));

            if (viewMat.ContainsKey(view))
                viewMat.Remove(view);
            viewMat.Add(view, new Image<Bgra, byte>(image));

            Image<Bgra, byte> combineImg = new Image<Bgra, byte>(frame.Size);
            frame.CopyTo(combineImg);
            CvInvoke.CvtColor(new Image<Bgra, byte>(image), modelMask, ColorConversion.Rgb2Gray);
            CvInvoke.Threshold(modelMask, modelMask, 250, 255, ThresholdType.BinaryInv);
            new Image<Bgra, byte>(image).Mat.CopyTo(combineImg, modelMask);

            Bitmap bitmap = combineImg.ToBitmap();
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return combineImg.ToJpegData(100);
            //return new Image<Bgra, byte>(image).ToJpegData(100);
        }

        public void DrawPlaneAndDisplay(int planeHeight)
        {
            if (showTarget == null) return;
            if (planeHeight < 1 || planeHeight > maxHeight) return;
            if (!Main.main.IsSliced)
                show = new Mat();
            currentHeight = planeHeight;
            frame.CopyTo(show);
            model.CopyTo(show, modelMask);
            double ratio = (double)currentHeight / maxHeight;
            ratio /= Main.main.threedview.ui.sliceToDxf.zoomZ;
            if (Main.main.threedview.ui.sliceToDxf.selLayerHeight > 0)
                ratio = (ratio * Main.main.threedview.ui.sliceToDxf.selLayerHeight) / 0.1;

            //tl,bl,rl,rt
            Point[] plane = new Point[4] 
            {
                GetViewPoint(frameBottom[0],modelTop[0],ratio),
                GetViewPoint(frameBottom[1],modelTop[1],ratio),
                GetViewPoint(frameBottom[2],modelTop[2],ratio),
                GetViewPoint(frameBottom[3],modelTop[3],ratio),
            };

            CvInvoke.Line(show, plane[0], plane[1], mainColor, 2); // left
            CvInvoke.Line(show, plane[2], plane[3], mainColor, 2); // right
            CvInvoke.Line(show, plane[0], plane[3], mainColor, 2); // back
            if (!IsOld3wl) model.CopyTo(show, modelMask);
            CvInvoke.Line(show, plane[1], plane[2], mainColor, 2);

            //   frameTop = new Point[4] { new Point(46, 33), new Point(14, 26), new Point(307, 26), new Point(275, 33) };
            //frameBottom = new Point[4] { new Point(49, 263), new Point(18, 319), new Point(303, 319), new Point(272, 263) };
            //CvInvoke.Line(show, new Point(10,28), new Point(12,319), blueColor, 1); // left
            //CvInvoke.Line(show, plane[2], plane[3], blueColor, 2); // right
            //CvInvoke.Line(show, plane[0], plane[3], blueColor, 2); // back
            //if (!IsOld3wl) model.CopyTo(show, modelMask);
            //CvInvoke.Line(show, plane[1], plane[2], blueColor, 2);

            showTarget.Source = GetBitmapSource();
        }

        Point GetViewPoint(Point bottom, Point top, double ratio)
        {
            if (IsOld3wl)
                return new Point((int)(bottom.X + (top.X - bottom.X) * (ratio)), (int)((bottom.Y - (15 * ratio)) + (top.Y - bottom.Y) * (ratio)));
            else
                return new Point((int)(bottom.X + (top.X - bottom.X) * (ratio)), (int)(bottom.Y + (top.Y - bottom.Y) * (ratio)));
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public System.Windows.Media.Imaging.BitmapSource GetBitmapSource()
        {
            IntPtr ptr = show.Bitmap.GetHbitmap();
            System.Windows.Media.Imaging.BitmapSource ret =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ptr,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ptr);
            return ret;
        }
    }
}
