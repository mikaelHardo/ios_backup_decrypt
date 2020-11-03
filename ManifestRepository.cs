using System.Data.SQLite;
using System.Linq;
using Dapper;

namespace Ios.Backup.Extractor
{
    public class ManifestRepository
    {
        private readonly string _dbFilePath;

        public ManifestRepository(string dbFilePath)
        {
            _dbFilePath = dbFilePath;
        }

        public bool OpenTempDb()
        {
            using (var conn = new SQLiteConnection($"Data Source={_dbFilePath}\\Manifest.db"))
            {
                var fileCount = conn.Query<int>("select count(*) from files").FirstOrDefault();
                return fileCount > 0;
            }
        }

        public DBFile GetFile(string path)
        {
            using (var conn = new SQLiteConnection($"Data Source={_dbFilePath}\\Manifest.db"))
            {
                var file = conn.Query<DBFile>(@"SELECT fileID, file
                FROM Files
                WHERE relativePath = @Path
                ORDER BY domain, relativePath
                LIMIT 1;", new {Path = path}).FirstOrDefault();
                return file;
            }
        }
    }

    public class DBFile
    {
        public string fileID { get; set; }

        public byte[] file { get; set; }
    }

}
