using Libclang.Core.Ast;
using NClang;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Libclang.Core.Parser
{
    public partial class FrameworkParser
    {
        public ICollection<KeyValuePair<string, IList<Types.DeclarationReferenceType>>> UnresolvedDeclarationReferences
        { get; private set; }

        public virtual IEnumerable<DocumentDeclaration> Parse(string filePath, string sdkPath, string cflags, string architecture)
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

            this.UnresolvedDeclarationReferences = context.usrToUnresolvedReferences;

            return context.documents;
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
