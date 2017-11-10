using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PictureMarkingTools
{
    class ImageExtractor : IDisposable
    {
        private readonly FileInfo fileInfo;
        private WordprocessingDocument wordprocessingDocument;

        private const string nameField = "name";
        private const string studentId = "StudentId";
        private readonly Dictionary<string, (string, string)> textFields;
        private readonly Dictionary<string, (string, string)> imagesFields;

        public ImageExtractor(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            UnZipWord();

            wordprocessingDocument = WordprocessingDocument.Open(fileInfo.FullName, true);
            textFields = GetTextFields();
            imagesFields = GetImagesMapping();
        }

        private string UpZipFolder => System.IO.Path.Combine(fileInfo.Directory.ToString(), fileInfo.Name.Replace(fileInfo.Extension, "")).ToString();
        private string MediaFolder => System.IO.Path.Combine(UpZipFolder, "word", "media").ToString();
        public string Id => textFields[studentId].Item2;
        public string Name => textFields[nameField].Item2;
        public List<string> Questions => imagesFields.Keys.OrderBy(q => q).ToList();

        public List<(string, string)> GetMarkingFolderMapping() => Questions.Select(q =>
                                                                             {
                                                                                 var f = new FileInfo(imagesFields[q].Item2);
                                                                                 return (imagesFields[q].Item2,
                                                                                 string.Format("{0}/{1}#{2}({3}){4}", q, Id, Name, this.fileInfo.Directory.Name, f.Extension));
                                                                             }).ToList();

        private Dictionary<string, (string, string)> GetTextFields()
        {
            MainDocumentPart mainDocumentPart = wordprocessingDocument.MainDocumentPart;
            var textBoxContents = mainDocumentPart.Document.Body.Descendants<TextInput>();
            var bookmarkStarts = mainDocumentPart.Document.Body.Descendants<BookmarkStart>();

            return textBoxContents.Select(textBoxContent =>
             {
                 var formFieldName = textBoxContent.Parent.Descendants<FormFieldName>().FirstOrDefault().Val;
                 var bookmarkStart = bookmarkStarts.FirstOrDefault(b => b.Name.ToString() == formFieldName.ToString());
                 var text = "";
                 try
                 {
                     text = bookmarkStart?.ElementsAfter().Skip(3).First().InnerText;
                 }
                 catch (InvalidOperationException)
                 {
                     Console.WriteLine("Cannot read field " + formFieldName);
                 }
                 var field = formFieldName.ToString();
                 return (field, text);
             }).ToDictionary(p => p.field);
        }

        private void DeleteUpZipFolder()
        {
            if (Directory.Exists(UpZipFolder))
                Directory.Delete(UpZipFolder, true);

        }
        private void UnZipWord()
        {
            DeleteUpZipFolder();
            Directory.CreateDirectory(UpZipFolder);
            ZipFile.ExtractToDirectory(fileInfo.FullName, UpZipFolder);
        }

        private Dictionary<string, (string, string)> GetImagesMapping()
        {
            MainDocumentPart mainDocumentPart = wordprocessingDocument.MainDocumentPart;

            ////////////////// TODO: REMOVE
            bool isFirstQ1d = true;
            bool isFirstQ3f = true;
            /////////////////////

            return mainDocumentPart.Document.Body.Descendants<SdtContentPicture>()
                 .Select(c => c.Parent) // w:sdtProperties
                 .Where(c => c != null)
                 .Select(sdtProperties =>
                 {
                     SdtProperties p = sdtProperties as SdtProperties;
                     SdtContentPicture pict = sdtProperties.LastChild as SdtContentPicture;
                     SdtAlias a = p.Elements<SdtAlias>().FirstOrDefault();
                     string id = a.Val.Value;
                     string embedRef = null;
                     Drawing dr = p.Parent.Descendants<Drawing>().FirstOrDefault();
                     Blip blip = dr?.Descendants<Blip>().FirstOrDefault();
                     embedRef = blip?.Embed;

                     return Tuple.Create(id, embedRef);
                 }).Select(idAndEmbedRef =>
                 {
                     IdPartPair idpp = wordprocessingDocument.MainDocumentPart.Parts.FirstOrDefault(pa => pa.RelationshipId == idAndEmbedRef.Item2);
                     var question = idAndEmbedRef.Item1;
                     var imagePath = System.IO.Path.Combine((UpZipFolder + idpp.OpenXmlPart.Uri.ToString().Replace(@"/", @"\")));

                     ////////////////// TODO: REMOVE                     
                     if (question.ToString() == "Q1d" && isFirstQ1d)
                     {
                         isFirstQ1d = false;
                         return (question + "1", imagePath);
                     }
                     else if (question.ToString() == "Q1d" && !isFirstQ1d)
                     {
                         return (question + "2", imagePath);
                     }

                     if (question.ToString() == "Q3f" && isFirstQ3f)
                     {
                         isFirstQ3f = false;
                         return (question + "1", imagePath);
                     }
                     else if (question.ToString() == "Q3f" && !isFirstQ3f)
                     {
                         return (question + "2", imagePath);
                     }
                     /////////////////////
                     return (question, imagePath);
                 }).ToDictionary(p => p.Item1);

        }

        public void Dispose()
        {
            wordprocessingDocument.Close();
        }
    }


}
