using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleApplication5
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern ushort RegisterClassEx(ref WNDCLASSEX window_class);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateWindowEx(uint ExStyle, IntPtr ClassAtom, IntPtr WindowName, int Style, int X, int Y, int Width, int Height, IntPtr HandleToParentWindow, IntPtr Menu, IntPtr Instance, IntPtr Param);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr WindowProcedure(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public extern static IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("gdi32.dll")]
        internal static extern int DescribePixelFormat(IntPtr deviceContext, int pixel, int pfdSize, ref PixelFormatDescriptor pixelFormat);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        private static WindowProcedure WndProcDelegate;

        static void Main(string[] args)
        {
            WndProcDelegate = WndProc;

            IntPtr hInstance = Process.GetCurrentProcess().Handle;
            IntPtr hClassName = Marshal.StringToHGlobalAuto(Guid.NewGuid().ToString());
            IntPtr hTitle = Marshal.StringToHGlobalAuto("TestProg");

            WNDCLASSEX wc = new WNDCLASSEX()
            {
                cbSize = WNDCLASSEX.SizeInBytes,
                style = 0,
                lpfnWndProc = WndProcDelegate,
                cbClsExtra = 0,
                hInstance = hInstance,
                lpszClassName = hClassName,
                hIcon = IntPtr.Zero,
                hIconSm = IntPtr.Zero,
                hCursor = IntPtr.Zero
            };

            ushort atom = RegisterClassEx(ref wc);

            if (atom == 0)
                throw new Exception("Failed to create window class.");

            IntPtr handle = CreateWindowEx(0, hClassName, hTitle, 0x008 | 0x00C | 0x1 | 0x0008, 0, 0, 640, 480, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
            if (handle == IntPtr.Zero)
                throw new Exception("Failed to create window.");

            ShowWindow(handle, 5);
            UpdateWindow(handle);

            PixelFormatDescriptor pfd = new PixelFormatDescriptor();

            int c = DescribePixelFormat(GetDC(handle), 1, PixelFormatDescriptor.SizeInBytes, ref pfd);

            using (StreamWriter sw = new StreamWriter("Results.txt"))
            {
                sw.AutoFlush = true;

                List<PixelFormatDescriptor> pfds = new List<PixelFormatDescriptor>();
                for (int i = 1; i < c; i++)
                {
                    DescribePixelFormat(GetDC(handle), i, PixelFormatDescriptor.SizeInBytes, ref pfd);
                    pfds.Add(pfd);

                    sw.WriteLine(pfd.Flags.ToString());
                }
            }
        }

        private static IntPtr WndProc(IntPtr handle, int message, IntPtr wParam, IntPtr lParam)
        {
            IntPtr? result = null;

            if (result.HasValue)
                return result.Value;
            return DefWindowProc(handle, message, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public WindowProcedure lpfnWndProc; // not WndProc
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public IntPtr lpszMenuName;
            public IntPtr lpszClassName;
            public IntPtr hIconSm;

            public static int SizeInBytes = Marshal.SizeOf(default(WNDCLASSEX));
        }

        /// <summary>
        /// Describes a pixel format. It is used when interfacing with the WINAPI to create a new Context.
        /// Found in WinGDI.h
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct PixelFormatDescriptor
        {
            internal short Size;
            internal short Version;
            internal PixelFormatDescriptorFlags Flags;
            internal PixelType PixelType;
            internal byte ColorBits;
            internal byte RedBits;
            internal byte RedShift;
            internal byte GreenBits;
            internal byte GreenShift;
            internal byte BlueBits;
            internal byte BlueShift;
            internal byte AlphaBits;
            internal byte AlphaShift;
            internal byte AccumBits;
            internal byte AccumRedBits;
            internal byte AccumGreenBits;
            internal byte AccumBlueBits;
            internal byte AccumAlphaBits;
            internal byte DepthBits;
            internal byte StencilBits;
            internal byte AuxBuffers;
            internal byte LayerType;
            private byte Reserved;
            internal int LayerMask;
            internal int VisibleMask;
            internal int DamageMask;

            public static int SizeInBytes = Marshal.SizeOf(default(PixelFormatDescriptor));
        }

        internal enum PixelType : byte
        {
            RGBA = 0,
            INDEXED = 1
        }

        [Flags]
        internal enum PixelFormatDescriptorFlags : int
        {
            // PixelFormatDescriptor flags
            DOUBLEBUFFER = 0x01,
            STEREO = 0x02,
            DRAW_TO_WINDOW = 0x04,
            DRAW_TO_BITMAP = 0x08,
            SUPPORT_GDI = 0x10,
            SUPPORT_OPENGL = 0x20,
            GENERIC_FORMAT = 0x40,
            NEED_PALETTE = 0x80,
            NEED_SYSTEM_PALETTE = 0x100,
            SWAP_EXCHANGE = 0x200,
            SWAP_COPY = 0x400,
            SWAP_LAYER_BUFFERS = 0x800,
            GENERIC_ACCELERATED = 0x1000,
            SUPPORT_DIRECTDRAW = 0x2000,
            SUPPORT_COMPOSITION = 0x8000,

            // PixelFormatDescriptor flags for use in ChoosePixelFormat only
            DEPTH_DONTCARE = unchecked((int)0x20000000),
            DOUBLEBUFFER_DONTCARE = unchecked((int)0x40000000),
            STEREO_DONTCARE = unchecked((int)0x80000000)
        }
    }
}
