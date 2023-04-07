using Client.Models;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Client.Commands;

namespace Client;
public partial class MainWindow : Window
{
    public ICommand GetCommand { get; set; }
    public ICommand PutCommand { get; set; }
    public ICommand PostCommand { get; set; }
    public ICommand ResetCommand { get; set; }
    private HttpClient _httpClient;

    public int Value
    {
        get { return (int)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(int), typeof(MainWindow));

    public string Key
    {
        get { return (string)GetValue(KeyProperty); }
        set { SetValue(KeyProperty, value); }
    }

    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.Register("Key", typeof(string), typeof(MainWindow));

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        _httpClient = new();

        ResetCommand = new RelayCommand(ExecuteResetCommand);
        PostCommand = new RelayCommand(ExecutePostCommand);
        GetCommand = new RelayCommand(ExecuteGetCommand);
        PutCommand = new RelayCommand(ExecutePutCommand);
    }

    private void ExecuteResetCommand(object? obj)
    {
        Key = string.Empty;
        Value = 0;
    }

    private async void ExecuteGetCommand(object? obj)
    {
        if (Key is not null)
        {
            var response = await _httpClient.GetAsync($"http://localhost:45678/?key={Key}");

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var keyValue = JsonSerializer.Deserialize<KeyValue>(content);
                Value = keyValue.Value;
                Key = string.Empty;
                Key += keyValue.Key;
            }
            else
                MessageBox.Show(response.StatusCode.ToString());
        }
    }

    private async void ExecutePostCommand(object? obj)
    {
        if (Key is null) return;
        var keyValue = new KeyValue()
        {
            Key = Key,
            Value = Value
        };
        var jsonStr = JsonSerializer.Serialize(keyValue);
        var content = new StringContent(jsonStr);
        var response = await _httpClient.PostAsync("http://localhost:45678/", content);

        if (response.StatusCode == HttpStatusCode.OK)
            MessageBox.Show("Posted succesfully done", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show("Posted error", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

        Key = string.Empty;
        Value = 0;
    }
    private async void ExecutePutCommand(object? obj)
    {
        if (Key is null) return;
        var keyValue = new KeyValue()
        {
            Key = Key,
            Value = Value
        };

        var jsonStr = JsonSerializer.Serialize(keyValue);
        var content = new StringContent(jsonStr);
        var response = await _httpClient.PutAsync("http://localhost:45678/", content);

        if (response.StatusCode == HttpStatusCode.OK)
            MessageBox.Show("Putted succesfully done", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show("Putted error", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

        Key = string.Empty;
        Value = 0;

    }
}
