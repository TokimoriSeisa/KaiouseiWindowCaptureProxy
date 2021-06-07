using ColorPickerWPF;
using KaiouseiWindowCaptureProxy.Interop;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace KaiouseiWindowCaptureProxy
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string[] _ignoreProcesses = { "applicationframehost", "shellexperiencehost", "systemsettings", "winstore.app", "searchui" };
        private IntPtr _hThumbnail = IntPtr.Zero;
        private IntPtr _hWnd = IntPtr.Zero;
        private NativeMethods.WinEventDelegate procDelegate;

        public MainWindow()
        {
            InitializeComponent();
            HookReizeEvent();
        }

        private void HookReizeEvent()
        {
            procDelegate = new NativeMethods.WinEventDelegate(WinEventProc);
            IntPtr hhook1 = NativeMethods.SetWinEventHook(NativeMethods.EVENT_OBJECT_NAMECHANGE, NativeMethods.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, procDelegate, 0, 0, NativeMethods.WINEVENT_OUTOFCONTEXT);
            IntPtr hhook2 = NativeMethods.SetWinEventHook(NativeMethods.EVENT_SYSTEM_MOVESIZEEND, NativeMethods.EVENT_SYSTEM_MOVESIZEEND, IntPtr.Zero, procDelegate, 0, 0, NativeMethods.WINEVENT_OUTOFCONTEXT);
        }

        private void FindWindows()
        {
            this.SourceMenuItem.Items.Clear();

            var wih = new WindowInteropHelper(this);
            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                // ignore invisible windows
                if (!NativeMethods.IsWindowVisible(hWnd))
                    return true;

                // ignore untitled windows
                var title = new StringBuilder(1024);
                NativeMethods.GetWindowText(hWnd, title, title.Capacity);
                if (string.IsNullOrWhiteSpace(title.ToString()))
                    return true;

                // ignore me
                if (wih.Handle == hWnd)
                    return true;

                NativeMethods.GetWindowThreadProcessId(hWnd, out var processId);

                // ignore by process name
                var process = Process.GetProcessById((int)processId);
                if (_ignoreProcesses.Contains(process.ProcessName.ToLower()))
                    return true;

                MenuItem newMenuItem = new MenuItem
                {
                    Tag = hWnd,
                    Header = $"{title} ({process.ProcessName}.exe)"
                };
                newMenuItem.Click += SetProxyProcess;
                this.SourceMenuItem.Items.Add(newMenuItem);

                return true;
            }, IntPtr.Zero);
        }

        private void SetProxyProcess(object sender, RoutedEventArgs e)
        {
            MenuItem SelectedMenuItem = (MenuItem)sender;
            this._hWnd = (IntPtr)SelectedMenuItem.Tag;

            RegisterProxyProcess();
        }

        private void UnregisterProxyProcess()
        {
            if (this._hThumbnail != IntPtr.Zero)
            {
                NativeMethods.DwmUnregisterThumbnail(this._hThumbnail);
            }
        }

        private void RegisterProxyProcess()
        {
            HideNoSignalStackPanel();
            UnregisterProxyProcess();

            var hr = NativeMethods.DwmRegisterThumbnail(new WindowInteropHelper(this).Handle, this._hWnd, out _hThumbnail);
            if (hr != 0)
                return;

            ProxyProcess_Resize();
            UpdateThumbnailProperties();
        }

        private void ProxyProcess_Resize()
        {
            RECT originWindowRect = new RECT();
            if (NativeMethods.GetWindowRect(this._hWnd, out originWindowRect) > 0)
            {
                int width = (int)(originWindowRect.right - originWindowRect.left);
                int height = (int)(originWindowRect.bottom - originWindowRect.top);
                this.Width = width;
                this.Height = height;
            }
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if(this._hWnd == IntPtr.Zero)
            {
                return;
            }

            ProxyProcess_Resize();
            UpdateThumbnailProperties();
        }

        private void UpdateThumbnailProperties()
        {
            var dpi = GetDpiScaleFactor();
            var props = new DWM_THUMBNAIL_PROPERTIES
            {
                fVisible = true,
                dwFlags = (int)(DWM_TNP.DWM_TNP_VISIBLE | DWM_TNP.DWM_TNP_OPACITY | DWM_TNP.DWM_TNP_RECTDESTINATION | DWM_TNP.DWM_TNP_SOURCECLIENTAREAONLY),
                opacity = 255,
                rcDestination = new RECT { left = 0, top = 0, bottom = (int)(this.MainGrid.ActualHeight * dpi.Y), right = (int)(this.MainGrid.ActualWidth * dpi.X) },
                fSourceClientAreaOnly = true
            };

            NativeMethods.DwmUpdateThumbnailProperties(_hThumbnail, ref props);
        }

        private Point GetDpiScaleFactor()
        {
            var source = PresentationSource.FromVisual(this);
            return source?.CompositionTarget != null ? new Point(source.CompositionTarget.TransformToDevice.M11, source.CompositionTarget.TransformToDevice.M22) : new Point(1.0d, 1.0d);
        }

        private void SetBackgroupToGreen(object sender, RoutedEventArgs e)
        {
            this.MainGrid.Background = Brushes.Lime;
        }

        private void SetBackgroupToBlue(object sender, RoutedEventArgs e)
        {
            this.MainGrid.Background = Brushes.Blue;
        }

        private void SetBackgroupToMagenta(object sender, RoutedEventArgs e)
        {
            this.MainGrid.Background = Brushes.Magenta;
        }

        private void SetBackgroupToWhite(object sender, RoutedEventArgs e)
        {
            this.MainGrid.Background = Brushes.White;
        }

        private void SetBackgroupToBlack(object sender, RoutedEventArgs e)
        {
            this.MainGrid.Background = Brushes.Black;
        }


        private void SetBackgroupToCustom(object sender, RoutedEventArgs e)
        {
            Color SelectedColor;
            bool ok = ColorPickerWindow.ShowDialog(out SelectedColor);
            if (ok)
            {
                this.MainGrid.Background = new SolidColorBrush(SelectedColor);
            }

        }
        void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            FindWindows();
        }

        private void HideNoSignalStackPanel()
        {
            this.NoSignalStackPanel.Visibility = Visibility.Hidden;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
