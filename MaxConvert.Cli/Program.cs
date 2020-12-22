using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;

namespace MaxConvert.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            FFmpeg.SetExecutablesPath(
                Path.Combine(@"C:\ProgramData\chocolatey\lib\ffmpeg\tools\ffmpeg\bin"));

            var pathPrefix = @"d:\360video";
            var files = new[] {"GS010554.360", "GS020554.360", "GS030554.360", "GS040554.360", "GS050554.360",};

            foreach (var file in files)
            {
                var inputPath = Path.Join(pathPrefix, file);
                var convertOutputPath = Path.Join(pathPrefix, "Output", $"{Path.GetFileNameWithoutExtension(inputPath)}.mp4");
            
                var convert = await FFmpeg.Conversions
                    .FromSnippet.ConvertWithHardware(inputPath, convertOutputPath,
                        HardwareAccelerator.auto, VideoCodec.hevc, VideoCodec.h264_nvenc);
                // convert.SetOutputTime(TimeSpan.FromSeconds(10));
                
                convert.OnProgress += ConvertOnOnProgress;
            
                await convert.Start();
            }

            var stagedFiles = files.Select(p =>
                Path.Join(pathPrefix, "Output", $"{Path.GetFileNameWithoutExtension(p)}.mp4"));
            var stringBuilder = new StringBuilder();
            foreach (var file in stagedFiles)
            {
                stringBuilder.AppendLine($"file '{file}'");
            }

            var inputListPath = Path.Join(pathPrefix, "Output", "files.txt");
            var outputPath = Path.Join(pathPrefix, "Output", "Concatenated.mp4");
            await File.WriteAllTextAsync(inputListPath, stringBuilder.ToString());

            var concatenate = FFmpeg.Conversions.New();

            concatenate.OnProgress += ConvertOnOnProgress;
            concatenate.OnDataReceived+= ConcatenateOnOnDataReceived;

            await concatenate.Start($"-f concat -safe 0 -i \"{inputListPath}\" -c copy \"{outputPath}\"");
        }

        private static void ConcatenateOnOnDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"ConcatenateOnOnDataReceived {e.Data}");
        }

        private static void ConvertOnOnProgress(object sender, ConversionProgressEventArgs args)
        {
            Console.WriteLine($"ConvertOnOnProgress {args.Percent}");
        }
    }
}