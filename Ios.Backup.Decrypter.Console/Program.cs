using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using Ios.Backup.Decrypter.Console.Models;
using Ios.Backup.Decrypter.Console.Repositories;
using Ios.Backup.Decrypter.Library;

namespace Ios.Backup.Decrypter.Console
{
    internal class Program
    {
        private static bool _extractCSV = true;
        
        static void Main(string[] args)
        {
            //Example: C:\\Users\\[username]\Apple\MobileSync\Backup\12348030-12004933163B902E
            var backupDir = "";

            //Password chosen in iTunes
            var passPhrase = "";

            var path = "Library/Safari/History.db";

            using (var extractor = new IosBackupClient(backupDir, passPhrase))
            {
                ExtractFile(extractor, path, "c:\\temp\\safari-history.db");
                ExtractFile(extractor, IosPathsDbs.CALL_HISTORY, "c:\\temp\\call-history.db");
                
                extractor.ExtractFiles(IosPathsFiles.CAMERA_ROLL, "c:\\temp\\camera_roll");
            }
        }

        private static void ExtractFile(IosBackupClient client, string path, string outFileName)
        {
            client.ExtractFile(path, outFileName);

            if (_extractCSV)
            {
                IEnumerable<dynamic> data = null;
                switch (path)
                {
                    case IosPathsDbs.SAFARI_HISTORY:
                        data = DataRepository.FetchHistoryData(outFileName);
                        break;
                }

                if (data != null)
                {
                    //TODO: support other files than db
                    WriteToCsv(data, outFileName.Replace(".db", ".csv"));
                }
            }
        }

        private static void WriteToCsv(IEnumerable<dynamic> dataList, string replace)
        {
            using (var writer = new StringWriter())
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(dataList);
                }

                File.WriteAllText(replace, writer.ToString());
            }
        }
    }
}
