using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Libclang.Core.Ast;
using Libclang.Core.Common;

namespace Libclang.Core.Generator
{
    public class TNSBridgeCategoriesWriter : TNSBridgeInterfacesWriter
    {
        public TNSBridgeCategoriesWriter(string path,
            Dictionary<InterfaceDeclaration, List<MethodDeclaration>> interfacesToMethodsMap,
            Dictionary<InterfaceDeclaration, List<MethodDeclaration>> interfacesToCategoriesMethodsMap,
            MultiDictionary<FunctionDeclaration, BaseRecordDeclaration> functionToRecords)
            : base(path, interfacesToMethodsMap, null, interfacesToCategoriesMethodsMap, functionToRecords)
        {
        }

        protected override void GenerateBindingsForClass(InterfaceDeclaration interfaceDecl)
        {
            GenerateJSDerivedHeader(interfaceDecl);
            GenerateJSDerivedImplementation(interfaceDecl);
        }

        protected override string DerivedHeaderTemplate
        {
            get { return @"#import <TNSBridgeInfrastructure/JSDerivedProtocol.h>
@interface {0}CategoriesImplementation : {0} <JSDerivedProtocol>

@property (nonatomic) JSContextRef tns_jscontext;
@property (nonatomic) JSObjectRef tns_object;
@property (nonatomic, retain) NSMutableDictionary *tns_overridenMethods;
@property (nonatomic, retain) NSRecursiveLock *tns_lock;

@end"; }
        }

        protected override void GenerateJSDerivedHeader(InterfaceDeclaration interfaceDecl)
        {
            using (StreamWriter writer = this.WriterForDeclaration(interfaceDecl, "CategoriesImplementation.h"))
            {
                writer.Write(string.Format(DerivedHeaderTemplate, interfaceDecl.Name));
            }
        }

        protected override string DerivedImplementationTemplate
        {
            get { return @"
#import ""{0}CategoriesImplementation.h""
#import <TNSBridgeInfrastructure/ObjectWrapper.h>
#import <TNSBridgeInfrastructure/MarshallingService.h>
#import <TNSBridgeInfrastructure/ObjCInheritance.h>
#import <TNSBridgeInfrastructure/ObjCInheritance+IsValidOverride.h>
#import <TNSBridgeInfrastructure/TNSRefValue.h>
#import <TNSBridgeInfrastructure/TNSBuffer.h>
#import <TNSBridgeInfrastructure/BigIntWrapper.h>
#import <TNSBridgeInfrastructure/Variadics.h>
{2}

@implementation {0}CategoriesImplementation
-(BOOL)isDeallocating {{ return NO; }}
{1}
@end
"; }
        }

        protected override void GenerateJSDerivedImplementation(InterfaceDeclaration interfaceDecl)
        {
            using (StreamWriter writer = this.WriterForDeclaration(interfaceDecl, "CategoriesImplementation.m"))
            {
                writer.Write(string.Format(DerivedImplementationTemplate, interfaceDecl.Name,
                    GenerateJSDerivedMethodsImplementations(interfaceDecl),
                    GetImports(interfacesToMethodsMap[interfaceDecl])));
            }
        }

        protected override string GenerateJSDerivedMethodsImplementations(InterfaceDeclaration interfaceDecl)
        {
            StringBuilder sb = new StringBuilder();

            foreach (MethodDeclaration method in interfacesToMethodsMap[interfaceDecl])
            {
                sb.AppendLine(GenerateJSDerivedMethodImplementation(interfaceDecl, method, isCategory: true));
            }

            return sb.ToString();
        }
    }
}
