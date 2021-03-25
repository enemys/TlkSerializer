using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace TlkSerializer
{
    static class TlkParser
    {
        private const int headerSize = 20;
        private const int stringDataSize = 40;
        public static bool ValidateHeader(byte[] tlkData)
        {
            string header;
            try
            {
                header = Encoding.UTF8.GetString(tlkData, 0, 8);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return header.Equals("TLK V3.0", StringComparison.Ordinal);
        }


        public static Tlk Parse(byte[] tlkData)
        {
            if (!ValidateHeader(tlkData)) throw new ArgumentException("File is not a valid TLK 3.0 file!");
            var languageId = BitConverter.ToInt32(tlkData, 8);
            var stringCount = BitConverter.ToInt32(tlkData, 12);
            var stringEntriesOffset = BitConverter.ToInt32(tlkData, 16);

            var stringDataTable = new List<StringDataElement>();
            var stringEntryTable = new List<string>();

            for(var offset = headerSize; offset < stringEntriesOffset; offset += stringDataSize)
            {
                stringDataTable.Add(new StringDataElement
                {
                    Flags = BitConverter.ToInt32(tlkData, offset),
                    SoundResRef = tlkData[(offset + 4)..(offset + 20)],
                    VolumeVariance = BitConverter.ToInt32(tlkData, offset + 20),
                    PitchVariance = BitConverter.ToInt32(tlkData, offset + 24),
                    OffsetToString = BitConverter.ToInt32(tlkData, offset + 28),
                    StringSize = BitConverter.ToInt32(tlkData, offset + 32),
                    SoundLength = BitConverter.ToSingle(tlkData, offset + 36)
                });
            }
            foreach(var entry in stringDataTable)
            {
                stringEntryTable.Add(System.Text.Encoding.UTF8.GetString(tlkData, stringEntriesOffset + entry.OffsetToString, entry.StringSize));
            }
            return new Tlk(languageId, stringCount, stringEntriesOffset, stringDataTable, stringEntryTable);
        }

        public static string TlkToText(Tlk tlk, bool fromZero = false)
        {
            var index = fromZero? 0 : 2<<23;
            
            return string.Join('\n', tlk.StringEntryTable.Select((e, i) => $"{index + i} := {e}"));
        }

        public static Tlk TextToTlk(string textTlk)
        {
            var stringDataTable = new List<StringDataElement>();
            var stringEntryTable = new List<string>();

            var buffer = new StringBuilder();
            foreach (var line in textTlk.Split('\n'))
            {
                var entryMatcher = new Regex(@"^\d+ := (.*)");
                var match = entryMatcher.Match(line);
                if(match.Success)
                {
                    stringEntryTable.Add(buffer.ToString());
                    buffer.Clear();
                    buffer.Append(match.Groups[1]);
                }
                else
                {
                    buffer.Append("\n" + line);
                }
            }
            stringEntryTable.Add(buffer.ToString());

            stringEntryTable = stringEntryTable.Skip(1).ToList(); //Skip first empty element always created by the loop above
            var accumulatedOffset = 0;
            foreach (var entry in stringEntryTable)
            {
                var byteLength = Encoding.UTF8.GetByteCount(entry);
                stringDataTable.Add(new StringDataElement
                {
                    Flags = entry.Length > 0 ? 1 : 0,
                    SoundResRef = new byte[16],
                    VolumeVariance = 0,
                    PitchVariance = 0,
                    OffsetToString = entry.Length > 0 ? accumulatedOffset : 0,
                    StringSize = byteLength,
                    SoundLength = 0.0f
                });
                accumulatedOffset += byteLength;
            }

            return new Tlk(0, stringEntryTable.Count, headerSize + stringEntryTable.Count * stringDataSize, stringDataTable, stringEntryTable);
        }

        public static byte[] TlkToBytes(Tlk tlk)
        {
            var header = Encoding.UTF8.GetBytes("TLK V3.0");
            var output = header.Concat(BitConverter.GetBytes(tlk.LanguageId))
                  .Concat(BitConverter.GetBytes(tlk.StringCount))
                  .Concat(BitConverter.GetBytes(tlk.StringEntriesOffset));
            foreach(var dataEntry in tlk.StringDataTable)
            {
                output = output.Concat(BitConverter.GetBytes(dataEntry.Flags))
                      .Concat(dataEntry.SoundResRef)
                      .Concat(BitConverter.GetBytes(dataEntry.VolumeVariance))
                      .Concat(BitConverter.GetBytes(dataEntry.PitchVariance))
                      .Concat(BitConverter.GetBytes(dataEntry.OffsetToString))
                      .Concat(BitConverter.GetBytes(dataEntry.StringSize))
                      .Concat(BitConverter.GetBytes(dataEntry.SoundLength));
            }
            foreach(var entry in tlk.StringEntryTable)
            {
                output = output.Concat(Encoding.UTF8.GetBytes(entry));
            }

            return output.ToArray();
        }
    }
}
