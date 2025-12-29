namespace Substrate.Generation.Core.Documents
{
    internal interface IDocument
    {
        string HintName { get; }   // e.g., Person.Notify.g.cs
        string Build();            // final generated source
    }
    internal abstract class DocumentBase : IDocument
    {
        protected DocumentBase(string hintName)
        {
            HintName = hintName;
        }

        public string HintName { get; }

        public abstract string Build();
    }
}