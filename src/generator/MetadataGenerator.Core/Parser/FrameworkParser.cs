using MetadataGenerator.Core.Ast;
using NClang;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MetadataGenerator.Core.Parser
{
    public partial class FrameworkParser
    {
        public virtual IEnumerable<ModuleDeclaration> Parse(string filePath, string sdkPath, string cflags, string architecture)
        {
            if (!File.Exists(filePath))
                throw new ArgumentException("The path to the umbrella header is not valid",
                    new FileNotFoundException(filePath));
            if (!Directory.Exists(sdkPath))
                throw new ArgumentException("The path to ios sdk is not valid", new DirectoryNotFoundException(sdkPath));

            var clangArgs = new List<string>()
            {
                "-v",
                "-x",
                "objective-c",
                "-arch",
                architecture,
                "-target",
                "arm-apple-darwin",
                "-std=gnu99",
                "-miphoneos-version-min=7.0",
                "-fmodule-maps",
                "-fmodule-map-file=" + Path.Combine(sdkPath, "usr", "include", "module.map"),
                "-fmodule-map-file=" + Path.Combine(sdkPath, "usr", "include", "dispatch", "module.map"),
                "-I", Path.Combine(sdkPath, "usr", "include", "objc"),
                "-isysroot",
                sdkPath
            };
            if (!string.IsNullOrEmpty(cflags))
            {
                foreach (string clangArg in System.Text.RegularExpressions.Regex.Split(cflags.Trim(), @"\s+"))
                {
                    clangArgs.Add(clangArg);
                }
            }

            ParserContext context = new ParserContext(sdkPath);

            using (ClangIndex index = ClangService.CreateIndex())
            {
                ClangTranslationUnit translationUnit;
                ErrorCode error = index.ParseTranslationUnit(filePath, clangArgs.ToArray(), new ClangUnsavedFile[0],
                    TranslationUnitFlags.SkipFunctionBodies, out translationUnit);
                if (error == ErrorCode.Success)
                {
                    using (translationUnit)
                    {
                        foreach (var diagnostic in translationUnit.DiagnosticSet.Items)
                        {
                            string message =
                                diagnostic.Format(DiagnosticDisplayOptions.DisplayOption |
                                                  DiagnosticDisplayOptions.DisplaySourceLocation);

                            if (diagnostic.Severity == DiagnosticSeverity.Fatal || diagnostic.Severity == DiagnosticSeverity.Error)
                            {
                                throw new InvalidOperationException(message);
                            }

                            Console.WriteLine(message);
                        }

                        CDeclarationVisitor cVisitor = new CDeclarationVisitor(context);
                        ObjCDeclarationVisitor objcVisitor = new ObjCDeclarationVisitor(context);

                        VisitDeclarations(index, translationUnit, cVisitor, objcVisitor);
                    }
                }
                else
                {
                    Console.WriteLine("*** ERROR PARSING TranslationUnit ***");
                    Console.WriteLine("ERROR CODE: " + error);
                }
            }

            foreach (KeyValuePair<string, IList<MetadataGenerator.Core.Types.DeclarationReferenceType>> item in context.usrToUnresolvedReferences)
            {
                foreach (MetadataGenerator.Core.Types.DeclarationReferenceType unresolvedReference in item.Value)
                {
                    unresolvedReference.Target = new UnresolvedDeclaration(item.Key);
                }
            }

            return context.modules;
        }

        private static void VisitDeclarations(ClangIndex index, ClangTranslationUnit translationUnit,
            params IDeclarationVisitor[] visitors)
        {
            using (var indexAction = index.CreateIndexAction())
            {
                var callback = new ClangIndexerCallbacks();
                foreach (var visitor in visitors)
                {
                    callback.IndexDeclaration +=
                        (IntPtr clientData, ClangIndex.DeclarationInfo declaration) =>
                            visitor.VisitDeclaration(declaration);
                }

                indexAction.IndexTranslationUnit(IntPtr.Zero, new[] {callback}, IndexOptionFlags.None, translationUnit);
            }
        }
    }
}
