using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace GitHubRepoManager
{
    public partial class MainWindow : Window
    {
        private string username;
        private string accessToken;
        private readonly HttpClient client;

        public MainWindow()
        {
            InitializeComponent();
            client = new HttpClient();
        }

        private void btnAuthenticate_Click(object sender, RoutedEventArgs e)
        {
            // Pobranie loginu i tokenu od użytkownika
            username = txtUsername.Text;
            accessToken = txtAccessToken.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(accessToken))
            {
                MessageBox.Show("Please enter both username and access token.");
                return;
            }

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            MessageBox.Show("Authenticated successfully.");
            GetRepositories();
        }

        private async Task GetRepositories()
        {
            // Pobranie listy repozytoriów
            string url = $"https://api.github.com/user/repos";
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("C#App");

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var repositories = JsonSerializer.Deserialize<Repo[]>(responseData);

                lstRepositories.Items.Clear();
                foreach (var repo in repositories)
                {
                    lstRepositories.Items.Add(repo.name);
                }
            }
            else
            {
                MessageBox.Show("Failed to retrieve repositories.");
            }
        }

        private async void btnAddRepo_Click(object sender, RoutedEventArgs e)
        {
            // Dodanie nowego repozytorium
            var repoName = txtNewRepoName.Text;
            if (string.IsNullOrEmpty(repoName))
            {
                MessageBox.Show("Please enter repository name.");
                return;
            }

            var newRepo = new { name = repoName, auto_init = true };
            var content = new StringContent(JsonSerializer.Serialize(newRepo), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.github.com/user/repos", content);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Repository '{repoName}' created successfully.");
                await GetRepositories(); // Odswieżenie listy repozytoriów
            }
            else
            {
                MessageBox.Show("Failed to create repository.");
            }
        }

        private async void btnDeleteRepo_Click(object sender, RoutedEventArgs e)
        {
            // Usuwanie wybranego repozytorium
            var selectedRepo = lstRepositories.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedRepo))
            {
                MessageBox.Show("Please select a repository to delete.");
                return;
            }

            var url = $"https://api.github.com/repos/{username}/{selectedRepo}";
            var response = await client.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Repository '{selectedRepo}' deleted successfully.");
                await GetRepositories(); // Odswieżenie listy repozytoriów
            }
            else
            {
                MessageBox.Show("Failed to delete repository.");
            }
        }
    }

    public class Repo
    {
        public string name { get; set; }
    }
}
