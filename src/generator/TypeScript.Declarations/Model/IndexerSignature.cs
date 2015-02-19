namespace TypeScript.Declarations.Model
{
    public class IndexerSignature : TypeScriptObject
    {
        public IType KeyType { get; set; }

        public IType ComponentType { get; set; }

        public string KeyName { get; set; }
    }
}
