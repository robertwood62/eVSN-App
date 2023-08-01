using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using elidar.iOS;
using eLiDAR.Helpers;
using Foundation;
using SQLite;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(IOSSQLite))]
namespace elidar.iOS
{   
    public class IOSSQLite : ISQLite
    {
        public SQLiteConnection GetConnection(string databaseName)
        {
            // Documents folder

            string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");
            string dbPath = Path.Combine(libFolder, databaseName);

            if (!File.Exists(dbPath) || !Directory.Exists(libFolder))
            {
                dbPath = FileAccessHelper.GetLocalFilePath(databaseName);
            }

            return new SQLiteConnection(dbPath);
        }
        public bool Export(string databaseName)
        {
            try
            {
                string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");
                string dbPath = Path.Combine(libFolder, databaseName);

                var bytes = File.ReadAllBytes(dbPath);

                var fileCopyName = Path.Combine(docFolder, string.Format("Database_{0:dd-MM-yyyy_HH-mm-ss-tt}.db", DateTime.Now));
                File.WriteAllBytes(fileCopyName, bytes);

                // Share the file using the iOS sharing mechanism
                var fileUrl = NSUrl.FromFilename(fileCopyName);
                var activityItems = new NSObject[] { fileUrl };
                var activityController = new UIActivityViewController(activityItems, null);
                var viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
                viewController.PresentViewController(activityController, true, null);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public string GetPath(string databaseName)
        {
            return FileAccessHelper.GetLocalFilePath(databaseName);
        }
        public class FileAccessHelper
        {
            public static string GetLocalFilePath(string filename)
            {
                string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

                if (!Directory.Exists(libFolder))
                {
                    Directory.CreateDirectory(libFolder);
                }

                string dbPath = Path.Combine(libFolder, filename);

                CopyDatabaseIfNotExists(dbPath);

                return dbPath;
            }


            private static void CopyDatabaseIfNotExists(string dbPath)
            {
                bool debug = false;
                if (debug)
                {
                    File.Delete(dbPath); 
                }
                if (!File.Exists(dbPath))
                {
                    var existingDb = NSBundle.MainBundle.PathForResource("eLiDAR", "sqlite");
                    File.Copy(existingDb, dbPath);
                }
            }
        }


    }
}