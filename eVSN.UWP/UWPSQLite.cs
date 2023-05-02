using System;  
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;  
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using eLiDAR.Helpers;
using eLiDAR.UWP;
using Nito.AsyncEx;
using SQLite;  
using Windows.Storage;  
using Xamarin.Forms;

[assembly: Dependency(typeof(SqlConnection))]  
  
namespace eLiDAR.UWP
{
    class SqlConnection : ISQLite
    {
        private string path;
        string databaseName;
        public SQLiteConnection GetConnection(string databaseName)
        {

            string documentPath = ApplicationData.Current.LocalFolder.Path;
            path = Path.Combine(documentPath, databaseName);

            // Use this if I need to reset the database
            bool isDebug = false;
            if (isDebug)
            {
                File.Delete(path);
            }

            // Set the database name to the class member (used by CheckFile)
            this.databaseName = databaseName;
            var result = AsyncContext.Run(CheckFile);
            if (result)
            {
                return new SQLiteConnection(path);
            }
            else
            { 
                return null; 
            }
        }

        async Task<bool> CheckFile()
        {
            var storageFile = IsolatedStorageFile.GetUserStoreForApplication();
            if (!storageFile.FileExists(path))
            {
                // copy storage file; replace if exists.
                var fileToRead = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + DatabaseHelper.DbTemplateFileName, UriKind.Absolute));
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile fileCopy = await fileToRead.CopyAsync(storageFolder, databaseName, NameCollisionOption.ReplaceExisting);
            }

            return true;
        }
    }
    internal static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return AsyncHelper._myTaskFactory
              .StartNew<Task<TResult>>(func)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            AsyncHelper._myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
