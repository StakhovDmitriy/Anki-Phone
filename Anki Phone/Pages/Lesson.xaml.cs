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
    public sealed partial class LessonPage : Page
    {
        private Lesson CurrentLesson;

        public LessonPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //инициализация основных параметров
            LessonDB CurrentLessonDB = e.Parameter as LessonDB;
            CurrentLesson = new Anki.Lesson();
            CurrentLesson.ID = CurrentLessonDB.ID;
            CurrentLesson.Name = CurrentLessonDB.Name;
            CurrentLesson.Notes = CurrentLessonDB.Notes;
            CurrentLesson.ParentId = CurrentLessonDB.ParentId;
            if (CurrentLesson == null) new Exception();
            CurrentLesson.Items = await DBHelper.GetItemsAsync<ItemDB>(CurrentLesson.ID);
            CurrentLesson.Tests = await DBHelper.GetItemsAsync<TestDB>(CurrentLesson.ID);
            //инициализация элементов
            foreach (ItemDB Item in CurrentLesson.Items)
            {
                VariableSizedWrapGrid stackPanel = new VariableSizedWrapGrid { Orientation = Orientation.Vertical };
                TextBlock tb = new TextBlock { Text = Item.Kanji };
                
                stackPanel.Children.Add(tb);
                TestesPanel.Children.Add(new StackPanel { Orientation = Orientation.Vertical });
            }
            //инициализация кнопок тестов
            foreach (TestDB test in CurrentLesson.Tests)
            {
                Button btn = new Button();
                btn.Click += ButtonClick;
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                btn.Height = 150;
                switch (test.Type)
                {
                    case TestType.ReadingTest:
                        btn.Content = new ReadingTest { LessonId = CurrentLesson.ID, Items = CurrentLesson.Items, From = test.From, To = test.To};
                        break;
                    case TestType.WritingTest:
                        btn.Content = new WritingTest { LessonId = CurrentLesson.ID, Items = CurrentLesson.Items, From = test.From, To = test.To};
                        break;
                    case TestType.ListeningTest:
                        break;
                }
                TestesPanel.Children.Add(btn);
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TestPage), (sender as Button).Content);
        }
    }
}
