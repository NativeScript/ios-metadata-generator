namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class ConstructSignature : TypeScriptObject
    {
        public ConstructSignature()
        {
            this.Parameters = new List<Parameter>();
        }

        public IList<Parameter> Parameters { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as ConstructSignature;
            return other != null
                && Enumerable.SequenceEqual(this.Parameters, other.Parameters);
        }
    }
}
