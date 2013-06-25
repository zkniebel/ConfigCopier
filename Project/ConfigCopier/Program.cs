using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConfigCopier {
    public class Program {
        public static DirectoryInfo _SourceDirectory { get; set; }
        public static DirectoryInfo _OutputDirectory { get; set; }
        public static List<string> _OutputRoots = new List<string>();

        public static void Main(string[] args) {
            if (args.Length >= 1) {
                if (args.Length > 1 && Directory.Exists(args[1])) {
                    _SourceDirectory = new DirectoryInfo(args[1]);
                } else {
                    _SourceDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                }
                
                if (Directory.Exists(args[0])) {
                    Directory.Delete(args[0], true);
                }
                _OutputDirectory = Directory.CreateDirectory(string.Format(@"{0}\{1}", args[0], _SourceDirectory.Name));
            } else {
                _SourceDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                _OutputDirectory = Directory.CreateDirectory(
                    string.Format(@"{0}\{1}", 
                        EnsureUniqueDirName(string.Format(@"{0}\{1}_Configs", _SourceDirectory.FullName, _SourceDirectory.Name)), 
                        _SourceDirectory.Name));
            }

            if (_OutputDirectory.FullName.Contains(_SourceDirectory.FullName)) {
                var relative = _OutputDirectory.FullName.Substring(_SourceDirectory.FullName.Length - 1);
                var format = @"{0}{1}";
                _OutputRoots = new List<string>();
                while (relative.IndexOf('\\') != -1 && relative != string.Empty) {
                    _OutputRoots.Add(string.Format(format, _SourceDirectory.FullName, relative.IndexOf('\\') == 0 ? relative.Substring(1) : relative));
                    relative = relative.Substring(0, relative.LastIndexOf('\\'));
                }
            } 

            CopyConfigs(_SourceDirectory, _OutputDirectory);

            return;
        }

        public static string EnsureUniqueDirName(string baseName) {
            var format = "{0} ({1})";
            var i = 0;
            var name = baseName;
            while (Directory.Exists(name)) {
                i++;
                name = string.Format(format, baseName, i);
            }
            return name;
        }

        public static void CopyConfigs(DirectoryInfo sourceDir, DirectoryInfo outputDir) {
            var outputDirPath = outputDir.FullName;
            var configSearchStr = "*.config*";

            var subDirPathFormat = @".\{0}";
            var dirs = sourceDir.GetDirectories().Where(i => 
                !_OutputRoots.Contains(i.FullName) && 
                i.GetFiles(configSearchStr, SearchOption.AllDirectories).Length > 0);
            foreach (DirectoryInfo d in dirs) {
                var di = outputDir.CreateSubdirectory(string.Format(subDirPathFormat, d.Name));
                CopyConfigs(d, di);
            }

            var filePathFormat = @"{0}\{1}";
            var files = sourceDir.GetFiles(configSearchStr);
            foreach (FileInfo f in files) {
                f.CopyTo(string.Format(filePathFormat, outputDirPath, f.Name));
            }
        }

    }
}
