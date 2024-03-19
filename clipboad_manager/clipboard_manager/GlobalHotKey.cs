using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace clipboard_manager
{
    public class GlobalHotKey : IDisposable
    {
        private readonly int _id;
        private readonly IntPtr _handle;
        private bool _isKeyRegistered;

        public GlobalHotKey(ModifierKeys modifier, Key key, Window window)
        {
            Modifier = modifier;
            Key = key;
            _handle = new WindowInteropHelper(window).Handle;
            _id = GetHashCode();
        }

        public ModifierKeys Modifier { get; }
        public Key Key { get; }

        public event EventHandler<EventArgs> HotkeyPressed;

        public void RegisterHotKey()
        {
            if (_isKeyRegistered)
            {
                return;
            }

            _isKeyRegistered = NativeMethods.RegisterHotKey(_handle, _id, (uint)Modifier, (uint)KeyInterop.VirtualKeyFromKey(Key));

            if (!_isKeyRegistered)
            {
                throw new InvalidOperationException("Failed to register hotkey");
            }

            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        public void UnregisterHotKey()
        {
            if (!_isKeyRegistered)
            {
                return;
            }

            _isKeyRegistered = !NativeMethods.UnregisterHotKey(_handle, _id);

            if (_isKeyRegistered)
            {
                throw new InvalidOperationException("Failed to unregister hotkey");
            }

            ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (msg.message != NativeMethods.WM_HOTKEY || (int)msg.wParam != _id)
            {
                return;
            }

            OnHotkeyPressed();
            handled = true;
        }

        private void OnHotkeyPressed()
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            UnregisterHotKey();
        }

        private static class NativeMethods
        {
            public const int WM_HOTKEY = 0x0312;

            [DllImport("user32.dll")]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

            [DllImport("user32.dll")]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        }
    }
}