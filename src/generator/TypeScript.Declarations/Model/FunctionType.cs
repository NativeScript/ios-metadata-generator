namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;

    public class FunctionType : TypeScriptObject, IType
    {
        public FunctionType()
        {
            this.Parameters = new List<Parameter>();
        }

        public IType ReturnType { get; set; }

        public IList<Parameter> Parameters { get; private set; }

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
