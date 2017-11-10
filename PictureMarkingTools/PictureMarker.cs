using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureMarkingTools
{
    public class PictureMarker
    {
        private readonly string markingFolderPath;
        private readonly MarkingFolder markingFolder;

        public PictureMarker(string markingFolderPath)
        {
            this.markingFolderPath = markingFolderPath;
            this.markingFolder = new MarkingFolder(markingFolderPath);
        }

        private string GetTempDirectory()
        {
            string path = Path.GetRandomFileName();
            return Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), path)).FullName;
        }

        public void CreateMarkingJobFolderStructure(string moodleSourceZip)
        {
            string tempFolder = GetTempDirectory();
            Console.WriteLine(tempFolder);
            ZipFile.ExtractToDirectory(moodleSourceZip, tempFolder);
            var files = markingFolder.GetFiles(tempFolder, "*.docx");

            Console.WriteLine("The number of docx files is {0}.", files.Count());
            foreach (var f in files)
            {
                Console.WriteLine(f.FullName);
                using (var imageExtractor = new ImageExtractor(f))
                {
                    markingFolder.Add(imageExtractor.GetMarkingFolderMapping());
                }
            }
            Directory.Delete(tempFolder, true);
        }

        public void GenerateMarkSheet()
        {
            var marksheet = new Marksheet(markingFolder.GetMarkingResult());
            marksheet.GenerateMarkSheet(markingFolderPath + "mark.csv");
        }
    }
}
