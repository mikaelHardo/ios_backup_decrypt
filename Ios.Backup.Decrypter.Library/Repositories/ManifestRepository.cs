using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;

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
            using (var conn = new SqliteConnection($"Data Source={_dbFilePath}\\Manifest.db"))
            {
                var fileCount = conn.Query<int>("select count(*) from files").FirstOrDefault();
                return fileCount > 0;
            }
        }

        public DBFile GetFile(string path)
        {
            using (var conn = new SqliteConnection($"Data Source={_dbFilePath}\\Manifest.db"))
            {
                var file = conn.Query<DBFile>(@"
                SELECT fileID, file
                FROM Files
                WHERE relativePath = @Path
                ORDER BY domain, relativePath
                LIMIT 1;", new {Path = path}).FirstOrDefault();
                return file;
            }
        }

        public IEnumerable<DBFile> GetFiles(string path)
        {
            using (var conn = new SqliteConnection($"Data Source={_dbFilePath}\\Manifest.db"))
            {
                var files = conn.Query<DBFile>(@"
                SELECT fileID, relativePath, file
                FROM Files
                WHERE relativePath LIKE @Path
                ORDER BY domain, relativePath;", new {Path = path});
                return files;
            }
        }
    }

    public class DBFile
    {
        public string fileID { get; set; }

        public byte[] file { get; set; }

        public string RelativePath { get; set; }
    }

}
