using CSharpOsu;
using CSharpOsu.Module;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace TotalPpCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OsuClient osu = new OsuClient(Api.apikey);
        private OsuUserBest[] userScores;
        private OsuUser[] player;
        private List<float> userTopPps;
        private float totalWithBonus, bonusPp;
        float totalPp;

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
            => this.WindowState = WindowState.Minimized;

        private void OnGoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                userScores = osu.GetUserBest(userIdText.Text, 0, 100);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnRecalcClick(object sender, RoutedEventArgs e)
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

        private void OnAddScoreClick(object sender, RoutedEventArgs e)
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

        private float CalcBonusPp(float totalPp)
        {
            foreach (var user in player) totalWithBonus = user.pp_raw;

            return totalWithBonus - totalPp;
        }
    }
}