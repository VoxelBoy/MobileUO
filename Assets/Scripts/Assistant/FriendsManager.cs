using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

using ClassicUO.Game;
using ClassicUO.Network;

namespace Assistant.Core
{
    public static class FriendsManager
    {
        //private static ComboBox _friendGroups;
        //private static ListBox _friendList;

        private static List<FriendGroup> FriendGroups = new List<FriendGroup>();

        /*public static void SetControls(ComboBox friendsGroup, ListBox friendsList)
        {
            _friendGroups = friendsGroup;
            _friendList = friendsList;
        }*/

        public static void OnTargetAddFriend(FriendGroup group)
        {
            UOSObjects.Player.SendMessage(MsgLevel.Friend, "Target a player to add to your friends list");
            Targeting.OneTimeTarget(group.OnAddFriendTarget);
        }

        public class Friend
        {
            public string Name { get; set; }
            public uint Serial { get; set; }

            public override string ToString()
            {
                return $"{Name} ({Serial})";
            }
        }

        public class FriendGroup
        {
            public string GroupName { get; set; }
            public bool Enabled { get; set; }
            public List<Friend> Friends { get; set; }

            public FriendGroup()
            {
                Friends = new List<Friend>();
            }

            /*public void AddHotKeys()
            {
                if (Engine.MainWindow == null)
                {
                    HotKey.Add(HKCategory.Friends, HKSubCat.None, $"Add Target To: {GroupName}", AddFriendToGroup);
                    HotKey.Add(HKCategory.Friends, HKSubCat.None, $"Toggle Group: {GroupName}", ToggleFriendGroup);
                    HotKey.Add(HKCategory.Friends, HKSubCat.None, $"Add All Mobiles: {GroupName}",
                        AddAllMobileAsFriends);
                    HotKey.Add(HKCategory.Friends, HKSubCat.None, $"Add All Humanoids: {GroupName}",
                        AddAllHumanoidsAsFriends);
                }
                else
                {
                    Engine.MainWindow.SafeAction(s =>
                    {
                        HotKey.Add(HKCategory.Friends, HKSubCat.None, $"Add Target To: {GroupName}", AddFriendToGroup);
                        HotKey.Add(HKCategory.Friends, HKSubCat.None, $"Toggle Group: {GroupName}", ToggleFriendGroup);
                        HotKey.Add(HKCategory.Friends, HKSubCat.None, $"Add All Mobiles: {GroupName}",
                            AddAllMobileAsFriends);
                        HotKey.Add(HKCategory.Friends, HKSubCat.None, $"Add All Humanoids: {GroupName}",
                            AddAllHumanoidsAsFriends);
                    });
                }
            }

            public void RemoveHotKeys()
            {
                if (Engine.MainWindow == null)
                {
                    HotKey.Remove($"Add Target To: {GroupName}");
                    HotKey.Remove($"Toggle Group: {GroupName}");
                    HotKey.Remove($"Add All Mobiles: {GroupName}");
                    HotKey.Remove($"Add All Humanoids: {GroupName}");
                }
                else
                {
                    Engine.MainWindow.SafeAction(s =>
                    {
                        HotKey.Remove($"Add Target To: {GroupName}");
                        HotKey.Remove($"Toggle Group: {GroupName}");
                        HotKey.Remove($"Add All Mobiles: {GroupName}");
                        HotKey.Remove($"Add All Humanoids: {GroupName}");
                    });
                }
            }*/

            public void AddFriendToGroup()
            {
                UOSObjects.Player.SendMessage(MsgLevel.Friend, $"Target friend to add to group '{GroupName}'");
                Targeting.OneTimeTarget(OnAddFriendTarget);
            }

            private void ToggleFriendGroup()
            {
                if (Enabled)
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Warning,
                        $"Friend group '{GroupName}' ({Friends.Count} friends) has been 'Disabled'");
                    Enabled = false;
                }
                else
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Info,
                        $"Friend group '{GroupName}' ({Friends.Count} friends) has been 'Enabled'");
                    Enabled = true;
                }
            }

            public override string ToString()
            {
                return $"{GroupName}";
            }

            public void OnAddFriendTarget(bool location, uint serial, Point3D loc, ushort gfx)
            {
                //Engine.MainWindow.SafeAction(s => s.ShowMe());

                if (!location && SerialHelper.IsMobile(serial) && serial != UOSObjects.Player.Serial)
                {
                    UOMobile m = UOSObjects.FindMobile(serial);

                    if (m == null)
                        return;

                    if (AddFriend(m.Name, serial))
                    {
                        m.ObjPropList.Add("(Friendly)");
                        m.OPLChanged();
                    }
                    else
                    {
                        UOSObjects.Player.SendMessage(MsgLevel.Warning, $"'{m.Name}' is already in '{GroupName}'");
                    }
                }
            }

            public bool AddFriend(string friendName, uint friendSerial)
            {
                if (Friends.Any(f => f.Serial == friendSerial) == false)
                {
                    Friend newFriend = new Friend
                    {
                        Name = friendName,
                        Serial = friendSerial
                    };

                    Friends.Add(newFriend);

                    /*if (_friendGroups.SelectedItem == this)
                    {
                        RedrawList(this);
                    }*/

                    UOSObjects.Player.SendMessage(MsgLevel.Friend, $"Added '{friendName}' to '{GroupName}'");

                    return true;
                }

                return false;
            }

            public void AddAllMobileAsFriends()
            {
                List<UOMobile> mobiles = UOSObjects.MobilesInRange(12);

                foreach (UOMobile mobile in mobiles)
                {
                    if (!IsFriend(mobile.Serial) && SerialHelper.IsMobile(mobile.Serial) && mobile.Serial != UOSObjects.Player.Serial)
                    {
                        if (AddFriend(mobile.Name, mobile.Serial))
                        {
                            mobile.ObjPropList.Add("(Friendly)");
                            mobile.OPLChanged();
                        }
                    }
                }
            }

            public void AddAllHumanoidsAsFriends()
            {
                List<UOMobile> mobiles = UOSObjects.MobilesInRange(12);

                foreach (UOMobile mobile in mobiles)
                {
                    if (!IsFriend(mobile.Serial) && SerialHelper.IsMobile(mobile.Serial) && mobile.Serial != UOSObjects.Player.Serial && mobile.IsHuman)
                    {
                        if (AddFriend(mobile.Name, mobile.Serial))
                        {
                            mobile.ObjPropList.Add("(Friendly)");
                            mobile.OPLChanged();
                        }
                    }
                }
            }
        }

        public static bool IsFriend(uint serial)
        {
            // Check if they have treat party as friends enabled and check the party if so
            if (!UOSObjects.Gump.FriendsListOnly && ((UOSObjects.Gump.FriendsParty && PacketHandlers.Party.Contains(serial)) || PacketHandlers.Faction.Contains(serial)))
                return true;

            bool isFriend = false;

            // Loop through each friends group that is enabled
            foreach (var friendGroup in FriendGroups)
            {
                if (friendGroup.Enabled && friendGroup.Friends.Any(f => f.Serial == serial))
                {
                    isFriend = true;
                    break;
                }
            }

            return isFriend;
        }

        public static void EnableFriendsGroup(FriendGroup group, bool enabled)
        {
            foreach (FriendGroup friendGroup in FriendGroups)
            {
                if (friendGroup == group)
                {
                    friendGroup.Enabled = enabled;
                    return;
                }
            }
        }

        public static bool FriendsGroupExists(string group)
        {
            foreach (FriendGroup friendGroup in FriendGroups)
            {
                if (friendGroup.GroupName.ToLower().Equals(group.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFriendsGroupEnabled(FriendGroup group)
        {
            foreach (FriendGroup friendGroup in FriendGroups)
            {
                if (friendGroup == group)
                {
                    return friendGroup.Enabled;
                }
            }

            return false;
        }

        public static bool RemoveFriend(FriendGroup group, int index)
        {
            foreach (var friendGroup in FriendGroups)
            {
                if (friendGroup == group)
                {
                    friendGroup.Friends.RemoveAt(index);

                    RedrawList(group);

                    return true;
                }
            }

            return false;
        }

        public static void ClearFriendGroup(string group)
        {
            foreach (var friendGroup in FriendGroups)
            {
                if (friendGroup.GroupName.Equals(group))
                {
                    friendGroup.Friends.Clear();
                    return;
                }
            }
        }

        public static bool DeleteFriendGroup(FriendGroup group)
        {
            foreach (FriendGroup friendGroup in FriendGroups)
            {
                if (friendGroup == group)
                {
                    //friendGroup.RemoveHotKeys();
                }
            }

            return FriendGroups.Remove(group);
        }

        public static void AddFriendGroup(string group)
        {
            FriendGroup friendGroup = new FriendGroup
            {
                Enabled = true,
                GroupName = group,
                Friends = new List<Friend>()
            };

            //friendGroup.AddHotKeys();

            FriendGroups.Add(friendGroup);

            RedrawGroup();
        }

        public static void ClearAll()
        {
            /*foreach (FriendGroup friendGroup in FriendGroups)
            {
                friendGroup.RemoveHotKeys();
            }*/

            FriendGroups.Clear();
        }

        private static void RedrawAll()
        {
            RedrawGroup();

            /*if (_friendGroups?.Items.Count > 0)
            {
                RedrawList((FriendGroup)_friendGroups.Items[0]);
            }
            else
            {
                RedrawList(null); // we just want to clear the list
            }*/
        }

        public static void RedrawGroup()
        {
            //TODO: redraw on gump
        }

        public static void RedrawList(FriendGroup group)
        {
            //TODO: redraw on gump
        }
    }
}
