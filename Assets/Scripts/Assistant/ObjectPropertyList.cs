using System.Diagnostics.Contracts;
using System.Text;
using System.Collections.Generic;

using ClassicUO.Network;
using ClassicUO.IO.Resources;

namespace Assistant
{
    internal class ObjectPropertyList
    {
        internal class OPLEntry
        {
            internal int Number = 0;
            internal string Args = "";

            internal OPLEntry(int num) : this(num, "")
            {
            }

            internal OPLEntry(int num, string args)
            {
                Number = num;
                Args = args;
            }
        }

        private List<int> m_StringNums = new List<int>();
        internal List<OPLEntry> Content { get; } = new List<OPLEntry>();

        private uint m_CustomHash;
        private uint m_Hash;
        private List<OPLEntry> m_CustomContent = new List<OPLEntry>();
        
        internal uint Hash
        {
            get { return m_Hash ^ m_CustomHash; }
            set { m_Hash = value; }
        }

        internal bool Customized
        {
            get { return m_CustomHash != 0; }
        }

        internal ObjectPropertyList(UOEntity owner)
        {
            Owner = owner;

            m_StringNums.AddRange(m_DefaultStringNums);
        }

        internal UOEntity Owner { get; } = null;

        internal void Read(Packet p, out string name)
        {
            Content.Clear();
            name = "";

            p.Seek(11); // seek to packet data

            //p.ReadUInt(); // serial from 5 to 9
            //p.ReadByte(); // 10
            //p.ReadByte(); // 11
            m_Hash = p.ReadUInt();

            m_StringNums.Clear();
            m_StringNums.AddRange(m_DefaultStringNums);
            int cliloc;
            List<(int, string)> list = new List<(int, string)>();
            while ((cliloc = (int)p.ReadUInt()) != 0)
            {
                string argument = p.ReadUnicodeReversed(p.ReadUShort());

                for (int i = 0; i < list.Count; i++)
                {
                    var temp = list[i];

                    if (temp.Item1 == cliloc && temp.Item2 == argument)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }

                list.Add((cliloc, argument));
            }
            for(int i = 0; i < list.Count; i++)
            {
                if(i == 0)
                    name = ClilocLoader.Instance.Translate(list[i].Item1, list[i].Item2, true);
                Content.Add(new OPLEntry(list[i].Item1, list[i].Item2));
            }
 
            for (int i = 0; i < m_CustomContent.Count; i++)
            {
                OPLEntry ent = m_CustomContent[i];
                if (m_StringNums.Contains(ent.Number))
                {
                    m_StringNums.Remove(ent.Number);
                }
                else
                {
                    for (int s = 0; s < m_DefaultStringNums.Length; s++)
                    {
                        if (ent.Number == m_DefaultStringNums[s])
                        {
                            ent.Number = GetStringNumber();
                            break;
                        }
                    }
                }
            }
        }

        internal void Add(int number)
        {
            if (number == 0)
                return;

            AddHash((uint)number);

            m_CustomContent.Add(new OPLEntry(number));
        }

        private static byte[] m_Buffer = new byte[0];

        internal void AddHash(uint val)
        {
            m_CustomHash ^= (val & 0x3FFFFFF);
            m_CustomHash ^= (val >> 26) & 0x3F;
        }

        static int GetHashCode32(string s)
        {
            unsafe
            {
                fixed (char* src = s)
                {
                    Contract.Assert(src[s.Length] == '\0', "src[this.Length] == '\\0'");
                    Contract.Assert(((int)src) % 4 == 0, "Managed string should start at 4 bytes boundary");

                    int hash1 = (5381 << 16) + 5381;
                    int hash2 = hash1;

                    // 32 bit machines. 
                    int* pint = (int*)src;
                    int len = s.Length;
                    while (len > 2)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len -= 4;
                    }

                    if (len > 0)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                    }
                    return hash1 + (hash2 * 1566083941);
                }
            }
        }

        internal void Add(int number, string arguments)
        {
            if (number == 0)
                return;

            AddHash((uint)number);
            AddHash((uint)GetHashCode32(arguments));
            m_CustomContent.Add(new OPLEntry(number, arguments));
        }

        internal void Add(int number, string format, object arg0)
        {
            Add(number, string.Format(format, arg0));
        }

        internal void Add(int number, string format, object arg0, object arg1)
        {
            Add(number, string.Format(format, arg0, arg1));
        }

        internal void Add(int number, string format, object arg0, object arg1, object arg2)
        {
            Add(number, string.Format(format, arg0, arg1, arg2));
        }

        internal void Add(int number, string format, params object[] args)
        {
            Add(number, string.Format(format, args));
        }

        private static int[] m_DefaultStringNums = new int[]
        {
            1042971,
            1070722,
            1114057,
            1114778,
            1114779,
            1149934
        };

        private int GetStringNumber()
        {
            if (m_StringNums.Count > 0)
            {
                int num = m_StringNums[0];
                m_StringNums.RemoveAt(0);
                return num;
            }
            else
            {
                return 1049644;
            }
        }

        private const string HTMLFormat = " <CENTER><BASEFONT COLOR=#FF0000>{0}</BASEFONT></CENTER> ";

        internal void Add(string text)
        {
            Add(GetStringNumber(), string.Format(HTMLFormat, text));
        }

        internal void Add(string format, string arg0)
        {
            Add(GetStringNumber(), string.Format(format, arg0));
        }

        internal void Add(string format, string arg0, string arg1)
        {
            Add(GetStringNumber(), string.Format(format, arg0, arg1));
        }

        internal void Add(string format, string arg0, string arg1, string arg2)
        {
            Add(GetStringNumber(), string.Format(format, arg0, arg1, arg2));
        }

        internal void Add(string format, params object[] args)
        {
            Add(GetStringNumber(), string.Format(format, args));
        }

        internal bool Remove(int number)
        {
            for (int i = 0; i < Content.Count; i++)
            {
                OPLEntry ent = Content[i];
                if (ent == null)
                    continue;

                if (ent.Number == number)
                {
                    for (int s = 0; s < m_DefaultStringNums.Length; s++)
                    {
                        if (m_DefaultStringNums[s] == ent.Number)
                        {
                            m_StringNums.Insert(0, ent.Number);
                            break;
                        }
                    }

                    Content.RemoveAt(i);
                    AddHash((uint)ent.Number);
                    if (!string.IsNullOrEmpty(ent.Args))
                        AddHash((uint)GetHashCode32(ent.Args));

                    return true;
                }
            }

            for (int i = 0; i < m_CustomContent.Count; i++)
            {
                OPLEntry ent = m_CustomContent[i];
                if (ent == null)
                    continue;

                if (ent.Number == number)
                {
                    for (int s = 0; s < m_DefaultStringNums.Length; s++)
                    {
                        if (m_DefaultStringNums[s] == ent.Number)
                        {
                            m_StringNums.Insert(0, ent.Number);
                            break;
                        }
                    }

                    m_CustomContent.RemoveAt(i);
                    AddHash((uint)ent.Number);
                    if (!string.IsNullOrEmpty(ent.Args))
                        AddHash((uint)GetHashCode32(ent.Args));
                    if (m_CustomContent.Count == 0)
                        m_CustomHash = 0;
                    return true;
                }
            }

            return false;
        }

        internal bool Remove(string str)
        {
            string htmlStr = string.Format(HTMLFormat, str);

            for (int i = 0; i < m_CustomContent.Count; i++)
            {
                OPLEntry ent = m_CustomContent[i];
                if (ent == null)
                    continue;

                for (int s = 0; s < m_DefaultStringNums.Length; s++)
                {
                    if (ent.Number == m_DefaultStringNums[s] && (ent.Args == htmlStr || ent.Args == str))
                    {
                        m_StringNums.Insert(0, ent.Number);

                        m_CustomContent.RemoveAt(i);

                        AddHash((uint)ent.Number);
                        if (!string.IsNullOrEmpty(ent.Args))
                            AddHash((uint)GetHashCode32(ent.Args));
                        return true;
                    }
                }
            }

            return false;
        }

        internal PacketWriter BuildPacket()
        {
            return new OPLPacket(this);
        }

        private class OPLPacket : PacketWriter
        {
            internal OPLPacket(ObjectPropertyList opl) : base(0xD6)
            {
                EnsureSize(128);
                WriteUShort(0x01);
                WriteUInt((opl.Owner != null ? opl.Owner.Serial : 0));
                WriteByte(0);
                WriteByte(0);
                WriteUInt(opl.m_Hash ^ opl.m_CustomHash);

                foreach (OPLEntry ent in opl.Content)
                {
                    if (ent != null && ent.Number != 0)
                    {
                        WriteUInt((uint)ent.Number);
                        if (!string.IsNullOrEmpty(ent.Args))
                        {
                            int byteCount = Encoding.Unicode.GetByteCount(ent.Args);

                            if (byteCount > m_Buffer.Length)
                                m_Buffer = new byte[byteCount];
                            
                            byteCount = Encoding.Unicode.GetBytes(ent.Args, 0, ent.Args.Length, m_Buffer, 0);
                            WriteUShort((ushort)byteCount);
                            WriteBytes(m_Buffer, 0, byteCount);
                        }
                        else
                        {
                            WriteUShort(0);
                        }
                    }
                }

                foreach (OPLEntry ent in opl.m_CustomContent)
                {
                    if (ent != null && ent.Number != 0)
                    {
                        string arguments = ent.Args;

                        WriteUInt((uint)ent.Number);

                        if (string.IsNullOrEmpty(arguments))
                            arguments = " ";
                        arguments += "\t ";

                        if (!string.IsNullOrEmpty(arguments))
                        {
                            int byteCount = Encoding.Unicode.GetByteCount(arguments);

                            if (byteCount > m_Buffer.Length)
                                m_Buffer = new byte[byteCount];

                            byteCount = Encoding.Unicode.GetBytes(arguments, 0, arguments.Length, m_Buffer, 0);

                            WriteUShort((ushort)byteCount);
                            WriteBytes(m_Buffer, 0, byteCount);
                        }
                        else
                        {
                            WriteUShort(0);
                        }
                    }
                }

                WriteUInt(0);
            }
        }
    }

    internal class OPLInfo : PacketWriter
    {
        internal OPLInfo(uint ser, uint hash) : base(0xDC)
        {
            WriteUInt(ser);
            WriteUInt(hash);
        }
    }
}
