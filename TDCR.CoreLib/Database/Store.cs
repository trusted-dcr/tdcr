using System;
using System.IO;
using System.Threading.Tasks;

namespace TDCR.CoreLib.Database
{
    public abstract class FileStore
    {
        protected string FilePath { get; }

        static FileStore()
        {
            if (!Directory.Exists(Constants.DataDir))
                Directory.CreateDirectory(Constants.DataDir);
        }

        protected FileStore(string subPath)
        {
            FilePath = Path.Combine(Constants.DataDir, subPath);
        }

        public void Truncate()
        {
            File.Open(FilePath, FileMode.Truncate).Dispose();
        }

        protected FileStream OpenFile()
        {
            return File.Open(FilePath, FileMode.OpenOrCreate);
        }

        protected byte[] ReadFile() => ReadFileAsync().Result;

        protected async Task<byte[]> ReadFileAsync()
        {
            return await File.ReadAllBytesAsync(FilePath);
        }

        protected void WriteFile(byte[] data) => WriteFileAsync(data).Wait();

        protected async Task WriteFileAsync(byte[] data)
        {
            await File.WriteAllBytesAsync(FilePath, data);
        }
    }
}
