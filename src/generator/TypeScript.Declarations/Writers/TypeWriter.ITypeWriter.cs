namespace TypeScript.Declarations.Writers
{
    using System.Linq;
    using TypeScript.Declarations.Model;

    internal partial class TypeWriter : ITypeWriter
    {
        private IIndentWriter writer;

        public TypeWriter(IIndentWriter textWriter)
        {
            this.writer = textWriter;
        }

        public void WriteType(IType type)
        {
            type.Accept(this);
        }

        private void WriteClass(ClassDeclaration classDeclaration)
        {
            this.Write(classDeclaration.Name);
        }

        private void WriteInterface(InterfaceDeclaration interfaceDeclaration)
        {
            this.Write(interfaceDeclaration.Name);
        }

        private void WriteArray(ArrayType array)
        {
            this.WriteType(array.ComponentType);
            this.WriteArrayBrackets();
        }

        private void WriteObjectType(ObjectType objectType)
        {
            this.WriteOpeningCurlyBracket();
            objectType.Properties.ApplyWithSeparators(this.WriteProperty, this.WriteSemicolonSeparator);
            if (objectType.Properties.Any() && objectType.Indexers.Any())
            {
                this.WriteSemicolonSeparator();
            }

            objectType.Indexers.ApplyWithSeparators(this.WriteIndexer, this.WriteSemicolonSeparator);
            this.WriteClosingCurlyBracket();
        }

        private void WriteFunctionType(FunctionType functionType)
        {
            this.WriteOpeningParenthesis();
            functionType.Parameters.ApplyWithSeparators(this.WriteParameter, this.WriteCommaSeparator);
            this.WriteClosingParenthesis();
            this.WriteLambdaReturnType(functionType.ReturnType);
        }

        private void WriteProperty(PropertySignature obj)
        {
            this.writer.Write(obj.Name);
            if (obj.IsOptional)
            {
                this.WriteQuestionMark();
            }

            this.WriteTypeAnnotation(obj.TypeAnnotation);
        }

        private void WriteIndexer(IndexerSignature obj)
        {
            this.WriteOpeningSquareBracket();
            this.Write(obj.KeyName);
            this.WriteTypeAnnotation(obj.KeyType);
            this.WriteClosingSquareBracket();
            this.WriteTypeAnnotation(obj.ComponentType);
        }

        private void WriteParameter(Parameter parameter)
        {
            this.Write(parameter.Name);
            this.WriteTypeAnnotation(parameter.TypeAnnotation);
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

        private void WriteLambdaReturnType(IType type)
        {
            this.WriteTypeAnnotationArrow();
            this.WriteType(type == null ? PrimitiveTypes.Void : type);
        }

        private void Write(string text)
        {
            this.writer.Write(text);
        }

        private void WriteLine(string text)
        {
            this.writer.WriteLine(text);
        }

        private void WriteLine()
        {
            this.writer.WriteLine();
        }
    }
}
