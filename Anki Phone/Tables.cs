using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.Xml;

namespace Anki.Tables
{
    public class TableBase
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int ParentId { get; set; }
        public string Notes { get; set; }
        public virtual Task<TableBase> GetParentAsync() { return null; }
        public virtual Task<List<TableBase>> GetChildrensAsync()
        {
            return null;
        }
    }

    public class UnitDB : TableBase
    {
        [Unique]
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
        public async override Task<List<TableBase>> GetChildrensAsync()
        {
            List<TableBase> list = new List<TableBase>();
            foreach (LessonDB lesson in await DBHelper.GetItemsAsync<LessonDB>(ID))
            {
                list.Add(lesson);
            }
            return list;
        }
    }

    public class LessonDB : TableBase
    {
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
            public async override Task<TableBase> GetParentAsync()
            {
                return await DBHelper.GetItemAsync<UnitDB>(ParentId);
            }
        public async override Task<List<TableBase>> GetChildrensAsync()
        {
            List<TableBase> list = new List<TableBase>();
            foreach(TestDB test in await DBHelper.GetItemsAsync<TestDB>())
            {
                list.Add(test);
            }
            foreach (ItemDB item in await DBHelper.GetItemsAsync<ItemDB>())
            {
                list.Add(item);
            }
            return list;
        }
    }

    public class TestDB : TableBase
    {
        public TestType Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public override string ToString()
        {
            return Type.ToString() + ": " + From + " -> " + To;
        }
        public async override Task<TableBase> GetParentAsync()
        {
                return await DBHelper.GetItemAsync<LessonDB>(ParentId);
        } 
    }
    public enum ElementType
    {
        Unit,
        Lesson,
        Item,
        Test
    }
    public enum TestType {
        ReadingTest,
        WritingTest,
        ListeningTest
    }

    public class ItemDB: TableBase
    {
        public string Kanji { get; set; }
        public string OnReading { get; set; }
        public string KunReading { get; set; }
        public string Meaning { get; set; }   
        public byte Progress { get; set; }
        public override string ToString()
        {
            return "Kanli: " + Kanji + ", OnReading: " + OnReading + ", KunReading: " + KunReading + ", Meaning:" + Meaning;
        }
            public async override Task<TableBase> GetParentAsync()
            {
                return await DBHelper.GetItemAsync<LessonDB>(ParentId);
            }
    }
}

namespace Anki
{
    public class Lesson : Tables.LessonDB
    {
        public List<Tables.ItemDB> Items;
        public List<Tables.TestDB> Tests;
    }

    public abstract class Test
    {
        public int LessonId { get; set; }
        public List<Tables.ItemDB> Items { get; set; }
        public string From { get; set; }
        public string To { get; set; }

    }

    public class ReadingTest : Test
    {
        public override string ToString()
        {
            return "Reading Test: " + From + " -> " + To;
        }
    }
    public class WritingTest : Test
    {
        public override string ToString()
        {
            return "Writing Test: " + From + " -> " + To;
        }
    }
    public class ReadingTestQuestion
    {
        public int Right; //Правильный ответ
        public byte IndexOfRightButton;
        public byte ButtonAmount = 4;
        public int? LastQuestion = null;
    }
}
