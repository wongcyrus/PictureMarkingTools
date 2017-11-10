using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureMarkingTools
{
    class Marksheet
    {
        private readonly List<MarkResult> markResultList;

        public Marksheet(List<MarkResult> markResultList)
        {
            this.markResultList = markResultList;
        }

        public void GenerateMarkSheet(string marksheetPath)
        {
            var markDict = markResultList.ToDictionary(m => m.Source + "#" + m.Question);
            var questions = markResultList.Select(m => m.Question).Distinct().OrderBy(c => c).ToList();
            var heading = new List<string>() { "Source", "Name", "Id", "Total" };
            heading.AddRange(questions);
            var sources = markResultList.Select(m => m.Source).Distinct().OrderBy(c => c).ToList();

            var lines = new List<string>() { string.Join(",", heading) };
            var markRows = sources.Select(source =>
            {
                var markResults = questions.Select(question => markDict[source + "#" + question]);
                var name = markResults.First().Name;
                var row = new List<string>() { source, markResults.First().Name.Trim(), markResults.First().Id.Trim(), markResults.Sum(m => m.Mark).ToString() };
                row.AddRange(markResults.Select(m => m.Mark.ToString()));
                return string.Join(",", row);
            });
            lines.AddRange(markRows);
            if (File.Exists(marksheetPath))
            {
                File.Delete(marksheetPath);
            }

            using (StreamWriter outputFile = new StreamWriter(marksheetPath))
            {
                foreach (string line in lines)
                    outputFile.WriteLine(line);
            }
        }
    }
}
