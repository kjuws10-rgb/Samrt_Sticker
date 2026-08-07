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

public sealed class ReminderDialog : Window
{
    private readonly DatePicker _date;
    private readonly ComboBox _hour;
    private readonly ComboBox _minute;
    private readonly ComboBox _before;
    public DateTime? ReminderAt { get; private set; }
    public int MinutesBefore { get; private set; }
    public bool ClearRequested { get; private set; }

    public ReminderDialog(DateTime? current, int minutesBefore)
    {
        Title = "이벤트 알림 설정"; Width = 360; Height = 330; ResizeMode = ResizeMode.NoResize; WindowStartupLocation = WindowStartupLocation.CenterOwner; Background = Brushes.White;
        var selected = current ?? DateTime.Now.AddHours(1);
        var root = new Grid { Margin = new Thickness(22) }; root.RowDefinitions.Add(new RowDefinition()); root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var panel = new StackPanel(); root.Children.Add(panel);
        panel.Children.Add(new TextBlock { Text = "이벤트 일정", FontSize = 21, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 15) });
        panel.Children.Add(new TextBlock { Text = "날짜", Foreground = Brushes.DimGray });
        _date = new DatePicker { SelectedDate = selected.Date, Margin = new Thickness(0, 4, 0, 12) }; panel.Children.Add(_date);
        panel.Children.Add(new TextBlock { Text = "시간", Foreground = Brushes.DimGray });
        var timeRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 12) };
        _hour = new ComboBox { Width = 90, ItemsSource = Enumerable.Range(0, 24), SelectedItem = selected.Hour }; _minute = new ComboBox { Width = 90, Margin = new Thickness(8, 0, 0, 0), ItemsSource = Enumerable.Range(0, 60), SelectedItem = selected.Minute }; timeRow.Children.Add(_hour); timeRow.Children.Add(_minute); panel.Children.Add(timeRow);
        panel.Children.Add(new TextBlock { Text = "알림 시점", Foreground = Brushes.DimGray });
        _before = new ComboBox { Margin = new Thickness(0, 4, 0, 0), DisplayMemberPath = "Label", SelectedValuePath = "Minutes", ItemsSource = new[] { new ReminderOffset("정각", 0), new ReminderOffset("5분 전", 5), new ReminderOffset("10분 전", 10), new ReminderOffset("30분 전", 30), new ReminderOffset("1시간 전", 60), new ReminderOffset("2시간 전", 120), new ReminderOffset("1일 전", 1440) } }; _before.SelectedValue = minutesBefore; if (_before.SelectedIndex < 0) _before.SelectedIndex = 0; panel.Children.Add(_before);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 18, 0, 0) }; Grid.SetRow(buttons, 1); root.Children.Add(buttons);
        var clear = new Button { Content = "알림 삭제", Foreground = Brushes.Firebrick }; clear.Click += (_, _) => { ClearRequested = true; DialogResult = true; }; var cancel = new Button { Content = "취소" }; cancel.Click += (_, _) => Close(); var save = new Button { Content = "저장", Background = new SolidColorBrush(Color.FromRgb(76, 139, 245)), Foreground = Brushes.White }; save.Click += Save_Click; buttons.Children.Add(clear); buttons.Children.Add(cancel); buttons.Children.Add(save); Content = root;
    }
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_date.SelectedDate is null || _hour.SelectedItem is not int hour || _minute.SelectedItem is not int minute) return;
        ReminderAt = _date.SelectedDate.Value.Date.AddHours(hour).AddMinutes(minute); MinutesBefore = _before.SelectedValue is int value ? value : 0; DialogResult = true;
    }
    private sealed record ReminderOffset(string Label, int Minutes);
}
