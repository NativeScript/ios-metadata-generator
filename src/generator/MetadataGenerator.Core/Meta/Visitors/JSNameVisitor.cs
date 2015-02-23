using System;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Meta.Utils;

namespace MetadataGenerator.Core.Meta.Visitors
{
    public class JSNameVisitor : IDeclarationVisitor
    {
        public JSNameVisitor()
        {
            SymbolsNamesCollection duplicates = new SymbolsNamesCollection();
            duplicates.Add(typeof(StructDeclaration), "kevent", "flock", "sigvec", "sigaction");
            duplicates.Add(typeof(UnionDeclaration), "wait");
            duplicates.Add(typeof(VarDeclaration), "timezone");
            duplicates.Add(typeof(ProtocolDeclaration), "NSObject", "AVVideoCompositionInstruction", "OS_dispatch_data");

            this.jsNameGenerator = new DefaultJsNameGenerator(duplicates);
        }

        private IJsNameGenerator jsNameGenerator;

        public void Visit(InterfaceDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(ProtocolDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(CategoryDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(StructDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(UnionDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(FieldDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(EnumDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(EnumMemberDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(FunctionDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(MethodDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(ParameterDeclaration declaration)
        {
        }

        public void Visit(PropertyDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);

            if (declaration.Getter != null)
            {
                declaration.Getter.Accept(this);
            }
            if (declaration.Setter != null)
            {
                declaration.Setter.Accept(this);
            }
        }

        public void Visit(ModuleDeclaration declaration)
        {
        }

        public void Visit(VarDeclaration declaration)
        {
            string jsName = this.jsNameGenerator.GenerateJsName(declaration);
            declaration.SetJSName(jsName);
        }

        public void Visit(TypedefDeclaration declaration)
        {
        }
    }
}
