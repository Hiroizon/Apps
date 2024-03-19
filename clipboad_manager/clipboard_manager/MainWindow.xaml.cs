using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace clipboard_manager
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;
        private GlobalHotKey _hotkey;
        private ObservableCollection<ClipboardHistoryItem> _clipboardHistory;
        private Thread _monitorThread;

        public MainWindow()
        {
            InitializeComponent();
            _clipboardHistory = new ObservableCollection<ClipboardHistoryItem>();
            ClipboardHistoryListView.ItemsSource = _clipboardHistory;

            LoadData();

            SetupTrayIcon();
            SetupHotkey();
            StartClipboardMonitor();
        }

        private void LoadData()
        {
            if (System.IO.File.Exists("data.json"))
            {
                var json = System.IO.File.ReadAllText("data.json");
                var data = System.Text.Json.JsonSerializer.Deserialize<List<ClipboardHistoryItem>>(json);
                foreach (var item in data)
                {
                    _clipboardHistory.Add(item);
                }
            }
        }

        private void SetupTrayIcon()
        {
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("Open", null, OnTrayMenuOpen);
            _trayMenu.Items.Add("Exit", null, OnTrayMenuExit);

            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon("Resources/app.ico"),
                Text = "Clipboard Manager",
                Visible = true,
                ContextMenuStrip = _trayMenu
            };
            _trayIcon.DoubleClick += OnTrayIconDoubleClick;
        }

        private void StartClipboardMonitor()
        {
            _monitorThread = new System.Threading.Thread(() =>
            {
                string previousText = "";
                while (true)
                {
                    System.Threading.Thread.Sleep(500);
                    if (System.Windows.Clipboard.ContainsText())
                    {
                        string currentText = System.Windows.Clipboard.GetText();
                        if (currentText != previousText)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                _clipboardHistory.Insert(0, new ClipboardHistoryItem { Text = currentText, IsPinned = false });
                                RefreshList();
                            });
                            previousText = currentText;
                        }
                    }
                }
            });
            _monitorThread.IsBackground = true;
            _monitorThread.SetApartmentState(System.Threading.ApartmentState.STA);
            _monitorThread.Start();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ShowItemMenu(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            var item = button.DataContext as ClipboardHistoryItem;

            var contextMenu = new ContextMenuStrip();
            var pinMenuItem = new ToolStripMenuItem { Text = item.IsPinned ? "Unpin" : "Pin" };
            pinMenuItem.Click += (s, args) => TogglePinStatus(item);
            contextMenu.Items.Add(pinMenuItem);

            var deleteMenuItem = new ToolStripMenuItem { Text = "Delete" };
            deleteMenuItem.Click += (s, args) => DeleteItem(item);
            contextMenu.Items.Add(deleteMenuItem);

            contextMenu.Show(System.Windows.Forms.Cursor.Position);
        }

        private void TogglePinStatus(ClipboardHistoryItem item)
        {
            item.IsPinned = !item.IsPinned;
            RefreshList();
        }

        private void RefreshList()
        {
            int pinnedCount = 0;
            int unpinnedCount = 0;

            foreach (var item in _clipboardHistory)
            {
                if (item.IsPinned)
                {
                    pinnedCount++;
                }
                else
                {
                    unpinnedCount++;
                }
            }

            var newList = new ClipboardHistoryItem[pinnedCount + unpinnedCount];
            int pinnedIndex = 0;
            int unpinnedIndex = pinnedCount;

            foreach (var item in _clipboardHistory)
            {
                if (item.IsPinned)
                {
                    newList[pinnedIndex++] = item;
                }
                else
                {
                    newList[unpinnedIndex++] = item;
                }
            }

            _clipboardHistory.Clear();
            foreach (var item in newList)
            {
                _clipboardHistory.Add(item);
            }
        }

        private void DeleteItem(ClipboardHistoryItem item)
        {
            _clipboardHistory.Remove(item);
        }

        private void CopySelectedText(object sender, System.Windows.RoutedEventArgs e)
        {
            string selectedText = string.Join(Environment.NewLine, ClipboardHistoryListView.SelectedItems.Cast<ClipboardHistoryItem>().Select(i => i.Text));
            System.Windows.Clipboard.SetText(selectedText);
        }
        private void SetupHotkey()
        {
            _hotkey = new GlobalHotKey(ModifierKeys.Control | ModifierKeys.Shift, Key.O, this);
            _hotkey.HotkeyPressed += OnHotkeyPressed;
            _hotkey.RegisterHotKey(); // ホットキーを登録
        }

        private void OnTrayMenuOpen(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void OnTrayMenuExit(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void OnHotkeyPressed(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }

            base.OnStateChanged(e);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData();
            _hotkey.UnregisterHotKey(); // ホットキーの登録を解除
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void SaveData()
        {
            var data = _clipboardHistory.Select(item => new { item.Text, item.IsPinned }).ToList();
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText("data.json", json);
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // マウスの左ボタンでクリックされた場合にのみ、ドラッグ操作を開始
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }

    public class ClipboardHistoryItem
    {
        public string Text { get; set; }
        public bool IsPinned { get; set; }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}