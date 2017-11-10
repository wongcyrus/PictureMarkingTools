using System;
using System.IO;
using System.Linq;

namespace PictureMarkingTools
{
    class Sample
    {
        static void Main(string[] args)
        {
            string sourceFolderPath = @"C:\Users\developer\Desktop\Word test\";
            string markingFolderPath = @"C:\Users\developer\Desktop\test\";
            bool marking = false;
            
            try
            {
                var markingFolder = new MarkingFolder(markingFolderPath);
                if (marking)
                {
                    var files = markingFolder.GetFiles(sourceFolderPath, "*.docx");

                    Console.WriteLine("The number of docx files is {0}.", files.Count());
                    foreach (var f in files)
                    {
                        Console.WriteLine(f.FullName);
                        using (var imageExtractor = new ImageExtractor(f))
                        {
                            markingFolder.Add(imageExtractor.GetMarkingFolderMapping());
                        }
                    }
                }
                else
                {
                    var marksheet = new Marksheet(markingFolder.GetMarkingResult());
                    marksheet.GenerateMarkSheet(sourceFolderPath + "mark.csv");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Completed!");
            Console.ReadKey();
        }

    }
}