using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Types;

namespace Libclang.Core.Generator
{
    //public static class MethodCallLoggerGenerator
    //{
    //    public static void Generate(IEnumerable<DocumentDeclaration> frameworks, string directory)
    //    {
    //        var allInterfaces = frameworks.SelectMany(x => x.Interfaces).DistinctBy(x => x.Name).OrderBy(x => x.Name);

    //        var declarationPreprocessor = new DeclarationsPreprocessor();
    //        declarationPreprocessor.Process(frameworks.ToList());

    //        var chunks = allInterfaces.Chunk(100).ToList();
    //        for (int i = 0; i < chunks.Count; i++)
    //        {
    //            using (var writer = new StreamWriter(Path.Combine(directory, string.Format("Data{0}.m", i))))
    //            {
    //                var formatter = new Formatter(writer);

    //                formatter.WriteLine(@"NSMutableString *output;");
    //                formatter.WriteLine();

    //                foreach (var @interface in chunks[i])
    //                {
    //                    GetImplementation(formatter, @interface, declarationPreprocessor.AllInterfacesToMethods[@interface.Name].DistinctBy(x => x.Name));
    //                    formatter.WriteLine();
    //                }
    //            }
    //        }
    //    }

    //    private static void GetImplementation(Formatter formatter, InterfaceDeclaration @interface, IEnumerable<MethodDeclaration> methods)
    //    {
    //        formatter.WriteLine("@implementation TNS_{0} : {0}", @interface.Name);

    //        foreach (var method in methods)
    //        {
    //            if (method.IsVariadic)
    //            {
    //                continue;
    //            }
    //            if (method.Name.IsEqualToAny("retain", "release", "load", "initialize", "alloc", "allocWithZone", "init"))
    //            {
    //                continue;
    //            }

    //            formatter.WriteLine("{0} {1} {{", (method.IsStatic ? "+" : "-"), BaseTNSBridgeWriter.GetMethodSignature(method, isJsType: false));
    //            formatter.Indent();

    //            formatter.WriteLine("[output appendString:@\"__CALLING: {0} - {1}\\n\"];", @interface.Name, method.Name);

    //            if (!method.ReturnType.IsVoid())
    //            {
    //                formatter.Write("return ");
    //            }

    //            formatter.WriteLine("[super {0}];", BaseTNSBridgeWriter.GetMessage(method, method.Parameters.Select(x => x.Name).ToList()));

    //            formatter.Outdent();
    //            formatter.WriteLine("}");
    //        }

    //        formatter.WriteLine("@end");
    //    }
    //}
}
