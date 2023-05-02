﻿using System.IO;
using SQLite;
using SQLiteSample.Droid.Implementaions;
using SQLiteSample.Helpers;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidSQLite))]
namespace SQLiteSample.Droid.Implementaions {
    public class AndroidSQLite : ISQLite {
        public SQLiteConnection GetConnection() {
			string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			// Documents folder
            var path = Path.Combine(documentsPath, DatabaseHelper.DbFileName);
			//var plat = new SQLite.Platform.XamarinAndroid.SQLitePlatformAndroid();
			var conn = new SQLiteConnection( path);
			// Return the database connection
			return conn;
		}
	}
}
