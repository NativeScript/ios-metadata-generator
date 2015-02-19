namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;

    public class TypeScriptObject
    {
        public TypeScriptObject()
        {
            this.Annotations = new HashSet<object>();
        }

        public ICollection<object> Annotations { get; private set; }
    }
}
