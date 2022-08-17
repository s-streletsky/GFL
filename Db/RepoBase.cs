using Dapper;
using System.Data.SQLite;
using System.Text;

namespace GeeksForLess_Test.Db
{
    public class RepoBase
    {
        readonly static string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.sqlite");

        public static void InitializeDatabase()
        {
            if (File.Exists(dbPath)) { return; }

            using (var connection = GetConnection())
            {
                var sql = File.ReadAllText("./Db/db.init.sql", Encoding.UTF8);
                connection.Execute(sql);
            }
        }

        internal static SQLiteConnection GetConnection(bool open = true)
        {
            var builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = dbPath;

            var connection = new SQLiteConnection(builder.ToString());
            if (open)
            {
                connection.Open();
            }
            
            return connection;
        }


    }
}
