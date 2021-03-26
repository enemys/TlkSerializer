using System;
using Xunit;
using TlkSerializer;
using System.Collections.Generic;

namespace TlkSerializerTests
{
    public class TlkParserTests
    {

        [Fact]
        public void ValidHeader()
        {
            var header = System.Text.Encoding.UTF8.GetBytes("TLK V3.0");
            var isValid = TlkParser.ValidateHeader(header);
            Assert.True(isValid);
        }

        [Theory]
        [InlineData("TLK V2.0")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("gibberish")]
        public void InvalidHeaders(string input)
        {
            byte[] header;
            if (input == null) header = null;
            else header = System.Text.Encoding.UTF8.GetBytes(input);
            var isValid = TlkParser.ValidateHeader(header);
            Assert.False(isValid);
        }

        [Fact]
        public void ValidTlk()
        {
            var bytes = System.IO.File.ReadAllBytes("..\\..\\..\\test files\\test.tlk");
            var tlk = TlkParser.BytesToTlk(bytes);
            Assert.Equal(3, tlk.StringCount);
            Assert.Equal("dummy", tlk.StringEntryTable[0]);
            Assert.Equal("yummy", tlk.StringEntryTable[1]);
            Assert.Equal("entry", tlk.StringEntryTable[2]);
            Assert.Equal(140, tlk.StringEntriesOffset);
            Assert.Equal(0, tlk.StringDataTable[0].OffsetToString);
            Assert.Equal(5, tlk.StringDataTable[0].StringSize);
            Assert.Equal(5, tlk.StringDataTable[1].OffsetToString);
            Assert.Equal(5, tlk.StringDataTable[1].StringSize);
            Assert.Equal(10, tlk.StringDataTable[2].OffsetToString);
            Assert.Equal(5, tlk.StringDataTable[2].StringSize);
        }

        [Theory]
        [InlineData("empty.file")]
        [InlineData("some.txt")]
        public void InvalidTlk(string fileName)
        {
            var bytes = System.IO.File.ReadAllBytes($"..\\..\\..\\test files\\{fileName}");
            try
            {
                TlkParser.BytesToTlk(bytes);
                Assert.True(false);
            }
            catch(ArgumentException)
            {
                Assert.True(true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TlkToText(bool fromZero)
        {
            var bytes = System.IO.File.ReadAllBytes("..\\..\\..\\test files\\test.tlk");
            var tlk = TlkParser.BytesToTlk(bytes);
            var text = TlkParser.TlkToText(tlk, fromZero);
            var offset = fromZero ? 0 : 2 << 23;
            string expected = $"{0+offset} := dummy\n{1+offset} := yummy\n{2+offset} := entry";
            Assert.Equal(expected, text);
        }

        [Theory]
        [InlineData("tlk.txt")]
        [InlineData("tlk2.txt")]
        public void TextToTlk(string fileName)
        {
            var text = System.IO.File.ReadAllText($"..\\..\\..\\test files\\{fileName}", System.Text.Encoding.UTF8);
            var tlk = TlkParser.TextToTlk(text);
            Assert.Equal(3, tlk.StringCount);
            Assert.Equal("dummy", tlk.StringEntryTable[0]);
            Assert.Equal("yummy", tlk.StringEntryTable[1]);
            Assert.Equal("entry", tlk.StringEntryTable[2]);
            Assert.Equal(140, tlk.StringEntriesOffset);
            Assert.Equal(0, tlk.StringDataTable[0].OffsetToString);
            Assert.Equal(5, tlk.StringDataTable[0].StringSize);
            Assert.Equal(5, tlk.StringDataTable[1].OffsetToString);
            Assert.Equal(5, tlk.StringDataTable[1].StringSize);
            Assert.Equal(10, tlk.StringDataTable[2].OffsetToString);
            Assert.Equal(5, tlk.StringDataTable[2].StringSize);
        }

        [Theory]
        [InlineData("empty.file")]
        [InlineData("some.txt")]
        [InlineData("other.txt")]

        public void InvalidText(string fileName)
        {
            var text = System.IO.File.ReadAllText($"..\\..\\..\\test files\\{fileName}", System.Text.Encoding.UTF8);
            try
            {
                TlkParser.TextToTlk(text);
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void TlkToBytes()
        {
            var expected = System.IO.File.ReadAllBytes("..\\..\\..\\test files\\test.tlk");
            var tlkE = TlkParser.BytesToTlk(expected);
            var tlk = new Tlk(0, 3, 140,
                new List<StringDataElement>
                {
                    new StringDataElement
                    {
                        Flags = 1,
                        OffsetToString = 0,
                        StringSize = 5
                    },
                    new StringDataElement
                    {
                        Flags = 1,
                        OffsetToString = 5,
                        StringSize = 5
                    },
                    new StringDataElement
                    {
                        Flags = 1,
                        OffsetToString = 10,
                        StringSize = 5
                    }
                },
                new List<string>
                {
                    "dummy",
                    "yummy",
                    "entry"
                }
                );
            var bytes = TlkParser.TlkToBytes(tlk);
            Assert.Equal(expected, bytes);
        }
    }


}
