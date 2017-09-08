using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Anki.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestPage : Page
    {
        Test CurrentTest;
        ReadingTestQuestion CurrentQuestion = new ReadingTestQuestion(); 
        DispatcherTimer SessionStopwatch;
        int time = 0;
        public TestPage()
        {
            this.InitializeComponent();           
            this.SizeChanged += MainPage_SizeChanged;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
            SessionStopwatch = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 100) };
            SessionStopwatch.Tick += Timer_Tick;
            SessionStopwatch.Start();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CurrentTest = e.Parameter as Test;
            if (CurrentTest is ReadingTest)
                ReadingModeGrid.Visibility = Visibility.Visible;
            else WritingModeGrid.Visibility = Visibility.Visible;
                InitGame();
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualHeight >= this.ActualWidth)
            {
                AnswerButtonsGrid.Width = this.ActualWidth;
                AnswerButtonsGrid.Height = AnswerButtonsGrid.Width / 2;
            }
            else
            {
                AnswerButtonsGrid.Height = this.ActualHeight / 2;
                AnswerButtonsGrid.Width = AnswerButtonsGrid.Height * 2;
            }
        }

        private async void Close_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Завершить тест?",
                Content = "Все ваши данные будут сохранены.",
                PrimaryButtonText = "Да",
                CloseButtonText = "Нет"
            };
            SessionStopwatch.Stop();
            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                Frame.Navigate(typeof(StudyPage));
            }
            else
            {
                SessionStopwatch.Start();
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }      
        }

        public void InitGame()
        {
            if (CurrentTest.Items.Count < 4)
            {
                Frame.Navigate(typeof(StudyPage));
                return;
            }

            
            List<byte> WrongList = new List<byte>();
            Random rnd = new Random();
            if (CurrentQuestion.LastQuestion == null)
                CurrentQuestion.Right = rnd.Next(CurrentTest.Items.Count);
            else do
                    CurrentQuestion.Right = rnd.Next(CurrentTest.Items.Count);
                while (CurrentQuestion.LastQuestion == CurrentQuestion.Right);
            CurrentQuestion.LastQuestion = CurrentQuestion.Right;
            CurrentQuestion.IndexOfRightButton = (byte)rnd.Next(CurrentQuestion.ButtonAmount);
            byte Wrong = 0;
            bool found; // найден ли следующий неправильный элемент
            for (int j = 0; j < CurrentQuestion.ButtonAmount - 1; j++)
            {
                found = false;
                while (!found)
                {
                    found = true;
                    Wrong = (byte)rnd.Next(CurrentTest.Items.Count);
                    foreach(byte WrongListItem in WrongList)
                    {
                        if (Wrong == WrongListItem) {
                            found = false;
                            break;
                        }
                    }
                    if (Wrong == CurrentQuestion.Right) found = false;
                }
                WrongList.Add(Wrong);
            }
            AnswerButtonsGrid.RowDefinitions.Clear();
            for (int i = 0; i < Math.Ceiling((double)CurrentQuestion.ButtonAmount / 2); i++)
            {
                AnswerButtonsGrid.RowDefinitions.Add(new RowDefinition());
            }

            int k = 0;
            AnswerButtonsGrid.Children.Clear();
            for (int i = 0; i < CurrentQuestion.ButtonAmount; i++)
            {
                //Стиль и функционал
                Button btn = new Button();
                btn.Click += AnswerButton_Click;
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                btn.VerticalAlignment = VerticalAlignment.Stretch;
                btn.Margin = new Thickness(5);
                btn.Tag = i;
                Grid.SetRow(btn, i / 2);
                Grid.SetColumn(btn, i % 2);

                //Контент
                ReadingQuestionLabel.Text = CurrentTest.Items[CurrentQuestion.Right].Kanji;
                if (i == CurrentQuestion.IndexOfRightButton)
                    btn.Content = CurrentTest.Items[CurrentQuestion.Right].Meaning;
                else
                {
                    btn.Content = CurrentTest.Items[WrongList[k]].Meaning;
                    k++;
                }
                AnswerButtonsGrid.Children.Add(btn);
            }
        }

        private async void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            byte ButtonN = Convert.ToByte(((Button)sender).Tag);
            if (CurrentQuestion.IndexOfRightButton == ButtonN)
            {
                ((Button)sender).Template = (ControlTemplate)Resources["ButtonStyleGreen"];
                await System.Threading.Tasks.Task.Delay(50);
                progressBar.Value++;
                if (progressBar.Value != progressBar.Maximum)
                    InitGame();
                else
                {
                    SessionStopwatch.Stop();
                    Results.Visibility = Visibility.Visible;
                    ResultTimeTextBlock.Text = Convert.ToString(time / 10) + "," + Convert.ToString(time % 10);
                }
            }
            else
            {
                ((Button)sender).Template = (ControlTemplate)Resources["ButtonStyleRed"];
                await System.Threading.Tasks.Task.Delay(50);
                ((Button)sender).ClearValue(TemplateProperty);
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            time += 1;
            TimeTextBlock.Text = Convert.ToString(time / 10) + "," + Convert.ToString(time % 10);
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StudyPage));
        }
    }
}
