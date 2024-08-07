using System;
using System.Collections.Generic;

using ClassicUO.Network;
using ClassicUO.Game;

namespace Assistant
{
    internal class UOEntity
    {
        internal class ContextMenuList : List<KeyValuePair<ushort, ushort>>
        {
            internal void Add(ushort key, ushort value)
            {
                var element = new KeyValuePair<ushort, ushort>(key, value);
                Add(element);
            }
        }

        private uint m_Serial;
        private Point3D m_Pos;
        private ushort m_Hue;
        private bool m_Deleted;
        private ContextMenuList m_ContextMenu = new ContextMenuList();
        protected ObjectPropertyList m_ObjPropList = null;

        internal ObjectPropertyList ObjPropList
        {
            get { return m_ObjPropList; }
        }

        internal UOEntity(uint ser)
        {
            m_ObjPropList = new ObjectPropertyList(this);

            m_Serial = ser;
            m_Deleted = false;
        }

        internal uint Serial
        {
            get { return m_Serial; }
        }

        internal virtual Point3D Position
        {
            get { return m_Pos; }
            set
            {
                if (value != m_Pos)
                {
                    var oldPos = m_Pos;
                    m_Pos = value;
                    OnPositionChanging(oldPos);
                }
            }
        }

        internal virtual Point3D WorldPosition => Position;

        internal bool Deleted
        {
            get { return m_Deleted; }
        }

        internal ContextMenuList ContextMenu
        {
            get { return m_ContextMenu; }
        }

        internal virtual ushort Hue
        {
            get { return m_Hue; }
            set { m_Hue = value; }
        }

        internal virtual void Remove()
        {
            m_Deleted = true;
        }

        internal virtual void OnPositionChanging(Point3D oldPos)
        {
        }

        public double GetDistanceToSqrt(UOEntity e)
        {
            int xDelta = WorldPosition.m_X - e.WorldPosition.m_X;
            int yDelta = WorldPosition.m_Y - e.WorldPosition.m_Y;

            return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
        }

        public override int GetHashCode()
        {
            return (int)m_Serial;
        }

        internal uint OPLHash
        {
            get
            {
                if (m_ObjPropList != null)
                    return m_ObjPropList.Hash;
                else
                    return 0;
            }
            set
            {
                if (m_ObjPropList != null)
                    m_ObjPropList.Hash = value;
            }
        }

        internal virtual ushort Graphic => 0;

        internal bool ModifiedOPL
        {
            get { return m_ObjPropList.Customized; }
        }

        internal void ReadPropertyList(Packet p, out string name)
        {
            m_ObjPropList.Read(p, out name);
        }

        /*internal Packet BuildOPLPacket()
        { 
            return m_ObjPropList.BuildPacket();
        }*/

        internal void OPLChanged()
        {
            Engine.Instance.SendToClient(new OPLInfo(Serial, OPLHash));
        }

        internal virtual string GetName()
        {
            return null;
        }
    }
}
