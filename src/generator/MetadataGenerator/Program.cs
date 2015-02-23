using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Common;
using MetadataGenerator.Core.Generator;
using MetadataGenerator.Core.Parser;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MetadataGenerator
{
    internal static class Program
    {
        private static string OutputPath;
        private static string XCodePath = @"/Applications/Xcode.app/Contents/Developer";

        private static string TypeScriptDirectoryPath;
        private static string TypeScriptDocs;

        private static void Main(string[] args)
        {
#if DEBUG
            Debug.Listeners.Add(new ConsoleTraceListener());
#endif

            var start = DateTime.Now;
            string sdkPath = "", umbrellaHeader = "", cflags = "";

            var optionSet = new OptionSet()
            {
                {"x|xcodePath=", "The path of the active developer directory of Xcode", v => XCodePath = v},
                {
                    "s|iosSDKPath=",
                    "Path to ios sdk for which metadata will be generated. Has precedence over that in xcode.",
                    v => sdkPath = v
                },
                {"u|header=", "Umbrella header file name", v => umbrellaHeader = v},
                {"o|output=", "Output path for the metadata file", v => OutputPath = v},
                {"ts|typescript=", "Output directory for TypeScript declarations", v => TypeScriptDirectoryPath = v},
                {"tsdoc|typescript-doc=", "TypeScript documentation options: mapping - print detailed iOS to JS mapping attributes", v => TypeScriptDocs = v},
                {"cflags=", "Additional arguments that will be passed to clang", v => cflags = v},
            };

            try
            {
                optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            if (string.IsNullOrEmpty(sdkPath))
            {
                sdkPath = System.IO.Path.Combine(XCodePath, @"Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk");
            }

            // Generate two metadata files in parallel
            Parallel.Invoke(
                () => GenerateAllBindings(umbrellaHeader, sdkPath, cflags, "armv7"),
                () => GenerateAllBindings(umbrellaHeader, sdkPath, cflags, "arm64")
            );

            Console.WriteLine(DateTime.Now - start);
        }

        private static void GenerateAllBindings(string umbrellaHeaderPath, string sdkPath, string cflags, string architecture)
        {
            List<ModuleDeclaration> frameworks =
                ParseIOSFrameworks(umbrellaHeaderPath, sdkPath, cflags, architecture).ToList();

            string outputPath = string.Format("Metadata-{0}", architecture);
            GenerateMetadata(frameworks, outputPath);
        }

        private static IEnumerable<ModuleDeclaration> ParseIOSFrameworks(string umbrellaHeaderPath, string sdkPath,
            string cflags, string architecture)
        {
            FrameworkParser parser = new FrameworkParser();
            Console.WriteLine("Parsing {0}", umbrellaHeaderPath);
            IEnumerable<ModuleDeclaration> parsedFrameworks = parser.Parse(umbrellaHeaderPath, sdkPath, cflags, architecture);

            return parsedFrameworks;
        }

        private static void GenerateMetadata(IEnumerable<ModuleDeclaration> frameworks, string folderName)
        {
            Console.WriteLine("Generating Metadata ({0})...", folderName);
            new DirectoryInfo(folderName).Clear();
            var finalFrameworks = new DeclarationsPreprocessor()
                .Process(frameworks)
                .Where(c => c.Parent == null);

            string finalOutputPath = folderName;
            if (!string.IsNullOrEmpty(OutputPath))
            {
                finalOutputPath = Path.Combine(OutputPath, folderName);
            }
            if (!string.IsNullOrWhiteSpace(finalOutputPath))
            {
                Directory.CreateDirectory(finalOutputPath);
            }
            var generator = new MetadataGenerator.Core.Meta.MetadataGenerator(finalOutputPath);
            generator.GenerateMetadata(finalFrameworks);
        }

        private static bool ShouldGenerateTypeScriptDeclarations()
        {
            return !string.IsNullOrWhiteSpace(TypeScriptDirectoryPath);
        }

        private static void CreateTypeScriptDirectory()
        {
            if (!Directory.Exists(TypeScriptDirectoryPath))
            {
                Directory.CreateDirectory(TypeScriptDirectoryPath);
            }
        }

        private static void GenerateTypeScriptDeclarations(MetadataGenerator.Core.Meta.Utils.ModuleDeclarationsContainer container)
        {
            CreateTypeScriptDirectory();

            var typeScriptBuilder = new TypeScript.Factory.TypeScriptFactory();
            typeScriptBuilder.MetaContainer = container;
            typeScriptBuilder.Create();

            var writer = new StringWriter();
            var docsProvider = CreateTypeScriptDocumentationProvider();
            TypeScript.Declarations.DeclarationWriter.Write(writer, typeScriptBuilder.Global, docsProvider);

            var text = writer.ToString();
            File.WriteAllText(Path.Combine(TypeScriptDirectoryPath, "iOS.d.ts"), text);
        }

        private static TypeScript.Declarations.Writers.DocumentationProvider CreateTypeScriptDocumentationProvider()
        {
            switch (TypeScriptDocs)
            {
                case "mapping": return new TypeScript.Factory.SourceTrackingDocumentationProvider();
            }

            return null;
        }
    }
}
