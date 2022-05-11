using CSharpOsu;
using CSharpOsu.Module;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TotalPpCalc
{
    public partial class MainWindow : Window
    {
        private readonly OsuClient osu = new OsuClient(Settings.Default.apiToken);
        private OsuUserBest[] userScores;
        private OsuUser[] player;
        private List<float> userTopPps;
        private float totalWithBonus, bonusPp, totalPp;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
            => Close();

        private void MinWindow(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private async void OnGoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                totalPpText.Text = "loading...";
                bonusPpText.Text = "loading...";
                userScores = await GetUserScores();
                if (userScores != null)
                {
                    userTopPps = new List<float>();

                    player = osu.GetUser(userIdText.Text);

                    foreach (var user in player) usernameText.Text = user.username;

                    foreach (var score in userScores)
                    {
                        userTopPps.Add(score.pp);
                    }

                    totalPp = 0;
                    for (int i = 0; i < 100; i++)
                    {
                        totalPp += (float)(userTopPps[i] * Math.Pow(0.95, i));
                    }

                    bonusPp = CalcBonusPp(totalPp);

                    totalPpText.Text = totalPp.ToString();
                    bonusPpText.Text = (totalPp + bonusPp).ToString();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                totalPpText.Text = "";
                bonusPpText.Text = "";
                usernameText.Text = "no user loaded";
                MessageBox.Show("No user found.");
            }
            catch (Exception ex)
            {
                totalPpText.Text = "";
                bonusPpText.Text = "";
                usernameText.Text = "no user loaded";
                MessageBox.Show(ex.Message);
            }
        }

        private void OnRecalcClick(object sender, RoutedEventArgs e)
        {
            if (usernameText.Text == "no user loaded") MessageBox.Show("Please load a valid user's scores before recalculating.");
            if (usernameText.Text == "loading scores...") MessageBox.Show("Please wait until all scores have finished loading.");
            if (usernameText.Text != "no user loaded" && usernameText.Text != "loading scores...")
            {
                userTopPps.Sort();
                userTopPps.Reverse();
                totalPp = 0;

                for (int i = 0; i < 100; i++)
                {
                    totalPp += (float)(userTopPps[i] * Math.Pow(0.95, i));
                }

                totalPpText.Text = totalPp.ToString();
                bonusPpText.Text = (totalPp + bonusPp).ToString();
            }
        }

        private void OnAddScoreClick(object sender, RoutedEventArgs e)
        {
            if (usernameText.Text == "no user loaded") MessageBox.Show("Please load a valid user's scores before adding any.");
            if (usernameText.Text == "loading scores...") MessageBox.Show("Please wait until all scores have finished loading.");
            if (usernameText.Text != "no user loaded" && usernameText.Text != "loading scores...")
            {
                try
                {
                    userTopPps.Add(Int32.Parse(newPpText.Text));
                    newPpText.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private float CalcBonusPp(float totalPp)
        {
            foreach (var user in player) totalWithBonus = user.pp_raw;

            return totalWithBonus - totalPp;
        }

        private async Task<OsuUserBest[]> GetUserScores()
        {
            if (userIdText.Text != "")
            {
                string userId = userIdText.Text;
                usernameText.Text = "loading scores...";
                return await Task.Run(() => osu.GetUserBest(userId, 0, 100));
            }
            totalPpText.Text = "";
            bonusPpText.Text = "";
            MessageBox.Show("Username field cannot be blank.");
            return null;
        }
    }
}
