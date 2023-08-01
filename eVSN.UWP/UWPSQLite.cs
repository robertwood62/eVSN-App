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
using Windows.ApplicationModel.DataTransfer;
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
        private bool isOperationCompleted;
        private string destFilePath;

        public bool Export(string databaseName)
        {
            try
            {
                string documentsPath = ApplicationData.Current.LocalFolder.Path;
                string dbPath = Path.Combine(documentsPath, databaseName);

                var fileCopyName = $"Database_{DateTime.Now:dd-MM-yyyy_HH-mm-ss-tt}.db";
                destFilePath = Path.Combine(documentsPath, fileCopyName);

                // Copy the file to the destination path
                File.Copy(dbPath, destFilePath);

                // Share the file using the Windows Community Toolkit
                var fileToShare = StorageFile.GetFileFromPathAsync(destFilePath).AsTask().Result;
                if (fileToShare != null)
                {
                    var dataTransferManager = DataTransferManager.GetForCurrentView();
                    dataTransferManager.DataRequested += OnDataRequested;

                    DataTransferManager.ShowShareUI();

                    // Wait for the sharing operation to complete before returning
                    //isOperationCompleted = false;
                    //while (!isOperationCompleted)
                    //{
                    //    Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.ProcessEvents(
                    //        Windows.UI.Core.CoreProcessEventsOption.ProcessAllIfPresent);
                    //}

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            request.Data.Properties.Title = "Share Database File";

            var deferral = request.GetDeferral();

            try
            {
                var fileToShare = StorageFile.GetFileFromPathAsync(destFilePath).AsTask().Result;
                if (fileToShare != null)
                {
                    request.Data.SetStorageItems(new[] { fileToShare });
                }
            }
            finally
            {
                deferral.Complete();
                isOperationCompleted = true;
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
