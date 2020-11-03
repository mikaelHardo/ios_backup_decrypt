using System;
using System.Collections.Generic;
using System.Text;

namespace Ios.Backup.Extractor
{
    public static class IosPaths
    {
        public const string ADDRESS_BOOK = "Library/AddressBook/AddressBook.sqlitedb";
        public const string TEXT_MESSAGES = "Library/SMS/sms.db";
        public const string CALL_HISTORY = "Library/CallHistoryDB/CallHistory.storedata";
        public const string NOTES = "Library/Notes/notes.sqlite";
        public const string HEALTH = "Health/healthdb.sqlite";
        public const string HEALTH_SECURE = "Health/healthdb_secure.sqlite";
        public const string SAFARI_HISTORY = "Library/Safari/History.db";
    }
}
