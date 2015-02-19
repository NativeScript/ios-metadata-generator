namespace TypeScript.Declarations.Writers
{
    using TypeScript.Declarations.Model;

    public class DocumentationProvider
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The instance is immutable.")]
        public static readonly DocumentationProvider Undocumented = new DocumentationProvider();

        public virtual string GetDoc(ClassDeclaration @class)
        {
            return null;
        }

        public virtual string GetDoc(InterfaceDeclaration @interface)
        {
            return null;
        }

        public virtual string GetDoc(EnumDeclaration @enum)
        {
            return null;
        }

        public virtual string GetDoc(FunctionDeclaration function)
        {
            return null;
        }

        public virtual string GetDoc(VariableStatement @var)
        {
            return null;
        }

        public virtual string GetDoc(PropertySignature property)
        {
            return null;
        }

        public virtual string GetDoc(MethodSignature method)
        {
            return null;
        }

        public virtual string GetDoc(ConstructSignature construct)
        {
            return null;
        }
    }
}
