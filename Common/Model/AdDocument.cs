
namespace MyDocs.Common.Model
{
    public class AdDocument : IDocument
    {
        public string Category { get; set; }

        public AdDocument(string category)
        {
            Category = category;
        }
    }
}
