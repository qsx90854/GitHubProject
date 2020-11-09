using Microsoft.Win32;
using System.Collections.Generic;

namespace XYZware_SLS.model
{
    public class Custom
    {
        private static RegistryKey baseKey;
        private static RegistryKey baseKey_RecentProject;
        private static Dictionary<string,string> dic;
        private static Dictionary<string, string> dic_RecentProject;

        public static void Initialize()
        {
            dic = new Dictionary<string, string>();
            dic["registryFolder"] = "SLSware";
            baseKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\"+dic["registryFolder"]);


            dic_RecentProject = new Dictionary<string, string>();
            dic_RecentProject["registryFolder"] = "SLSware";
            baseKey_RecentProject = Registry.CurrentUser.CreateSubKey("SOFTWARE\\"+dic_RecentProject["registryFolder"]);
        }

        public static bool GetBool(string name, bool def)
        {
            if (!dic.ContainsKey(name)) return def;
            string val = dic[name];
            if (val == "1" || val == "yes" || val == "true") return true;
            if (val == "0" || val == "no" || val == "false") return false;
            return def;
        }

        public static int GetInteger(string name, int def)
        {
            if (!dic.ContainsKey(name)) return def;
            string val = dic[name];
            int ival = def;
            int.TryParse(val, out ival);
            return ival;
        }

        public static string GetString(string name, string def)
        {
            if (!dic.ContainsKey(name)) return def;
            return dic[name];
        }

        public static RegistryKey BaseKey
        {
            get { return baseKey; }
        }

        public static XYZRegistryKey GetValue(string[] subkey, string name)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SLSware");

                foreach (string sKey in subkey)
                {
                    RegistryKey _key = key.CreateSubKey(sKey);
                    key = _key;
                }

                object value = key.GetValue(name, null);

                if (value != null)
                    return new XYZRegistryKey(name, value.ToString(), key.GetValueKind(name));
                else
                    return new XYZRegistryKey(name, null, RegistryValueKind.Unknown);
            }
            catch
            {
                return new XYZRegistryKey(name, null, RegistryValueKind.Unknown);
            }
        }


        //Create Recent model for Open Project
        public static bool GetBool_RecentProject(string name, bool def)
        {
            if (!dic_RecentProject.ContainsKey(name)) return def;
            string val = dic_RecentProject[name];
            if (val == "1" || val == "yes" || val == "true") return true;
            if (val == "0" || val == "no" || val == "false") return false;
            return def;
        }
        public static int GetInteger_RecentProject(string name, int def)
        {
            if (!dic_RecentProject.ContainsKey(name)) return def;
            string val = dic_RecentProject[name];
            int ival = def;
            int.TryParse(val, out ival);
            return ival;
        }
        public static string GetString_RecentProject(string name, string def)
        {
            if (!dic_RecentProject.ContainsKey(name)) return def;
            return dic_RecentProject[name];
        }
        public static RegistryKey BaseKey_RecentProject
        {
            get { return baseKey_RecentProject; }
        }
        public static XYZRegistryKey GetValue_RecentProject(string[] subkey, string name)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SLSware");

                foreach (string sKey in subkey)
                {
                    RegistryKey _key = key.CreateSubKey(sKey);
                    key = _key;
                }

                object value = key.GetValue(name, null);

                if (value != null)
                    return new XYZRegistryKey(name, value.ToString(), key.GetValueKind(name));
                else
                    return new XYZRegistryKey(name, null, RegistryValueKind.Unknown);
            }
            catch
            {
                return new XYZRegistryKey(name, null, RegistryValueKind.Unknown);
            }
        }
        //<><><>

    }

    public class XYZRegistryKey
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public RegistryValueKind Kind { get; set; }

        public XYZRegistryKey(string name, string value, RegistryValueKind kind)
        {
            Name = name;
            Value = value;
            Kind = kind;
        }
    }
}
