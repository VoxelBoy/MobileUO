using System;
using System.Collections.Generic;
using System.Text;

namespace Assistant
{
    internal class Foods
    {
        private static Dictionary<string, List<string>> Groups = new Dictionary<string, List<string>>();
        private static Dictionary<string, ushort> Names = new Dictionary<string, ushort>();

        internal static void AddFood(string group, string name, ushort id)
        {
            if(!string.IsNullOrEmpty(group) && !string.IsNullOrEmpty(name) && id > 0)
            {
                if (!Groups.TryGetValue(group, out var l))
                    Groups[group] = l = new List<string>();
                l.Add(name);
                Names[name] = id;
            }
        }

        private static HashSet<ushort> _found = new HashSet<ushort>();
        internal static HashSet<ushort> GetFoodGraphics(string name)
        {
            _found.Clear();
            if (name == "any")
            {
                foreach(ushort id in Names.Values)
                    _found.Add(id);
            }
            else
            {
                ushort val;
                if(Groups.TryGetValue(name, out var l))
                {
                    foreach(string s in l)
                    {
                        _found.Add(Names[s]);
                    }
                }
                if (Names.TryGetValue(name, out val))
                    _found.Add(val);
            }
            return _found;
        }
    }
}
