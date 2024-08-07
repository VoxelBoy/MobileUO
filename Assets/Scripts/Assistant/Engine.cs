using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CUO_API;
using ClassicUO.Network;
using ClassicUO;
using ClassicUO.IO.Resources;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.Game.Managers;
using Assistant.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Assistant
{
    public class FeatureBit
    {
        public static readonly int WeatherFilter = 0;
        public static readonly int LightFilter = 1;
        public static readonly int SmartLT = 2;
        public static readonly int RangeCheckLT = 3;
        public static readonly int AutoOpenDoors = 4;
        public static readonly int UnequipBeforeCast = 5;
        public static readonly int AutoPotionEquip = 6;
        public static readonly int BlockHealPoisoned = 7;
        public static readonly int LoopingMacros = 8; // includes fors and macros running macros
        public static readonly int UseOnceAgent = 9;
        public static readonly int RestockAgent = 10;
        public static readonly int SellAgent = 11;
        public static readonly int BuyAgent = 12;
        public static readonly int PotionHotkeys = 13;
        public static readonly int RandomTargets = 14;
        public static readonly int ClosestTargets = 15;
        public static readonly int OverheadHealth = 16;
        public static readonly int AutolootAgent = 17;
        public static readonly int BoneCutterAgent = 18;
        public static readonly int AdvancedMacros = 19;//we can't use any scripting language here
        public static readonly int AutoRemount = 20;
        public static readonly int AutoBandage = 21;
        public static readonly int EnemyTargetShare = 22;
        public static readonly int FilterSeason = 23;
        public static readonly int SpellTargetShare = 24;
        public static readonly int HumanoidHealthChecks = 25;
        public static readonly int SpeechJournalChecks = 26;

        public static readonly int MaxBit = 26;
    }

    internal enum ContainerType
    {
        None,
        Ground,
        Serial,
        Any
    }

    internal static class UOSObjects
    {
        private static ClassicUO.Game.UI.Gumps.AssistantGump _Gump;

        internal static ClassicUO.Game.UI.Gumps.AssistantGump Gump
        {
            get
            {
                if (_Gump == null || _Gump.IsDisposed)
                {
                    _Gump = new ClassicUO.Game.UI.Gumps.AssistantGump();
                    AfterBuild();
                }
                return _Gump;
            }
            set
            {
                if (_Gump != value)
                {
                    ClassicUO.Game.UI.Gumps.AssistantGump old = _Gump;
                    _Gump = value;
                    old?.Dispose();
                    if(_Gump != null)
                        AfterBuild();
                }
            }
        }

        internal static void AfterBuild(byte itr = 0)
        {
            if (_Gump != null && !_Gump.IsDisposed)
            {
                _Gump.LoadConfig();
                XmlFileParser.LoadProfile(_Gump, _Gump.ProfileSelected);
            }
            else if(itr < 10)
                Timer.DelayedCallbackState(TimeSpan.FromMilliseconds(200), AfterBuild, ++itr).Start();
        }

        internal static Dictionary<uint, UOItem> Items { get; } = new Dictionary<uint, UOItem>();
        internal static Dictionary<uint, UOMobile> Mobiles { get; } = new Dictionary<uint, UOMobile>();

        internal static void ClearAll()
        {
            Items.Clear();
            Mobiles.Clear();
        }

        internal static PlayerData Player;

        internal static string OrigPlayerName
        {
            get;
            set;
        }

        internal static UOItem FindItem(uint serial)
        {
            Items.TryGetValue(serial, out UOItem it);
            return it;
        }

        internal static UOItem FindItemByType(int itemId, int color = -1, int range = -1, ContainerType cnttype = ContainerType.Any)
        {
            foreach (UOItem item in Items.Values)
            {
                if (item.ItemID == itemId && (color == -1 || item.Hue == color) && (range == -1 || Utility.InRange(Player.Position, item.WorldPosition, range)) && (cnttype == ContainerType.Any || (cnttype == ContainerType.Ground && item.OnGround) || (cnttype == ContainerType.Serial && item.Container != null)))
                    return item;
            }

            return null;
        }

        internal static List<UOItem> FindItemsByTypes(HashSet<ushort> itemIds, int color = -1, int range = -1, ContainerType cnttype = ContainerType.Any)
        {
            List<UOItem> items = new List<UOItem>();

            Parallel.ForEach(Items.Values, item =>
            {
                if (itemIds.Contains(item.ItemID) && (color == -1 || item.Hue == color) && (range == -1 || Utility.InRange(Player.Position, item.WorldPosition, range)) && (cnttype == ContainerType.Any || (cnttype == ContainerType.Ground && item.OnGround) || (cnttype == ContainerType.Serial && item.Container != null)))
                    items.Add(item);
            });

            return items;
        }

        internal static List<UOItem> FindItemsByName(string name)
        {
            List<UOItem> items = new List<UOItem>();

            Parallel.ForEach(Items.Values, item =>
            {
                if (item.DisplayName.ToLower().StartsWith(name.ToLower()))
                    items.Add(item);
            });

            return items;
        }

        internal static List<UOMobile> FindMobilesByName(string name)
        {
            List<UOMobile> mobiles = new List<UOMobile>();

            Parallel.ForEach(Mobiles.Values, mobile =>
            {
                if (mobile.Name != null && mobile.Name.ToLower().Equals(name.ToLower()))
                    mobiles.Add(mobile);
            });

            return mobiles;
        }

        internal static UOMobile FindMobile(uint serial)
        {
            Mobiles.TryGetValue(serial, out UOMobile m);
            return m;
        }

        internal static UOEntity FindEntity(uint serial)
        {
            if (Mobiles.TryGetValue(serial, out UOMobile m))
                return m;
            if (Items.TryGetValue(serial, out UOItem i))
                return i;
            return null;
        }

        internal static UOEntity FindEntityByType(int graphic, int hue = -1, int range = 24)
        {
            foreach (UOEntity ie in EntitiesInRange(range, false))
            {
                if(ie.Graphic == graphic && (hue == -1 || hue  == ie.Hue))
                    return ie;
            }

            return null;
        }

        internal static List<UOEntity> EntitiesInRange(int range, bool restrictrange = true)
        {
            List<UOEntity> list = new List<UOEntity>();

            if (UOSObjects.Player == null || range < 0)
                return list;

            foreach (UOItem i in Items.Values)
            {
                if (Utility.InRange(Player.Position, i.GetWorldPosition(), restrictrange ? Math.Min(range, World.ClientViewRange) : range))
                    list.Add(i);
            }
            foreach (UOMobile m in Mobiles.Values)
            {
                if (Utility.InRange(UOSObjects.Player.Position, m.Position, restrictrange ? Math.Min(range, World.ClientViewRange) : range))
                    list.Add(m);
            }

            return list;
        }

        internal static List<UOItem> ItemsInRange()
        {
            return ItemsInRange(World.ClientViewRange);
        }

        internal static List<UOItem> ItemsInRange(int range, bool restrictrange = true, bool sort = false)
        {
            List<UOItem> list = new List<UOItem>();

            if (Player == null)
                return list;

            foreach(UOItem i in Items.Values)
            {
                if (Utility.InRange(Player.Position, i.GetWorldPosition(), restrictrange ? Math.Min(range, World.ClientViewRange) : range))
                    list.Add(i);
            }

            if(sort)
                list.Sort(Targeting.Instance);

            return list;
        }

        internal static List<UOMobile> MobilesInRange(int range, bool restrictrange = true, bool sort = false)
        {
            List<UOMobile> list = new List<UOMobile>();

            if (Player == null)
                return list;

            foreach (UOMobile m in Mobiles.Values)
            {
                if (Utility.InRange(Player.Position, m.Position, restrictrange ? Math.Min(range, World.ClientViewRange) : range))
                    list.Add(m);
            }
            list.Remove(Player);

            if (sort)
                list.Sort(Targeting.Instance);

            return list;
        }

        internal static void SnapShot(bool quiet = true)
        {
            // try
            // {
            //     int w = Client.Game.GraphicManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
            //     int h = Client.Game.GraphicManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
            //     //pull the picture from the buffer 
            //     Color[] colors = new Color[w * h];
            //     Client.Game.GraphicManager.GraphicsDevice.GetBackBufferData(colors);
            //     string path = Path.Combine(ClassicUO.Configuration.Profile.DataPath, "Screenshots");
            //     ClassicUO.Utility.FileSystemHelper.CreateFolderIfNotExists(path);
            //     DateTime date = Engine.MistedDateTime;
            //     path = Path.Combine(path, $"AssistUO_{date.Year}-{date.Month}-{date.Day}_{date.Hour}-{date.Minute}-{date.Second}_{date.Millisecond}.png");
            //     Task.Factory.StartNew(() =>
            //     {
            //         using (Texture2D texture = new Texture2D(Client.Game.GraphicManager.GraphicsDevice, w, h, false, SurfaceFormat.Color))
            //         {
            //             texture.SetData(colors);
            //
            //             using (Stream stream = File.Create(path))
            //             {
            //                 texture.SaveAsPng(stream, texture.Width, texture.Height);
            //
            //                 if (!quiet)
            //                     UOSObjects.Player.SendMessage(MsgLevel.Info, $"Screenshot stored in: {path}");
            //             }
            //         }
            //     });
            // }
            // catch
            // {
            // }
        }

        internal static string GetDefaultItemName(ushort graphic)
        {
            return (graphic < TileDataLoader.Instance.StaticData.Length ? TileDataLoader.Instance.StaticData[graphic].Name : TileDataLoader.Instance.StaticData[0].Name).Replace("%", "");
        }

        internal class PlayerDistanceComparer : IComparer<UOEntity>
        {
            public static IComparer<UOEntity> Instance = new PlayerDistanceComparer();

            public PlayerDistanceComparer()
            {
            }

            public int Compare(UOEntity x, UOEntity y)
            {
                return Utility.Distance(Player.Position, x.WorldPosition).CompareTo(Utility.Distance(Player.Position, y.WorldPosition));
            }
        }

        internal static List<UOMobile> MobilesInRange()
        {
            return MobilesInRange(World.ClientViewRange);
        }

        internal static void AddItem(UOItem item)
        {
            Items[item.Serial] = item;
        }

        internal static void AddMobile(UOMobile mob)
        {
            Mobiles[mob.Serial] = mob;
        }

        internal static void RequestMobileStatus(UOMobile m)
        {
            Engine.Instance.SendToServer(new StatusQuery(m));
        }

        internal static void RemoveMobile(UOMobile mob)
        {
            Mobiles.Remove(mob.Serial);
        }

        internal static void RemoveItem(UOItem item)
        {
            Items.Remove(item.Serial);
        }

        internal static byte ClientViewRange { get; set; } = 18;
        internal static bool Recording { get; set; } = false;
    }

    internal enum PacketAction : byte
    {
        None   = 0x0,
        Viewer = 0x1,
        Filter = 0x2,
        Both   = 0x3
    }

    internal class Engine
    {
        internal static readonly UOSteamClient Instance = new UOSteamClient();

        public static unsafe void Install(PluginHeader* plugin)
        {
            if(!Instance.Install(plugin))
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        internal static bool UsePostKRPackets => Client.Version >= ClientVersion.CV_6017;
        internal static bool UseNewMobileIncoming => Client.Version >= ClientVersion.CV_70331;
        internal static bool UsePostSAChanges => Client.Version >= ClientVersion.CV_7000;
        internal static bool UsePostHSChanges => Client.Version >= ClientVersion.CV_7090;

        private static int _PreviousHour = -1;
        private static int _Differential;
        public static int Differential//to use in all cases where you rectify normal clocks obtained with utctimer!
        {
            get
            {
                if (_PreviousHour != DateTime.UtcNow.Hour)
                {
                    _PreviousHour = DateTime.UtcNow.Hour;
                    _Differential = DateTimeOffset.Now.Offset.Hours;
                }
                return _Differential;
            }
        }
        public static DateTime MistedDateTime => DateTime.UtcNow.AddHours(Differential);

        internal class UOSteamClient
        {
            public static OnPacketSendRecv _sendToClient, _sendToServer, _recv, _send;
            public static OnGetPacketLength _getPacketLength;
            public static OnGetPlayerPosition _getPlayerPosition;
            public static OnCastSpell _castSpell;
            public static OnGetStaticImage _getStaticImage;
            public static OnTick _tick;
            public static RequestMove _requestMove;
            public static OnSetTitle _setTitle;
            public static OnGetUOFilePath _uoFilePath;


            public static OnHotkey _onHotkeyPressed;
            public static OnMouse _onMouse;
            public static OnUpdatePlayerPosition _onUpdatePlayerPosition;
            public static OnClientClose _onClientClose;
            public static OnInitialize _onInitialize;
            public static OnConnected _onConnected;
            public static OnDisconnected _onDisconnected;
            public static OnFocusGained _onFocusGained;
            public static OnFocusLost _onFocusLost;

            public unsafe bool Install(PluginHeader* header)
            {
                PacketHandlers.Initialize();
                Filter.Initialize();
                Targeting.Initialize();
                HotKeys.Initialize();
                Scavenger.Initialize();
                Vendors.Buy.Initialize();
                Vendors.Sell.Initialize();
                // _sendToClient = (OnPacketSendRecv)Marshal.GetDelegateForFunctionPointer(header->Recv, typeof(OnPacketSendRecv));
                // _sendToServer = (OnPacketSendRecv)Marshal.GetDelegateForFunctionPointer(header->Send, typeof(OnPacketSendRecv));
                // _getPacketLength = (OnGetPacketLength)Marshal.GetDelegateForFunctionPointer(header->GetPacketLength, typeof(OnGetPacketLength));
                // _getPlayerPosition = (OnGetPlayerPosition)Marshal.GetDelegateForFunctionPointer(header->GetPlayerPosition, typeof(OnGetPlayerPosition));
                // _castSpell = (OnCastSpell)Marshal.GetDelegateForFunctionPointer(header->CastSpell, typeof(OnCastSpell));
                // _getStaticImage = (OnGetStaticImage)Marshal.GetDelegateForFunctionPointer(header->GetStaticImage, typeof(OnGetStaticImage));
                // _requestMove = (RequestMove)Marshal.GetDelegateForFunctionPointer(header->RequestMove, typeof(RequestMove));
                // _setTitle = (OnSetTitle)Marshal.GetDelegateForFunctionPointer(header->SetTitle, typeof(OnSetTitle));
                // _uoFilePath = (OnGetUOFilePath)Marshal.GetDelegateForFunctionPointer(header->GetUOFilePath, typeof(OnGetUOFilePath));
                _tick = Tick;
                _recv = OnRecv;
                _send = OnSend;
                _onHotkeyPressed = OnHotKeyHandler;
                _onMouse = OnMouseHandler;
                _onUpdatePlayerPosition = OnPlayerPositionChanged;
                _onClientClose = OnClientClosing;
                _onInitialize = OnInitialize;
                _onConnected = OnConnected;
                _onDisconnected = OnDisconnected;
                _onFocusGained = OnFocusGained;
                _onFocusLost = OnFocusLost;
                // header->Tick = Marshal.GetFunctionPointerForDelegate(_tick);
                // header->OnRecv = Marshal.GetFunctionPointerForDelegate(_recv);
                // header->OnSend = Marshal.GetFunctionPointerForDelegate(_send);
                // header->OnHotkeyPressed = Marshal.GetFunctionPointerForDelegate(_onHotkeyPressed);
                // header->OnMouse = Marshal.GetFunctionPointerForDelegate(_onMouse);
                // header->OnPlayerPositionChanged = Marshal.GetFunctionPointerForDelegate(_onUpdatePlayerPosition);
                // header->OnClientClosing = Marshal.GetFunctionPointerForDelegate(_onClientClose);
                // header->OnInitialize = Marshal.GetFunctionPointerForDelegate(_onInitialize);
                // header->OnConnected = Marshal.GetFunctionPointerForDelegate(_onConnected);
                // header->OnDisconnected = Marshal.GetFunctionPointerForDelegate(_onDisconnected);
                // header->OnFocusGained = Marshal.GetFunctionPointerForDelegate(_onFocusGained);
                // header->OnFocusLost = Marshal.GetFunctionPointerForDelegate(_onFocusLost);
                return true;
            }

            private void OnClientClosing()
            {

            }

            private void OnConnected()
            {
                ScriptManager.OnLogin();
                UIManager.Add(UOSObjects.Gump);
            }

            private void OnDisconnected()
            {
                UOSObjects.Gump?.Dispose();
                ScriptManager.OnLogout();
                UOSObjects.ClearAll();
                DressList.ClearAll();
                FriendsManager.ClearAll();
                Organizer.ClearAll();
                Scavenger.ClearAll();
                Vendors.Buy.ClearAll();
            }

            private void OnFocusGained()
            {

            }

            private void OnFocusLost()
            {

            }

            private void Tick()
            {
                Timer.Slice();
            }

            private void OnPlayerPositionChanged(int x, int y, int z)
            {
                UOSObjects.Player.Position = new Point3D(x, y, z);
            }

            private bool OnRecv(ref byte[] data, ref int length)
            {
                byte id = data[0];
                PacketAction pkta = PacketHandler.HasServerViewerFilter(id);
                bool result = true;
                //m_In += (uint)length;

                Packet packet;
                switch(pkta)
                {
                    case PacketAction.Both:
                    case PacketAction.Filter:
                        packet = new Packet(data, length);
                        result = !PacketHandler.OnServerPacket(id, packet, pkta);

                        data = packet.ToArray();
                        length = packet.Length;
                        break;
                    case PacketAction.Viewer:
                        packet = new Packet(data, length);
                        result = !PacketHandler.OnServerPacket(id, packet, pkta);
                        break;
                }

                return result;
            }

            private bool OnSend(ref byte[] data, ref int length)
            {
                //m_Out += (uint)length;
                bool result = true;
                byte id = data[0];
                PacketAction pkta = PacketHandler.HasClientViewerFilter(id);

                Packet packet;
                switch (pkta)
                {
                    case PacketAction.Both:
                    case PacketAction.Filter:
                        packet = new Packet(data, length);
                        result = !PacketHandler.OnClientPacket(id, packet, pkta);

                        data = packet.ToArray();
                        length = packet.Length;
                        break;
                    case PacketAction.Viewer:
                        packet = new Packet(data, length);
                        result = !PacketHandler.OnClientPacket(id, packet, pkta);
                        break;
                }

                return result;
            }

            private void OnMouseHandler(int button, int wheel)
            {
                if (World.Player == null)
                    return;
                if (wheel > 0)
                    button = 0x101;
                else if (wheel < 0)
                    button = 0x102;
                else
                    button -= 1;
                if (HotKeys.GetVKfromSDL(button, _KeyMod, out uint vkey))
                    HotKeys.NonBlockHotKeyAction(vkey);
            }

            private void OnInitialize()
            {
            }

            private int _KeyMod;
            private bool OnHotKeyHandler(int key, int mod, bool ispressed)
            {
                if (ispressed)
                {
                    if (HotKeys.GetVKfromSDL(key, mod, out uint vkey))
                        return HotKeys.NonBlockHotKeyAction(vkey);
                }
                _KeyMod = mod;
                return true;
            }

            internal void SendToClient(PacketBase p)
            {
                byte[] data = p.ToArray();
                int l = p.Length;
                _sendToClient(ref data, ref l);
            }

            internal void SendToServer(PacketBase p)
            {
                byte[] data = p.ToArray();
                int l = p.Length;
                _sendToServer(ref data, ref l);
            }

            internal void RequestMove(Direction m_Dir, bool run = true)
            {
                _requestMove((int)m_Dir, run);
            }

            private ulong m_Features = 0;
            public bool AllowBit(int bit)
            {
                return (m_Features & (1U << bit)) == 0;
            }

            public void SetFeatures(ulong features)
            {
                m_Features = features;
                if(!Engine.Instance.AllowBit(FeatureBit.AutolootAgent))
                {
                    UOSObjects.Gump.DisableAutoLoot();
                }
                if(!Engine.Instance.AllowBit(FeatureBit.LoopingMacros))
                {
                    UOSObjects.Gump.DisableLoop();
                }
                UOSObjects.Gump.UpdateVendorsListGump();
            }

            private static char[] _Exceptions = new char[] { ' ', '-', '_' };
            internal static bool Validate(string name, int minLength = 3, int maxLength = 24, bool allowLetters = true, bool allowDigits = true, int maxExceptions = 0, bool noExceptionsAtStart = true, char[] exceptions = null)
            {
                if (name == null || name.Length < minLength || name.Length > maxLength)
                {
                    return false;
                }
                if (exceptions == null)
                    exceptions = _Exceptions;
                int exceptCount = 0;
                name = name.ToLower();

                if (!allowLetters || !allowDigits || (exceptions.Length > 0 && (noExceptionsAtStart || maxExceptions < int.MaxValue)))
                {
                    int length = name.Length;
                    for (int i = 0; i < length; ++i)
                    {
                        char c = name[i];
                        if (c >= 'a' && c <= 'z')
                        {
                            if (!allowLetters)
                            {
                                return false;
                            }

                            exceptCount = 0;
                        }
                        else if (c >= '0' && c <= '9')
                        {
                            if (!allowDigits)
                            {
                                return false;
                            }

                            exceptCount = 0;
                        }
                        else
                        {
                            bool except = false;

                            for (int j = 0; !except && j < exceptions.Length; ++j)
                            {
                                if (c == exceptions[j])
                                {
                                    except = true;
                                }
                            }

                            if (!except || (i == 0 && noExceptionsAtStart))
                            {
                                return false;
                            }

                            if (exceptCount++ == maxExceptions)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
        }
    }
}
