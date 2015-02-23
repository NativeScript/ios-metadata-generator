using Libclang.Core.Ast;
using System;
using System.Linq;
using System.Text;

namespace Libclang.Core.Meta.Utils
{
    public interface IJsNameGenerator
    {
        string GenerateJsName(InterfaceDeclaration declaration);

        string GenerateJsName(ProtocolDeclaration declaration);

        string GenerateJsName(CategoryDeclaration declaration);

        string GenerateJsName(StructDeclaration declaration);

        string GenerateJsName(UnionDeclaration declaration);

        string GenerateJsName(FieldDeclaration declaration);

        string GenerateJsName(EnumDeclaration declaration);

        string GenerateJsName(EnumMemberDeclaration declaration);

        string GenerateJsName(FunctionDeclaration declaration);

        string GenerateJsName(MethodDeclaration declaration);

        string GenerateJsName(ParameterDeclaration declaration);

        string GenerateJsName(PropertyDeclaration declaration);

        string GenerateJsName(ModuleDeclaration declaration);

        string GenerateJsName(VarDeclaration declaration);

        string GenerateJsName(TypedefDeclaration declaration);
    }

    public class DefaultJsNameGenerator : IJsNameGenerator
    {
        public DefaultJsNameGenerator(SymbolsNamesCollection duplicates)
        {
            this.duplicates = duplicates;
        }

        private readonly SymbolsNamesCollection duplicates;

        public string GenerateJsName(InterfaceDeclaration declaration)
        {
            string jsName = this.CalculateJsName(declaration,
                () => declaration.Name,
                c => c + "Interface");
            return jsName;
        }

        public string GenerateJsName(ProtocolDeclaration declaration)
        {
            string jsName = this.CalculateJsName(declaration,
                () => declaration.Name,
                c => c + "Protocol");
            return jsName;
        }

        public string GenerateJsName(CategoryDeclaration declaration)
        {
            string jsName = this.CalculateJsName(declaration,
                () => declaration.Name,
                c => c + "Category");
            return jsName;
        }

        public string GenerateJsName(StructDeclaration declaration)
        {
            string jsName = this.CalculateJsName(declaration,
                () => declaration.PublicName,
                c => c + "Struct");
            return jsName;
        }

        public string GenerateJsName(UnionDeclaration declaration)
        {
            string jsName = this.CalculateJsName(declaration,
                () => declaration.PublicName,
                c => c + "Union");
            return jsName;
        }

        public string GenerateJsName(FieldDeclaration declaration)
        {
            return declaration.Name;
        }

        public string GenerateJsName(EnumDeclaration declaration)
        {
            string jsName = this.CalculateJsName(declaration,
                () => declaration.PublicName,
                c => c + "Enum");
            return jsName;
        }

        public string GenerateJsName(EnumMemberDeclaration declaration)
        {
            return declaration.Name;
        }

        public string GenerateJsName(FunctionDeclaration declaration)
        {
            string jsName = this.CalculateJsName(declaration,
                () => declaration.Name,
                c => c + "Function");
            return jsName;
        }

        public string GenerateJsName(MethodDeclaration declaration)
        {
            string[] methodNameTokens = declaration.Name.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder result = new StringBuilder(methodNameTokens[0]);
            for (int i = 1; i < methodNameTokens.Length; i++)
            {
                result.Append(Char.ToUpper(methodNameTokens[i][0]));
                result.Append(methodNameTokens[i].Substring(1));
            }

            return result.ToString();
        }

        public string GenerateJsName(ParameterDeclaration declaration)
        {
            return declaration.Name;
        }

        public string GenerateJsName(PropertyDeclaration declaration)
        {
            string jsName = declaration.Name;
            var @interface = declaration.Parent as InterfaceDeclaration;
            if (@interface != null)
            {
                var methods = @interface
                    .AllMethods()
                    .Where(method => !method.IsStatic
                        && !method.IsImplicit
                        && method.Name == declaration.Name
                        && declaration.Getter != method
                        && declaration.Setter != method);

                if (methods.Any())
                {
                    jsName = declaration.Name + "Property";
                }
            }
            return jsName;
        }

        public string GenerateJsName(ModuleDeclaration declaration)
        {
            return declaration.Name;
        }

        public string GenerateJsName(VarDeclaration declaration)
        {
            string jsName = this.CalculateJsName(declaration,
                () => declaration.Name,
                c => c + "Var");
            return jsName;
        }

        public string GenerateJsName(TypedefDeclaration declaration)
        {
            string jsName = this.CalculateJsName(declaration,
                () => declaration.Name,
                c => c + "TypeDef");
            return jsName;
        }

        private string CalculateJsName(BaseDeclaration declaration, Func<string> generateJsName, Func<string, string> resolveConflict)
        {
            string jsName = generateJsName();
            if (this.duplicates.Contains(jsName, declaration.GetType()))
            {
                jsName = resolveConflict(jsName);
            }
            return jsName;
        }
    }
}
