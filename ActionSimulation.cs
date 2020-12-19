using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenShare
{
    public static class ActionSimulation
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public POINT(Point pt)
            {
                this.x = pt.X;
                this.y = pt.Y;
            }
        }

        [DllImport("user32")]
        static extern IntPtr WindowFromPoint(POINT pt);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN   = 0x2,
            LEFTUP     = 0x4,
            MIDDLEDOWN = 0x20,
            MIDDLEUP   = 0x40,
            MOVE       = 0x1,
            ABSOLUTE   = 0x8000,
            RIGHTDOWN  = 0x8,
            RIGHTUP    = 0x10
        }

        [DllImport("user32")]
        private static extern void mouse_event(MouseEventFlags flags, int x, int y, int data, int extraInfo);

        const int WM_CHAR    = 0x102;

        [DllImport("user32")]
        static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private static IntPtr hWnd = IntPtr.Zero;

        public static void SendChar(char ch)
        {
            PostMessage(hWnd, WM_CHAR, (int)ch, 0);
        }

        public static void MouseLeftClick(int x, int y)
        {
            Point prevCursorPos = Cursor.Position;
            Cursor.Position = new Point(x, y);
            mouse_event(MouseEventFlags.LEFTDOWN | MouseEventFlags.ABSOLUTE, x, y, 0, 0);
            mouse_event(MouseEventFlags.LEFTUP, 0, 0, 0, 0);
            Cursor.Position = prevCursorPos;
            hWnd = WindowFromPoint(new POINT(x, y));
        }
    }
}
