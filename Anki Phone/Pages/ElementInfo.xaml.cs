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
using System.Threading.Tasks;
using Anki.Tables;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Anki.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ElementInfoPage : Page
    {
        private bool isNew = true; 
        private TableBase Table;
        TextBox UnitName = new TextBox();
        TextBox LessonName = new TextBox();
        TextBox Notes = new TextBox();
        TextBox Kanji = new TextBox();
        TextBox OnReading = new TextBox();
        TextBox KunReading = new TextBox();
        //TextBox Reading = new TextBox();
        TextBox Meaning = new TextBox();
        ComboBox UnitNameCB = new ComboBox {HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
        ComboBox LessonNameCB = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
        ComboBox TestTypeCB = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch};
        ComboBox FromCB = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
        ComboBox ToCB = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
        public ElementInfoPage()
        {
            this.InitializeComponent();
            ClassSelection.ItemsSource = Enum.GetValues(typeof(ElementType)).Cast<ElementType>().ToList();
            TestTypeCB.ItemsSource = Enum.GetValues(typeof(TestType)).Cast<TestType>().ToList();
            List<String> items = new List<String>{ "kanji", "kunreading", "onreading", "meaning" };
            FromCB.ItemsSource = items;
            ToCB.ItemsSource = items;
            UnitNameCB.SelectionChanged += UnitChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) ChooseTypeGrid.Visibility = Visibility.Visible;
            else {
                isNew = false;
                Table = e.Parameter as TableBase;
                InitUIElements();
            }
        }

        private void Choose(object sender, RoutedEventArgs e)
        {
            ChooseTypeGrid.Visibility = Visibility.Collapsed;
            switch (ClassSelection.SelectedItem.ToString())
            {
                case "Unit":
                    Table = new UnitDB();
                    InitNewUnitUIElements();
                    break;
                case "Lesson":
                    Table = new LessonDB();
                    InitNewLessonUIElements();
                    break;
                case "Item":
                    Table = new ItemDB();
                    InitNewItemUIElements();
                    break;
                case "Test":
                    Table = new TestDB();
                    InitNewTestUIElements();
                    break;
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            async void ShowDialog()
            {
                    ContentDialog Dialog = new ContentDialog
                    {
                        Title = "Не введено обязательное данное",
                        Content = "Введите все нужные данные",
                        CloseButtonText = "Ок"
                    };
                   await Dialog.ShowAsync(); 
            }
            
            if (Notes.Text != String.Empty)
                Table.Notes = Notes.Text;
            if (Table is UnitDB)
            {
                (Table as UnitDB).Name = UnitName.Text;   
            }
            if (Table is LessonDB)
            {
                if (UnitNameCB.SelectedItem == null || LessonName.Text == String.Empty)
                { ShowDialog(); return; }
                (Table as LessonDB).Name = LessonName.Text;
                (Table as LessonDB).ParentId = (UnitNameCB.SelectedItem as UnitDB).ID;
            }
            if (Table is ItemDB)
            {
                if (UnitNameCB.SelectedItem == null || LessonNameCB.SelectedItem == null)
                { ShowDialog(); return; }
                (Table as ItemDB).Kanji = Kanji.Text;
                (Table as ItemDB).Meaning = Meaning.Text;
                (Table as ItemDB).ParentId = (LessonNameCB.SelectedItem as LessonDB).ID;
            }
            if (Table is TestDB)
            {
                if (UnitNameCB.SelectedItem == null || LessonNameCB.SelectedItem == null || TestTypeCB.SelectedItem == null)
                { ShowDialog(); return; }
                List<TestType> TestTypeList = Enum.GetValues(typeof(TestType)).Cast<TestType>().ToList();
                foreach(TestType testType in TestTypeList)
                    if ((TestType)TestTypeCB.SelectedItem == testType)
                        (Table as TestDB).Type = testType;
                (Table as TestDB).From = FromCB.SelectedItem.ToString().ToLower();
                (Table as TestDB).To = ToCB.SelectedItem.ToString().ToLower();
                (Table as TestDB).ParentId = (LessonNameCB.SelectedItem as LessonDB).ID;
            }
            DBHelper.SaveItemAsync(Table);
            Frame.Navigate(typeof(StudyPage));
        }
        private void Delete(object sender, RoutedEventArgs e)
        {
            DBHelper.DeleteItemAsync(Table);
            Frame.Navigate(typeof(StudyPage));
        }
        /// ///////////////////////////////////////////////////////////////////////////////////////////
        private async void UnitChanged(object sender, SelectionChangedEventArgs args)
        {
            LessonNameCB.ItemsSource = await DBHelper.GetItemsAsync<LessonDB>((UnitNameCB.SelectedItem as UnitDB).ID);
        }
        /// ///////////////////////////////////////////////////////////////////////////////////////////
        private void InitNewUnitUIElements()
        {
            AddUnitUIElements();
        }
        private async void InitNewLessonUIElements()
        {
            await SetBoxItemsAsync<UnitDB>(UnitNameCB);
            AddLessonUIElements();
        }
        private async void InitNewItemUIElements()
        {
            await SetBoxItemsAsync<UnitDB>(UnitNameCB);
            AddItemUIElements();
        }
        private async void InitNewTestUIElements()
        {
            await SetBoxItemsAsync<UnitDB>(UnitNameCB); 
           AddTestUIElements();
        }
        /// /////////////////////////////////////////////////////////////////////////////////////////
        private void InitUIElements()
        {
            if (Table is UnitDB) InitUnitUIElements();
            else if (Table is LessonDB) InitLessonUIElements();
            else if (Table is ItemDB) InitItemUIElements();
            else if (Table is TestDB) { InitTestUIElements(); AddTestUIElements(); }
            if (Table.Notes != null)
                Notes.Text = Table.Notes;
        }
        /// /////////////////////////////////////////////////////////////////////////////////////////
        private void InitUnitUIElements()
        {
            UnitName.Text = (Table as UnitDB).Name;
            AddUnitUIElements();
        }
        private async void InitLessonUIElements()
        {
            await SetBoxItemsAsync<UnitDB>(UnitNameCB);
            System.Diagnostics.Debug.WriteLine(UnitNameCB.Items.Count);
            await SetSelectionAsync(UnitNameCB);
            LessonName.Text = (Table as LessonDB).Name;
            AddLessonUIElements();
        }
        private async void InitItemUIElements()
        {
            SetBoxItemsAsync<UnitDB>(UnitNameCB);
            SetSelectionAsync(UnitNameCB);
            SetBoxItemsAsync<LessonDB>(LessonNameCB);
            SetSelectionAsync(LessonNameCB);
            Kanji.Text = (Table as ItemDB).Kanji;
            OnReading.Text = (Table as ItemDB).OnReading;
            KunReading.Text = (Table as ItemDB).KunReading;
            Meaning.Text = (Table as ItemDB).Meaning;
            AddItemUIElements();
        }
        private async void InitTestUIElements()
        {
            await SetBoxItemsAsync<UnitDB>(UnitNameCB);
            await SetSelectionAsync(UnitNameCB);
            await SetBoxItemsAsync<LessonDB>(LessonNameCB);
            await SetSelectionAsync(LessonNameCB);
            TestTypeCB.SelectedItem = (Table as TestDB).Type;
            FromCB.SelectedItem = (Table as TestDB).From;
            ToCB.SelectedItem = (Table as TestDB).To;
            
        }
        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void AddUnitUIElements()
        {
            AddToTableOfComonents(UnitName, "Название темы");
            AddToTableOfComonents(Notes, "Комментарий");
        }
        private void AddLessonUIElements()
        {
            AddToTableOfComonents(UnitNameCB, "Название темы");
            AddToTableOfComonents(LessonName, "Название урока");
            AddToTableOfComonents(Notes, "Комментарий");
        }
        private void AddItemUIElements()
        {
            AddToTableOfComonents(UnitNameCB, "Название темы");
            AddToTableOfComonents(LessonNameCB, "Название урока");
            AddToTableOfComonents(Kanji, "Кандзи");
            AddToTableOfComonents(OnReading, "Онное чтение");
            AddToTableOfComonents(KunReading, "Кунное чтение");
            AddToTableOfComonents(Meaning, "Значение");
            AddToTableOfComonents(Notes, "Комментарий");
        }
    private void AddTestUIElements()
        {
            AddToTableOfComonents(UnitNameCB, "Название темы");
            AddToTableOfComonents(LessonNameCB, "Название урока");
            AddToTableOfComonents(TestTypeCB, "Тип теста");
            AddToTableOfComonents(FromCB, "Из");
            AddToTableOfComonents(ToCB, "В");
            AddToTableOfComonents(Notes, "Комментарий");
        }
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void AddToTableOfComonents(FrameworkElement Box, String TextBlockText)
        {
            int Row = TableOfComponents.RowDefinitions.Count;
            TableOfComponents.RowDefinitions.Add(new RowDefinition());
            TextBlock tb = new TextBlock { Text = TextBlockText, VerticalAlignment = VerticalAlignment.Center };
            TableOfComponents.Children.Add(tb);
            Grid.SetRow(tb, Row);
            Grid.SetColumn(tb, 0);
            TableOfComponents.Children.Add(Box);
            Grid.SetRow(Box, Row);
            Grid.SetColumn(Box, 1);
        }
        private async Task SetBoxItemsAsync<T>(Selector Box) where T : TableBase, new()
        {
            if (new T() is LessonDB)
                Box.ItemsSource = (UnitNameCB.SelectedItem as UnitDB).GetChildrensAsync();
            else Box.ItemsSource = await DBHelper.GetItemsAsync<T>();
        }
        private async Task SetSelectionAsync(Selector Box)
        {
            if(Box == UnitNameCB && Table is LessonDB)
            {
                UnitDB unit = await Table.GetParentAsync() as UnitDB;
                Box.SelectedItem = unit;
            }

            if (Box == UnitNameCB && (Table is ItemDB || Table is TestDB))
            {
                LessonDB lesson = await Table.GetParentAsync() as LessonDB;
                Box.SelectedItem = await lesson.GetParentAsync() as UnitDB;
            }

            if (Box == LessonNameCB && (Table is ItemDB || Table is TestDB))
            {
                Box.SelectedItem = await Table.GetParentAsync() as LessonDB;
            }
        }
    }
}
