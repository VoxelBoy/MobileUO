using System;
using System.Runtime.InteropServices;

namespace CUO_API
{
    public delegate void OnCastSpell(int idx);
    public delegate short OnGetPacketLength(int id);
    public delegate bool OnGetPlayerPosition(out int x, out int y, out int z);
    public delegate void OnGetStaticImage(ushort g, ref ArtInfo art);
    public delegate string OnGetUOFilePath();
    public delegate void OnClientClose();
    public delegate void OnConnected();
    public delegate void OnDisconnected();
    public delegate void OnFocusGained();
    public delegate void OnFocusLost();
    public delegate bool OnHotkey(int key, int mod, bool pressed);
    public delegate void OnInitialize();
    public delegate void OnMouse(int button, int wheel);
    public delegate void OnUpdatePlayerPosition(int x, int y, int z);
    public delegate bool OnPacketSendRecv(ref byte[] data, ref int length);
    public delegate bool RequestMove(int dir, bool run);
    public delegate void OnSetTitle(string title);
    public delegate void OnTick();

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ArtInfo
    {
        public long Address;
        public long Size;
        public long CompressedSize;
    }

    public struct PluginHeader
    {
        public int ClientVersion;
        public IntPtr HWND;
        public IntPtr OnRecv;
        public IntPtr OnSend;
        public IntPtr OnHotkeyPressed;
        public IntPtr OnMouse;
        public IntPtr OnPlayerPositionChanged;
        public IntPtr OnClientClosing;
        public IntPtr OnInitialize;
        public IntPtr OnConnected;
        public IntPtr OnDisconnected;
        public IntPtr OnFocusGained;
        public IntPtr OnFocusLost;
        public IntPtr GetUOFilePath;
        public IntPtr Recv;
        public IntPtr Send;
        public IntPtr GetPacketLength;
        public IntPtr GetPlayerPosition;
        public IntPtr CastSpell;
        public IntPtr GetStaticImage;
        public IntPtr Tick;
        public IntPtr RequestMove;
        public IntPtr SetTitle;
    }
}