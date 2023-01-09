namespace NGen
{
    public class NPath
    {
        private string Path { get; set; }
        public NPath(string path)
        {
            this.Path = path;
        }
        public static NPath GetBaseDirectory() => new NPath(AppContext.BaseDirectory);
        public static NPath GetCurrentDirectory() => new NPath(AppContext.BaseDirectory).UpDirectory().UpDirectory().UpDirectory().UpDirectory();

        public NPath UpDirectory()
        {
            Path = Path.TrimFromEnd(System.IO.Path.DirectorySeparatorChar);
            return (NPath)this.MemberwiseClone();
        }

        public string ToString() => Path;

        public NPath SubDirectory(string newPath)
        {
            this.Path = System.IO.Path.Combine(Path, newPath);
            return (NPath)this.MemberwiseClone();
        }

        public Task WriteFileAsync(string FileName, string Data) => System.IO.File.WriteAllTextAsync(System.IO.Path.Combine(this.Path, FileName), Data);
        
        public void WriteFile(string FileName, string Data) => System.IO.File.WriteAllText(System.IO.Path.Combine(this.Path, FileName), Data);
        
        public Task WriteFileAsync(string Data) => System.IO.File.WriteAllTextAsync(this.Path, Data);
        
        public void WriteFile(string Data) => System.IO.File.WriteAllText(this.Path, Data);

        public Task<string> ReadFileAsync(string FileName) => System.IO.File.ReadAllTextAsync(System.IO.Path.Combine(this.Path, FileName));
        
        public string ReadFile(string FileName) => System.IO.File.ReadAllText(System.IO.Path.Combine(this.Path, FileName));

        public Task<string> ReadFileAsync() => System.IO.File.ReadAllTextAsync(this.Path);
        
        public string ReadFile() => System.IO.File.ReadAllText(this.Path);

        public NPath SelectFile(string FileName)
        {
            this.Path = System.IO.Path.Combine(Path, FileName);
            return (NPath)this.MemberwiseClone();
        }

        public NPath EnsureExsit()
        {
            System.IO.Directory.CreateDirectory(Path);
            return this;
        }
        public NPath CleanDirectory()
        {
            var files = System.IO.Directory.GetFiles(Path);
            var directories = System.IO.Directory.GetDirectories(Path);

            foreach (var file in files)
                System.IO.File.Delete(file);

            foreach (var directory in directories)
                System.IO.Directory.Delete(directory , true);

            return this;
        }
    }
}
