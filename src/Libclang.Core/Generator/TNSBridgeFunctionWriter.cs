using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Types;

namespace Libclang.Core.Generator
{
    public class TNSBridgeFunctionWriter : BaseTNSBridgeWriter
    {
        private readonly List<DocumentDeclaration> frameworks;

        public TNSBridgeFunctionWriter(List<DocumentDeclaration> frameworks,
            MultiDictionary<FunctionDeclaration, BaseRecordDeclaration> functionToRecords)
            : base(functionToRecords)
        {
            this.frameworks = frameworks;
        }

        protected override string JSContext
        {
            get { return "[JSContext currentContext]"; }
        }

        protected override string JSContextRef
        {
            get { return "[" + JSContext + " JSGlobalContextRef]"; }
        }

        public void Generate(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                new DirectoryInfo(directory).Clear();
            }

            List<FunctionDeclaration> functions =
                frameworks.SelectMany(x => x.Functions)
                    .Where(x => x.IsValidFunction())
                    .DistinctBy(x => x.Name)
                    .OrderBy(x => x.GetFrameworkName())
                    .ToList();

            using (var writer = new StreamWriter(Path.Combine(directory, "Functions.h")))
            {
                writer.WriteLine("NSDictionary *TNSGetFunctionsMap();");
            }

            using (var writer = new StreamWriter(Path.Combine(directory, "Functions.m")))
            {
                IFormatter formatter = new Formatter(writer);
                formatter.WriteLine("#import <TNSBridgeInfrastructure/TNSRefValue.h>");
                formatter.WriteLine("#import <TNSBridgeInfrastructure/TNSBuffer.h>");
                formatter.WriteLine("#import <TNSBridgeInfrastructure/BigIntWrapper.h>");
                formatter.WriteLine("#import <TNSBridgeInfrastructure/Variadics.h>");
                formatter.WriteLine("#import <ffi.h>");

                foreach (
                    string header in
                        functions.Where(x => functionToRecords.ContainsKey(x))
                            .SelectMany(x => functionToRecords[x])
                            .Select(x => x.GetFileName())
                            .Distinct())
                {
                    formatter.WriteLine("#import \"{0}\"", header + ".h");
                }

                formatter.WriteLine();

                formatter.WriteLine(GetFunctionPointerBindings(functions));
                formatter.WriteLine();

                formatter.WriteLine("NSDictionary *TNSGetFunctionsMap() {");
                formatter.Indent();

                formatter.WriteLine("static NSMutableDictionary *result = nil;");
                formatter.WriteLine();

                formatter.WriteLine("if (result == nil) {");
                formatter.Indent();

                formatter.WriteLine("result = [NSMutableDictionary new];");

                foreach (FunctionDeclaration function in functions)
                {
                    formatter.Write("result[@\"{0}\"] = ", function.Name);
                    GenerateBlockForFunction(formatter, function);
                    formatter.WriteLine(";");
                    formatter.WriteLine();
                }

                formatter.Outdent();
                formatter.WriteLine("}");

                formatter.WriteLine("return result;");

                formatter.Outdent();
                formatter.WriteLine("}");
            }
        }
    }
}
