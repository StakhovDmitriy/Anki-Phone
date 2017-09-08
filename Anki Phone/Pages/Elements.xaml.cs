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
using Anki.Tables;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Anki.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ElementsPage : Page
    {
        Type CurrentType = typeof(UnitDB);
        public ElementsPage()
        {
            this.InitializeComponent();

            //кнопка назад
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            onAppearingAsync();
        }

        async void onAppearingAsync()
        {
            List<UnitDB> units = await DBHelper.GetItemsAsync<UnitDB>();
            foreach(UnitDB unit in units)
            {
                list.Items.Add(new ListViewItem { ContextFlyout = this.Resources["ElementMenuFlyout"] as MenuFlyout, Content= unit });
            }            
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

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StudyPage));
        }

        private void Element_Info(object sender, RoutedEventArgs e)
        {
            
        }
        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ElementInfoPage), e.ClickedItem);  
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //AddElement
            Frame.Navigate(typeof(ElementInfoPage));
        }

        private async void Forward_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentType == typeof(UnitDB))
            {
                Back.Content = "Темы";
                TypeLabel.Text = "Уроки";
                Forward.Content = "Элементы";
                list.Visibility = Visibility.Collapsed;
                LessonList.Visibility = Visibility.Visible;
                LessonList.ItemsSource = await DBHelper.GetItemsAsync<LessonDB>();
                CurrentType = typeof(LessonDB);
            }
            else if (CurrentType == typeof(LessonDB))
            {
                Back.Content = "Уроки";
                TypeLabel.Text = "Элементы";
                Forward.Content = "Тесты";
                LessonList.Visibility = Visibility.Collapsed;
                ItemList.Visibility = Visibility.Visible;
                ItemList.ItemsSource = await DBHelper.GetItemsAsync<ItemDB>();
                CurrentType = typeof(ItemDB);
            }
            else if (CurrentType == typeof(ItemDB))
            {
                Back.Content = "Элементы";
                TypeLabel.Text = "Тесты";
                Forward.Content = "";
                ItemList.Visibility = Visibility.Collapsed;
                TestList.Visibility = Visibility.Visible;
                TestList.ItemsSource = await DBHelper.GetItemsAsync<TestDB>();
                CurrentType = typeof(TestDB);
            }
        }
    }
}
