#include <clang/AST/Decl.h>
#include <sstream>
#include <clang/AST/DeclObjC.h>
#include <iostream>
#include "IdentifierFactory.h"

using namespace std;

// TODO: this static maps contains hardcoded symbol names from the iOS SDK for which we know there is conflict
// in their JS name and must be renamed. Maybe there is better way to pass this map to the IdentifierGenerator. For
// third-party libraries the map may be parsed from file or something more flexible.
static map<clang::Decl::Kind, vector<string>> IosSdkNamesToRecalculate = {
        { clang::Decl::Kind::Record, { "kevent", "flock", "sigvec", "sigaction", "wait" } },
        { clang::Decl::Kind::Var, { "timezone" } },
        { clang::Decl::Kind::ObjCProtocol, { "NSObject", "AVVideoCompositionInstruction", "OS_dispatch_data" } }
};

map<clang::Decl::Kind, vector<string>>& Meta::IdentifierFactory::getIosSdkNamesToRecalculate() {
    return IosSdkNamesToRecalculate;
}

void splitString(const std::string &s, char delim, vector<string> &elems) {
    stringstream ss(s);
    string item;
    while (getline(ss, item, delim)) {
        elems.push_back(item);
    }
}

std::string Meta::IdentifierFactory::getJsName(const clang::Decl& decl, bool throwIfEmpty) {
    Identifier id = getIdentifier(decl, false);
    if(id.jsName.empty())
        throw IdentifierCreationException(id, "Unknown js name for declaration.");
    return id.jsName;
}

std::string Meta::IdentifierFactory::getModule(const clang::Decl& decl, bool throwIfEmpty) {
    Identifier id = getIdentifier(decl, false);
    if(id.module.empty())
        throw IdentifierCreationException(id, "Unknown module name for declaration.");
    return id.module;
}

std::string Meta::IdentifierFactory::getFileName(const clang::Decl& decl, bool throwIfEmpty) {
    Identifier id = getIdentifier(decl, false);
    if(id.fileName.empty())
        throw IdentifierCreationException(id, "Unknown file name for declaration.");
    return id.fileName;
}

Meta::Identifier Meta::IdentifierFactory::getIdentifier(const clang::Decl& decl, bool throwIfEmpty) {
    // check for cached Identifier
    std::unordered_map<const clang::Decl*, Identifier>::const_iterator cachedId = _cache.find(&decl);
    if(cachedId != _cache.end()) {
        return cachedId->second;
    }

    Identifier id;
    // calculate file name
    clang::SourceLocation location = _sourceManager.getFileLoc(decl.getLocation());
    clang::FileID fileId = _sourceManager.getDecomposedLoc(location).first;
    const clang::FileEntry *entry = _sourceManager.getFileEntryForID(fileId);
    id.fileName = entry->getName();

    // calculate module name
    clang::Module *owningModule = _headerSearch.findModuleForHeader(entry).getModule();
    // If the declaration is in header which is not included in modulemap file, we don't try to figure out a module name for these headers.
    // This is the case for some headers in usr/include (e.g. sqlite) and third-party headers without modulemap.
    id.module = owningModule ? owningModule->getFullModuleName() : "";

    // calculate js name
    std::string originalName = calculateOriginalName(decl);
    std::string recalculationMapName = originalName;
    if(decl.getKind() == clang::Decl::Kind::ObjCProperty || decl.getKind() == clang::Decl::Kind::ObjCMethod) {
        if(const clang::ObjCContainerDecl *containerDecl = clang::dyn_cast<clang::ObjCContainerDecl>(decl.getDeclContext()))
            recalculationMapName = calculateOriginalName(*containerDecl) + "." + originalName;
    }
    std::string jsName = calculateJsName(decl, originalName);
    if(!jsName.empty()) {
        std::vector<std::string> namesToCheck = _namesToRecalculate[decl.getKind()];
        if (std::find(namesToCheck.begin(), namesToCheck.end(), recalculationMapName) != namesToCheck.end()) {
            jsName = recalculateJsName(decl, jsName);
        }
    }
    id.jsName = jsName;

    // add to cache
    _cache.insert(std::pair<const clang::Decl*, Identifier>(&decl, id));

    if(throwIfEmpty) {
        if (id.jsName.empty())
            throw IdentifierCreationException(id, "Unknown js name for declaration.");
        if (id.fileName.empty())
            throw IdentifierCreationException(id, "Unknown file name for declaration.");
        if (id.module.empty())
            throw IdentifierCreationException(id, "Unknown module for declaration.");
    }

    return id;
}

string Meta::IdentifierFactory::calculateOriginalName(const clang::Decl& decl) {

    switch(decl.getKind()) {
        case clang::Decl::Kind::Function :
        case clang::Decl::Kind::ObjCInterface :
        case clang::Decl::Kind::ObjCProtocol :
        case clang::Decl::Kind::ObjCCategory :
        case clang::Decl::Kind::ObjCProperty :
        case clang::Decl::Kind::Field :
        case clang::Decl::Kind::EnumConstant :
        case clang::Decl::Kind::Var :
            return clang::dyn_cast<clang::NamedDecl>(&decl)->getNameAsString();
        case clang::Decl::Kind::ObjCMethod : {
            if(const clang::ObjCMethodDecl *method = clang::dyn_cast<clang::ObjCMethodDecl>(&decl)) {
                return method->getSelector().getAsString();
            }
            return "";
        }
        case clang::Decl::Kind::Record : {
            if(const clang::RecordDecl *record = clang::dyn_cast<clang::RecordDecl>(&decl)) {
                if(!record->hasNameForLinkage()) {
                    // It is absolutely anonymous record. It has neither name nor typedef name.
                    return "";
                }

                // TODO: check the correctness of the name in scenarios like this:
                // struct a { int field; }
                // typedef a b;
                // It should return 'a' but I am not sure if this will be the result.

                // First we check if have a typedef name, and get it if exists
                // TODO: Do this check for enums, too
                if(record->getNextDeclInContext() != nullptr) {
                    if (const clang::TypedefDecl *nextDecl = clang::dyn_cast<clang::TypedefDecl>(record->getNextDeclInContext())) {
                        if (const clang::ElaboratedType *innerElaboratedType = clang::dyn_cast<clang::ElaboratedType>(nextDecl->getUnderlyingType().getTypePtr())) {
                            if (const clang::RecordType *recordType = clang::dyn_cast<clang::RecordType>(innerElaboratedType->desugar().getTypePtr())) {
                                if (recordType->getDecl() == record) {
                                    return nextDecl->getFirstDecl()->getNameAsString();
                                }
                            }
                        }
                    }
                }
                // The record has no typedef name, so we return its name.
                return record->getNameAsString();
            }
            return "";
        }
        case clang::Decl::Kind::Enum : {
            if(const clang::EnumDecl *enumDecl = clang::dyn_cast<clang::EnumDecl>(&decl)) {
                if(!enumDecl->hasNameForLinkage()) {
                    // enumDecl->hasNameForLinkage() - http://clang.llvm.org/doxygen/classclang_1_1TagDecl.html#aa0c620992e6aca248368dc5c7c463687 (description what this method does)
                    return "";
                }
                if(const clang::TypedefNameDecl *typedefDecl = enumDecl->getTypedefNameForAnonDecl()) {
                    return typedefDecl->getNameAsString();
                }
                return enumDecl->getNameAsString();
            }
            return "";
        }
        default:
            throw logic_error(string("Can't generate original name for ") + decl.getDeclKindName() + " type of declaration.");
    }
}

string Meta::IdentifierFactory::calculateJsName(const clang::Decl& decl, std::string originalName) {
    switch(decl.getKind()) {
        case clang::Decl::Kind::Record :
        case clang::Decl::Kind::Enum :
        case clang::Decl::Kind::Function :
        case clang::Decl::Kind::ObjCInterface :
        case clang::Decl::Kind::ObjCProtocol :
        case clang::Decl::Kind::ObjCCategory :
        case clang::Decl::Kind::ObjCProperty :
        case clang::Decl::Kind::Field :
        case clang::Decl::Kind::EnumConstant :
        case clang::Decl::Kind::Var :
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
            throw logic_error(string("Can't calculate js name for ") + decl.getDeclKindName() + " type of declaration.");
    }
}

string Meta::IdentifierFactory::recalculateJsName(const clang::Decl& decl, std::string calculatedJsName) {
    switch(decl.getKind()) {
        case clang::Decl::Kind::Record : {
            const clang::RecordDecl *record = llvm::dyn_cast<clang::RecordDecl>(&decl);
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
        case clang::Decl::Kind::Var :
            return calculatedJsName + "Var";
        case clang::Decl::Kind::Field :
            return calculatedJsName + "Field";
        case clang::Decl::Kind::EnumConstant :
            return calculatedJsName + "Field";
        default:
            throw logic_error(string("Can't recalculate js name for ") + decl.getDeclKindName() + " type of declaration.");

    }
}
