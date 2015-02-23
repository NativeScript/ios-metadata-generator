using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Meta.Visitors;
using System;
using System.IO;

namespace MetadataGenerator.Core.Meta.Filters
{
    internal class SymbolsCounter : IMetaContainerDeclarationVisitor
    {
        public int Structs { get; private set; }

        public int Unions { get; private set; }

        public int Functions { get; private set; }

        public int Vars { get; private set; }

        public int Enumerations { get; private set; }

        public int Protocols { get; private set; }

        public int Interfaces { get; private set; }

        public int Categories { get; private set; }

        public int Methods { get; private set; }

        public int PropertiesMethods { get; private set; } // Getters and setters

        public int Properties { get; private set; }

        public SymbolsCounter(string outputfile)
        {
            this.outputfile = outputfile;
        }

        private string outputfile;

        public void Visit(Ast.InterfaceDeclaration declaration)
        {
            this.Interfaces++;
        }

        public void Visit(Ast.ProtocolDeclaration declaration)
        {
            this.Protocols++;
        }

        public void Visit(Ast.CategoryDeclaration declaration)
        {
            this.Categories++;
        }

        public void Visit(Ast.StructDeclaration declaration)
        {
            this.Structs++;
        }

        public void Visit(Ast.UnionDeclaration declaration)
        {
            this.Unions++;
        }

        public void Visit(Ast.FieldDeclaration declaration)
        {
        }

        public void Visit(Ast.EnumDeclaration declaration)
        {
            this.Enumerations++;
        }

        public void Visit(Ast.EnumMemberDeclaration declaration)
        {
        }

        public void Visit(Ast.FunctionDeclaration declaration)
        {
            this.Functions++;
        }

        public void Visit(Ast.MethodDeclaration declaration)
        {
            this.Methods++;
        }

        public void Visit(Ast.ParameterDeclaration declaration)
        {
        }

        public void Visit(Ast.PropertyDeclaration declaration)
        {
            this.Properties++;
            if (declaration.Getter != null)
            {
                this.PropertiesMethods++;
            }
            if (declaration.Setter != null)
            {
                this.PropertiesMethods++;
            }
        }

        public void Visit(Ast.ModuleDeclaration declaration)
        {
            // TODO: Perhaps we should count that too
        }

        public void Visit(Ast.VarDeclaration declaration)
        {
            this.Vars++;
        }

        public void Visit(Ast.TypedefDeclaration declaration)
        {
        }

        public void Begin(Utils.ModuleDeclarationsContainer metaContainer)
        {
        }

        public void End(Utils.ModuleDeclarationsContainer metaContainer)
        {
            int topLevelSymbols = this.Structs + this.Unions + this.Functions + this.Vars + this.Enumerations +
                                  this.Interfaces + this.Protocols;
            int allSymbols = topLevelSymbols + this.Methods + this.Properties + this.PropertiesMethods;

            string folderPath = Path.GetDirectoryName(this.outputfile);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using (var fs = File.Open(this.outputfile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine("Structs: {0}", this.Structs);
                    writer.WriteLine("Unions: {0}", this.Unions);
                    writer.WriteLine("Functinos: {0}", this.Functions);
                    writer.WriteLine("Vars: {0}", this.Vars);
                    writer.WriteLine("Enumerations: {0}", this.Enumerations);
                    writer.WriteLine("Interfaces: {0}", this.Interfaces);
                    writer.WriteLine("Protocols: {0}", this.Protocols);
                    writer.WriteLine("Methods: {0}", this.Methods);
                    writer.WriteLine("Properties Methods(getters and setters): {0}", this.PropertiesMethods);
                    writer.WriteLine("Properties: {0}", this.Properties);
                    writer.WriteLine("-------------------------");
                    writer.WriteLine("Top Level Symbols: {0}", topLevelSymbols);
                    writer.WriteLine("All Symbols: {0}", allSymbols);
                    writer.WriteLine("(Categories: {0})", this.Categories);
                }
            }
        }
    }
}
