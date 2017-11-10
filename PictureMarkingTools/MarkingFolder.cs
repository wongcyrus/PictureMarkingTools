using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureMarkingTools
{
    class MarkingFolder
    {
        public MarkingFolder(string directoryPath)
        {
            DirectoryInfo = new DirectoryInfo(directoryPath);
        }

        private DirectoryInfo DirectoryInfo { get; }

        public void Add(List<(string, string)> imageNamePairs)
        {
            imageNamePairs.ForEach(i =>
            {
                string distFile = Path.Combine(DirectoryInfo.FullName, i.Item2.ToString());
                var f = new FileInfo(distFile);
                if (!Directory.Exists(f.DirectoryName))
                {
                    Directory.CreateDirectory(f.DirectoryName);
                }
                string sourceFile = i.Item1;

                File.Copy(sourceFile, distFile, true);
            });
        }

        public List<FileInfo> GetFiles(string filePath, string searchPattern)
        {
            var dirInfo = new DirectoryInfo(filePath);
            var hiddenFolders = dirInfo.GetDirectories("*", SearchOption.AllDirectories)
                .Where(d => (d.Attributes & FileAttributes.Hidden) != 0)
                .Select(d => d.FullName);
            var files = dirInfo.GetFiles(searchPattern, SearchOption.AllDirectories)
                .Where(f => (f.Attributes & FileAttributes.Hidden) == 0 &&
                    !hiddenFolders.Any(d => f.FullName.StartsWith(d)));
            return files.ToList();
        }

        public List<MarkResult> GetMarkingResult()
        {
            var files = GetFiles(this.DirectoryInfo.FullName, "*");
            Console.Out.WriteLine(files.Count);
            return files.Select(f =>
            {
                var segements = f.FullName.Remove(0, this.DirectoryInfo.FullName.Length + 1).Split('\\');
                Console.WriteLine(string.Join(",", segements));

                string question = "", filename = "", id, name, source;
                int mark = 0;
                if (segements.Length == 2)
                {
                    question = segements[0];
                    filename = segements[1];
                }
                else if (segements.Length == 3)
                {
                    question = segements[0];
                    mark = int.Parse(segements[1]);
                    filename = segements[2];
                }
                var nameSegements = filename.Split('#');
                id = nameSegements[0];
                nameSegements = nameSegements[1].Split('(');
                name = nameSegements[0];
                source = nameSegements[1].Split(')')[0];
                Console.Out.WriteLine((id, name, source, question, mark));
                return new MarkResult(id, name, source, question, mark);
            }).ToList();

        }
    }
}
