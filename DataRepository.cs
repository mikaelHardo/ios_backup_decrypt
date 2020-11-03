using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Ios.Backup.Extractor
{
    public class DataRepository
    {

        public static IEnumerable<dynamic> FetchHistoryData(string path)
        {
            using (var conn = new SqliteConnection($"Data Source={path}"))
            {
                var data = conn.Query<dynamic>(@"
SELECT datetime(v.visit_time + strftime('%s', '2001-01-01 00:00:00'), 'unixepoch', 'localtime') as visit_time, i.url, v.title
from history_visits as v left join history_items as i on v.history_item = i.id
order by v.visit_time desc");
                return data;
            }
        }
    }
}
