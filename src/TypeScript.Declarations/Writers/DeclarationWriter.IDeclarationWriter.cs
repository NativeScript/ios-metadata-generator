namespace TypeScript.Declarations.Writers
{
    using System.Collections.Generic;
    using System.Linq;
    using TypeScript.Declarations.Model;

    internal partial class DeclarationWriter : IDeclarationWriter
    {
        private ISourceUnitWriter output;
        private DocumentationProvider documentationProvider;

        public DeclarationWriter(ISourceUnitWriter output, DocumentationProvider documentationProvider)
        {
            this.output = output;
            this.documentationProvider = documentationProvider;
        }

        public void WriteDeclaration(Declaration declaration)
        {
            declaration.Accept(this);
        }

        private void WriteClass(ClassDeclaration @class)
        {
            var docs = this.documentationProvider.GetDoc(@class);
            this.WriteDocComment(docs);

            this.output.WriteIndent();

            // NOTE: If we are in declared module we do not have to write declare here.
            this.output.Write("declare ");
            this.output.Write("class ");
            this.output.Write(@class.Name);
            this.output.Write(" ");

            // type params

            // extends
            if (@class.Extends != null)
            {
                this.output.Write("extends ");
                this.output.WriteType(@class.Extends);
                this.output.Write(" ");
            }

            // implements
            @class.Implements.ApplyWithSeparators(this.WriteSpaceSuffixedImplement, this.WriteType, this.WriteCommaSeparator, this.WriteSpace);

            // class members
            this.output.WriteLine("{");
            using (this.output.EnterIndentScope())
            {
                // vars
                this.WriteProperties(@class.Properties);

                // constructors
                this.WriteConstructorSignatures(@class.Constructors);

                // methods
                this.WriteMethodSignatures(@class.Methods);
            }

            this.output.WriteIndent();
            this.output.WriteLine("}");
        }

        private void WriteInterface(InterfaceDeclaration @interface)
        {
            var docs = this.documentationProvider.GetDoc(@interface);
            this.WriteDocComment(docs);

            this.output.WriteIndent();
            this.output.Write("interface ");
            this.output.Write(@interface.Name);
            this.output.Write(" ");

            // extends
            this.output.WriteLine("{");
            using (this.output.EnterIndentScope())
            {
                // vars
                this.WriteProperties(@interface.Properties);

                // methods
                // NOTE: TypeScript does not support static methods in interfaces, We have to print some help about them though.
                this.WriteMethodSignatures(@interface.Methods.Where(m => !m.IsStatic));
            }

            this.output.WriteIndent();
            this.output.WriteLine("}");
        }

        private void WriteMethodSignatures(IEnumerable<MethodSignature> methods)
        {
            methods.Apply(this.WriteMethodSignature);
        }

        private void WriteMethodSignature(MethodSignature method)
        {
            var docs = this.documentationProvider.GetDoc(method);
            this.WriteDocComment(docs);

            this.output.WriteIndent();
            if (method.IsStatic)
            {
                this.output.Write("static ");
            }

            this.output.Write(method.Name);
            if (method.IsOptional)
            {
                this.WriteQuestionMark();
            }

            // type parameters

            // method parameters
            this.WriteParameters(method.Parameters);

            // return type
            this.WriteTypeAnnotation(method.ReturnType);
            this.output.WriteLine(";");
        }

        private void WriteParameters(IEnumerable<Parameter> parameters)
        {
            this.output.Write("(");
            parameters.ApplyWithSeparators(this.WriteParameter, this.WriteCommaSeparator);
            this.output.Write(")");
        }

        private void WriteFunction(FunctionDeclaration @function)
        {
            var docs = this.documentationProvider.GetDoc(@function);
            this.WriteDocComment(docs);

            this.output.WriteIndent();
            this.output.Write("declare function ");
            this.output.Write(@function.Name);

            // method parameters
            this.WriteParameters(@function.Parameters);

            // return type
            this.WriteTypeAnnotation(@function.ReturnType);
            this.output.WriteLine(";");
        }

        private void WriteParameter(Parameter parameter)
        {
            this.Write(parameter.Name);
            if (parameter.IsOptional)
            {
                this.WriteQuestionMark();
            }
            this.WriteTypeAnnotation(parameter.TypeAnnotation);
        }

        private void WriteProperties(IEnumerable<PropertySignature> properties)
        {
            properties.Apply(this.WriteProperty);
        }

        private void WriteProperty(PropertySignature property)
        {
            var docs = this.documentationProvider.GetDoc(property);
            this.WriteDocComment(docs);

            this.output.WriteIndent();
            if (property.IsStatic)
            {
                this.output.Write("static ");
            }

            this.output.Write(property.Name);
            if (property.IsOptional)
            {
                this.WriteQuestionMark();
            }

            this.WriteTypeAnnotation(property.TypeAnnotation);
            this.output.WriteLine(";");
        }

        private void WriteConstructorSignatures(IEnumerable<ConstructSignature> constructors)
        {
            constructors.Apply(this.WriteConstructorSignature);
        }

        private void WriteConstructorSignature(ConstructSignature constructor)
        {
            var docs = this.documentationProvider.GetDoc(constructor);
            this.WriteDocComment(docs);

            this.output.WriteIndent();
            this.output.Write("constructor");
            this.WriteParameters(constructor.Parameters);
            this.output.WriteLine(";");
        }

        private void WriteEnum(EnumDeclaration @enum)
        {
            var docs = this.documentationProvider.GetDoc(@enum);
            this.WriteDocComment(docs);

            this.output.WriteIndent();

            // NOTE: If we are in declared module we do not have to write declare here.
            this.output.Write("declare ");
            this.output.Write("enum ");
            this.output.Write(@enum.Name);
            this.output.WriteLine(" {");

            using (this.output.EnterIndentScope())
            {
                // members
                this.WriteEnumMembers(@enum);
            }

            this.WriteIndent();
            this.WriteLine("}");
        }

        private void WriteEnumMembers(EnumDeclaration @enum)
        {
            @enum.Members.ApplyWithSeparators(null, this.WriteEnumMember, this.WriteCommaSeparatorNewLine, this.WriteLine);
        }

        private void WriteEnumMember(EnumElement enumElement)
        {
            this.output.WriteIndent();
            this.output.Write(enumElement.Name);
            if (!string.IsNullOrWhiteSpace(enumElement.Value))
            {
                this.Write(" = ");
                this.Write(enumElement.Value);
            }
        }

        private void WriteVar(VariableStatement @var)
        {
            var docs = this.documentationProvider.GetDoc(@var);
            this.WriteDocComment(docs);

            this.WriteIndent();
            this.Write("declare ");
            this.Write("var ");
            this.Write(@var.Name);

            this.WriteTypeAnnotation(@var.TypeAnnotation);
            this.WriteLine(";");
        }

        private void WriteTypeAnnotation(IType type)
        {
            if (type == null)
            {
                return;
            }

            this.WriteTypeAnnotationColon();
            this.WriteType(type);
        }

        private void WriteDocComment(string docs)
        {
            if (!string.IsNullOrWhiteSpace(docs))
            {
                this.WriteIndent();
                this.WriteLine("/**");

                foreach (var line in docs.Split('\n'))
                {
                    this.WriteIndent();
                    this.Write(" * ");
                    this.Write(line);
                    this.WriteLine();
                }

                this.WriteIndent();
                this.WriteLine(" */");
            }
        }

        private void WriteIndent()
        {
            this.output.WriteIndent();
        }

        private void Write(string text)
        {
            this.output.Write(text);
        }

        private void WriteLine()
        {
            this.output.WriteLine();
        }

        private void WriteLine(string text)
        {
            this.output.WriteLine(text);
        }

        private void WriteType(IType obj)
        {
            this.output.WriteType(obj);
        }
    }
}
