using MyDocs.Common.Model;
using Serializable = MyDocs.Common.Model.Serializable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common
{
    public static class Extensions
    {
        public static string GetHumanReadableDescription(this Document document)
        {
            return GetHumanReadableDescription(document.Id, document.Tags);
        }

        public static string GetHumanReadableDescription(this Serializable.Document document)
        {
            return GetHumanReadableDescription(document.Id, document.Tags);
        }

        private static string GetHumanReadableDescription(Guid documentId, IEnumerable<string> tags)
        {
            tags = tags.Select(RemoveInvalidFileNameChars);
            return String.Format("{0} ({1})", String.Join("-", tags), documentId);
        }

        private static string RemoveInvalidFileNameChars(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}
