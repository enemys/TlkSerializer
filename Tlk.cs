using System;
using System.Collections.Generic;
using System.Text;

namespace TlkSerializer
{
    class Tlk
    {
        public int LanguageId;
        public int StringCount;
        public int StringEntriesOffset;
        public List<StringDataElement> StringDataTable;
        public List<string> StringEntryTable;

        public Tlk(int languageId, int stringCount, int stringEntriesOffset, List<StringDataElement> stringDataTable, List<string> stringEntryTable)
        {
            LanguageId = languageId;
            StringCount = stringCount;
            StringEntriesOffset = stringEntriesOffset;
            StringDataTable = stringDataTable;
            StringEntryTable = stringEntryTable;
        }
    }
}
