using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Types;

namespace Libclang.Core.Generator
{
    public class TNSBridgeEnumWriter : BaseTNSBridgeWriter
    {
        private readonly List<DocumentDeclaration> frameworks;

        public TNSBridgeEnumWriter(List<DocumentDeclaration> frameworks)
            : base(null)
        {
            this.frameworks = frameworks;
        }

        protected override string JSContext
        {
            get { throw new NotImplementedException(); }
        }

        protected override string JSContextRef
        {
            get { throw new NotImplementedException(); }
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

            List<EnumDeclaration> enums = frameworks
                .SelectMany(x => x.Enums)
                .DistinctBy(x => x.GetNameOrTypedefName())
                .OrderBy(x => x.GetFrameworkName())
                .ToList();

            using (var writer = new StreamWriter(Path.Combine(directory, "Enumerations.h")))
            {
                writer.WriteLine("void TNSInitEnumerationExports(JSContext *context);");
            }

            using (var writer = new StreamWriter(Path.Combine(directory, "Enumerations.m")))
            {
                IFormatter formatter = new Formatter(writer);

                formatter.WriteLine("void TNSInitEnumerationExports(JSContext *context) {");
                formatter.Indent();

                formatter.WriteLine("JSValue *currentObject;");
                formatter.WriteLine();

                foreach (var @enum in enums)
                {
                    var frameworkName = @enum.GetFrameworkName();
                    var enumName = @enum.GetNameOrTypedefName();

                    if (enumName == null)
                    {
                        continue;
                    }

                    formatter.WriteLine("// " + @enum.GetNameOrTypedefName());
                    formatter.WriteLine(
                        "currentObject = context[@\"{0}\"][@\"{1}\"] = [JSValue valueWithNewObjectInContext:context];",
                        frameworkName, @enum.GetNameOrTypedefName());

                    foreach (var field in @enum.Fields)
                    {
                        if (field.Value > int.MaxValue || field.Value < int.MinValue)
                        {
                            Console.WriteLine(@enum.Name + " - " + field.Name + " - " + field.Value);
                        }

                        formatter.WriteLine("currentObject[@\"{2}\"] = @({2});", frameworkName,
                            @enum.GetNameOrTypedefName(), field.Name);
                    }
                    formatter.WriteLine();
                }

                formatter.WriteLine("currentObject = nil;");

                formatter.Outdent();
                formatter.WriteLine("}");
            }
        }
    }
}
