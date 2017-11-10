using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureMarkingTools
{
    class MarkResult
    {
        public MarkResult(string id, string name, string source, string question, int mark)
        {
            Id = id;
            Name = name;
            Source = source;
            Question = question;
            Mark = mark;
        }

        public string Id { get; }
        public string Name { get; }
        public string Source { get; }
        public string Question { get; }
        public int Mark { get; }
    }
}
