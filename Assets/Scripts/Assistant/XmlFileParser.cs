using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using ClassicUO;
using ClassicUO.Configuration;
using ClassicUO.Utility.Logging;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.Scenes;
using ClassicUO.IO.Resources;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Assistant;

using SDL2;

using AssistantGump = ClassicUO.Game.UI.Gumps.AssistantGump;
using ClassicUO.Utility.Collections;

namespace Assistant
{
    #region XmlFileLoaderSaver
    internal static class XmlFileParser
    {
        internal static readonly CultureInfo Culture;

        static XmlFileParser()
        {
            Culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        }

        private static string GetAttribute(XmlElement node, string attributeName, string defaultValue = null)
        {
            if (node == null)
            {
                return defaultValue;
            }

            XmlAttribute attr = node.Attributes[attributeName];

            if (attr == null)
            {
                return defaultValue;
            }

            return attr.Value;
        }

        private static bool GetAttributeBool(XmlElement node, string attributeName, bool defaultValue = false)
        {
            if (node == null)
            {
                return defaultValue;
            }

            XmlAttribute attr = node.Attributes[attributeName];

            if (attr == null)
            {
                return defaultValue;
            }

            return bool.TryParse(attr.Value, out bool b) && b;
        }

        private static uint GetAttributeUInt(XmlElement node, string attributeName, uint defaultvalue = 0x0)
        {
            if (node == null)
            {
                return defaultvalue;
            }

            XmlAttribute attr = node.Attributes[attributeName];

            if (attr == null)
            {
                return defaultvalue;
            }
            uint i;
            if (attr.Value.StartsWith("0x"))
            {
                if (!uint.TryParse(attr.Value.Substring(2), NumberStyles.HexNumber, Culture, out i))
                {
                    i = defaultvalue;
                }
            }
            else
            {
                if (!uint.TryParse(attr.Value.Split('_')[0], out i))
                {
                    i = defaultvalue;
                }
            }
            return i;
        }

        private static ushort GetAttributeUShort(XmlElement node, string attributeName, ushort defaultvalue = 0x0)
        {
            return (ushort)GetAttributeUInt(node, attributeName, defaultvalue);
        }

        private static string GetText(XmlElement node, string defaultValue)
        {
            if (node == null)
            {
                return defaultValue;
            }

            return node.InnerText;
        }

        private static bool GetBool(XmlElement node, bool defaultValue)
        {
            if (node == null)
            {
                return defaultValue;
            }

            return node.InnerText == bool.TrueString;
        }

        private static byte GetByte(XmlElement node, byte defaultvalue = 0x0)
        {
            return (byte)GetUInt(node, defaultvalue);
        }

        private static ushort GetUShort(XmlElement node, ushort defaultvalue = 0x0)
        {
            return (ushort)GetUInt(node, defaultvalue);
        }

        private static uint GetUInt(XmlElement node, uint defaultvalue = 0x0)
        {
            if (node == null || string.IsNullOrEmpty(node.InnerText))
            {
                return defaultvalue;
            }
            uint i;
            if (node.InnerText.StartsWith("0x"))
            {
                if (!uint.TryParse(node.InnerText.Substring(2), NumberStyles.HexNumber, Culture, out i))
                {
                    i = defaultvalue;
                }
            }
            else
            {
                if (!uint.TryParse(node.InnerText, out i))
                {
                    i = defaultvalue;
                }
            }
            return i;
        }

        internal static void LoadConfig(FileInfo info, AssistantGump gump)
        {
            if (gump == null || gump.IsDisposed || UOSObjects.Player == null)
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(info.FullName);
            }
            catch
            {
                return;
            }
            if (doc == null)
            {
                return;
            }

            XmlElement root = doc["config"];
            if (root == null)
            {
                return;
            }
            foreach (XmlElement data in root.GetElementsByTagName("data"))
            {
                switch (GetAttribute(data, "name"))
                {
                    case "LastProfile":
                    {
                        gump.LastProfile = GetText(data, "").Replace(".xml", "");
                        break;
                    }
                    case "SmartProfile":
                    {
                        gump.SmartProfile = GetBool(data, false);
                        break;
                    }
                    /*case "NegotiateFeatures":
                    {
                        gump.NegotiateFeatures = GetBool(data, false);
                        break;
                    }*/
                }
                /*for (int i = 0; i < Filter.List.Count; i++)
                {
                    Filter f = Filter.List[i];
                    if (f.XmlName == name)
                    {
                        bool.TryParse(GetText(data, "False"), out bool enabled);
                        AssistantGump.FiltersCB[i].IsChecked = enabled;
                    }
                }*/
            }
            XmlElement sub = root["links"];
            if (sub != null && gump.SmartProfile)
            {
                LoginScene scene = Client.Game.GetScene<LoginScene>();
                foreach (XmlElement link in sub.GetElementsByTagName("link"))
                {
                    uint serial = GetAttributeUInt(link, "serial");
                    string account = GetAttribute(link, "account");
                    string profile = GetAttribute(link, "profile");
                    if (string.IsNullOrWhiteSpace(profile) || string.IsNullOrWhiteSpace(account) || serial == 0)
                    {
                        continue;
                    }

                    FileInfo profileinfo = new FileInfo(Path.Combine(Profile.ProfilePath, profile));
                    if (profileinfo.Exists && SerialHelper.IsMobile(serial) && World.Player.Serial == serial && ProfileManager.Current.Username == account)
                    {
                        gump.LastProfile = profileinfo.Name;
                        break;
                    }
                }
            }
            sub = root["snapshots"];
            if(sub != null)
            {
                UOSObjects.Gump.SnapOwnDeath = GetBool(sub["ownDeath"], false);
                UOSObjects.Gump.SnapOtherDeath = GetBool(sub["othersDeath"], false);
            }
            sub = root["autoloot"];
            gump.ItemsToLoot.Clear();
            if (sub != null)
            {
                foreach (XmlElement item in sub.ChildNodes)
                {
                    ushort type = GetAttributeUShort(item, "type");
                    ushort limit = Math.Min((ushort)60000, GetAttributeUShort(item, "limit"));
                    string name = GetAttribute(item, "name");
                    if (type > 0 && type < TileDataLoader.Instance.StaticData.Length && !string.IsNullOrEmpty(name))
                        gump.ItemsToLoot[type] = (limit, name);
                }
            }
            else
                gump.ItemsToLoot.Clear();
            gump.UpdateAutolootList();
            sub = root["gump"];
            if(sub != null)
            {
                int x = GetAttributeUShort(sub, "x", 300);
                int y = GetAttributeUShort(sub, "y", 300);
                gump.X = x;
                gump.Y = y;
            }
        }

        internal static void LoadSpellDef(FileInfo info, AssistantGump gump)
        {
            XmlDocument doc = new XmlDocument();
            if(!info.Exists)
            {
                try
                {
                    using (StreamWriter w = new StreamWriter(info.FullName, false))
                    {
                        w.Write(Resources.defaultSpells);
                        w.Flush();
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"UOSteam -> Exception in LoadSpellDef: {e}");
                }
            }
            try
            {
                doc.Load(info.FullName);
            }
            catch (Exception e)
            {
                Log.Warn($"Exception in LoadSpellDef: {e}");
                return;
            }
            if (doc == null)
            {
                return;
            }
            XmlElement root = doc["spells"];
            if (root == null)
            {
                return;
            }

            Dictionary<uint, Reagents> reagsdict = new Dictionary<uint, Reagents>()
                {
                    { 0xF78, Reagents.BatWing },
                    { 0xF7A, Reagents.BlackPearl },
                    { 0xF7B, Reagents.Bloodmoss },
                    { 0xF7E, Reagents.Bone },
                    { 0xF7D, Reagents.DaemonBlood },
                    { 0xF80, Reagents.DemonBone },
                    { 0xF82, Reagents.DragonsBlood },
                    { 0x4077, Reagents.DragonsBlood },//new clients
                    { 0xF81, Reagents.FertileDirt },
                    { 0xF84, Reagents.Garlic },
                    { 0xF85, Reagents.Ginseng },
                    { 0xF8F, Reagents.GraveDust },
                    { 0xF86, Reagents.MandrakeRoot },
                    { 0xF88, Reagents.Nightshade },
                    { 0xF8E, Reagents.NoxCrystal },
                    { 0xF8A, Reagents.PigIron },
                    { 0xF8D, Reagents.SpidersSilk },
                    { 0xF8C, Reagents.SulfurousAsh },
                    { 0xF79, Reagents.Blackmoor },
                    { 0xF7C, Reagents.Bloodspawn },
                    { 0xF90, Reagents.DeadWood },
                    { 0xF91, Reagents.WyrmHeart }
                };
            //enum tryparse is performance awful, better to do it this way
            Dictionary<string, TargetType> getTargetFlag = new Dictionary<string, TargetType>()
                {
                    {"neutral", TargetType.Neutral},
                    {"harmful", TargetType.Harmful},
                    {"beneficial", TargetType.Beneficial}
                };
            int id, circle;
            string name, classname;
            Spell.SpellsByID.Clear();
            Spell.SpellsByName.Clear();
            string GetClassName(int spellid)
            {
                switch(spellid / 100)
                {
                    case 0:
                        return "Magery";
                    case 1:
                        return "Necromancy";
                    case 2:
                        return "Chivalry";
                    case 3:
                        return "Undefined";
                    case 4:
                        return "Bushido";
                    case 5:
                        return "Ninjisu";
                    case 6:
                        if(spellid < 678)
                            return "Mysticism";
                        return "Spellweaving";
                    case 7:
                        return "Bardic";
                    default:
                        return "Unknown";
                }
            }
            Dictionary<string, List<string>> orderedspells = new Dictionary<string, List<string>>();
            foreach (XmlElement spell in root.GetElementsByTagName("spell"))
            {
                id = (int)GetAttributeUInt(spell, "id");
                circle = (int)GetAttributeUInt(spell, "circle");//this is used on spell categorization
                name = GetAttribute(spell, "name");
                if (id > 0 && !string.IsNullOrEmpty(name))
                {
                    int iconid = (int)GetAttributeUInt(spell, "iconid"), smalliconid = (int)GetAttributeUInt(spell, "smalliconid"),
                        manacost = (int)GetAttributeUInt(spell, "mana"), tithingcost = (int)GetAttributeUInt(spell, "tithing"),
                        minskill = (int)GetAttributeUInt(spell, "minskill");
                    uint timeout = GetAttributeUInt(spell, "timeout");
                    string ttype = GetAttribute(spell, "flag", "neutral"), words = GetAttribute(spell, "words", string.Empty), reagentstring = GetAttribute(spell, "reagents", string.Empty);
                    if (string.IsNullOrWhiteSpace(ttype) || !getTargetFlag.TryGetValue(ttype.Trim().ToLower(), out TargetType targetType))
                        targetType = TargetType.Neutral;
                    string[] reagsarr = reagentstring.Split(',');
                    List<Reagents> reagslist = new List<Reagents>();
                    foreach (string s in reagsarr)
                    {
                        bool hex = s.StartsWith("0x");
                        if (uint.TryParse(hex ? s.Substring(2) : s, hex ? NumberStyles.HexNumber : NumberStyles.Integer, CultureInfo.InvariantCulture, out uint ru) && reagsdict.TryGetValue(ru, out Reagents reag) && reag != Reagents.None)
                        {
                            reagslist.Add(reag);
                        }
                    }
                    if(string.IsNullOrEmpty(classname = GetAttribute(spell, "classname")))
                        classname = GetClassName(id);
                    if (!orderedspells.TryGetValue(classname, out List<string> list))
                        orderedspells[classname] = list = new List<string>();
                    list.Add(name);
                    SpellDefinition.FullIndexSetModifySpell(id, id % 100, iconid, smalliconid, minskill, manacost, tithingcost, name, words, targetType, reagslist.ToArray());
                    name = name.ToLower(Culture);
                    Spell.SpellsByID[id] = Spell.SpellsByName[name] = Spell.SpellsByName[name.Replace(" ", "")] = new Spell((int)targetType, id, circle, words, reagslist);
                }
            }
            foreach(KeyValuePair<string, List<string>> kvp in orderedspells)
            {
                gump.AddSpellsToHotkeys(kvp.Key, kvp.Value);
            }
        }

        internal static void LoadSkillDef(FileInfo info, AssistantGump gump)
        {
            List<SkillEntry> skillEntries = new List<SkillEntry>();
            XmlDocument doc = new XmlDocument();
            if(!info.Exists)
            {
                try
                {
                    using (StreamWriter w = new StreamWriter(info.FullName, false))
                    {
                        w.Write(Resources.defaultSkills);
                        w.Flush();
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"UOSteam -> Exception in LoadSkillDef: {e}");
                }
            }
            try
            {
                doc.Load(info.FullName);
            }
            catch (Exception e)
            {
                Log.Warn($"UOSteam -> Exception in LoadSkillDef: {e}");
            }

            int i = 0, count = 0;
            XmlElement root;
            if (doc != null && (root = doc["skills"]) != null)
            {
                foreach (XmlElement skill in root.GetElementsByTagName("skill"))
                {
                    string name = GetAttribute(skill, "name");
                    bool passive = GetAttributeBool(skill, "passive", true);
                    if (!string.IsNullOrEmpty(name))
                        skillEntries.Add(new SkillEntry(i++, name, !passive));
                    count++;
                }
                if (count == i && skillEntries.Count > 0)
                {
                    SetAllSkills(skillEntries);
                }
                else
                    Log.Warn($"Skills count isn't equal to readed skills in LoadSkills: {count} present vs {i} correctly readed");
            }
            else
            {
                skillEntries.AddRange(SkillsLoader.Instance.Skills);
            }

            Dictionary<int, string> skn = new Dictionary<int, string>
            {
                [-1] = "Last"
            };
            for (i = 0; i < skillEntries.Count; i++)
            {
                SkillEntry sk = skillEntries[i];
                if (sk.HasAction)
                    skn[sk.Index] = sk.Name;
            }
            HotKeys.SkillHotKeys.CleanUP();
            gump.SkillsHK.SetItemsValue(skn);
            ScriptManager.SkillMap.Clear();
            foreach (KeyValuePair<int, string> kvp in skn)
            {
                ScriptManager.SkillMap[kvp.Value.ToLower(Culture)] = kvp.Key;
                ScriptManager.SkillMap[kvp.Value.Replace(" ", "").ToLower(Culture)] = kvp.Key;
            }
            HotKeys.SkillHotKeys.Initialize();
        }

        internal static void LoadBodyDef(FileInfo info)
        {
            XmlDocument doc = new XmlDocument();
            if (!info.Exists)
            {
                try
                {
                    using (StreamWriter w = new StreamWriter(info.FullName, false))
                    {
                        w.Write(Resources.defaultBodies);
                        w.Flush();
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"UOSteam -> Exception in LoadBodyDef: {e}");
                }
            }
            try
            {
                doc.Load(info.FullName);
            }
            catch (Exception e)
            {
                Log.Warn($"UOSteam -> Exception in LoadBodyDef: {e}");
            }

            XmlElement root;
            if (doc != null && (root = doc["bodies"]) != null)
            {
                XmlElement basic = root["humanoid"];
                ushort body;
                string bodyname;
                if (basic != null)
                {

                    foreach (XmlElement bodies in basic.GetElementsByTagName("body"))
                    {
                        body = GetAttributeUShort(bodies, "graphic");
                        if(body > 0)
                        {
                            bodyname = GetAttribute(bodies, "name");
                            Targeting.Humanoid.Add(body);
                        }
                    }
                }
                basic = root["transformation"];
                if (basic != null)
                {
                    foreach (XmlElement bodies in basic.GetElementsByTagName("body"))
                    {
                        body = GetAttributeUShort(bodies, "graphic");
                        if (body > 0)
                        {
                            bodyname = GetAttribute(bodies, "name");
                            Targeting.Transformation.Add(body);
                        }
                    }
                }
            }
        }

        internal static void LoadFoodDef(FileInfo info)
        {
            XmlDocument doc = new XmlDocument();
            if (!info.Exists)
            {
                try
                {
                    using (StreamWriter w = new StreamWriter(info.FullName, false))
                    {
                        w.Write(Resources.defaultFoods);
                        w.Flush();
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"UOSteam -> Exception in LoadFoodDef: {e}");
                }
            }
            try
            {
                doc.Load(info.FullName);
            }
            catch (Exception e)
            {
                Log.Warn($"UOSteam -> Exception in LoadFoodDef: {e}");
            }

            XmlElement root;
            if (doc != null && (root = doc["foods"]) != null)
            {
                string groupname, name;
                ushort graphic;
                foreach (XmlElement group in root.GetElementsByTagName("group"))
                {
                    foreach (XmlElement food in group.GetElementsByTagName("food"))
                    {
                        Foods.AddFood(GetAttribute(group, "name"), GetAttribute(food, "name"), GetAttributeUShort(food, "graphic"));
                    }
                }
            }
        }

        internal static void LoadBuffDef(FileInfo info, AssistantGump gump)
        {
            XmlDocument doc = new XmlDocument();
            if (!info.Exists)
            {
                try
                {
                    using (StreamWriter w = new StreamWriter(info.FullName, false))
                    {
                        w.Write(Resources.defaultBuffIcons);
                        w.Flush();
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"UOSteam -> Exception in LoadBuffDef: {e}");
                }
            }
            try
            {
                doc.Load(info.FullName);
            }
            catch (Exception e)
            {
                Log.Warn($"UOSteam -> Exception in LoadBuffDef: {e}");
            }

            XmlElement root;
            
            if (doc != null && (root = doc["bufficons"]) != null)
            {
                foreach(XmlElement icons in root.GetElementsByTagName("icon"))
                {
                    int icon = (int)GetAttributeUInt(icons, "id");
                    if(icon > 0)
                    {
                        string name = GetAttribute(icons, "name");
                        if(!string.IsNullOrEmpty(name))
                        {
                            name = name.ToLower(Culture);
                            PlayerData.BuffNames[name] = PlayerData.BuffNames[name.Replace(" ", "")] = icon;
                        }
                    }
                }
            }
        }

        private static void SetAllSkills(List<SkillEntry> entries)
        {
            SkillsLoader.Instance.Skills.Clear();
            SkillsLoader.Instance.Skills.AddRange(entries);
            SkillsLoader.Instance.SortedSkills.Clear();
            SkillsLoader.Instance.SortedSkills.AddRange(entries);
            SkillsLoader.Instance.SortedSkills.Sort((a, b) => a.Name.CompareTo(b.Name));
        }


        internal static void SaveConfig(AssistantGump gump)
        {
            try
            {
                FileInfo info = new FileInfo(Path.Combine(Profile.DataPath, $"assistant.xml"));
                using (StreamWriter op = new StreamWriter(info.FullName))
                {
                    XmlTextWriter xml = new XmlTextWriter(op)
                    {
                        Formatting = Formatting.Indented,
                        IndentChar = ' ',
                        Indentation = 1
                    };

                    xml.WriteStartDocument(true);
                    xml.WriteStartElement("config");

                    xml.WriteStartElement("data");
                    xml.WriteAttributeString("name", "LastProfile");
                    xml.WriteString($"{gump.LastProfile}.xml");
                    xml.WriteEndElement();

                    xml.WriteStartElement("data");
                    xml.WriteAttributeString("name", "SmartProfile");
                    xml.WriteString($"{gump.SmartProfile}");
                    xml.WriteEndElement();

                    xml.WriteStartElement("snapshots");
                    xml.WriteStartElement("ownDeath");
                    xml.WriteString(UOSObjects.Gump.SnapOwnDeath.ToString());
                    xml.WriteEndElement();
                    xml.WriteStartElement("othersDeath");
                    xml.WriteString(UOSObjects.Gump.SnapOtherDeath.ToString());
                    xml.WriteEndElement();
                    xml.WriteEndElement();
                    /*xml.WriteStartElement("data");
                    xml.WriteAttributeString("name", "NegotiateFeatures");
                    xml.WriteString($"{gump.NegotiateFeatures}");
                    xml.WriteEndElement();*/
                    if (gump.ItemsToLoot.Count > 0)
                    {
                        xml.WriteStartElement("autoloot");
                        foreach (KeyValuePair<ushort, (ushort, string)> kvp in gump.ItemsToLoot)
                        {
                            xml.WriteStartElement("item");
                            xml.WriteAttributeString("type", $"{kvp.Key}");
                            xml.WriteAttributeString("limit", $"{kvp.Value.Item1}");
                            xml.WriteAttributeString("name", $"{kvp.Value.Item2}");
                            xml.WriteEndElement();
                        }
                        xml.WriteEndElement();
                    }

                    xml.WriteStartElement("gump");
                    xml.WriteAttributeString("x", gump.X.ToString());
                    xml.WriteAttributeString("y", gump.Y.ToString());
                    xml.WriteEndElement();

                    xml.WriteEndElement();
                }
            }
            catch { }
        }
        internal static bool SaveProfile(string name = null)
        {
            AssistantGump gump = UOSObjects.Gump;
            if (gump == null)
                return false;
            SaveConfig(gump);
            string filename = name ?? gump.LastProfile;
            if (!string.IsNullOrWhiteSpace(filename))
            {
                try
                {
                    FileInfo info = new FileInfo(Path.Combine(Profile.ProfilePath, $"{filename}.xml"));
                    using (StreamWriter op = new StreamWriter(info.FullName))
                    {
                        XmlTextWriter xml = new XmlTextWriter(op)
                        {
                            Formatting = Formatting.Indented,
                            IndentChar = ' ',
                            Indentation = 1
                        };

                        xml.WriteStartDocument(true);
                        //BEGIN PROFILE
                        xml.WriteStartElement("profile");
                        for (int i = 0; i < Filter.List.Count; i++)
                        {
                            Filter f = Filter.List[i];
                            xml.WriteStartElement("data");
                            xml.WriteAttributeString("name", f.XmlName);
                            xml.WriteString(f.Enabled.ToString());
                            xml.WriteEndElement();
                        }
                        void WriteData(string mname, string vval)
                        {
                            xml.WriteStartElement("data");
                            xml.WriteAttributeString("name", mname);
                            xml.WriteString(vval);
                            xml.WriteEndElement();
                        }
                        WriteData("UseObjectsQueue", gump.UseObjectsQueue.ToString());
                        WriteData("UseTargetQueue", gump.UseTargetQueue.ToString());
                        WriteData("ShowBandageTimerStart", gump.ShowBandageTimerStart.ToString());
                        WriteData("ShowBandageTimerEnd", gump.ShowBandageTimerEnd.ToString());
                        WriteData("ShowBandageTimerOverhead", gump.ShowBandageTimerOverhead.ToString());
                        WriteData("ShowCorpseNames", gump.ShowCorpseNames.ToString());
                        WriteData("OpenCorpses", gump.OpenCorpses.ToString());
                        WriteData("ShowMobileHits", gump.ShowMobileHits.ToString());
                        WriteData("HandsBeforePotions", gump.HandsBeforePotions.ToString());
                        WriteData("HandsBeforeCasting", gump.HandsBeforeCasting.ToString());
                        WriteData("HighlightCurrentTarget", gump.HighlightCurrentTarget.ToString());
                        WriteData("HighlightCurrentTargetHue", gump.HighlightCurrentTargetHue.ToString());
                        WriteData("BlockInvalidHeal", gump.BlockInvalidHeal.ToString());
                        WriteData("BoneCutter", gump.BoneCutter.ToString());
                        WriteData("AutoMount", gump.AutoMount.ToString());
                        WriteData("AutoBandage", gump.AutoBandage.ToString());
                        WriteData("AutoBandageScale", gump.AutoBandageScale.ToString());
                        WriteData("AutoBandageCount", gump.AutoBandageCount.ToString());
                        WriteData("AutoBandageStart", gump.AutoBandageStart.ToString());
                        WriteData("AutoBandageFormula", gump.AutoBandageFormula.ToString());
                        WriteData("AutoBandageHidden", gump.AutoBandageHidden.ToString());
                        WriteData("OpenDoors", gump.OpenDoors.ToString());
                        WriteData("UseDoors", gump.UseDoors.ToString());
                        WriteData("ShowMobileFlags", gump.ShowMobileFlags.ToString());
                        WriteData("CountStealthSteps", gump.CountStealthSteps.ToString());
                        WriteData("FriendsListOnly", gump.FriendsListOnly.ToString());
                        WriteData("FriendsParty", gump.FriendsParty.ToString());
                        WriteData("MoveConflictingItems", gump.MoveConflictingItems.ToString());
                        WriteData("PreventDismount", gump.PreventDismount.ToString());
                        WriteData("PreventAttackFriends", gump.PreventAttackFriends.ToString());
                        WriteData("AutoSearchContainers", gump.AutoSearchContainers.ToString());
                        WriteData("AutoAcceptParty", gump.AutoAcceptParty.ToString());

                        WriteData("OpenCorpsesRange", $"0x{gump.OpenCorpsesRange:X2}");
                        //WriteData("UseObjectsLimit", $"0x{gump.UseObjectsLimit:X2}");
                        WriteData("SmartTargetRange", gump.SmartTargetRange.ToString());
                        WriteData("SmartTargetRangeValue", $"0x{gump.SmartTargetRangeValue:X2}");
                        WriteData("FixedSeason", $"0x{gump.FixedSeason:X2}");
                        WriteData("SmartTarget", $"0x{gump.SmartTarget:X2}");
                        WriteData("TargetShare", $"0x{gump.TargetShare:X2}");
                        WriteData("AutoBandageStartValue", $"0x{gump.AutoBandageStartValue:X2}");
                        WriteData("SpellsTargetShare", $"0x{gump.SpellsTargetShare:X2}");
                        WriteData("OpenDoorsMode", $"0x{gump.OpenDoorsMode:X2}");
                        WriteData("OpenCorpsesMode", $"0x{gump.OpenCorpsesMode:X2}");
                        WriteData("CustomCaptionMode", $"0x{gump.CustomCaptionMode:X2}");
                        WriteData("GrabHotBag", $"0x{gump.GrabHotBag:X8}");
                        WriteData("MountSerial", $"0x{gump.MountSerial:X8}");
                        WriteData("BladeSerial", $"0x{gump.BladeSerial:X8}");
                        WriteData("AutoBandageTarget", $"0x{gump.AutoBandageTarget:X8}");
                        WriteData("AutoBandageDelay", $"{gump.AutoBandageDelay}");
                        WriteData("ActionDelay", $"{gump.ActionDelay}");
                        WriteData("DressTypeDefault", gump.TypeDress.ToString());
                        WriteData("ReturnToParentScript", gump.ReturnToParentScript.ToString());

                        #region friends
                        if (gump.FriendDictionary.Count > 0)
                        {
                            xml.WriteStartElement("friends");
                            foreach (KeyValuePair<uint, string> kvp in gump.FriendDictionary)
                            {
                                xml.WriteStartElement("friend");
                                xml.WriteAttributeString("name", kvp.Value);
                                xml.WriteString($"0x{kvp.Key:X}");
                                xml.WriteEndElement();
                            }
                            xml.WriteEndElement();
                        }
                        if (ScriptManager.MacroDictionary.Count > 0)
                        {
                            xml.WriteStartElement("macros");
                            foreach (KeyValuePair<string, HotKeyOpts> kvp in ScriptManager.MacroDictionary)
                            {
                                xml.WriteStartElement("macro");
                                xml.WriteAttributeString("loop", kvp.Value.Loop.ToString());
                                xml.WriteAttributeString("name", kvp.Key);
                                xml.WriteAttributeString("interrupt", (!kvp.Value.NoAutoInterrupt).ToString());
                                xml.WriteString(kvp.Value.Macro.Replace('\n', ';'));
                                xml.WriteEndElement();
                            }
                            xml.WriteEndElement();
                        }
                        if (HotKeys.AllHotKeys.Count > 0)
                        {
                            xml.WriteStartElement("hotkeys");
                            foreach (KeyValuePair<uint, HotKeyOpts> kvp in HotKeys.AllHotKeys)
                            {
                                xml.WriteStartElement("hotkey");
                                xml.WriteAttributeString("action", kvp.Value.Action);
                                xml.WriteAttributeString("key", $"0x{kvp.Key:X}");
                                xml.WriteAttributeString("pass", kvp.Value.PassToUO.ToString());
                                if (!string.IsNullOrEmpty(kvp.Value.Param))
                                {
                                    xml.WriteAttributeString("param", kvp.Value.Param);
                                }
                                xml.WriteEndElement();
                            }
                            xml.WriteEndElement();
                        }
                        #endregion

                        #region autoloot
                        xml.WriteStartElement("autoloot");

                        xml.WriteStartElement("enabled");
                        xml.WriteString(gump.AutoLoot.ToString());
                        xml.WriteEndElement();

                        xml.WriteStartElement("container");
                        xml.WriteString($"0x{gump.AutoLootContainer:X}");
                        xml.WriteEndElement();

                        xml.WriteStartElement("guards");
                        xml.WriteString(gump.NoAutoLootInGuards.ToString());
                        xml.WriteEndElement();

                        xml.WriteEndElement();
                        #endregion

                        #region dresslist
                        foreach(DressList dl in DressList.DressLists)
                        {
                            if (dl == null)
                                continue;
                            xml.WriteStartElement("dresslist");
                            xml.WriteAttributeString("name", dl.Name);
                            if(SerialHelper.IsItem(dl.UndressBag))
                                xml.WriteAttributeString("container", $"0x{dl.UndressBag:X8}");
                            foreach (KeyValuePair<Layer, DressItem> kvp in dl.LayerItems)
                            {
                                xml.WriteStartElement("item");
                                xml.WriteAttributeString("layer", ((int)kvp.Key).ToString());
                                if(kvp.Value.ObjType > 0)
                                    xml.WriteAttributeString("type", $"0x{kvp.Value.ObjType:X4}");
                                xml.WriteString($"0x{kvp.Value.Serial:X8}");
                                xml.WriteEndElement();
                            }
                            xml.WriteEndElement();
                        }
                        #endregion

                        #region organizers
                        if (Organizer.Organizers.Count > 0)
                        {
                            xml.WriteStartElement("organizer");
                            for(int i = 0; i < Organizer.Organizers.Count; ++i)
                            {
                                Organizer org = Organizer.Organizers[i];
                                if (org == null)
                                    continue;

                                xml.WriteStartElement("group");
                                xml.WriteAttributeString("source", $"0x{org.SourceCont:X}");
                                xml.WriteAttributeString("stack", $"{org.Stack}");
                                xml.WriteAttributeString("target", $"0x{org.TargetCont:X}");
                                xml.WriteAttributeString("loop", $"{org.Loop}");
                                xml.WriteAttributeString("complete", $"{org.Complete}");
                                xml.WriteAttributeString("name", org.Name);
                                if(org.Items.Count > 0)
                                {
                                    foreach(ItemDisplay oi in org.Items)
                                    {
                                        xml.WriteStartElement("item");
                                        xml.WriteAttributeString("amount", $"0x{oi.Amount:X}");
                                        xml.WriteAttributeString("graphic", $"0x{oi.Graphic:X4}");
                                        xml.WriteAttributeString("hue", $"0x{oi.Hue:X4}");
                                        xml.WriteAttributeString("name", oi.Name);
                                        xml.WriteEndElement();
                                    }
                                }
                                xml.WriteEndElement();
                            }
                            xml.WriteEndElement();
                        }
                        #endregion

                        #region scavenger
                        xml.WriteStartElement("scavenger");
                        xml.WriteAttributeString("enabled", Scavenger.Enabled.ToString());
                        var scav = Scavenger.ItemIDsHues;
                        if (scav.Count > 0)
                        {
                            foreach (List<ItemDisplay> list in scav.Values)
                            {
                                foreach(ItemDisplay id in list)
                                {
                                    xml.WriteStartElement("scavenge");
                                    xml.WriteAttributeString("graphic", $"0x{id.Graphic:X4}");
                                    xml.WriteAttributeString("color", $"0x{id.Hue:X4}");
                                    xml.WriteAttributeString("enabled", id.Enabled.ToString());
                                    xml.WriteAttributeString("name", id.Name);
                                    xml.WriteEndElement();
                                }
                            }
                        }
                        xml.WriteEndElement();
                        #endregion

                        #region vendors
                        if (Vendors.Buy.BuyList.Count > 0 || Vendors.Sell.SellList.Count > 0)
                        {
                            xml.WriteStartElement("vendors");
                            if(Vendors.Buy.BuySelected != null)
                            {
                                xml.WriteStartElement("buystate");
                                xml.WriteAttributeString("enabled", Vendors.Buy.BuySelected.Enabled.ToString());
                                xml.WriteAttributeString("list", Vendors.Buy.BuySelected.Name);
                                xml.WriteEndElement();
                            }
                            if (Vendors.Sell.SellSelected != null)
                            {
                                xml.WriteStartElement("sellstate");
                                xml.WriteAttributeString("enabled", Vendors.Sell.SellSelected.Enabled.ToString());
                                xml.WriteAttributeString("list", Vendors.Sell.SellSelected.Name);
                                xml.WriteEndElement();
                            }
                            foreach (var kvp in Vendors.Buy.BuyList)
                            {
                                xml.WriteStartElement("shoppinglist");
                                xml.WriteAttributeString("limit", kvp.Key.MaxAmount.ToString());
                                xml.WriteAttributeString("name", kvp.Key.Name);
                                xml.WriteAttributeString("type", "Buy");
                                xml.WriteAttributeString("complete", kvp.Key.Complete.ToString());

                                foreach (BuySellEntry bse in kvp.Value)
                                {
                                    xml.WriteStartElement("item");
                                    xml.WriteAttributeString("graphic", $"0x{bse.ItemID:X}");
                                    xml.WriteAttributeString("amount", bse.Amount.ToString());
                                    xml.WriteEndElement();
                                }

                                xml.WriteEndElement();
                            }
                            foreach (var kvp in Vendors.Sell.SellList)
                            {
                                xml.WriteStartElement("shoppinglist");
                                xml.WriteAttributeString("limit", kvp.Key.MaxAmount.ToString());
                                xml.WriteAttributeString("name", kvp.Key.Name);
                                xml.WriteAttributeString("type", "Sell");
                                xml.WriteAttributeString("complete", kvp.Key.Complete.ToString());

                                foreach (BuySellEntry bse in kvp.Value)
                                {
                                    xml.WriteStartElement("item");
                                    xml.WriteAttributeString("graphic", $"0x{bse.ItemID:X}");
                                    xml.WriteAttributeString("amount", bse.Amount.ToString());
                                    xml.WriteEndElement();
                                }

                                xml.WriteEndElement();
                            }
                            xml.WriteEndElement();
                        }
                        #endregion
                        //END PROFILE
                        xml.WriteEndElement();
                        xml.Close();
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        #region SDL-VK_KEY_CONVERTER
        internal static readonly Dictionary<SDL.SDL_Keycode, (uint, string)> SDLkeyToVK = new Dictionary<SDL.SDL_Keycode, (uint, string)>()
            {
                {SDL.SDL_Keycode.SDLK_UNKNOWN, (0x00, "NONE")},
                //0x01 -> 0x03 left - right - control break on MOUSE
                {(SDL.SDL_Keycode)2, (0x04, "Middle Mouse")},//MOUSE MIDDLE
                {(SDL.SDL_Keycode)3, (0x0103, "X1 Mouse")},//mouse X1
                {(SDL.SDL_Keycode)4, (0x0104, "X2 Mouse")},//mouse X2
                {(SDL.SDL_Keycode)0x101, (0x0101, "Scroll UP")},//mouse scroll UP
                {(SDL.SDL_Keycode)0x102, (0x0102, "Scroll DOWN")},//mouse scroll DOWN
                {SDL.SDL_Keycode.SDLK_BACKSPACE, (0x08, "Backspace")},
                {SDL.SDL_Keycode.SDLK_TAB, (0x09, "Tab")},
                //0x0A -> 0x0B RESERVED
                {SDL.SDL_Keycode.SDLK_CLEAR, (0x0C, "Clear")},
                {SDL.SDL_Keycode.SDLK_KP_ENTER, (0x0D, "Return")},
                {SDL.SDL_Keycode.SDLK_RETURN, (0x0D, "Return")},
                //0x0E -> 0x0F UNDEFINED
                //0x10 -> Shift
                //0x11 -> CTRL
                //0x12 -> ALT
                {SDL.SDL_Keycode.SDLK_PAUSE, (0x13, "Pause")},
                {SDL.SDL_Keycode.SDLK_CAPSLOCK, (0x14, "CAPS")},
                //0x15 IME Kana/Hanguel mode
                //0x16 UNDEFINED
                //0x17 IME Junja mode
                //0x18 IME final mode
                //0x19 IME Hanja mode
                //0x1A UNDEFINED
                {SDL.SDL_Keycode.SDLK_ESCAPE, (0x1B, "Esc")},
                //0x1C IME Convert
                //0x1D IME NonConvert
                //0x1E IME Accept
                //0x1F IME mode change request
                {SDL.SDL_Keycode.SDLK_SPACE, (0x20, "Space")},
                {SDL.SDL_Keycode.SDLK_PAGEUP, (0x21, "Page UP")},
                {SDL.SDL_Keycode.SDLK_PAGEDOWN, (0x22, "Page DOWN")},
                {SDL.SDL_Keycode.SDLK_END, (0x23, "END")},
                {SDL.SDL_Keycode.SDLK_HOME, (0x24, "HOME")},
                {SDL.SDL_Keycode.SDLK_LEFT, (0x25, "Left")},
                {SDL.SDL_Keycode.SDLK_UP, (0x26, "Up")},
                {SDL.SDL_Keycode.SDLK_RIGHT, (0x27, "Right")},
                {SDL.SDL_Keycode.SDLK_DOWN, (0x28, "Down")},
                {SDL.SDL_Keycode.SDLK_SELECT, (0x29, "Select")},
                //0x2A Print KEY (no print SCREEN)
                {SDL.SDL_Keycode.SDLK_EXECUTE, (0x2B, "Execute")},
                {SDL.SDL_Keycode.SDLK_PRINTSCREEN, (0x2C, "Stamp")},
                {SDL.SDL_Keycode.SDLK_INSERT, (0x2D, "INS")},
                {SDL.SDL_Keycode.SDLK_DELETE, (0x2E, "DEL")},
                {SDL.SDL_Keycode.SDLK_HELP, (0x2F, "Help")},
                {SDL.SDL_Keycode.SDLK_0, (0x30, "0")},
                {SDL.SDL_Keycode.SDLK_1, (0x31, "1")},
                {SDL.SDL_Keycode.SDLK_2, (0x32, "2")},
                {SDL.SDL_Keycode.SDLK_3, (0x33, "3")},
                {SDL.SDL_Keycode.SDLK_4, (0x34, "4")},
                {SDL.SDL_Keycode.SDLK_5, (0x35, "5")},
                {SDL.SDL_Keycode.SDLK_6, (0x36, "6")},
                {SDL.SDL_Keycode.SDLK_7, (0x37, "7")},
                {SDL.SDL_Keycode.SDLK_8, (0x38, "8")},
                {SDL.SDL_Keycode.SDLK_9, (0x39, "9")},
                //0x40 UNDEFINED
                {SDL.SDL_Keycode.SDLK_a, (0x41, "A")},
                {SDL.SDL_Keycode.SDLK_b, (0x42, "B")},
                {SDL.SDL_Keycode.SDLK_c, (0x43, "C")},
                {SDL.SDL_Keycode.SDLK_d, (0x44, "D")},
                {SDL.SDL_Keycode.SDLK_e, (0x45, "E")},
                {SDL.SDL_Keycode.SDLK_f, (0x46, "F")},
                {SDL.SDL_Keycode.SDLK_g, (0x47, "G")},
                {SDL.SDL_Keycode.SDLK_h, (0x48, "H")},
                {SDL.SDL_Keycode.SDLK_i, (0x49, "I")},
                {SDL.SDL_Keycode.SDLK_j, (0x4A, "J")},
                {SDL.SDL_Keycode.SDLK_k, (0x4B, "K")},
                {SDL.SDL_Keycode.SDLK_l, (0x4C, "L")},
                {SDL.SDL_Keycode.SDLK_m, (0x4D, "M")},
                {SDL.SDL_Keycode.SDLK_n, (0x4E, "N")},
                {SDL.SDL_Keycode.SDLK_o, (0x4F, "O")},
                {SDL.SDL_Keycode.SDLK_p, (0x50, "P")},
                {SDL.SDL_Keycode.SDLK_q, (0x51, "Q")},
                {SDL.SDL_Keycode.SDLK_r, (0x52, "R")},
                {SDL.SDL_Keycode.SDLK_s, (0x53, "S")},
                {SDL.SDL_Keycode.SDLK_t, (0x54, "T")},
                {SDL.SDL_Keycode.SDLK_u, (0x55, "U")},
                {SDL.SDL_Keycode.SDLK_v, (0x56, "V")},
                {SDL.SDL_Keycode.SDLK_w, (0x57, "W")},
                {SDL.SDL_Keycode.SDLK_x, (0x58, "X")},
                {SDL.SDL_Keycode.SDLK_y, (0x59, "Y")},
                {SDL.SDL_Keycode.SDLK_z, (0x5A, "Z")},
                {SDL.SDL_Keycode.SDLK_LGUI, (0x5B, "Left Win")},
                {SDL.SDL_Keycode.SDLK_RGUI, (0x5C, "Right Win")},
                {SDL.SDL_Keycode.SDLK_APPLICATION, (0x5D, "Application")},
                //0x5E RESERVED
                {SDL.SDL_Keycode.SDLK_SLEEP, (0x5F, "Sleep")},
                {SDL.SDL_Keycode.SDLK_KP_0, (0x60, "NUM 0")},
                {SDL.SDL_Keycode.SDLK_KP_1, (0x61, "NUM 1")},
                {SDL.SDL_Keycode.SDLK_KP_2, (0x62, "NUM 2")},
                {SDL.SDL_Keycode.SDLK_KP_3, (0x63, "NUM 3")},
                {SDL.SDL_Keycode.SDLK_KP_4, (0x64, "NUM 4")},
                {SDL.SDL_Keycode.SDLK_KP_5, (0x65, "NUM 5")},
                {SDL.SDL_Keycode.SDLK_KP_6, (0x66, "NUM 6")},
                {SDL.SDL_Keycode.SDLK_KP_7, (0x67, "NUM 7")},
                {SDL.SDL_Keycode.SDLK_KP_8, (0x68, "NUM 8")},
                {SDL.SDL_Keycode.SDLK_KP_9, (0x69, "NUM 9")},
                {SDL.SDL_Keycode.SDLK_KP_MULTIPLY, (0x6A, "KP *")},
                {SDL.SDL_Keycode.SDLK_KP_PLUS, (0x6B, "KP +")},
                {SDL.SDL_Keycode.SDLK_SEPARATOR, (0x6C, "Separator")},
                {SDL.SDL_Keycode.SDLK_KP_MINUS, (0x6D, "KP -")},
                {SDL.SDL_Keycode.SDLK_DECIMALSEPARATOR, (0x6E, "Decimal")},
                {SDL.SDL_Keycode.SDLK_KP_DIVIDE, (0x6F, "KP /")},
                {SDL.SDL_Keycode.SDLK_F1, (0x70, "F1")},
                {SDL.SDL_Keycode.SDLK_F2, (0x71, "F2")},
                {SDL.SDL_Keycode.SDLK_F3, (0x72, "F3")},
                {SDL.SDL_Keycode.SDLK_F4, (0x73, "F4")},
                {SDL.SDL_Keycode.SDLK_F5, (0x74, "F5")},
                {SDL.SDL_Keycode.SDLK_F6, (0x75, "F6")},
                {SDL.SDL_Keycode.SDLK_F7, (0x76, "F7")},
                {SDL.SDL_Keycode.SDLK_F8, (0x77, "F8")},
                {SDL.SDL_Keycode.SDLK_F9, (0x78, "F9")},
                {SDL.SDL_Keycode.SDLK_F10, (0x79, "F10")},
                {SDL.SDL_Keycode.SDLK_F11, (0x7A, "F11")},
                {SDL.SDL_Keycode.SDLK_F12, (0x7B, "F12")},
                {SDL.SDL_Keycode.SDLK_F13, (0x7C, "F13")},
                {SDL.SDL_Keycode.SDLK_F14, (0x7D, "F14")},
                {SDL.SDL_Keycode.SDLK_F15, (0x7E, "F15")},
                {SDL.SDL_Keycode.SDLK_F16, (0x7F, "F16")},
                {SDL.SDL_Keycode.SDLK_F17, (0x80, "F17")},
                {SDL.SDL_Keycode.SDLK_F18, (0x81, "F18")},
                {SDL.SDL_Keycode.SDLK_F19, (0x82, "F19")},
                {SDL.SDL_Keycode.SDLK_F20, (0x83, "F20")},
                {SDL.SDL_Keycode.SDLK_F21, (0x84, "F21")},
                {SDL.SDL_Keycode.SDLK_F22, (0x85, "F22")},
                {SDL.SDL_Keycode.SDLK_F23, (0x86, "F23")},
                {SDL.SDL_Keycode.SDLK_F24, (0x87, "F24")},
                //0x88 is UNASSIGNED
                {SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR, (0x90, "Num Lock")},
                {SDL.SDL_Keycode.SDLK_SCROLLLOCK, (0x91, "Scroll Lock")},
                //0x92 -> 0x96 OEM SPECIFIC
                //0x97 -> 0x9f UNASSIGNED
                {SDL.SDL_Keycode.SDLK_MENU, (0xA4, "Menu")},
                //0xA4 is Left Menu, while 0xA5 is Right Menu
                {SDL.SDL_Keycode.SDLK_AC_BACK, (0xA6, "App Back")},
                {SDL.SDL_Keycode.SDLK_AC_FORWARD, (0xA7, "App Forward")},
                {SDL.SDL_Keycode.SDLK_AC_REFRESH, (0xA8, "App Refresh")},
                {SDL.SDL_Keycode.SDLK_AC_STOP, (0xA9, "App Stop")},
                {SDL.SDL_Keycode.SDLK_AC_SEARCH, (0xAA, "App Search")},
                {SDL.SDL_Keycode.SDLK_AC_BOOKMARKS, (0xAB, "App Bookmark")},
                {SDL.SDL_Keycode.SDLK_AC_HOME, (0xAC, "App Home")},
                {SDL.SDL_Keycode.SDLK_AUDIOMUTE, (0xAD, "Mute")},
                {SDL.SDL_Keycode.SDLK_VOLUMEDOWN, (0xAE, "Volume DOWN")},
                {SDL.SDL_Keycode.SDLK_VOLUMEUP, (0xAF, "Volume UP")},
                {SDL.SDL_Keycode.SDLK_AUDIONEXT, (0xB0, "Next Track")},
                {SDL.SDL_Keycode.SDLK_AUDIOPREV, (0xB1, "Prev Track")},
                {SDL.SDL_Keycode.SDLK_AUDIOSTOP, (0xB2, "Stop Track")},
                {SDL.SDL_Keycode.SDLK_AUDIOPLAY, (0xB3, "Play Track")},
                {SDL.SDL_Keycode.SDLK_MAIL, (0xB4, "Mail")},
                {SDL.SDL_Keycode.SDLK_MEDIASELECT, (0xB5, "Sel Track")},
                {SDL.SDL_Keycode.SDLK_APP1, (0xB6, "Lauch App1")},
                {SDL.SDL_Keycode.SDLK_APP2, (0xB7, "Lauch App2")},
                //0xB8 -> 0xB9 RESERVED
                {SDL.SDL_Keycode.SDLK_SEMICOLON, (0xBA, ";")},
                {SDL.SDL_Keycode.SDLK_COMMA, (0xBB, ",")},
                {SDL.SDL_Keycode.SDLK_PLUS, (0xBC, "+")},
                {SDL.SDL_Keycode.SDLK_MINUS, (0xBD, "-")},
                {SDL.SDL_Keycode.SDLK_PERIOD, (0xBE, ".")},
                {SDL.SDL_Keycode.SDLK_SLASH, (0xBF, "/")},
                {SDL.SDL_Keycode.SDLK_BACKQUOTE, (0xC0, "`")},
                //0xC1 -> 0xD7 RESERVED
                //0xD8 -> 0xDA UNASSIGNED
                {SDL.SDL_Keycode.SDLK_LEFTBRACKET, (0xDB, "[")},
                {SDL.SDL_Keycode.SDLK_BACKSLASH, (0xDC, "\\")},
                {SDL.SDL_Keycode.SDLK_RIGHTBRACKET, (0xDD, "]")},
                {SDL.SDL_Keycode.SDLK_QUOTE, (0xDE, "'")},
                // 0xDF - no use -- 0xE0 RESERVED -- 0xE1 OEM Specific
                {SDL.SDL_Keycode.SDLK_LESS, (0xE2, "<")}
            };
        internal static readonly Dictionary<int, uint> SDLmodToVK = new Dictionary<int, uint>()
            {
                { 1, 512 },
                { 2, 512 },
                { 3, 512},
                { 64, 1024 },
                { 128, 1024 },
                { 192, 1024 },
                { 193, 1536 },
                { 194, 1536 },
                { 195, 1536 },
                { 256, 2048 },
                { 512, 2048 },
                { 768, 2048 },
                { 769, 2560 },
                { 770, 2560 },
                { 771, 2560 },
                { 832, 3072 },
                { 896, 3072 },
                { 960, 3072 },
                { 961, 3584 },
                { 962, 3584 },
                { 963, 3584 }
            };
        #endregion
        #region VK_KEY_CONVERTER
        internal static Dictionary<uint, SDL.SDL_Keycode> vkToSDLkey = new Dictionary<uint, SDL.SDL_Keycode>()
            {
                {0x00, SDL.SDL_Keycode.SDLK_UNKNOWN},
                {0x04, (SDL.SDL_Keycode)0x02},
                {0x103, (SDL.SDL_Keycode)0x03},
                {0x104, (SDL.SDL_Keycode)0x04},
                {0x101, (SDL.SDL_Keycode)0x101},
                {0x102, (SDL.SDL_Keycode)0x102},
                {0x08, SDL.SDL_Keycode.SDLK_BACKSPACE},
                {0x09, SDL.SDL_Keycode.SDLK_TAB},
                {0x0C, SDL.SDL_Keycode.SDLK_CLEAR},
                {0x0D, SDL.SDL_Keycode.SDLK_RETURN},
                {0x13, SDL.SDL_Keycode.SDLK_PAUSE},
                {0x14, SDL.SDL_Keycode.SDLK_CAPSLOCK},
                {0x1B, SDL.SDL_Keycode.SDLK_ESCAPE},
                {0x20, SDL.SDL_Keycode.SDLK_SPACE},
                {0x21, SDL.SDL_Keycode.SDLK_PAGEUP},
                {0x22, SDL.SDL_Keycode.SDLK_PAGEDOWN},
                {0x23, SDL.SDL_Keycode.SDLK_END},
                {0x24, SDL.SDL_Keycode.SDLK_HOME},
                {0x25, SDL.SDL_Keycode.SDLK_LEFT},
                {0x26, SDL.SDL_Keycode.SDLK_UP},
                {0x27, SDL.SDL_Keycode.SDLK_RIGHT},
                {0x28, SDL.SDL_Keycode.SDLK_DOWN},
                {0x29, SDL.SDL_Keycode.SDLK_SELECT},
                {0x2B, SDL.SDL_Keycode.SDLK_EXECUTE},
                {0x2C, SDL.SDL_Keycode.SDLK_PRINTSCREEN},
                {0x2D, SDL.SDL_Keycode.SDLK_INSERT},
                {0x2E, SDL.SDL_Keycode.SDLK_DELETE},
                {0x2F, SDL.SDL_Keycode.SDLK_HELP},
                {0x30, SDL.SDL_Keycode.SDLK_0},
                {0x31, SDL.SDL_Keycode.SDLK_1},
                {0x32, SDL.SDL_Keycode.SDLK_2},
                {0x33, SDL.SDL_Keycode.SDLK_3},
                {0x34, SDL.SDL_Keycode.SDLK_4},
                {0x35, SDL.SDL_Keycode.SDLK_5},
                {0x36, SDL.SDL_Keycode.SDLK_6},
                {0x37, SDL.SDL_Keycode.SDLK_7},
                {0x38, SDL.SDL_Keycode.SDLK_8},
                {0x39, SDL.SDL_Keycode.SDLK_9},
                {0x41, SDL.SDL_Keycode.SDLK_a},
                {0x42, SDL.SDL_Keycode.SDLK_b},
                {0x43, SDL.SDL_Keycode.SDLK_c},
                {0x44, SDL.SDL_Keycode.SDLK_d},
                {0x45, SDL.SDL_Keycode.SDLK_e},
                {0x46, SDL.SDL_Keycode.SDLK_f},
                {0x47, SDL.SDL_Keycode.SDLK_g},
                {0x48, SDL.SDL_Keycode.SDLK_h},
                {0x49, SDL.SDL_Keycode.SDLK_i},
                {0x4A, SDL.SDL_Keycode.SDLK_j},
                {0x4B, SDL.SDL_Keycode.SDLK_k},
                {0x4C, SDL.SDL_Keycode.SDLK_l},
                {0x4D, SDL.SDL_Keycode.SDLK_m},
                {0x4E, SDL.SDL_Keycode.SDLK_n},
                {0x4F, SDL.SDL_Keycode.SDLK_o},
                {0x50, SDL.SDL_Keycode.SDLK_p},
                {0x51, SDL.SDL_Keycode.SDLK_q},
                {0x52, SDL.SDL_Keycode.SDLK_r},
                {0x53, SDL.SDL_Keycode.SDLK_s},
                {0x54, SDL.SDL_Keycode.SDLK_t},
                {0x55, SDL.SDL_Keycode.SDLK_u},
                {0x56, SDL.SDL_Keycode.SDLK_v},
                {0x57, SDL.SDL_Keycode.SDLK_w},
                {0x58, SDL.SDL_Keycode.SDLK_x},
                {0x59, SDL.SDL_Keycode.SDLK_y},
                {0x5A, SDL.SDL_Keycode.SDLK_z},
                {0x5B, SDL.SDL_Keycode.SDLK_LGUI},
                {0x5C, SDL.SDL_Keycode.SDLK_RGUI},
                {0x5D, SDL.SDL_Keycode.SDLK_APPLICATION},
                {0x5F, SDL.SDL_Keycode.SDLK_SLEEP},
                {0x60, SDL.SDL_Keycode.SDLK_KP_0},
                {0x61, SDL.SDL_Keycode.SDLK_KP_1},
                {0x62, SDL.SDL_Keycode.SDLK_KP_2},
                {0x63, SDL.SDL_Keycode.SDLK_KP_3},
                {0x64, SDL.SDL_Keycode.SDLK_KP_4},
                {0x65, SDL.SDL_Keycode.SDLK_KP_5},
                {0x66, SDL.SDL_Keycode.SDLK_KP_6},
                {0x67, SDL.SDL_Keycode.SDLK_KP_7},
                {0x68, SDL.SDL_Keycode.SDLK_KP_8},
                {0x69, SDL.SDL_Keycode.SDLK_KP_9},
                {0x6A, SDL.SDL_Keycode.SDLK_KP_MULTIPLY},
                {0x6B, SDL.SDL_Keycode.SDLK_KP_PLUS},
                {0x6C, SDL.SDL_Keycode.SDLK_SEPARATOR},
                {0x6D, SDL.SDL_Keycode.SDLK_KP_MINUS},
                {0x6E, SDL.SDL_Keycode.SDLK_DECIMALSEPARATOR},
                {0x6F, SDL.SDL_Keycode.SDLK_KP_DIVIDE},
                {0x70, SDL.SDL_Keycode.SDLK_F1},
                {0x71, SDL.SDL_Keycode.SDLK_F2},
                {0x72, SDL.SDL_Keycode.SDLK_F3},
                {0x73, SDL.SDL_Keycode.SDLK_F4},
                {0x74, SDL.SDL_Keycode.SDLK_F5},
                {0x75, SDL.SDL_Keycode.SDLK_F6},
                {0x76, SDL.SDL_Keycode.SDLK_F7},
                {0x77, SDL.SDL_Keycode.SDLK_F8},
                {0x78, SDL.SDL_Keycode.SDLK_F9},
                {0x79, SDL.SDL_Keycode.SDLK_F10},
                {0x7A, SDL.SDL_Keycode.SDLK_F11},
                {0x7B, SDL.SDL_Keycode.SDLK_F12},
                {0x7C, SDL.SDL_Keycode.SDLK_F13},
                {0x7D, SDL.SDL_Keycode.SDLK_F14},
                {0x7E, SDL.SDL_Keycode.SDLK_F15},
                {0x7F, SDL.SDL_Keycode.SDLK_F16},
                {0x80, SDL.SDL_Keycode.SDLK_F17},
                {0x81, SDL.SDL_Keycode.SDLK_F18},
                {0x82, SDL.SDL_Keycode.SDLK_F19},
                {0x83, SDL.SDL_Keycode.SDLK_F20},
                {0x84, SDL.SDL_Keycode.SDLK_F21},
                {0x85, SDL.SDL_Keycode.SDLK_F22},
                {0x86, SDL.SDL_Keycode.SDLK_F23},
                {0x87, SDL.SDL_Keycode.SDLK_F24},
                {0x90, SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR},
                {0x91, SDL.SDL_Keycode.SDLK_SCROLLLOCK},
                {0xA4, SDL.SDL_Keycode.SDLK_MENU},
                {0xA6, SDL.SDL_Keycode.SDLK_AC_BACK},
                {0xA7, SDL.SDL_Keycode.SDLK_AC_FORWARD},
                {0xA8, SDL.SDL_Keycode.SDLK_AC_REFRESH},
                {0xA9, SDL.SDL_Keycode.SDLK_AC_STOP},
                {0xAA, SDL.SDL_Keycode.SDLK_AC_SEARCH},
                {0xAB, SDL.SDL_Keycode.SDLK_AC_BOOKMARKS},
                {0xAC, SDL.SDL_Keycode.SDLK_AC_HOME},
                {0xAD, SDL.SDL_Keycode.SDLK_AUDIOMUTE},
                {0xAE, SDL.SDL_Keycode.SDLK_VOLUMEDOWN},
                {0xAF, SDL.SDL_Keycode.SDLK_VOLUMEUP},
                {0xB0, SDL.SDL_Keycode.SDLK_AUDIONEXT},
                {0xB1, SDL.SDL_Keycode.SDLK_AUDIOPREV},
                {0xB2, SDL.SDL_Keycode.SDLK_AUDIOSTOP},
                {0xB3, SDL.SDL_Keycode.SDLK_AUDIOPLAY},
                {0xB4, SDL.SDL_Keycode.SDLK_MAIL},
                {0xB5, SDL.SDL_Keycode.SDLK_MEDIASELECT},
                {0xB6, SDL.SDL_Keycode.SDLK_APP1},
                {0xB7, SDL.SDL_Keycode.SDLK_APP2},
                {0xBA, SDL.SDL_Keycode.SDLK_SEMICOLON},
                {0xBB, SDL.SDL_Keycode.SDLK_COMMA},
                {0xBC, SDL.SDL_Keycode.SDLK_PLUS},
                {0xBD, SDL.SDL_Keycode.SDLK_MINUS},
                {0xBE, SDL.SDL_Keycode.SDLK_PERIOD},
                {0xBF, SDL.SDL_Keycode.SDLK_SLASH},
                {0xC0, SDL.SDL_Keycode.SDLK_BACKQUOTE},
                {0xDB, SDL.SDL_Keycode.SDLK_LEFTBRACKET},
                {0xDC, SDL.SDL_Keycode.SDLK_BACKSLASH},
                {0xDD, SDL.SDL_Keycode.SDLK_RIGHTBRACKET},
                {0xDE, SDL.SDL_Keycode.SDLK_QUOTE},
                {0xE2, SDL.SDL_Keycode.SDLK_LESS}
            };
        internal static Dictionary<uint, SDL.SDL_Keymod> vkmToSDLmod = new Dictionary<uint, SDL.SDL_Keymod>()
            {
                { 0x200, SDL.SDL_Keymod.KMOD_SHIFT },
                { 0x400, SDL.SDL_Keymod.KMOD_CTRL },
                { 0x600, SDL.SDL_Keymod.KMOD_CTRL | SDL.SDL_Keymod.KMOD_SHIFT },
                { 0x800, SDL.SDL_Keymod.KMOD_ALT },
                { 0xA00, SDL.SDL_Keymod.KMOD_ALT | SDL.SDL_Keymod.KMOD_SHIFT },
                { 0xC00, SDL.SDL_Keymod.KMOD_ALT | SDL.SDL_Keymod.KMOD_CTRL },
                { 0xE00, SDL.SDL_Keymod.KMOD_ALT | SDL.SDL_Keymod.KMOD_SHIFT | SDL.SDL_Keymod.KMOD_CTRL }
            };
        #endregion

        #region SubClassesUtilities
        private class ShoppingList
        {
            internal Dictionary<ushort, uint> GraphicAmount { get; }
            internal string Name { get; set; }
            private uint m_Limit = 999;
            internal uint Limit { get => m_Limit; set => m_Limit = Math.Min(999, value); }
            internal bool Complete { get; set; }

            internal ShoppingList(uint limit, string name, bool complete, Dictionary<ushort, uint> itemlist)
            {
                Name = name;
                Limit = limit;
                Complete = complete;
                GraphicAmount = itemlist;
            }
        }

        private class Counter
        {
            internal bool Image { get; set; }
            internal ushort Graphic { get; set; }
            internal ushort Color { get; set; }
            internal string Name { get; set; }
            internal bool Enabled { get; set; }
            internal Counter(bool image, ushort graphic, ushort color, string name, bool enabled)
            {
                Image = image;
                Graphic = graphic;
                Color = color;
                Name = name;
                Enabled = enabled;
            }
        }
        #endregion

        internal static void LoadProfile(AssistantGump gump, string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                FileInfo info = new FileInfo(Path.Combine(Profile.ProfilePath, $"{filename}.xml"));
                if (!info.Exists)
                {
                    return;
                }
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(info.FullName);
                }
                catch (Exception e)
                {
                    Log.Warn($"Exception in LoadProfile: {e}");
                    return;
                }
                if (doc == null)
                {
                    return;
                }

                XmlElement root = doc["profile"];
                if (root == null)
                {
                    return;
                }
                foreach (XmlElement data in root.GetElementsByTagName("data"))
                {
                    string name = GetAttribute(data, "name");
                    if(name.StartsWith("Filter"))
                    {
                        for(int i = 0; i < Filter.List.Count; i++)
                        {
                            Filter f = Filter.List[i];
                            if (f.XmlName == name)
                            {
                                bool.TryParse(GetText(data, "False"), out bool enabled);
                                AssistantGump.FiltersCB[i].IsChecked = enabled;
                            }
                        }
                    }
                    else
                    {
                        switch(name.ToLower(Culture))
                        {
                            case "useobjectsqueue":
                                gump.UseObjectsQueue = GetBool(data, true);
                                break;
                            case "usetargetqueue":
                                gump.UseTargetQueue = GetBool(data, true);
                                break;
                            case "showbandagetimerstart":
                                gump.ShowBandageTimerStart = GetBool(data, false);
                                break;
                            case "showbandagetimerend":
                                gump.ShowBandageTimerEnd = GetBool(data, false);
                                break;
                            case "showbandagetimeroverhead":
                                gump.ShowBandageTimerOverhead = GetBool(data, false);
                                break;
                            case "showcorpsenames":
                                gump.ShowCorpseNames = GetBool(data, false);
                                break;
                            case "opencorpses":
                                gump.OpenCorpses = GetBool(data, false);
                                break;
                            case "showmobilehits":
                                gump.ShowMobileHits = GetBool(data, false);
                                break;
                            case "handsbeforepotions":
                                gump.HandsBeforePotions = GetBool(data, false);
                                break;
                            case "handsbeforecasting":
                                gump.HandsBeforeCasting = GetBool(data, false);
                                break;
                            case "highlightcurrenttarget":
                                gump.HighlightCurrentTarget = GetBool(data, false);
                                break;
                            case "highlightcurrenttargethue":
                                gump.HighlightCurrentTargetHue = GetUShort(data);
                                break;
                            case "blockinvalidheal":
                                gump.BlockInvalidHeal = GetBool(data, false);
                                break;
                            case "bonecutter":
                                gump.BoneCutter = GetBool(data, false);
                                break;
                            case "automount":
                                gump.AutoMount = GetBool(data, false);
                                break;
                            case "autobandage":
                                gump.AutoBandage = GetBool(data, false);
                                break;
                            case "autobandagescale":
                                gump.AutoBandageScale = GetBool(data, false);
                                break;
                            case "autobandagecount":
                                gump.AutoBandageCount = GetBool(data, false);
                                break;
                            case "autobandagestart":
                                gump.AutoBandageStart = GetBool(data, false);
                                break;
                            case "autobandageformula":
                                gump.AutoBandageFormula = GetBool(data, false);
                                break;
                            case "autobandagehidden":
                                gump.AutoBandageHidden = GetBool(data, false);
                                break;
                            case "opendoors":
                                gump.OpenDoors = GetBool(data, false);
                                break;
                            case "usedoors":
                                gump.UseDoors = GetBool(data, false);
                                break;
                            case "showmobileflags":
                                gump.ShowMobileFlags = GetBool(data, true);
                                break;
                            case "countstealthsteps":
                                gump.CountStealthSteps = GetBool(data, true);
                                break;
                            case "friendslistonly":
                                gump.FriendsListOnly = GetBool(data, false);
                                break;
                            case "friendsparty":
                                gump.FriendsParty = GetBool(data, true);
                                break;
                            case "moveconflictingitems":
                                gump.MoveConflictingItems = GetBool(data, false);
                                break;
                            case "preventdismount":
                                gump.PreventDismount = GetBool(data, true);
                                break;
                            case "preventattackfriends":
                                gump.PreventAttackFriends = GetBool(data, true);
                                break;
                            case "autosearchcontainers":
                                gump.AutoSearchContainers = GetBool(data, true);
                                break;
                            case "autoacceptparty":
                                gump.AutoAcceptParty = GetBool(data, false);
                                break;
                            case "opencorpsesrange":
                                gump.OpenCorpsesRange = GetByte(data);
                                break;
                            /*case "useobjectslimit":
                                gump.UseObjectsLimit = GetByte(data);
                                break;*/
                            case "smarttargetrange":
                                gump.SmartTargetRange = GetBool(data, true);
                                break;
                            case "smarttargetrangevalue":
                                gump.SmartTargetRangeValue = GetByte(data);
                                break;
                            case "fixedseason":
                                gump.FixedSeason = GetByte(data);
                                break;
                            case "smarttarget":
                                gump.SmartTarget = GetByte(data);
                                break;
                            case "targetshare":
                                gump.TargetShare = GetByte(data);
                                break;
                            case "autobandagestartvalue":
                                gump.AutoBandageStartValue = GetByte(data);
                                break;
                            case "spellstargetshare":
                                gump.SpellsTargetShare = GetByte(data);
                                break;
                            case "opendoorsmode":
                                gump.OpenDoorsMode = GetByte(data);
                                break;
                            case "opencorpsesmode":
                                gump.OpenCorpsesMode = GetByte(data);
                                break;
                            case "customcaptionmode":
                                gump.CustomCaptionMode = GetByte(data);
                                break;
                            case "grabhotbag":
                                gump.GrabHotBag = GetUInt(data);
                                break;
                            case "mountserial":
                                gump.MountSerial = GetUInt(data);
                                break;
                            case "bladeserial":
                                gump.BladeSerial = GetUInt(data);
                                break;
                            case "autobandagetarget":
                                gump.AutoBandageTarget = GetUInt(data);
                                break;
                            case "autobandagedelay":
                                gump.AutoBandageDelay = GetUInt(data);
                                break;
                            case "actiondelay":
                                gump.ActionDelay = GetUInt(data);
                                break;
                            case "dresstypedefault":
                                gump.TypeDress = GetBool(data, false);
                                break;
                            case "returntoparentscript":
                                gump.ReturnToParentScript = GetBool(data, false);
                                break;
                        }
                    }
                }

                XmlElement sub = root["friends"];
                gump.FriendDictionary.Clear();
                if (sub != null)
                {
                    foreach (XmlElement friend in sub.ChildNodes)//.GetElementsByTagName("friend"))
                    {
                        if (friend.Name != "friend")
                        {
                            continue;
                        }

                        string name = GetAttribute(friend, "name", "(unknown)");
                        uint serial = GetUInt(friend);
                        if (SerialHelper.IsMobile(serial))
                        {
                            gump.FriendDictionary[serial] = name;
                        }
                    }
                }
                gump.UpdateFriendListGump();

                sub = root["autoloot"];
                if (sub != null)
                {
                    foreach (XmlElement lootitem in sub.ChildNodes)
                    {
                        if (lootitem.Name == "enabled")
                        {
                            gump.AutoLoot = GetBool(lootitem, false);
                            continue;
                        }
                        else if(lootitem.Name == "container")
                        {
                            gump.AutoLootContainer = GetUInt(lootitem, 0);
                            continue;
                        }
                        else if(lootitem.Name == "guards")
                        {
                            gump.NoAutoLootInGuards = GetBool(lootitem, false);
                            continue;
                        }
                    }
                }
                else
                {
                    gump.AutoLoot = false;
                    gump.AutoLootContainer = 0;
                    gump.NoAutoLootInGuards = false;
                }

                sub = root["scavenger"];
                var dict = Scavenger.ItemIDsHues;
                dict.Clear();
                if (sub != null)
                {
                    string name;
                    foreach (XmlElement counter in sub.ChildNodes)
                    {
                        if (counter.Name != "scavenge")
                        {
                            continue;
                        }

                        ushort graphic = (ushort)GetAttributeUInt(counter, "graphic");
                        short color = (short)GetAttributeUShort(counter, "color");
                        name = GetAttribute(counter, "name");
                        if (string.IsNullOrEmpty(name))
                            name = UOSObjects.GetDefaultItemName(graphic);
                        if (!dict.TryGetValue(graphic, out var list))
                            dict[(ushort)graphic] = list = new List<ItemDisplay>();
                         list.Add(new ItemDisplay(graphic, name, color, GetAttributeBool(counter, "enabled")));
                    }
                    gump.UpdateScavengerItemsGump();
                }
                sub = root["autosearchexemptions"];
                List<string> exempts = new List<string>();
                if (sub != null)
                {
                    foreach (XmlElement exempt in sub.ChildNodes)
                    {
                        if (exempt.Name != "exemption")
                        {
                            continue;
                        }

                        string s = GetAttribute(exempt, "group", string.Empty);
                        if (s != string.Empty)
                        {
                            exempts.Add(s.Replace(" ", string.Empty));
                        }
                    }
                }
                sub = root["objects"];
                Dictionary<string, uint> objects = new Dictionary<string, uint>();
                if (sub != null)
                {
                    foreach (XmlElement obj in sub.ChildNodes)
                    {
                        if (obj.Name != "obj")
                        {
                            continue;
                        }

                        string s = GetAttribute(obj, "name");
                        if (!string.IsNullOrEmpty(s))
                        {
                            uint ser = GetUInt(obj);
                            if (SerialHelper.IsItem(ser))
                            {
                                objects[s] = ser;
                            }
                        }
                    }
                }
                sub = root["macros"];
                ScriptManager.MacroDictionary.Clear();
                if (sub != null)
                {
                    foreach (XmlElement key in sub.ChildNodes)
                    {
                        if (key.Name != "macro")
                        {
                            continue;
                        }

                        string name = GetAttribute(key, "name"), macro = GetText(key, null);
                        if (name != null && macro != null)
                        {
                            MacroAction ac = MacroAction.None;
                            if (!GetAttributeBool(key, "interrupt", true))
                                ac |= MacroAction.NoInterrupt;
                            if (GetAttributeBool(key, "loop"))
                                ac |= MacroAction.Loop;
                            ScriptManager.MacroDictionary.Add(name, new HotKeyOpts(ac, "macro.play", name) { Macro = macro.Replace(';', '\n') });
                        }
                    }
                }
                gump.UpdateMacroListGump();
                sub = root["hotkeys"];
                HotKeys.ClearHotkeys();
                if (sub != null)
                {
                    foreach (XmlElement key in sub.ChildNodes)
                    {
                        if (key.Name != "hotkey")
                        {
                            continue;
                        }

                        string action = GetAttribute(key, "action"), param = GetAttribute(key, "param");
                        uint keyval = GetAttributeUInt(key, "key");
                        if (action != null)//param could be missing
                        {
                            if (keyval > 0)
                            {
                                HotKeys.AddHotkey(keyval, new HotKeyOpts(GetAttributeBool(key, "pass") ? MacroAction.PassToUO : MacroAction.None, action, param), null, ref action, gump, true);
                            }
                        }
                    }
                }

                string[] sp;
                sub = root["vendors"];
                Vendors.Buy.BuyList.Clear();
                Vendors.Sell.SellList.Clear();
                if (sub != null)
                {
                    bool foundbuy = false, foundsell = false;
                    (string, bool) buystate = (null, false);
                    (string, bool) sellstate = (null, false);
                    foreach (XmlElement list in sub.ChildNodes)
                    {
                        if (list.Name == "buystate")
                        {
                            buystate.Item1 = GetAttribute(list, "list");
                            buystate.Item2 = GetAttributeBool(list, "enabled");
                            foundbuy = !string.IsNullOrEmpty(buystate.Item1);
                        }
                        else if(list.Name == "sellstate")
                        {
                            sellstate.Item1 = GetAttribute(list, "list");
                            sellstate.Item2 = GetAttributeBool(list, "enabled");
                            foundsell = !string.IsNullOrEmpty(sellstate.Item1);
                        }
                        if (foundbuy && foundsell)
                            break;
                    }
                    foreach (XmlElement list in sub.ChildNodes)
                    {
                        if (list.Name != "shoppinglist")
                        {
                            continue;
                        }
                        string name = GetAttribute(list, "name");
                        string type = GetAttribute(list, "type");
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
                        {
                            sp = name.Split('-');
                            if (sp.Length > 1 && ushort.TryParse(sp[1], out ushort num) && num > 0 && num <= ushort.MaxValue)
                            {
                                --num;
                                Vendors.IBuySell ibs = null;
                                List<BuySellEntry> bselist = null;
                                if (type == "Buy")
                                {
                                    ibs = new Vendors.Buy(num);
                                    Vendors.Buy.BuyList[ibs] = bselist = new List<BuySellEntry>();
                                    if (ibs.Name == buystate.Item1)
                                    {
                                        ibs.Selected = ibs;
                                        ibs.Enabled = buystate.Item2;
                                    }
                                }
                                else if (type == "Sell")
                                {
                                    ibs = new Vendors.Sell(num);
                                    Vendors.Sell.SellList[ibs] = bselist = new List<BuySellEntry>();
                                    if (ibs.Name == sellstate.Item1)
                                    {
                                        ibs.Selected = ibs;
                                        ibs.Enabled = sellstate.Item2;
                                    }
                                }
                                if (ibs != null && bselist != null)
                                {
                                    ibs.MaxAmount = GetAttributeUShort(list, "limit", 0x1);
                                    ibs.Complete = GetAttributeBool(list, "complete");
                                    foreach (XmlElement items in list.GetElementsByTagName("item"))
                                    {
                                        ushort graphic = GetAttributeUShort(items, "graphic");
                                        if (graphic > 0)
                                        {
                                            bselist.Add(new BuySellEntry(graphic, Math.Max((ushort)1, Math.Min((ushort)999, GetAttributeUShort(items, "amount")))));
                                        }
                                    }
                                }
                            }
                        }
                        gump.UpdateVendorsListGump();
                        gump.AddVendorsListToHotkeys();
                    }
                }

                ushort max = 0;
                XmlNodeList nodelist = root.GetElementsByTagName("dresslist");
                DressList.DressLists.Clear();
                if (nodelist.Count > 0)
                {
                    SortedDictionary<int, DressList> lists = new SortedDictionary<int, DressList>();
                    foreach (XmlElement list in root.GetElementsByTagName("dresslist"))
                    {
                        string name = GetAttribute(list, "name");
                        uint undressbag = GetAttributeUInt(list, "container");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            sp = name.Split('-');
                            if (sp.Length > 1 && ushort.TryParse(sp[1], out ushort num) && num <= ushort.MaxValue)
                            {
                                --num;
                                Dictionary<Layer, DressItem> dresses = new Dictionary<Layer, DressItem>();
                                foreach (XmlElement item in list.ChildNodes)
                                {
                                    if (item.Name != "item")
                                    {
                                        continue;
                                    }

                                    Layer layer = (Layer)GetAttributeUInt(item, "layer");
                                    ushort type = (ushort)GetAttributeUInt(item, "type");
                                    if (layer > Layer.Invalid && layer <= Layer.LastUserValid && layer != Layer.Backpack && layer != Layer.FacialHair && layer != Layer.Hair)
                                    {
                                        uint serial = GetUInt(item);
                                        if (SerialHelper.IsItem(serial))//Max Item Value - MaxItemValue
                                        {
                                            if (type >= TileDataLoader.Instance.StaticData.Length)
                                            {
                                                //we have an invalid loaded type? reset to zero, so we allow the subsystem to recalculate it on next dress action
                                                type = 0;
                                            }
                                            dresses[layer] = new DressItem(serial, type);
                                        }
                                    }
                                }
                                lists[num] = new DressList(name, dresses, undressbag);
                                if (num > max)
                                    max = num;
                            }
                        }
                    }
                    for (int i = 0; i <= max; ++i)
                    {
                        DressList.DressLists.Add(null);
                    }
                    foreach (KeyValuePair<int, DressList> kvp in lists)
                    {
                        DressList.DressLists[kvp.Key] = kvp.Value;
                    }
                }
                gump.UpdateDressListGump();
                gump.AddDressListToHotkeys();

                sub = root["organizer"];
                Organizer.Organizers.Clear();
                if (sub != null)
                {
                    max = 0;
                    SortedDictionary<int, Organizer> organizers = new SortedDictionary<int, Organizer>();
                    foreach (XmlElement group in sub.ChildNodes)
                    {
                        if (group.Name != "group")
                        {
                            continue;
                        }

                        string name = GetAttribute(group, "name");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            sp = name.Split('-');
                            if (sp.Length > 1 && ushort.TryParse(sp[1], out ushort num) && num <= ushort.MaxValue)
                            {
                                bool stack = GetAttributeBool(group, "stack"), complete = GetAttributeBool(group, "complete"), loop = GetAttributeBool(group, "loop");
                                uint source = GetAttributeUInt(group, "source"), target = GetAttributeUInt(group, "target");
                                Organizer org = new Organizer(name) { Stack = stack, Loop = loop, Complete = complete, SourceCont = source, TargetCont = target };
                                foreach (XmlElement item in group.GetElementsByTagName("item"))
                                {
                                    ushort graphic = (ushort)GetAttributeUInt(item, "graphic");
                                    if (graphic > 0)
                                    {
                                        uint amt = GetAttributeUInt(item, "amount");
                                        short hue = (short)GetAttributeUShort(item, "hue", ushort.MaxValue);
                                        name = GetAttribute(item, "name");
                                        if (string.IsNullOrEmpty(name))
                                            name = UOSObjects.GetDefaultItemName(graphic);
                                        ItemDisplay oi = new ItemDisplay(graphic, name, hue);
                                        if (!org.Items.Contains(oi))
                                            org.Items.Add(oi);
                                    }
                                }
                                organizers[num - 1] = org;
                                if (num > max)
                                    max = num;
                            }
                        }
                    }
                    if (max > 0)
                    {
                        for (int i = 0; i < max; ++i)
                        {
                            Organizer.Organizers.Add(null);
                        }
                        foreach (KeyValuePair<int, Organizer> kvp in organizers)
                        {
                            Organizer.Organizers[kvp.Key] = kvp.Value;
                        }
                    }
                }
                gump.UpdateOrganizerListGump();
                gump.AddOrganizerListToHotkeys();

                gump.OnProfileChanged();
            }
        }
    }
    #endregion
}
