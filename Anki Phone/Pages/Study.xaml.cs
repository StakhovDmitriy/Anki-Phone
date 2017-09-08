using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;
using System.Threading;
using Anki.Tables;
// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Anki.Pages
{


    //Here is changed
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class StudyPage : Page
    {
        Windows.Storage.ApplicationDataContainer localSettings =
   Windows.Storage.ApplicationData.Current.LocalSettings;
        public StudyPage()
        {
            this.InitializeComponent();
            //кнопка назад
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
           NavView_Loaded();
        }

        private void App_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame.CanGoBack)
            {
                frame.GoBack(); // переход назад
                e.Handled = true; // указываем, что событие обработано
            }
        }
        private async void NavView_Loaded()
        {
            Units.Items.Clear();
            Lessons.Items.Clear();
            //Список курсов
                List<UnitDB> units = await DBHelper.GetItemsAsync<UnitDB>();
                if (units.Count == 0)
                {
                    Units.Items.Add(new TextBlock { Text = "Нет курсов", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
                }

                foreach (UnitDB unit in units)
                {
                    Units.Items.Add(new ListViewItem() { Content = unit, Tag = unit.ID, ContextFlyout = this.Resources["ElementMenuFlyout"] as MenuFlyout });
                }

            //Список уроков
            if (localSettings.Values["lastUnit"] != null)
            {
                //StudyNavigationView.SelectedItem = StudyNavigationView.MenuItems[(int)localSettings.Values["lastUnit"]-1];   //Не работает, но так как не принципиально, можно пока забить
                List<LessonDB> lessons = await DBHelper.GetItemsAsync<LessonDB>((int)localSettings.Values["lastUnit"]);
                if(lessons.Count == 0)
                {
                    NoLessonstLabel.Visibility = Visibility.Visible;
                }
                else NoLessonstLabel.Visibility = Visibility.Collapsed;
                foreach (LessonDB lesson in lessons)
                {
                   Lessons.Items.Add(new ListViewItem {Content = lesson, ContextFlyout = this.Resources["ElementMenuFlyout"] as MenuFlyout });
                }
            }
            else
            {
                ChooseUnitLabel.Visibility = Visibility.Visible;
            }       
        }

        private async void Unit_Click(object sender, ItemClickEventArgs args)
        {
                Lessons.Items.Clear();
                foreach (LessonDB lesson in await DBHelper.GetItemsAsync<LessonDB>())
                {
                    if (lesson.ParentId == ((UnitDB)args.ClickedItem).ID)
                    {
                        Lessons.Items.Add(lesson);
                    }
                }
                localSettings.Values["lastUnit"] = ((UnitDB)args.ClickedItem).ID;
            NavigationSplitView.IsPaneOpen = false;
        }

        private void Lesson_Click(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(LessonPage), e.ClickedItem);
        }

        
        private void ImportFromXMLFile(object sender, RoutedEventArgs e)
        {
            ImportXML.Import();
            NavView_Loaded();
        }
        private async void DownloadFromNetwork(object sender, RoutedEventArgs e)
        {
            /*StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (folder != null)
            {
                StorageFile file = await folder.CreateFileAsync("NewFile.jpg", CreationCollisionOption.GenerateUniqueName);
                Uri durl = new Uri(linkBox.Text.ToString());
                BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
                DownloadOperation downloadOperation = backgroundDownloader.CreateDownload(durl, file);
                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                try
                {
                    await downloadOperation.StartAsync().AsTask(cancellationToken.Token);
                }
                catch { }
                linkBox.Text = file.Path.ToString();
            }
            */
        }
        private void Edit_Elements(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ElementsPage));
        }
        
        private void AddNewElement(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ElementInfoPage));
        }
        //Далее будет реализовано через нативную поддержку NavView
        private void HamburgerMenuReverse(object sender, RoutedEventArgs e)
        {
            NavigationSplitView.IsPaneOpen = !NavigationSplitView.IsPaneOpen;
        }
        //Контекстное меню
        private void Edit(object sender, RoutedEventArgs e)                                               //????????????
        {
            // Frame.Navigate(typeof(ElementInfo), (e.OriginalSource as MenuFlyoutItem).DataContext);
            // MenuFlyoutItem mfi = sender as MenuFlyoutItem;
            // MenuFlyout MenuFlyout = mfi.Parent as MenuFlyout;  
        }
    }
}
