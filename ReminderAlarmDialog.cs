using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComboBox = System.Windows.Controls.ComboBox;
using Button = System.Windows.Controls.Button;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace SmartSticker;

public sealed class ReminderAlarmDialog : Window
{
    public int? SnoozeMinutes { get; private set; }
    public bool Dismissed { get; private set; }
    public ReminderAlarmDialog(string preview, DateTime eventAt)
    {
        Title = "Smart Sticker 알림"; Width = 390; Height = 260; ResizeMode = ResizeMode.NoResize; WindowStartupLocation = WindowStartupLocation.CenterOwner; Topmost = true;
        var panel = new StackPanel { Margin = new Thickness(24) }; panel.Children.Add(new TextBlock { Text = "⏰ 이벤트 알림", FontSize = 22, FontWeight = FontWeights.SemiBold }); panel.Children.Add(new TextBlock { Text = eventAt.ToString("yyyy년 M월 d일 HH:mm"), Foreground = Brushes.DimGray, Margin = new Thickness(0, 6, 0, 14) }); panel.Children.Add(new TextBlock { Text = preview, TextWrapping = TextWrapping.Wrap, MaxHeight = 65 });
        var snooze = new ComboBox { Margin = new Thickness(0, 15, 0, 8), ItemsSource = new[] { new SnoozeOption("5분 후 다시 알림", 5), new SnoozeOption("10분 후 다시 알림", 10), new SnoozeOption("30분 후 다시 알림", 30), new SnoozeOption("1시간 후 다시 알림", 60) }, DisplayMemberPath = "Label", SelectedIndex = 1 }; panel.Children.Add(snooze);
        var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right }; var dismiss = new Button { Content = "알람 종료" }; dismiss.Click += (_, _) => { Dismissed = true; DialogResult = true; }; var again = new Button { Content = "다시 알림", Background = new SolidColorBrush(Color.FromRgb(76, 139, 245)), Foreground = Brushes.White }; again.Click += (_, _) => { if (snooze.SelectedItem is SnoozeOption option) SnoozeMinutes = option.Minutes; DialogResult = true; }; row.Children.Add(dismiss); row.Children.Add(again); panel.Children.Add(row); Content = panel;
    }
    private sealed record SnoozeOption(string Label, int Minutes);
}
