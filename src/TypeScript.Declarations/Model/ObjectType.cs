namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;

    public class ObjectType : TypeScriptObject, IType
    {
        public ObjectType()
        {
            this.Properties = new List<PropertySignature>();
            this.Indexers = new List<IndexerSignature>();
        }

        public IList<PropertySignature> Properties { get; private set; }

        public IList<IndexerSignature> Indexers { get; private set; }

        public void Accept(TypeVisitor visitor)
        {
            if (visitor == null)
            {
                throw new System.ArgumentNullException("visitor");
            }

            visitor.Visit(this);
        }
    }
}
