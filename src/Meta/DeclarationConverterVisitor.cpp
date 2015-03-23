#include "DeclarationConverterVisitor.h"
#include <iostream>

using namespace std;

bool Meta::DeclarationConverterVisitor::VisitFunctionDecl(clang::FunctionDecl *function) {
    if(!function->isThisDeclarationADefinition()) {
        return Visit<clang::FunctionDecl>(function);
    }
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitVarDecl(clang::VarDecl *var) {
    // If the var declaration is exactly VarDecl but not an inheritor of VarDecl (e.g. ParmVarDecl)
    if(var->isThisDeclarationADefinition() && var->getKind() == clang::Decl::Var) {
        return Visit<clang::VarDecl>(var);
    }
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitEnumDecl(clang::EnumDecl *enumDecl) {
    if(enumDecl->isThisDeclarationADefinition()) {
        return Visit<clang::EnumDecl>(enumDecl);
    }
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitRecordDecl(clang::RecordDecl *record) {
    if(record->isThisDeclarationADefinition()) {
        return Visit<clang::RecordDecl>(record);
    }
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitObjCInterfaceDecl(clang::ObjCInterfaceDecl *interface) {
    if(interface->isThisDeclarationADefinition()) {
        return Visit<clang::ObjCInterfaceDecl>(interface);
    }
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitObjCProtocolDecl(clang::ObjCProtocolDecl *protocol) {
    if(protocol->isThisDeclarationADefinition()) {
        return Visit<clang::ObjCProtocolDecl>(protocol);
    }
    return true;
}