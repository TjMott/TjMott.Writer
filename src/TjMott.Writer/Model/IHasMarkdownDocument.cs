using System;

namespace TjMott.Writer.Model
{
    public interface IHasMarkdownDocument : IDbType
    {
        long? MarkdownDocumentId { get; set; }
    }
}
