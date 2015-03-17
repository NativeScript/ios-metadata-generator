#include <clang/AST/Decl.h>
#include <sstream>
#include <clang/AST/DeclObjC.h>
#include "JsNameGenerator.h"

using namespace std;

static map<clang::Decl::Kind, vector<string>> IosSdkNamesToRecalculate = {
        { clang::Decl::Kind::Record, { "kevent", "flock", "sigvec", "sigaction", "wait" } },
        { clang::Decl::Kind::Var, { "timezone" } },
        { clang::Decl::Kind::ObjCProtocol, { "NSObject", "AVVideoCompositionInstruction", "OS_dispatch_data" } }
};

map<clang::Decl::Kind, vector<string>>& Meta::JsNameGenerator::getIosSdkNamesToRecalculate() {
    return IosSdkNamesToRecalculate;
}

void splitString(const std::string &s, char delim, vector<string> &elems) {
    stringstream ss(s);
    string item;
    while (getline(ss, item, delim)) {
        elems.push_back(item);
    }
}

string Meta::JsNameGenerator::calculateOriginalName(clang::NamedDecl& decl) {
    switch(decl.getKind()) {
        case clang::Decl::Kind::Function :
        case clang::Decl::Kind::ObjCInterface :
        case clang::Decl::Kind::ObjCProtocol :
        case clang::Decl::Kind::ObjCCategory :
        case clang::Decl::Kind::ObjCProperty :
            return decl.getNameAsString();
        case clang::Decl::Kind::ObjCMethod : {
            clang::NamedDecl *declPtr = &decl;
            if(clang::ObjCMethodDecl *method = clang::dyn_cast<clang::ObjCMethodDecl>(declPtr)) {
                return method->getSelector().getAsString();
            }
            throw logic_error("Invalid declaration.");
        }
        case clang::Decl::Kind::Record : {
            clang::NamedDecl *declPtr = &decl;
            if(clang::RecordDecl *record = clang::dyn_cast<clang::RecordDecl>(declPtr)) {
                if(!record->hasNameForLinkage()) {
                    throw "Can't generate JS name for anonymous record which is defined outside typedef.";
                }
                if(clang::TypedefNameDecl *typedefDecl = record->getTypedefNameForAnonDecl()) {
                    return typedefDecl->getNameAsString();
                }
                return record->getNameAsString();
            }
            throw logic_error("Invalid declaration.");
        }
        case clang::Decl::Kind::Enum : {
            clang::NamedDecl *declPtr = &decl;
            if(clang::EnumDecl *enumDecl = clang::dyn_cast<clang::EnumDecl>(declPtr)) {
                if(!enumDecl->hasNameForLinkage()) {
                    throw "Can't generate JS name for anonymous enumeration which is defined outside typedef.";
                }
                if(clang::TypedefNameDecl *typedefDecl = enumDecl->getTypedefNameForAnonDecl()) {
                    return typedefDecl->getNameAsString();
                }
                return enumDecl->getNameAsString();
            }
            throw logic_error("Invalid declaration.");
        }
        default:
            throw logic_error("Can't generate original name for that type of declaration.");
    }
}

string Meta::JsNameGenerator::calculateJsName(clang::NamedDecl& decl, std::string originalName) {
    switch(decl.getKind()) {
        case clang::Decl::Kind::Record :
        case clang::Decl::Kind::Enum :
        case clang::Decl::Kind::Function :
        case clang::Decl::Kind::ObjCInterface :
        case clang::Decl::Kind::ObjCProtocol :
        case clang::Decl::Kind::ObjCCategory :
        case clang::Decl::Kind::ObjCProperty :
            return originalName;
        case clang::Decl::Kind::ObjCMethod : {
            vector<string> tokens;
            splitString(originalName, ':', tokens);
            for (vector<string>::size_type i = 1; i < tokens.size(); ++i) {
                tokens[i][0] = toupper(tokens[i][0]);
                tokens[0] += tokens[i];
            }
            return tokens[0];
        }
        default:
            throw logic_error("Can't generate JS name for that type of declaration.");
    }
}

string Meta::JsNameGenerator::recalculateJsName(clang::NamedDecl& decl, std::string calculatedJsName) {
    switch(decl.getKind()) {
        case clang::Decl::Kind::Record : {
            clang::NamedDecl *declPtr = &decl;
            clang::RecordDecl *record = llvm::dyn_cast<clang::RecordDecl>(declPtr);
            return calculatedJsName + (record->isStruct() ? "Struct" : "Union");
        }
        case clang::Decl::Kind::Function :
            return calculatedJsName + "Function";
        case clang::Decl::Kind::Enum :
            return calculatedJsName + "Enum";
        case clang::Decl::Kind::ObjCInterface :
            return calculatedJsName + "Interface";
        case clang::Decl::Kind::ObjCProtocol :
            return calculatedJsName + "Protocol";
        case clang::Decl::Kind::ObjCCategory :
            return calculatedJsName + "Category";
        case clang::Decl::Kind::ObjCMethod :
            return calculatedJsName + "Method";
        case clang::Decl::Kind::ObjCProperty :
            return calculatedJsName + "Property";
        default:
            throw logic_error("Can't generate JS name for that type of declaration.");

    }
}