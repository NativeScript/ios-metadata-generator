using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MetadataGenerator.Core.Parser
{
    public partial class FrameworkParser
    {
        public interface IModuleResolver
        {
            ModuleDeclaration GetModule(NClang.ClangCursor cursor);
        }

        class AutomaticModuleResolver : IModuleResolver
        {
            public AutomaticModuleResolver(IList<ModuleDeclaration> modules, string sdkPath)
            {
                this.modules = modules;
                this.sdkPath = sdkPath;
            }

            private IList<ModuleDeclaration> modules;
            private string sdkPath;

            public ModuleDeclaration GetModule(NClang.ClangCursor cursor)
            {
                var file = cursor.Location.FileLocation.File;
                NClang.ClangModule cModule = null;
                if (file == null)
                {
                    return null;

                }
                cModule = cursor.TranslationUnit.GetModuleForFile(file);
                ModuleDeclaration module = null;
                if (cModule != null)
                {
                    module = GetModule(cModule);
                }
                else if (file.FileName.EndsWith("sqlite3.h"))
                {
                    module = GetOrCreateModule("sqlite3", "sqlite3", c => { });
                }
                else if (!string.IsNullOrEmpty(this.sdkPath) && !file.FileName.StartsWith(sdkPath))
                {
                    string moduleName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                    module = GetOrCreateModule(moduleName, moduleName, c => { });
                }

                return module;
            }

            private ModuleDeclaration GetOrCreateModule(string name, string fullname, Action<ModuleDeclaration> initModule)
            {
                ModuleDeclaration module = this.modules.SingleOrDefault(c => c.FullName == fullname);
                if (module == null)
                {
                    module = new ModuleDeclaration(name, fullname);
                    initModule(module);
                    this.modules.Add(module);
                }
                return module;
            }

            private ModuleDeclaration GetModule(NClang.ClangModule clangModule)
            {
                return GetOrCreateModule(clangModule.Name, clangModule.FullName, module =>
                {
                    NClang.ClangModule clangModuleParent = clangModule.Parent;
                    if (clangModuleParent != null)
                    {
                        module.Parent = GetModule(clangModuleParent);
                        module.Parent.Submodules.Add(module);
                    }
                });
            }
        }

        class SingleModuleResolver : IModuleResolver
        {
            public SingleModuleResolver(ModuleDeclaration document)
            {
                this.document = document;
            }

            private ModuleDeclaration document;

            public ModuleDeclaration GetModule(NClang.ClangCursor cursor)
            {
                return this.document;
            }
        }

        public class ParserContext
        {
            public ParserContext(string sdkPath)
            {
                this.resolver = new AutomaticModuleResolver(this.modules, sdkPath);
            }

            public ParserContext(ModuleDeclaration document)
            {
                this.resolver = new SingleModuleResolver(document);
            }

            public readonly IList<ModuleDeclaration> modules = new List<ModuleDeclaration>();

            public readonly IDictionary<string, BaseDeclaration> usrToDeclaration =
                new Dictionary<string, BaseDeclaration>();

            public readonly IDictionary<string, IList<Types.DeclarationReferenceType>> usrToUnresolvedReferences =
                new Dictionary<string, IList<Types.DeclarationReferenceType>>();

            public readonly IDictionary<string, BaseDeclaration> nameToDeclaration =
                new Dictionary<string, BaseDeclaration>();

            public readonly IModuleResolver resolver;
            private uint anonymousIDCount = 1;

            public void AddToUSRCache(BaseDeclaration declaration)
            {
                if (declaration.USR != null && !this.usrToDeclaration.ContainsKey(declaration.USR))
                {
                    this.usrToDeclaration.Add(declaration.USR, declaration);
                }
            }

            public T GetFromUSRCache<T>(string usr)
                where T : BaseDeclaration
            {
                BaseDeclaration decl = null;
                this.usrToDeclaration.TryGetValue(usr, out decl);
                return decl as T;
            }

            public void AddToNameCache(BaseDeclaration declaration)
            {
                if (!string.IsNullOrEmpty(declaration.Name) && !this.nameToDeclaration.ContainsKey(declaration.Name))
                {
                    this.nameToDeclaration.Add(declaration.Name, declaration);
                }
            }

            public T GetFromNameCache<T>(string fullName)
                where T : BaseDeclaration
            {
                return this.GetFromNameCache(fullName) as T;
            }

            public BaseDeclaration GetFromNameCache(string fullName)
            {
                BaseDeclaration decl = null;
                this.nameToDeclaration.TryGetValue(fullName, out decl);
                return decl;
            }

            public uint GetAnonymousTypeID()
            {
                uint currentId = this.anonymousIDCount;
                this.anonymousIDCount += 1;
                return currentId;
            }
        }
    }
}
