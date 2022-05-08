using System;

namespace TjMott.Writer.Models
{
    public interface IHasMarkdownDocument : IDbType
    {
        long? MarkdownDocumentId { get; set; }
    }
}
