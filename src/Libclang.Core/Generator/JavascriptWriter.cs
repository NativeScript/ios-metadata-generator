using System;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Types;

namespace Libclang.Core.Generator
{
    public class JavascriptWriter : BaseWriter
    {
        public JavascriptWriter(DocumentDeclaration documentDeclaration, IFormatter formatter)
            : base(documentDeclaration, formatter)
        {
        }

        protected override void VisitDocumentDeclaration(DocumentDeclaration documentDeclaration)
        {
            formatter.WriteLine("var {0}Bindings = {{", documentDeclaration.Name);
            formatter.Indent();

            base.VisitDocumentDeclaration(documentDeclaration);

            formatter.Outdent();
            formatter.WriteLine("};");
        }

        protected override void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            formatter.WriteLine("{0}: interop.ForeignFunction(null, \"{0}\", {1}, [{2}]),",
                functionDeclaration.Name,
                MapType(functionDeclaration.ReturnType),
                string.Join(", ", functionDeclaration.Parameters.Select(x => MapType(x.Type)))
                );

            base.VisitFunctionDeclaration(functionDeclaration);
        }

        protected override void VisitStructDeclaration(StructDeclaration structDeclaration)
        {
            formatter.WriteLine("{0}: new interop.StructType({{", structDeclaration.Name);
            formatter.Indent();

            foreach (var structFieldDeclaration in structDeclaration.Fields)
            {
                formatter.WriteLine("{0}: {1},", structFieldDeclaration.Name, MapType(structFieldDeclaration.Type));
            }

            formatter.Outdent();
            formatter.WriteLine("}),");
        }

        private static string MapType(TypeDefinition type)
        {
            return type.ToString();
        }
    }
}
