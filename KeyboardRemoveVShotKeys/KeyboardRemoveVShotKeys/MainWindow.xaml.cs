using Ai2.WinApi;
using KeyboardRemoveVShotKeys.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;

namespace KeyboardRemoveVShotKeys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InputSimulator _inputSimulator;

        private int _interval = 5;
        public MainWindow()
        {
            InitializeComponent();

            _inputSimulator = new InputSimulator();
        }

        private void RefreshWndRect(WindowInfo wnd, out int x, out int y)
        {
            x = 0; y = 0;

            if (Win32.GetWindowRect(wnd.Handle, out var lpRect))
            {
                x = lpRect.Left; 
                y = lpRect.Top;
            }
        }

        private void ButtonClick(WindowInfo topWnd, WindowInfo comboboxWnd, WindowInfo button)
        {
            while (true)
            {
                Thread.Sleep(_interval);

                //检测主窗口是否还存在
                var isWndExists = Win32.IsWindow(topWnd.Handle);
                WriteLog($"窗口是否存在： {isWndExists}");
                if (!isWndExists)
                {
                    break;
                }

                //检测按钮的 状态是否为 可 点击
                var buttonStyle = (WindowStyles)Win32.GetWindowLong(button.Handle, (int)WindowLongFlags.GWL_STYLE); // GWL_STYLE -16 获得窗口样式
                WriteLog($"buttonStyle={buttonStyle}");
                if ((buttonStyle & WindowStyles.WS_DISABLED) == WindowStyles.WS_DISABLED) //不可用
                {
                    WriteLog($"按钮不可点击， 退出操作\r\n");
                    break;
                }

                RefreshWndRect(button, out var x, out var y);
                WriteLog($"按钮可点击， 执行操作. x= {x} \t y= {y}");
                Win32.SetCursorPos(x + button.Rect.Value.Size.Width / 2, y + 10);
                Thread.Sleep(_interval);
                NativeMethods.MouseClick();
                //NativeMethods.MouseClick(button.Handle, new POINT { x = x + 10, y = y + 10 });

                //var hotKeys = NativeMethods.GetWindowText(comboboxWnd.Handle);
                //if (!string.IsNullOrEmpty(hotKeys) && Regex.IsMatch(hotKeys, @"[A-Z]\s?,\s?[A-Z]"))
                //{
                //    MessageBox.Show("多键");
                //    NativeMethods.MouseClick(button.Rect.Value.Left + 10, button.Rect.Value.Top + 10);
                //}
            }
        }

        private void Process(WindowInfo topWnd, WindowInfo listboxWnd, WindowInfo comboboxWnd, WindowInfo buttuon, WindowInfo textboxWnd)
        {
            var loopCount = 10000;
            var fn = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l.dat");
            if (File.Exists(fn))
            {
                var lines = File.ReadLines(fn).ToArray();
                if (lines != null && lines.Length > 0)
                {
                    int.TryParse(lines[0], out loopCount);
                }
            }

            var index = 0;
            while (index < loopCount)
            {
                //检测主窗口是否还存在
                var isWndExists = Win32.IsWindow(topWnd.Handle);
                WriteLog($"窗口是否存在： {isWndExists}");
                if (!isWndExists)
                {
                    break;
                }

                Win32.BringWindowToTop(topWnd.Handle);
                Thread.Sleep(_interval);
                Win32.SetForegroundWindow(topWnd.Handle);
                
                Thread.Sleep(_interval);
                WriteLog($"设置输入焦点: {textboxWnd.Handle.ToString("x8")}");
                //设置 show commnads contains 输入框 焦点
                Win32.SetFocus(textboxWnd.Handle);
                Thread.Sleep(_interval);
                
                RefreshWndRect(textboxWnd, out var x, out var y);
                WriteLog($"点击一次. x= {x} \t y= {y}");
                //NativeMethods.MouseClick(textboxWnd.Handle, new POINT { x = x + 20, y = y + 20});
                Win32.SetCursorPos(x + textboxWnd.Rect.Value.Size.Width / 2, y + 10);
                Thread.Sleep(_interval);
                NativeMethods.MouseClick();

                //发送一个 tab 按键，移动焦点到 listbox
                Thread.Sleep(_interval);
                WriteLog($"发送Tab");
                _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.TAB);

                //发送下 向下箭头 按键，
                Thread.Sleep(_interval);
                WriteLog($"发送 ↓");
                _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.DOWN);

                ButtonClick(topWnd, comboboxWnd, buttuon);
                Thread.Sleep(_interval);

                index++;
            }
        }

        private void WriteLog(string log)
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            
            using (var fs = File.AppendText(logFile))
            {
                fs.WriteLine(log);
                fs.Flush();
            }
        }

        private void BtnStartClick(object sender, RoutedEventArgs e)
        {
            //Task.Run(() =>
            //{
            //    Thread.Sleep(5 * _interval);
            //    Win32.SetCursorPos(417, 269);


            //    for (int i = 0; i < 10; i++)
            //    {
            //        Thread.Sleep(_interval);
            //        _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.DOWN);
            //    }

            //});

            //return;

            Task.Run(() =>
            {
                //查找窗口
                var topWnds = NativeMethods.GetTopWindows();
                if (topWnds != null && topWnds.Count > 0)
                {
                    var dialogWnd = topWnds.SingleOrDefault(w => w.IsVisible && w.Caption == "Options" && w.ClassName == "#32770");
                    if (dialogWnd != null)
                    {
                        var subWnds = NativeMethods.GetChildrenWindows(dialogWnd);
                        //WriteLog($"subWnds.Count = {subWnds.Count}");

                        var subDialogWnd = subWnds.SingleOrDefault(w => w.IsVisible && string.IsNullOrEmpty(w.Caption) && w.ClassName == "#32770");
                        if (subDialogWnd == null )
                        {
                            MessageBox.Show("subDialogWnd -> 未能查找到目标窗口");
                        }
                        else
                        {
                            WindowInfo comboboxWnd = null;
                            WindowInfo buttonWnd = null;
                            WindowInfo listboxWnd = null;
                            WindowInfo textBoxWnd = null;

                            var controls = NativeMethods.GetChildrenWindows(subDialogWnd);

                            if (controls != null && controls.Count > 0)
                            {
                                //查找输入框
                                var editCtrls = controls.Where(w => w.ClassName == "Edit");
                                foreach (var item in editCtrls)
                                {
                                    if (textBoxWnd == null)
                                    {
                                        textBoxWnd = item;
                                    }
                                    else if (item.Rect.Value.Top < textBoxWnd.Rect.Value.Top)
                                    {
                                        textBoxWnd = item;
                                    }
                                }

                                //查找listbox
                                listboxWnd = controls.SingleOrDefault(w => w.ClassName == "ListBox");


                                //查找Combobox
                                var comboboxs = controls.Where(w => w.ClassName == "ComboBox").OrderBy(w => w.Rect.Value.Top);
                                comboboxWnd = controls[1];

                                //查找button
                                buttonWnd = controls.SingleOrDefault(w => w.ClassName == "Button" && w.Caption == "&Remove");

                            }

                            if (listboxWnd == null)
                            {
                                MessageBox.Show("listboxWnd, 未能查找到目标窗口");
                                return;
                            }

                            if (comboboxWnd == null)
                            {
                                MessageBox.Show("comboboxWnd, 未能查找到目标窗口");
                                return;
                            }

                            if (buttonWnd == null)
                            {
                                MessageBox.Show("buttonWnd, 未能查找到目标窗口");
                                return;
                            }

                            if (textBoxWnd == null)
                            {
                                MessageBox.Show("textBoxWnd, 未能查找到目标窗口");
                                return;
                            }

                            //MessageBox.Show($"listboxWnd=>{listboxWnd.Handle}\r\ncomboboxWnd=>{comboboxWnd.Handle}\r\buttonWnd=>{buttonWnd.Handle}");
                            WriteLog($"窗口句柄: topWnd={dialogWnd.Handle.ToString("x8")}\t listboxWnd={listboxWnd.Handle.ToString("x8")} \t comboboxWnd={comboboxWnd.Handle.ToString("x8")} \t textBoxWnd={textBoxWnd.Handle.ToString("x8")} \t buttonWnd={buttonWnd.Handle.ToString("x8")}");
                            Process(dialogWnd, listboxWnd, comboboxWnd, buttonWnd, textBoxWnd);

                        }
                    }
                    else
                    {
                        MessageBox.Show("dialogWnd -> 未能查找到目标窗口");
                    }
                }
                else
                {
                    MessageBox.Show("topWnds -> 未能查找到目标窗口");
                }
            });
        }
    }
}
