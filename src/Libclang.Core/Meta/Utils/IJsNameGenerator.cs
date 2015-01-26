using System;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Generator;
using System.Text;
using System.Collections.Generic;

namespace Libclang.Core.Meta.Utils
{
    public interface IJsNameGenerator
    {
        string GenerateJsName(BaseDeclaration declaration);

        string TryResolveConflict(string jsName, BaseDeclaration declaration);
    }

    public class DefaultJsNameGenerator : IJsNameGenerator
    {
        public string GenerateJsName(BaseDeclaration declaration)
        {
            if (declaration is BaseRecordDeclaration)
            {
                return (declaration as BaseRecordDeclaration).PublicName;
            }
            else if (declaration is EnumDeclaration)
            {
                return (declaration as EnumDeclaration).PublicName;
            }
            else if (declaration is MethodDeclaration)
            {
                return GenerateMethodJsName(declaration as MethodDeclaration);
            }
            else if (declaration is PropertyDeclaration)
            {
                var property = (PropertyDeclaration)declaration;
                var @interface = property.Parent as InterfaceDeclaration;
                if (@interface != null)
                {
                    var methods = @interface
                        .AllMethods()
                        .Where(method => !method.IsStatic
                            && !method.IsImplicit
                            && method.Name == property.Name
                            && property.Getter != method
                            && property.Setter != method);

                    if (methods.Any())
                    {
                        return property.Name + "Property";
                    }
                }
            }

            return declaration.Name;
        }

        private string GenerateMethodJsName(MethodDeclaration method)
        {
            string[] methodNameTokens = method.Name.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder result = new StringBuilder(methodNameTokens[0]);
            for (int i = 1; i < methodNameTokens.Length; i++)
            {
                result.Append(Char.ToUpper(methodNameTokens[i][0]));
                result.Append(methodNameTokens[i].Substring(1));
            }

            return result.ToString();
        }

        public string TryResolveConflict(string jsName, BaseDeclaration declaration)
        {
            if (declaration is StructDeclaration)
            {
                return jsName + "Struct";
            }
            else if (declaration is UnionDeclaration)
            {
                return jsName + "Union";
            }
            else if (declaration is VarDeclaration)
            {
                return jsName + "Var";
            }
            else if (declaration is FunctionDeclaration)
            {
                return jsName + "Function";
            }
            else if (declaration is EnumDeclaration)
            {
                return jsName + "Enum";
            }
            else if (declaration is InterfaceDeclaration)
            {
                return jsName + "Interface";
            }
            else if (declaration is ProtocolDeclaration)
            {
                return jsName + "Protocol";
            }

            return jsName;
        }
    }
}
