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

std::shared_ptr<Meta::ModuleId> Meta::IdentifierFactory::getModule(const clang::Module* module) {
    // check for cached Module
    std::unordered_map<const clang::Module*, std::shared_ptr<ModuleId>>::const_iterator cachedModule = _moduleCache.find(module);
    if(cachedModule != _moduleCache.end())
        return cachedModule->second;

    // calculate module name
    std::shared_ptr<Meta::ModuleId> metaModule = (module == nullptr) ? nullptr : std::make_shared<Meta::ModuleId>(module->getFullModuleName(), module->isPartOfFramework(), module->IsSystem);

    // insert it in cache
    _moduleCache.insert(std::pair<const clang::Module*, std::shared_ptr<ModuleId>>(module, metaModule));
    return metaModule;
}

std::shared_ptr<Meta::HeaderFileId> Meta::IdentifierFactory::getHeaderFile(const clang::FileEntry *entry) {
    // check for cached HeaderFile
    std::unordered_map<const clang::FileEntry*, std::shared_ptr<HeaderFileId>>::const_iterator cachedFile = _fileCache.find(entry);
    if(cachedFile != _fileCache.end())
        return cachedFile->second;

    // create HeaderFile
    std::shared_ptr<HeaderFileId> headerFile = (entry == nullptr) ? nullptr : std::make_shared<HeaderFileId>(entry->getName(),
                                                                          getModule(_headerSearch.findModuleForHeader(entry).getModule()));

    // insert it in cache
    _fileCache.insert(std::pair<const clang::FileEntry*, std::shared_ptr<HeaderFileId>>(entry, headerFile));
    return headerFile;
}

Meta::DeclId Meta::IdentifierFactory::getIdentifier(const clang::Decl& decl, bool throwIfEmpty) {
    // check for cached Identifier
    std::unordered_map<const clang::Decl*, DeclId>::const_iterator cachedId = _declCache.find(&decl);
    if(cachedId != _declCache.end()) {
        return cachedId->second;
    }

    std::string name;
    std::string jsName;
    std::shared_ptr<HeaderFileId> file;

    // calculate name
    if(const clang::NamedDecl* namedDecl = clang::dyn_cast<clang::NamedDecl>(&decl))
        name = namedDecl->getNameAsString();

    // calculate js name
    std::string originalName = calculateOriginalName(decl);
    std::string recalculationMapName = originalName;
    if(decl.getKind() == clang::Decl::Kind::ObjCProperty || decl.getKind() == clang::Decl::Kind::ObjCMethod) {
        if(const clang::ObjCContainerDecl *containerDecl = clang::dyn_cast<clang::ObjCContainerDecl>(decl.getDeclContext()))
            recalculationMapName = calculateOriginalName(*containerDecl) + "." + originalName;
    }
    jsName = calculateJsName(decl, originalName);
    if(!jsName.empty()) {
        std::vector<std::string> namesToCheck = _namesToRecalculate[decl.getKind()];
        if (std::find(namesToCheck.begin(), namesToCheck.end(), recalculationMapName) != namesToCheck.end()) {
            jsName = recalculateJsName(decl, jsName);
        }
    }

    // calculate file entry
    clang::SourceLocation location = _sourceManager.getFileLoc(decl.getLocation());
    clang::FileID fileId = _sourceManager.getDecomposedLoc(location).first;
    const clang::FileEntry *entry = _sourceManager.getFileEntryForID(fileId);
    file = getHeaderFile(entry);

    DeclId id(name, jsName, file);

    // add to cache
    _declCache.insert(std::pair<const clang::Decl*, DeclId>(&decl, id));

    if(throwIfEmpty) {
        // if name is empty we don't throw exception, it's OK the declaration to be anonymous
        if (id.jsName.empty())
            throw IdentifierCreationException(id, "Unknown js name for declaration.");
        if (id.file == nullptr)
            throw IdentifierCreationException(id, "Unknown file for declaration.");
        if (id.file->module == nullptr)
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
                    return ""; // It is absolutely anonymous record. It has neither name nor typedef name.
                }

                /*
                 * Check if the next declaration in the context is typedef declaration to the given record and if true - use the name of the typedef.
                 * Example:
                 *          typedef struct _ugly_name {
                 *              int field;
                 *          } NiceName;
                 * The algorithm should detect the typedef and use NiceName instead of  _ugly_name.
                 * Example:
                 *          struct _ugly_name {
                 *              int field;
                 *          }
                 *          typedef struct _ugly_name NiceName;
                 * Here, the algorithm will also detect the typedef and use NiceName instead of  _ugly_name,
                 * because the typedef declaration is still the next declaration in context.
                 */
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
                    return ""; // It is absolutely anonymous record. It has neither name nor typedef name.
                }

                /*
                 * Check if the next declaration in the context is typedef declaration to the given enum and if true - use the name of the typedef.
                 * Example:
                 *          typedef enum _ugly_name {
                 *              field1, field2
                 *          } NiceName;
                 * The algorithm should detect the typedef and use NiceName instead of  _ugly_name.
                 * Example:
                 *          enum _ugly_name {
                 *              field1, field2
                 *          }
                 *          typedef enum _ugly_name NiceName;
                 * Here, the algorithm will also detect the typedef and use NiceName instead of  _ugly_name,
                 * because the typedef declaration is still the next declaration in context.
                 */
                if(enumDecl->getNextDeclInContext() != nullptr) {
                    if (const clang::TypedefDecl *nextDecl = clang::dyn_cast<clang::TypedefDecl>(enumDecl->getNextDeclInContext())) {
                        if (const clang::ElaboratedType *innerElaboratedType = clang::dyn_cast<clang::ElaboratedType>(nextDecl->getUnderlyingType().getTypePtr())) {
                            if (const clang::EnumType *enumType = clang::dyn_cast<clang::EnumType>(innerElaboratedType->desugar().getTypePtr())) {
                                if (enumType->getDecl() == enumDecl) {
                                    return nextDecl->getFirstDecl()->getNameAsString();
                                }
                            }
                        }
                    }
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
