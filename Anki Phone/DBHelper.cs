using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anki.Tables;
using SQLite;
using System.IO;

namespace Anki
{
    public interface IFileHelper
    {
        string GetLocalFilePath(string filename);
    }
    static class DBHelper
    {
        private static readonly SQLiteAsyncConnection connection = new SQLiteAsyncConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite"));
        public static void CreateTables()
        {         
            connection.CreateTableAsync<UnitDB>().Wait();
            connection.CreateTableAsync<LessonDB>().Wait();
            connection.CreateTableAsync<ItemDB>().Wait();
            connection.CreateTableAsync<TestDB>().Wait();
        }
        public static async Task<List<T>>  GetItemsAsync<T>() where T : new()
        {
            try {
                return await connection.Table<T>().ToListAsync();
        }
            catch (Exception) {
                CreateTables();
                return await connection.Table<T>().ToListAsync();
            }
        }

        public static async Task<List<T>> GetItemsAsync<T>(int ParentId) where T :  new()
        {
            List<T> elements = new List<T>();
                foreach (T element in await GetItemsAsync<T>())
                {
                    if ((element as TableBase).ParentId == ParentId)
                        elements.Add(element);
                }
            return elements;
        }

        public static Task<List<T>> GetItemsNotDoneAsync<T>() where T : new()
        {
            return connection.QueryAsync<T>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
        }

        public async static Task<T> GetItemAsync<T>(int id) where T : TableBase, new()
        {
            return await connection.Table<T>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }
        public static Task<int> SaveItemAsync(TableBase item)
        {
            if (item.ID != 0)
            {
                return connection.UpdateAsync(item);
            }
            else
            {
               return  connection.InsertAsync(item);
            }
        }

        public static Task<int> DeleteItemAsync(TableBase item)
        {
            return connection.DeleteAsync(item);
        }

        
    }
}
