// Platforms/Windows/GlobalHotkey.cs
using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WinRT.Interop; // WindowNative

namespace TestMauiApp.WinUI;

public static class GlobalHotkey
{
    const int WM_HOTKEY = 0x0312;

    // Modifiers (can be OR'ed)
    const uint MOD_ALT = 0x0001;
    const uint MOD_CONTROL = 0x0002;
    const uint MOD_SHIFT = 0x0004;
    const uint MOD_WIN = 0x0008;

    // Subclass definitions
    private delegate IntPtr SubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, IntPtr dwRefData);

    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("comctl32.dll", SetLastError = true)]
    static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, UIntPtr uIdSubclass, IntPtr dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    static extern bool RemoveWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, UIntPtr uIdSubclass);

    [DllImport("comctl32.dll", SetLastError = true)]
    static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    private static SubclassProc? _proc;
    private static IntPtr _hwnd = IntPtr.Zero;
    private static int _hotkeyId = 1; // choose any positive int
    private static DispatcherQueue? _dispatcher;

    /// <summary>
    /// Call once (e.g., in MainWindow ctor on Windows) to register Ctrl+Shift+G as a global hotkey.
    /// Change modifiers/key as you like.
    /// </summary>
    public static void InitAndRegister(Microsoft.UI.Xaml.Window win, Action onHotkey)
    {
        _dispatcher = win.DispatcherQueue;
        _hwnd = WindowNative.GetWindowHandle(win);
        _proc = WndProc;

        // Example hotkey: Ctrl + Shift + G  (VK codes use ASCII for letters)
        uint modifiers = MOD_CONTROL | MOD_SHIFT;
        uint vk = 0x47; // 'G'

        if (!RegisterHotKey(_hwnd, _hotkeyId, modifiers, vk))
            throw new InvalidOperationException("RegisterHotKey failed. Try a different key combo (it may already be in use).");

        // Hook window messages
        if (!SetWindowSubclass(_hwnd, _proc, (UIntPtr)1, IntPtr.Zero))
            throw new InvalidOperationException("SetWindowSubclass failed.");

        // Local handler to invoke your app code safely on the UI thread
        _onHotkey = onHotkey;
    }

    public static void Unregister()
    {
        if (_hwnd != IntPtr.Zero)
        {
            UnregisterHotKey(_hwnd, _hotkeyId);
            if (_proc != null) RemoveWindowSubclass(_hwnd, _proc, (UIntPtr)1);
            _hwnd = IntPtr.Zero;
            _proc = null;
            _onHotkey = null;
        }
    }

    private static Action? _onHotkey;

    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, IntPtr dwRefData)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
        {
            // Marshal back to UI thread
            if (_dispatcher != null && _onHotkey != null)
                _dispatcher.TryEnqueue(() => _onHotkey.Invoke());
            return IntPtr.Zero;
        }
        return DefSubclassProc(hWnd, msg, wParam, lParam);
    }
}
