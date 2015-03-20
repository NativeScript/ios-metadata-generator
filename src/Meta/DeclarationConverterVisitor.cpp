#include "DeclarationConverterVisitor.h"
#include <iostream>

using namespace std;

bool Meta::DeclarationConverterVisitor::VisitFunctionDecl(clang::FunctionDecl *function) {
    return Visit<clang::FunctionDecl>(function);
}

bool Meta::DeclarationConverterVisitor::VisitVarDecl(clang::VarDecl *var) {
    // If the var declaration is exactly VarDecl but not an inheritor of VarDecl (e.g. ParmVarDecl)
    if(var->getKind() == clang::Decl::Var) {
        return Visit<clang::VarDecl>(var);
    }
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitEnumDecl(clang::EnumDecl *enumDecl) {
    return Visit<clang::EnumDecl>(enumDecl);
}

bool Meta::DeclarationConverterVisitor::VisitRecordDecl(clang::RecordDecl *record) {
    return Visit<clang::RecordDecl>(record);
}

bool Meta::DeclarationConverterVisitor::VisitObjCInterfaceDecl(clang::ObjCInterfaceDecl *interface) {
    return Visit<clang::ObjCInterfaceDecl>(interface);
}

bool Meta::DeclarationConverterVisitor::VisitObjCProtocolDecl(clang::ObjCProtocolDecl *protocol) {
    return Visit<clang::ObjCProtocolDecl>(protocol);
}