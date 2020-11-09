using System.Globalization;

namespace XYZware_SLS.model
{
    public class GCodeShort
    {
        public float x, y, z;
        // Bit 0-19 : Layer 
        // Bit 20-23 : Tool (removed)
        // Bit 24-29 : Compressed command
        int flags;
        public string text;
        public GCodeShort(string cmd)
        {
            text = cmd;
            flags = 1048575 + (0 << 24);    // 1048575 = 0xFFFFF
            //--- MODEL_SLA
            x = y = z = -99999;
            //---
            parse();
        }

        public int layer
        {
            get
            {
                return flags & 1048575;
            }
            set
            {
                flags = (flags & ~1048575) | value;
            }
        }

        public bool hasLayer
        {
            get
            {
                return (flags & 1048575) != 1048575;
            }
        }

        public int compressedCommand
        {
            set
            {
                flags = (flags & ~(63 << 24)) | (value << 24);
            }
            get
            {
                return (flags >> 24) & 63;
            }
        }

        public int Length
        {
            get { return text.Length; }
        }

        public bool hasX { get { return x != -99999; } }
        public bool hasY { get { return y != -99999; } }
        //--- MODEL_SLA
        public bool hasL { get { return z != -99999; } }
        //---

        public bool addCode(char c, string val)
        {
            double d;
            double.TryParse(val, NumberStyles.Float, XYZLib.XYZ.Model.GCode.format, out d);
            switch (c)
            {
                case 'G':
                    {
                        int g = (int)d;
                        if (g == 0 || g == 1) compressedCommand = 1;                // G0  ~ G1     => compressedCommand 1
                        else if (g >= 90 && g <= 92) compressedCommand = g - 84;    // G90 ~ G92    => compressedCommand 6 ~ 8
                        else if (g == 28) compressedCommand = 4;                    // G28          => compressedCommand 4
                        return true;
                    }
                case 'M':
                    {
                        int m = (int)d;
                        //--- MODEL_SLA
                        if (m == 600 || m == 602 || m == 609) compressedCommand = 13;   // laser on     
                        if (m == 601) compressedCommand = 14;                           // laser off
                        //---
                        return true;
                    }
                case 'X':
                    x = (float)d;
                    break;
                case 'Y':
                    y = (float)d;
                    break;
                //--- MODEL_SLA
                case 'L':
                    z = (float)d;
                    break;
                //---
            }
            return false;
        }

        private void parse()
        {
            int l = text.Length, i;
            int mode = 0; // 0 = search code, 1 = search value
            char code = ';';
            int p1 = 0;
            for (i = 0; i < l; i++)
            {
                char c = text[i];
                if (mode == 0 && c >= 'A' && c <= 'Z')
                {
                    code = c;
                    mode = 1;
                    p1 = i + 1;
                    continue;
                }
                else if (mode == 1)
                {
                    if (c == ' ' || c == '\t' || c == ';')
                    {
                        if (addCode(code, text.Substring(p1, i - p1)))
                        {
                            if (compressedCommand == 0) return; // Not interresting
                        }
                        mode = 0;
                    }
                }
                if (c == ';') break;
            }
            if (mode == 1)
            {
                addCode(code, text.Substring(p1, l - p1));
            }
        }
    }
}
