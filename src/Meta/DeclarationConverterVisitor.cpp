#include "DeclarationConverterVisitor.h"
#include <iostream>

using namespace std;

bool Meta::DeclarationConverterVisitor::VisitFunctionDecl(clang::FunctionDecl *function) {
    // TODO: We don't support variadic functions but we save in metadata flags whether a function is variadic or not.
    // If we not plan in the future to support variadic functions this redundant flag should be removed.
    if(!function->isThisDeclarationADefinition() && !function->isVariadic() && !function->isInlined()) {
        try {
            addToResult(this->_metaFactory.createFunctionMeta(*function));
        } catch(EntityCreationException& e) {
            cout << (e.isError() ? "Error:" : "Notice:") << " Function " << function->getNameAsString() << " is not included. Reason: " << e.getMessage() << endl;
        }
    }
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitVarDecl(clang::VarDecl *var) {
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitEnumDecl(clang::EnumDecl *enumDecl) {
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitRecordDecl(clang::RecordDecl *record) {
    if(record->isThisDeclarationADefinition() && record->hasNameForLinkage() && record->isStruct()) {
        // record->hasNameForLinkage() - http://clang.llvm.org/doxygen/classclang_1_1TagDecl.html#aa0c620992e6aca248368dc5c7c463687 (description what this method does)
        try {
            addToResult(this->_metaFactory.createRecordMeta(*record));
        } catch(EntityCreationException& e) {
            cout << (e.isError() ? "Error:" : "Notice:") << " Struct " << record->getNameAsString() << " not included. Reason: " << e.getMessage() << endl;
        }
    }
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitObjCInterfaceDecl(clang::ObjCInterfaceDecl *interface) {
    if (interface->isThisDeclarationADefinition()) {
        try {
            addToResult(this->_metaFactory.createInterfaceMeta(*interface));
        } catch(EntityCreationException& e) {
            cout << (e.isError() ? "Error:" : "Notice:") << " Interface " << interface->getNameAsString() << " not included. Reason: " << e.getMessage() << endl;
        }
    }
    return true;
}

bool Meta::DeclarationConverterVisitor::VisitObjCProtocolDecl(clang::ObjCProtocolDecl *protocol) {
    if(protocol->isThisDeclarationADefinition()) {
        try {
            addToResult(this->_metaFactory.createProtocolMeta(*protocol));
        } catch(EntityCreationException& e) {
            cout << (e.isError() ? "Error:" : "Notice:") << " Protocol " << protocol->getNameAsString() << " not included. Reason: " << e.getMessage() << endl;
        }
    }
    return true;
}