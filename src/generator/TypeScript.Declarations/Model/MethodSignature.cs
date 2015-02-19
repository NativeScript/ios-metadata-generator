namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class MethodSignature : TypeScriptObject
    {
        public MethodSignature()
        {
            this.Parameters = new List<Parameter>();
        }

        public string Name { get; set; }

        public IType ReturnType { get; set; }

        public IList<Parameter> Parameters { get; private set; }

        public bool IsStatic { get; set; }

        public bool IsOptional { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as MethodSignature;
            if (other == null)
            {
                return false;
            }

            return this.Name == other.Name
                && this.Parameters.SequenceEqual(other.Parameters)
                && this.ReturnType == other.ReturnType;
        }

        public override int GetHashCode()
        {
            var nameHash = this.Name == null ? 0 : this.Name.GetHashCode();
            var returnTypeHash = this.ReturnType == null ? 0 : this.ReturnType.GetHashCode();
            var paramsHash = this.Parameters.Count;
            var isStaticHash = this.IsStatic.GetHashCode();
            var isOptionalHash = this.IsOptional.GetHashCode();
            return ((paramsHash * 2699 + returnTypeHash * 2707 + nameHash * 2857) << 1 + isStaticHash) << 1 + isOptionalHash;
        }
    }
}
