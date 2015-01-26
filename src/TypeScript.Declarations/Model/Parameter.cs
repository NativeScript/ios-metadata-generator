namespace TypeScript.Declarations.Model
{
    public class Parameter : TypeScriptObject
    {
        public string Name { get; set; }

        public bool IsOptional { get; set; }

        public IType TypeAnnotation { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Parameter;
            return other != null
                && object.Equals(this.TypeAnnotation, other.TypeAnnotation)
                && this.IsOptional == other.IsOptional;
        }

        public override int GetHashCode()
        {
            var nameHash = this.Name == null ? 0 : this.Name.GetHashCode();
            var optionalHash = this.IsOptional.GetHashCode();
            var typeHash = this.TypeAnnotation == null ? 0 : this.TypeAnnotation.GetHashCode();
            return (nameHash * 3517 + typeHash * 3559) << 1 + optionalHash;
        }
    }
}
