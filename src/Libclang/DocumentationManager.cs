using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using Libclang.Core.Ast;
using Libclang.Core.Generator;
using Libclang.Core.Parser;
using Libclang.DocsetParser;

namespace Libclang
{
    internal class DocumentationManager
    {
        private readonly string docsetDirectory;

        private readonly IEnumerable<TokenMetadata> metadata;

        public DocumentationManager(string docsetDirectory, IEnumerable<TokenMetadata> metadata)
        {
            if (!Directory.Exists(docsetDirectory))
            {
                throw new ArgumentException("Docset directory does not exist");
            }

            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            this.docsetDirectory = docsetDirectory;
            this.metadata = metadata.ToList();
        }

        public void Document(string frameworksPath, string frameworksOutputPath)
        {
            var startTime = DateTime.Now;

            Console.WriteLine("Loading frameworks...");
            var nameToDocumentElement = LoadFrameworkElements(frameworksPath);

            #region C

            var cPath = Path.Combine(docsetDirectory, "C");

            var nameToFunctions = GroupDeclarationsByName(nameToDocumentElement.Values, "function");
            string functionPath = Path.Combine(cPath, "func", "-");
            Console.WriteLine("Documenting func: {0}", Directory.EnumerateFiles(functionPath, "*.xml").Count());
            AddDocumentationToElements(functionPath, nameToFunctions);

            #endregion

            #region Objective-C

            var objcPath = Path.Combine(docsetDirectory, "Objective-C");

            var nameToInterfaceAndCategory = GetNameToInterfaceAndCategories(nameToDocumentElement.Values);
            EnumerateParentDirectories(Path.Combine(objcPath, "clm"), nameToInterfaceAndCategory, members =>
                members.SelectMany(x => x.Elements("method")).Where(x => x.HasTrueAttribute("static")));
            EnumerateParentDirectories(Path.Combine(objcPath, "instm"), nameToInterfaceAndCategory, members =>
                members.SelectMany(x => x.Elements("method")).Where(x => !x.HasTrueAttribute("static")));
            EnumerateParentDirectories(Path.Combine(objcPath, "instp"), nameToInterfaceAndCategory, members =>
                members.SelectMany(x => x.Elements("property")));

            var nameToProtocol = GroupDeclarationsByName(nameToDocumentElement.Values, "protocol");
            EnumerateParentDirectories(Path.Combine(objcPath, "intfcm"), nameToProtocol, members =>
                members.SelectMany(x => x.Elements("method")).Where(x => x.HasTrueAttribute("static")));
            EnumerateParentDirectories(Path.Combine(objcPath, "intfm"), nameToProtocol, members =>
                members.SelectMany(x => x.Elements("method")).Where(x => !x.HasTrueAttribute("static")));
            EnumerateParentDirectories(Path.Combine(objcPath, "intfp"), nameToProtocol, members =>
                members.SelectMany(x => x.Elements("property")));

            #endregion

            #region Enum, Record, Var

            var nameToEnums = GroupDeclarationsByName(nameToDocumentElement.Values, "enum");
            var nameToEnumMembers = GroupDeclarationsByName(nameToEnums.Values.SelectMany(x => x), "field");
            var enumMetadata = metadata.Where(x => x.Type.Equals(typeof (EnumDeclaration))).ToList();
            Console.WriteLine("Documenting enums: ", enumMetadata.Count);
            foreach (var data in enumMetadata)
            {
                if (nameToEnums.ContainsKey(data.Name))
                {
                    AddDocumentationToElements(nameToEnums[data.Name], MakeDocumentationFromMetadata(data, "Enum"));
                }
                else if (nameToEnumMembers.ContainsKey(data.Name))
                {
                    AddDocumentationToElements(nameToEnumMembers[data.Name],
                        MakeDocumentationFromMetadata(data, "EnumMember"));
                }
                else
                {
                    Console.WriteLine("Missing enum: " + data);
                }
            }

            var nameToStruct = GroupDeclarationsByName(nameToDocumentElement.Values, "struct");
            var nameToUnion = GroupDeclarationsByName(nameToDocumentElement.Values, "union");
            var recordMetadata = metadata.Where(x => x.Type.Equals(typeof (BaseRecordDeclaration))).ToList();
            Console.WriteLine("Documenting records: ", recordMetadata.Count);
            foreach (var data in recordMetadata)
            {
                if (nameToStruct.ContainsKey(data.Name))
                {
                    AddDocumentationToElements(nameToStruct[data.Name], MakeDocumentationFromMetadata(data, "Struct"));
                }
                else if (nameToUnion.ContainsKey(data.Name))
                {
                    AddDocumentationToElements(nameToUnion[data.Name], MakeDocumentationFromMetadata(data, "Union"));
                }
                else
                {
                    Console.WriteLine("Missing record: " + data);
                }
            }

            var nameToTypedef = GroupDeclarationsByName(nameToDocumentElement.Values, "typedef");
            var typedefMetadata = metadata.Where(x => x.Type.Equals(typeof (TypedefDeclaration))).ToList();
            Console.WriteLine("Documenting typedefs: ", typedefMetadata.Count);
            foreach (var data in typedefMetadata)
            {
                if (nameToTypedef.ContainsKey(data.Name))
                {
                    AddDocumentationToElements(nameToTypedef[data.Name], MakeDocumentationFromMetadata(data, "Typedef"));
                }
                else
                {
                    Console.WriteLine("Missing typedef: " + data);
                }
            }

            var nameToVar = GroupDeclarationsByName(nameToDocumentElement.Values, "var");
            var varMetadata = metadata.Where(x => x.Type.Equals(typeof (VarDeclaration))).ToList();
            Console.WriteLine("Documenting vars: ", varMetadata.Count);
            foreach (var data in varMetadata)
            {
                if (nameToVar.ContainsKey(data.Name))
                {
                    AddDocumentationToElements(nameToVar[data.Name], MakeDocumentationFromMetadata(data, "Var"));
                }
                else
                {
                    Console.WriteLine("Missing var: " + data);
                }
            }

            #endregion

            Console.WriteLine("Saving frameworks...");
            SaveFrameworkElements(nameToDocumentElement, frameworksOutputPath);
            Console.WriteLine("Finished in: " + (DateTime.Now - startTime));
        }

        // Windows filesystem doesn't support ':' in filenames, so we replace it with '#' on the Mac side. One way to do this is:
        // $ find . -depth -name '*:*' -execdir bash -c 'mv "$1" "${1//:/#}"' _ {} \;
        private string NormalizeName(string method)
        {
            return method.Replace("#", ":");
        }

        private string GetName(XElement element)
        {
            return element.Attribute("name").Value;
        }

        private IDictionary<string, IEnumerable<XElement>> GroupDeclarationsByName(
            IEnumerable<XElement> documentElements, string type, string attribute = "name")
        {
            var nameToElements = documentElements
                .SelectMany(x => x.Elements(type))
                .GroupBy(x => x.Attribute(attribute).Value)
                .ToDictionary(x => x.Key, x => (IEnumerable<XElement>) x.ToList());
            return nameToElements;
        }

        private IDictionary<string, IEnumerable<XElement>> GetNameToInterfaceAndCategories(
            IEnumerable<XElement> elements)
        {
            var nameToInterfaceElements = GroupDeclarationsByName(elements, "interface");
            var nameToCategoryElements = GroupDeclarationsByName(elements, "category", "interface");

            return nameToInterfaceElements.ToDictionary(x => x.Key, x =>
            {
                var result = new List<XElement>(x.Value);

                if (nameToCategoryElements.ContainsKey(x.Key))
                {
                    result.AddRange(nameToCategoryElements[x.Key]);
                }

                return (IEnumerable<XElement>) result;
            });
        }

        private void EnumerateParentDirectories(string path,
            IDictionary<string, IEnumerable<XElement>> nameToParentElements,
            Func<IEnumerable<XElement>, IEnumerable<XElement>> filter)
        {
            var documentationDirectories = Directory.GetDirectories(path);
            Console.WriteLine("Documenting {0}: {1}", Path.GetFileName(path), documentationDirectories.Length);
            foreach (var documentationDirectory in documentationDirectories)
            {
                var parrentName = Path.GetFileNameWithoutExtension(documentationDirectory);
                if (!nameToParentElements.ContainsKey(parrentName))
                {
                    Console.WriteLine("  Parent not found: " + parrentName);
                    continue;
                }

                var nameToNested = filter(nameToParentElements[parrentName])
                    .GroupBy(x => GetName(x))
                    .ToDictionary(x => x.Key, x => (IEnumerable<XElement>) x.ToList());

                AddDocumentationToElements(documentationDirectory, nameToNested, parrentName);
            }
        }

        private void AddDocumentationToElements(string documentationDirectory,
            IDictionary<string, IEnumerable<XElement>> nameToMembers, string parentName = "c")
        {
            foreach (var documentationEntry in Directory.GetFiles(documentationDirectory, "*.xml"))
            {
                var name = NormalizeName(Path.GetFileNameWithoutExtension(documentationEntry));
                if (!nameToMembers.ContainsKey(name))
                {
                    Console.WriteLine("    Member not found: {0}.{1}", parentName, name);
                    continue;
                }

                var documentationEntryContent = XDocument.Load(documentationEntry).Root;
                AddDocumentationToElements(nameToMembers[name], documentationEntryContent);
            }
        }

        private void AddDocumentationToElements(IEnumerable<XElement> elements, XElement documentation)
        {
            var wrapper = new XElement("documentation", documentation);
            foreach (var element in elements)
            {
                if (element.Element("documentation") != null)
                {
                    Console.WriteLine("Duplicate documentation: {0}, {1}",
                        element.Element("documentation").ToString().Length, wrapper.ToString().Length);
                    continue;
                }

                element.AddFirst(wrapper);
            }
        }

        private XElement MakeDocumentationFromMetadata(TokenMetadata data, string type)
        {
            return new XElement(type,
                new XElement("Abstract", data.Abstract),
                new XElement("Declaration", data.Declaration),
                new XElement("Anchor"), data.Anchor,
                new XElement("DeprecationSummary", data.DeprecationSummary)
                );
        }

        private IDictionary<string, XElement> LoadFrameworkElements(string frameworksPath)
        {
            return Directory.EnumerateFiles(frameworksPath).ToDictionary(
                x => Path.GetFileNameWithoutExtension(x),
                x => XDocument.Load(x).Root);
        }

        private void SaveFrameworkElements(IDictionary<string, XElement> frameworkElements, string frameworksOutputPath)
        {
            Directory.CreateDirectory(frameworksOutputPath);
            new DirectoryInfo(frameworksOutputPath).Clear();

            foreach (var item in frameworkElements)
            {
                var outputPath = Path.Combine(frameworksOutputPath, item.Key + ".xml");
                item.Value.Save(outputPath);
            }
        }
    }
}
