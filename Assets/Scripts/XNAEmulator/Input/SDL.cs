using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SDL2
{
    public static class SDL
    {
        public enum SDL_MessageBoxFlags
        {
            SDL_MESSAGEBOX_ERROR
        }

        public enum SDL_bool
        {
            SDL_FALSE,
            SDL_TRUE
        }

        public enum SDL_EventType : uint
        {
            SDL_WINDOWEVENT = 512, // 0x00000200
            SDL_KEYDOWN = 768, // 0x00000300
            SDL_KEYUP = 769, // 0x00000301
            SDL_TEXTINPUT = 771, // 0x00000303
            SDL_MOUSEMOTION = 1024, // 0x00000400
            SDL_MOUSEBUTTONDOWN = 1025, // 0x00000401
            SDL_MOUSEBUTTONUP = 1026, // 0x00000402
            SDL_MOUSEWHEEL = 1027, // 0x00000403
            SDL_AUDIODEVICEADDED = 4352, // 0x00001100
            SDL_AUDIODEVICEREMOVED = 4353 // 0x00001101
        }

        public enum SDL_Keycode
        {
            SDLK_UNKNOWN = 0,
            SDLK_BACKSPACE = 8,
            SDLK_TAB = 9,
            SDLK_RETURN = 13, // 0x0000000D
            SDLK_ESCAPE = 27, // 0x0000001B
            SDLK_SPACE = 32, // 0x00000020
            SDLK_EXCLAIM = 33, // 0x00000021
            SDLK_QUOTEDBL = 34, // 0x00000022
            SDLK_HASH = 35, // 0x00000023
            SDLK_DOLLAR = 36, // 0x00000024
            SDLK_PERCENT = 37, // 0x00000025
            SDLK_AMPERSAND = 38, // 0x00000026
            SDLK_QUOTE = 39, // 0x00000027
            SDLK_LEFTPAREN = 40, // 0x00000028
            SDLK_RIGHTPAREN = 41, // 0x00000029
            SDLK_ASTERISK = 42, // 0x0000002A
            SDLK_PLUS = 43, // 0x0000002B
            SDLK_COMMA = 44, // 0x0000002C
            SDLK_MINUS = 45, // 0x0000002D
            SDLK_PERIOD = 46, // 0x0000002E
            SDLK_SLASH = 47, // 0x0000002F
            SDLK_0 = 48, // 0x00000030
            SDLK_1 = 49, // 0x00000031
            SDLK_2 = 50, // 0x00000032
            SDLK_3 = 51, // 0x00000033
            SDLK_4 = 52, // 0x00000034
            SDLK_5 = 53, // 0x00000035
            SDLK_6 = 54, // 0x00000036
            SDLK_7 = 55, // 0x00000037
            SDLK_8 = 56, // 0x00000038
            SDLK_9 = 57, // 0x00000039
            SDLK_COLON = 58, // 0x0000003A
            SDLK_SEMICOLON = 59, // 0x0000003B
            SDLK_LESS = 60, // 0x0000003C
            SDLK_EQUALS = 61, // 0x0000003D
            SDLK_GREATER = 62, // 0x0000003E
            SDLK_QUESTION = 63, // 0x0000003F
            SDLK_AT = 64, // 0x00000040
            SDLK_LEFTBRACKET = 91, // 0x0000005B
            SDLK_BACKSLASH = 92, // 0x0000005C
            SDLK_RIGHTBRACKET = 93, // 0x0000005D
            SDLK_CARET = 94, // 0x0000005E
            SDLK_UNDERSCORE = 95, // 0x0000005F
            SDLK_BACKQUOTE = 96, // 0x00000060
            SDLK_a = 97, // 0x00000061
            SDLK_b = 98, // 0x00000062
            SDLK_c = 99, // 0x00000063
            SDLK_d = 100, // 0x00000064
            SDLK_e = 101, // 0x00000065
            SDLK_f = 102, // 0x00000066
            SDLK_g = 103, // 0x00000067
            SDLK_h = 104, // 0x00000068
            SDLK_i = 105, // 0x00000069
            SDLK_j = 106, // 0x0000006A
            SDLK_k = 107, // 0x0000006B
            SDLK_l = 108, // 0x0000006C
            SDLK_m = 109, // 0x0000006D
            SDLK_n = 110, // 0x0000006E
            SDLK_o = 111, // 0x0000006F
            SDLK_p = 112, // 0x00000070
            SDLK_q = 113, // 0x00000071
            SDLK_r = 114, // 0x00000072
            SDLK_s = 115, // 0x00000073
            SDLK_t = 116, // 0x00000074
            SDLK_u = 117, // 0x00000075
            SDLK_v = 118, // 0x00000076
            SDLK_w = 119, // 0x00000077
            SDLK_x = 120, // 0x00000078
            SDLK_y = 121, // 0x00000079
            SDLK_z = 122, // 0x0000007A
            SDLK_DELETE = 127, // 0x0000007F
            SDLK_CAPSLOCK = 1073741881, // 0x40000039
            SDLK_F1 = 1073741882, // 0x4000003A
            SDLK_F2 = 1073741883, // 0x4000003B
            SDLK_F3 = 1073741884, // 0x4000003C
            SDLK_F4 = 1073741885, // 0x4000003D
            SDLK_F5 = 1073741886, // 0x4000003E
            SDLK_F6 = 1073741887, // 0x4000003F
            SDLK_F7 = 1073741888, // 0x40000040
            SDLK_F8 = 1073741889, // 0x40000041
            SDLK_F9 = 1073741890, // 0x40000042
            SDLK_F10 = 1073741891, // 0x40000043
            SDLK_F11 = 1073741892, // 0x40000044
            SDLK_F12 = 1073741893, // 0x40000045
            SDLK_PRINTSCREEN = 1073741894, // 0x40000046
            SDLK_SCROLLLOCK = 1073741895, // 0x40000047
            SDLK_PAUSE = 1073741896, // 0x40000048
            SDLK_INSERT = 1073741897, // 0x40000049
            SDLK_HOME = 1073741898, // 0x4000004A
            SDLK_PAGEUP = 1073741899, // 0x4000004B
            SDLK_END = 1073741901, // 0x4000004D
            SDLK_PAGEDOWN = 1073741902, // 0x4000004E
            SDLK_RIGHT = 1073741903, // 0x4000004F
            SDLK_LEFT = 1073741904, // 0x40000050
            SDLK_DOWN = 1073741905, // 0x40000051
            SDLK_UP = 1073741906, // 0x40000052
            SDLK_NUMLOCKCLEAR = 1073741907, // 0x40000053
            SDLK_KP_DIVIDE = 1073741908, // 0x40000054
            SDLK_KP_MULTIPLY = 1073741909, // 0x40000055
            SDLK_KP_MINUS = 1073741910, // 0x40000056
            SDLK_KP_PLUS = 1073741911, // 0x40000057
            SDLK_KP_ENTER = 1073741912, // 0x40000058
            SDLK_KP_1 = 1073741913, // 0x40000059
            SDLK_KP_2 = 1073741914, // 0x4000005A
            SDLK_KP_3 = 1073741915, // 0x4000005B
            SDLK_KP_4 = 1073741916, // 0x4000005C
            SDLK_KP_5 = 1073741917, // 0x4000005D
            SDLK_KP_6 = 1073741918, // 0x4000005E
            SDLK_KP_7 = 1073741919, // 0x4000005F
            SDLK_KP_8 = 1073741920, // 0x40000060
            SDLK_KP_9 = 1073741921, // 0x40000061
            SDLK_KP_0 = 1073741922, // 0x40000062
            SDLK_KP_PERIOD = 1073741923, // 0x40000063
            SDLK_APPLICATION = 1073741925, // 0x40000065
            SDLK_POWER = 1073741926, // 0x40000066
            SDLK_KP_EQUALS = 1073741927 // 0x40000067
        }

        [Flags]
        public enum SDL_Keymod : ushort
        {
            KMOD_NONE = 0,
            KMOD_LSHIFT = 1,
            KMOD_RSHIFT = 2,
            KMOD_LCTRL = 64, // 0x0040
            KMOD_RCTRL = 128, // 0x0080
            KMOD_LALT = 256, // 0x0100
            KMOD_RALT = 512, // 0x0200
            KMOD_LGUI = 1024, // 0x0400
            KMOD_RGUI = 2048, // 0x0800
            KMOD_NUM = 4096, // 0x1000
            KMOD_CAPS = 8192, // 0x2000
            KMOD_MODE = 16384, // 0x4000
            KMOD_RESERVED = 32768, // 0x8000
            KMOD_CTRL = KMOD_RCTRL | KMOD_LCTRL, // 0x00C0
            KMOD_SHIFT = KMOD_RSHIFT | KMOD_LSHIFT, // 0x0003
            KMOD_ALT = KMOD_RALT | KMOD_LALT, // 0x0300
            KMOD_GUI = KMOD_RGUI | KMOD_LGUI // 0x0C00
        }

        public enum SDL_PACKEDLAYOUT_ENUM
        {
            SDL_PACKEDLAYOUT_1555
        }

        public enum SDL_PIXELORDER_ENUM
        {
            SDL_PACKEDORDER_ARGB = 3
        }

        public enum SDL_PIXELTYPE_ENUM
        {
            SDL_PIXELTYPE_PACKED16
        }

        public enum SDL_SYSWM_TYPE
        {
            SDL_SYSWM_WINDOWS
        }

        public enum SDL_WindowEventID : byte
        {
            SDL_WINDOWEVENT_ENTER,
            SDL_WINDOWEVENT_LEAVE,
            SDL_WINDOWEVENT_FOCUS_GAINED,
            SDL_WINDOWEVENT_FOCUS_LOST
        }

        public const string SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH = "SDL_MOUSE_FOCUS_CLICKTHROUGH";
        public const uint SDL_BUTTON_LEFT = 1;
        public const uint SDL_BUTTON_MIDDLE = 2;
        public const uint SDL_BUTTON_RIGHT = 3;
        public const uint SDL_BUTTON_X1 = 4;
        public const uint SDL_BUTTON_X2 = 5;

        public static readonly uint SDL_PIXELFORMAT_ARGB1555 = SDL_DEFINE_PIXELFORMAT(
            SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16, SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ARGB,
            SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_1555, 16, 2);

        internal static byte[] UTF8_ToNative(string s)
        {
            if (s == null)
                return null;
            return Encoding.UTF8.GetBytes(s + "\0");
        }

        public static unsafe string UTF8_ToManaged(IntPtr s, bool freePtr = false)
        {
            if (s == IntPtr.Zero)
                return null;
            var numPtr = (byte*) (void*) s;
            while (*numPtr != 0)
                ++numPtr;
            var num = (int) (numPtr - (byte*) (void*) s);
            if (num == 0)
                return string.Empty;
            var chars1 = stackalloc char[num];
            var chars2 = Encoding.UTF8.GetChars((byte*) (void*) s, num, chars1, num);
            var str = new string(chars1, 0, chars2);
            if (!freePtr)
                return str;
            SDL_free(s);
            return str;
        }

        internal static void SDL_free(IntPtr memblock)
        {
        }

        public static void SDL_VERSION(out SDL_version x)
        {
            x.major = 2;
            x.minor = 0;
            x.patch = 9;
        }

        private static IntPtr INTERNAL_SDL_GL_GetProcAddress(byte[] proc)
        {
            return IntPtr.Zero;
        }

        public static IntPtr SDL_GL_GetProcAddress(string proc)
        {
            return INTERNAL_SDL_GL_GetProcAddress(UTF8_ToNative(proc));
        }

        public static uint SDL_DEFINE_PIXELFORMAT(
            SDL_PIXELTYPE_ENUM type,
            SDL_PIXELORDER_ENUM order,
            SDL_PACKEDLAYOUT_ENUM layout,
            byte bits,
            byte bytes)
        {
            return (uint) (268435456 | ((byte) type << 24) | ((byte) order << 20) | ((byte) layout << 16) |
                           (bits << 8)) | bytes;
        }

        public static IntPtr SDL_CreateRGBSurfaceWithFormatFrom(
            IntPtr pixels,
            int width,
            int height,
            int depth,
            int pitch,
            uint format)
        {
            return IntPtr.Zero;
        }

        public static void SDL_FreeSurface(IntPtr surface){}

        public static SDL_bool SDL_HasClipboardText()
        {
            return SDL_bool.SDL_FALSE;
        }

        private static IntPtr INTERNAL_SDL_GetClipboardText()
        {
            return IntPtr.Zero;
        }

        public static string SDL_GetClipboardText()
        {
            return UTF8_ToManaged(INTERNAL_SDL_GetClipboardText());
        }

        private static int INTERNAL_SDL_SetClipboardText(byte[] text)
        {
            return 1;
        }

        public static int SDL_SetClipboardText(string text)
        {
            return INTERNAL_SDL_SetClipboardText(UTF8_ToNative(text));
        }

        public static int SDL_CaptureMouse(SDL_bool enabled)
        {
            return 1;
        }

        public static IntPtr SDL_CreateColorCursor(IntPtr surface, int hot_x, int hot_y)
        {
            return IntPtr.Zero;
        }

        public static void SDL_SetCursor(IntPtr cursor)
        {
        }

        public static void SDL_FreeCursor(IntPtr cursor){}

        public static uint SDL_GetTicks()
        {
            return 0;
        }

        public static SDL_bool SDL_GetWindowWMInfo(
            IntPtr window,
            ref SDL_SysWMinfo info)
        {
            return SDL_bool.SDL_TRUE;
        }

        public static IntPtr SDL_GL_GetCurrentWindow()
        {
            return IntPtr.Zero;
        }

        public struct SDL_version
        {
            public byte major;
            public byte minor;
            public byte patch;
        }

        public struct SDL_DisplayEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public uint display;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public int data1;
        }

        public struct SDL_WindowEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public uint windowID;
            public SDL_WindowEventID windowEvent;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public int data1;
            public int data2;
        }

        public struct SDL_KeyboardEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public uint windowID;
            public byte state;
            public byte repeat;
            private byte padding2;
            private byte padding3;
            public SDL_Keysym keysym;
        }

        public struct SDL_TextEditingEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public uint windowID;
            public unsafe fixed byte text[32];
            public int start;
            public int length;
        }

        public struct SDL_TextInputEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public uint windowID;
            public unsafe fixed byte text[32];
        }

        public struct SDL_MouseMotionEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public uint windowID;
            public uint which;
            public byte state;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public int x;
            public int y;
            public int xrel;
            public int yrel;
        }

        public struct SDL_MouseButtonEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public uint windowID;
            public uint which;
            public byte button;
            public byte state;
            public byte clicks;
            private byte padding1;
            public int x;
            public int y;
        }

        public struct SDL_MouseWheelEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public uint windowID;
            public uint which;
            public int x;
            public int y;
            public uint direction;
        }

        public struct SDL_JoyAxisEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public int which;
            public byte axis;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public short axisValue;
            public ushort padding4;
        }

        public struct SDL_JoyBallEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public int which;
            public byte ball;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public short xrel;
            public short yrel;
        }

        public struct SDL_JoyHatEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public int which;
            public byte hat;
            public byte hatValue;
            private byte padding1;
            private byte padding2;
        }

        public struct SDL_JoyButtonEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public int which;
            public byte button;
            public byte state;
            private byte padding1;
            private byte padding2;
        }

        public struct SDL_JoyDeviceEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public int which;
        }

        public struct SDL_ControllerAxisEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public int which;
            public byte axis;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public short axisValue;
            private ushort padding4;
        }

        public struct SDL_ControllerButtonEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public int which;
            public byte button;
            public byte state;
            private byte padding1;
            private byte padding2;
        }

        public struct SDL_ControllerDeviceEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public int which;
        }

        public struct SDL_AudioDeviceEvent
        {
            public uint type;
            public uint timestamp;
            public uint which;
            public byte iscapture;
            private byte padding1;
            private byte padding2;
            private byte padding3;
        }

        public struct SDL_TouchFingerEvent
        {
            public uint type;
            public uint timestamp;
            public long touchId;
            public long fingerId;
            public float x;
            public float y;
            public float dx;
            public float dy;
            public float pressure;
        }

        public struct SDL_MultiGestureEvent
        {
            public uint type;
            public uint timestamp;
            public long touchId;
            public float dTheta;
            public float dDist;
            public float x;
            public float y;
            public ushort numFingers;
            public ushort padding;
        }

        public struct SDL_DollarGestureEvent
        {
            public uint type;
            public uint timestamp;
            public long touchId;
            public long gestureId;
            public uint numFingers;
            public float error;
            public float x;
            public float y;
        }

        public struct SDL_DropEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public IntPtr file;
            public uint windowID;
        }

        public struct SDL_SensorEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public int which;
            public unsafe fixed float data[6];
        }

        public struct SDL_QuitEvent
        {
            public SDL_EventType type;
            public uint timestamp;
        }

        public struct SDL_UserEvent
        {
            public uint type;
            public uint timestamp;
            public uint windowID;
            public int code;
            public IntPtr data1;
            public IntPtr data2;
        }

        public struct SDL_SysWMEvent
        {
            public SDL_EventType type;
            public uint timestamp;
            public IntPtr msg;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct SDL_Event
        {
            [FieldOffset(0)] public SDL_EventType type;
            [FieldOffset(0)] public SDL_DisplayEvent display;
            [FieldOffset(0)] public SDL_WindowEvent window;
            [FieldOffset(0)] public SDL_KeyboardEvent key;
            [FieldOffset(0)] public SDL_TextEditingEvent edit;
            [FieldOffset(0)] public SDL_TextInputEvent text;
            [FieldOffset(0)] public SDL_MouseMotionEvent motion;
            [FieldOffset(0)] public SDL_MouseButtonEvent button;
            [FieldOffset(0)] public SDL_MouseWheelEvent wheel;
            [FieldOffset(0)] public SDL_JoyAxisEvent jaxis;
            [FieldOffset(0)] public SDL_JoyBallEvent jball;
            [FieldOffset(0)] public SDL_JoyHatEvent jhat;
            [FieldOffset(0)] public SDL_JoyButtonEvent jbutton;
            [FieldOffset(0)] public SDL_JoyDeviceEvent jdevice;
            [FieldOffset(0)] public SDL_ControllerAxisEvent caxis;
            [FieldOffset(0)] public SDL_ControllerButtonEvent cbutton;
            [FieldOffset(0)] public SDL_ControllerDeviceEvent cdevice;
            [FieldOffset(0)] public SDL_AudioDeviceEvent adevice;
            [FieldOffset(0)] public SDL_SensorEvent sensor;
            [FieldOffset(0)] public SDL_QuitEvent quit;
            [FieldOffset(0)] public SDL_UserEvent user;
            [FieldOffset(0)] public SDL_SysWMEvent syswm;
            [FieldOffset(0)] public SDL_TouchFingerEvent tfinger;
            [FieldOffset(0)] public SDL_MultiGestureEvent mgesture;
            [FieldOffset(0)] public SDL_DollarGestureEvent dgesture;
            [FieldOffset(0)] public SDL_DropEvent drop;
        }
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int SDL_EventFilter(IntPtr userdata, IntPtr sdlevent);
        
        public static void SDL_AddEventWatch(SDL.SDL_EventFilter filter, IntPtr userdata)
        {}

        public struct SDL_Keysym
        {
            public SDL_Keycode sym;
            public SDL_Keymod mod;
        }

        public struct INTERNAL_windows_wminfo
        {
            public IntPtr window;
            public IntPtr hdc;
        }

        public struct INTERNAL_winrt_wminfo
        {
            public IntPtr window;
        }

        public struct INTERNAL_x11_wminfo
        {
            public IntPtr display;
            public IntPtr window;
        }

        public struct INTERNAL_directfb_wminfo
        {
            public IntPtr dfb;
            public IntPtr window;
            public IntPtr surface;
        }

        public struct INTERNAL_cocoa_wminfo
        {
            public IntPtr window;
        }

        public struct INTERNAL_uikit_wminfo
        {
            public IntPtr window;
            public uint framebuffer;
            public uint colorbuffer;
            public uint resolveFramebuffer;
        }

        public struct INTERNAL_wayland_wminfo
        {
            public IntPtr display;
            public IntPtr surface;
            public IntPtr shell_surface;
        }

        public struct INTERNAL_mir_wminfo
        {
            public IntPtr connection;
            public IntPtr surface;
        }

        public struct INTERNAL_android_wminfo
        {
            public IntPtr window;
            public IntPtr surface;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INTERNAL_SysWMDriverUnion
        {
            [FieldOffset(0)] public INTERNAL_windows_wminfo win;
            [FieldOffset(0)] public INTERNAL_winrt_wminfo winrt;
            [FieldOffset(0)] public INTERNAL_x11_wminfo x11;
            [FieldOffset(0)] public INTERNAL_directfb_wminfo dfb;
            [FieldOffset(0)] public INTERNAL_cocoa_wminfo cocoa;
            [FieldOffset(0)] public INTERNAL_uikit_wminfo uikit;
            [FieldOffset(0)] public INTERNAL_wayland_wminfo wl;
            [FieldOffset(0)] public INTERNAL_mir_wminfo mir;
            [FieldOffset(0)] public INTERNAL_android_wminfo android;
        }

        public struct SDL_SysWMinfo
        {
            public SDL_version version;
            public SDL_SYSWM_TYPE subsystem;
            public INTERNAL_SysWMDriverUnion info;
        }

        public static void SDL_ShowSimpleMessageBox(SDL_MessageBoxFlags sdlMessageboxError, string error, string msg, IntPtr windowHandle)
        {
        }

        public static void SDL_GetGlobalMouseState(out int i, out int i1)
        {
            i = 0;
            i1 = 0;
        }

        public static void SDL_GetWindowPosition(IntPtr windowHandle, out int i, out int i1)
        {
            i = 0;
            i1 = 0;
        }

        public static void SDL_GetMouseState(out int positionX, out int positionY)
        {
            positionX = 0;
            positionY = 0;
        }

        public static void SDL_GetWindowBordersSize(IntPtr windowHandle, out int i, out int i1, out int i2, out int i3)
        {
            i = 0;
            i1 = 0;
            i2 = 0;
            i3 = 0;
        }

        public static void SDL_SetWindowPosition(IntPtr windowHandle, int i, int i1)
        {
        }

        public enum SDL_WindowFlags
        {
            SDL_WINDOW_BORDERLESS = 0,
            SDL_WINDOW_MAXIMIZED = 1
        }

        public static SDL_WindowFlags SDL_GetWindowFlags(IntPtr windowHandle)
        {
            return SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
        }

        public static void SDL_SetWindowBordered(IntPtr windowHandle, SDL_bool sdlFalse)
        {
        }

        public static void SDL_GetCurrentDisplayMode(int i, out SDL_DisplayMode sdlDisplayMode)
        {
            sdlDisplayMode = new SDL_DisplayMode();
        }

        public struct SDL_DisplayMode
        {
            public int w;
            public int h;
        }

        public static void SDL_MaximizeWindow(IntPtr windowHandle)
        {
        }

        public static void SDL_RestoreWindow(IntPtr windowHandle)
        {
        }

        //NOTE: Not implemented properly on purpose. Any usages so far are not relevant to MobileUO
        public static string SDL_GetPlatform()
        {
            return string.Empty;
        }
    }
}