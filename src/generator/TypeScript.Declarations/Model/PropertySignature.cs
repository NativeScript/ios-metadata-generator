namespace TypeScript.Declarations.Model
{
    public class PropertySignature : TypeScriptObject
    {
        public string Name { get; set; }

        public IType TypeAnnotation { get; set; }

        public bool IsOptional { get; set; }

        public bool IsStatic { get; set; }
    }
}
