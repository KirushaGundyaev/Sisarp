using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace FileWatcher
{
    class Logger
    {
        private object control = new object();
        private FileSystemWatcher watcher;
        private bool enabled = true;
        private string path = "C:\\Source";

        public Logger()
        {
            watcher = new FileSystemWatcher(path);
            watcher.Filter = "*.txt";
            watcher.Created += FileTransfer;
        }

        public void Start()
        {
            watcher.EnableRaisingEvents = true;
            while (enabled)
            {
                Thread.Sleep(10000);
            }
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            enabled = false;
        }

        private void FileTransfer(object sender, FileSystemEventArgs e)
        {
            lock (control)
            {
                var directory = new DirectoryInfo(path);
                var fileName = e.Name;
                var filePath = Path.Combine(path, fileName);
                var datetime = DateTime.Now;
                var subPath = Path.Combine(datetime.ToString("yyyy", DateTimeFormatInfo.InvariantInfo),
                                            datetime.ToString("MM", DateTimeFormatInfo.InvariantInfo),
                                            datetime.ToString("dd", DateTimeFormatInfo.InvariantInfo));
                var newPath = Path.Combine(path, subPath, $"{Path.GetFileNameWithoutExtension(fileName)}_" +
                    $"{datetime.ToString(@"yyyy_MM_dd_HH_mm_ss", DateTimeFormatInfo.InvariantInfo)}" +
                    $"{Path.GetExtension(fileName)}");
                var pathForCompress = Path.ChangeExtension(newPath, "gz");
                var pathTarget = Path.Combine("C:\\Target", Path.GetFileName(pathForCompress));
                var pathTargetTXT = Path.Combine(Path.ChangeExtension(pathTarget, "txt")); //меняю на txt


                if (!directory.Exists)
                {
                    directory.Create();
                }

                directory.CreateSubdirectory(subPath);
                File.Move(filePath, newPath);
                ClassOperations.EncryptFile(newPath, newPath);
                ClassOperations.Compress(newPath, pathForCompress);
                File.Move(pathForCompress, pathTarget); //теперь в Target, но все еще сжат
                ClassOperations.Decompress(pathTarget, pathTargetTXT); // распаковывыю файл
                ClassOperations.DecryptFile(pathTargetTXT, pathTargetTXT);
                ClassOperations.AddToArchive(pathTargetTXT);
                File.Delete(newPath);//очистка мусора
                File.Delete(pathTarget);
                File.Delete(pathTargetTXT);
            }
        }
    }
}
