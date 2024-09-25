using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HTTP_Task_Manager_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient _httpClient;
        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:27001/") };
        }

        private async void GetProcesses_Click(object sender, RoutedEventArgs e)
        {
            var response = await _httpClient.GetAsync("/");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var processes = JsonSerializer.Deserialize<List<string>>(jsonResponse);
                ProcessList.ItemsSource = processes;
            }
        }

        private async void RunTask_Click(object sender, RoutedEventArgs e)
        {
            var taskName = NewTaskTextBox.Text;
            if (!string.IsNullOrWhiteSpace(taskName))
            {
                var content = new StringContent(taskName, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/", content);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Task started successfully!");
                }
            }
        }

        private async void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            var selectedProcess = (string)ProcessList.SelectedItem;
            if (selectedProcess != null)
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, "/");
                request.Content = new StringContent(JsonSerializer.Serialize(selectedProcess), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Task killed successfully!");
                }
                else
                {
                    MessageBox.Show("Failed to kill the process.");
                }
            }
        }

    }
}