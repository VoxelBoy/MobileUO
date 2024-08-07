namespace Assistant.Core
{
    internal static class Dress
    {
        private static UOItem m_Right, m_Left;

        public static void ToggleRight()
        {
            if (UOSObjects.Player == null)
                return;

            UOItem item = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);
            if (item == null)
            {
                if (m_Right != null)
                    m_Right = UOSObjects.FindItem(m_Right.Serial);

                if (m_Right != null && m_Right.IsChildOf(UOSObjects.Player.Backpack))
                {
                    // try to also undress conflicting hand(s)
                    UOItem conflict = UOSObjects.Player.GetItemOnLayer(Layer.LeftHand);
                    if (conflict != null && (conflict.IsTwoHanded || m_Right.IsTwoHanded))
                    {
                        Unequip(DressList.GetLayerFor(conflict));
                    }

                    Equip(m_Right, DressList.GetLayerFor(m_Right));
                }
                else
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Force, "You must disarm something before you can arm it");
                }
            }
            else
            {
                Unequip(DressList.GetLayerFor(item));
                m_Right = item;
            }
        }

        public static void ToggleLeft()
        {
            if (UOSObjects.Player == null || UOSObjects.Player.Backpack == null)
                return;

            UOItem item = UOSObjects.Player.GetItemOnLayer(Layer.LeftHand);
            if (item == null)
            {
                if (m_Left != null)
                    m_Left = UOSObjects.FindItem(m_Left.Serial);

                if (m_Left != null && m_Left.IsChildOf(UOSObjects.Player.Backpack))
                {
                    UOItem conflict = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);
                    if (conflict != null && (conflict.IsTwoHanded || m_Left.IsTwoHanded))
                    {
                        Unequip(DressList.GetLayerFor(conflict));
                    }

                    Equip(m_Left, DressList.GetLayerFor(m_Left));
                }
                else
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Force, "You must disarm something before you can arm it");
                }
            }
            else
            {
                Unequip(DressList.GetLayerFor(item));
                m_Left = item;
            }
        }

        public static bool Equip(UOItem item, Layer layer)
        {
            if (layer == Layer.Invalid || layer > Layer.LastUserValid || item == null || item.Layer == Layer.Invalid ||
                item.Layer > Layer.LastUserValid)
                return false;

            if (item != null && UOSObjects.Player != null && item.IsChildOf(UOSObjects.Player.Backpack))
            {
                DragDropManager.DragDrop(item, UOSObjects.Player, layer);
                return true;
            }

            return false;
        }

        public static bool Unequip(Layer layer)
        {
            if (layer == Layer.Invalid || layer > Layer.LastUserValid)
                return false;

            UOItem item = UOSObjects.Player.GetItemOnLayer(layer);
            if (item != null)
            {
                UOItem pack = DressList.FindUndressBag(item);
                if (pack != null)
                {
                    DragDropManager.DragDrop(item, pack);
                    return true;
                }
            }

            return false;
        }
    }
}
