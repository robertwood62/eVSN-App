using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using eLiDAR;
using eLiDAR.Droid;
using eLiDAR.Helpers;
using SQLite;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidSQLite))]
namespace eLiDAR.Droid {

    public class AndroidSQLite : ISQLite {
        public SQLiteConnection GetConnection(string databaseName) 
        {
			string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			// Documents folder
            var path = Path.Combine(documentsPath, databaseName);

            // Use this if I need to reset the database
            bool isDebug = false;
            if (isDebug)
            {
                File.Delete(path);
            }

            if ((!File.Exists(path)))
            {
                using var binaryReader = new BinaryReader(Android.App.Application.Context.Assets.Open(DatabaseHelper.DbTemplateFileName));
                using var binaryWriter = new BinaryWriter(new FileStream(path, FileMode.Create));
                byte[] buffer = new byte[2048];
                int length = 0;
                while ((length = binaryReader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    binaryWriter.Write(buffer, 0, length);
                }
            }
            return new SQLiteConnection(path);
		}

        public bool Export(string databaseName)
        {
            try {
                string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string dbPath = Path.Combine(documentsPath, databaseName);

                var fileCopyName = $"Database_{DateTime.Now:dd-MM-yyyy_HH-mm-ss-tt}.db";
                var destFilePath = Path.Combine(documentsPath, fileCopyName);

                // Copy the file to the destination path
                File.Copy(dbPath, destFilePath);

                // Use Xamarin.Essentials.Share to share the file
                Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Share Database File",
                    File = new ShareFile(destFilePath, "application/octet-stream")
                });
            } catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
