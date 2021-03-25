using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace TlkSerializer
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFile = new Option<FileInfo>(
                    new string[] { "-i", "--input-file" },
                    ".tlk or text file to convert");
            inputFile.IsRequired = true;
            inputFile.ExistingOnly();

            var outputFile = new Option<FileInfo>(
                    new string[] { "-o", "--output-file" },
                    "Output file path");
            outputFile.IsRequired = true;

            var fromZero = new Option<bool>(
                    new string[] { "-z", "--from-zero" },
                    "Start numbering output text lines from zero (ignored for .tlk generation)");

            var rootCommand = new RootCommand{inputFile, outputFile, fromZero};
            rootCommand.Handler = CommandHandler.Create((FileInfo inputFile, FileInfo outputFile, bool fromZero) =>
            {
                var byteStream = inputFile.OpenRead();
                var inputBytes = new byte[byteStream.Length];
                byteStream.Read(inputBytes);
                byteStream.Close();
                if (TlkParser.ValidateHeader(inputBytes))
                {
                    var tlk = TlkParser.Parse(inputBytes);
                    var textTlk = TlkParser.TlkToText(tlk, fromZero);
                    if (outputFile.Exists) outputFile.Delete();
                    var outStream = outputFile.CreateText();
                    outStream.Write(textTlk);
                    outStream.Close();
                }
                else
                {
                    var textTlk = System.Text.Encoding.UTF8.GetString(inputBytes);
                    var tlk = TlkParser.TextToTlk(textTlk);
                    var tlkBytes = TlkParser.TlkToBytes(tlk);
                    if (outputFile.Exists) outputFile.Delete();
                    var outStream = outputFile.Create();
                    outStream.Write(tlkBytes);
                    outStream.Close();
                }
            });
            rootCommand.Invoke(args);
        }
    }
}
