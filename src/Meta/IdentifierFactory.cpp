#include <clang/AST/Decl.h>
#include <clang/AST/Attr.h>
#include <sstream>
#include <clang/AST/DeclObjC.h>
#include <iostream>
#include "IdentifierFactory.h"
#include "Utils.h"

using namespace std;

// TODO: this static maps contains hardcoded symbol names from the iOS SDK for which we know there is conflict
// in their JS name and must be renamed. Maybe there is better way to pass this map to the IdentifierGenerator. For
// third-party libraries the map may be parsed from file or something more flexible.
static map<clang::Decl::Kind, vector<string> > IosSdkNamesToRecalculate = {
    { clang::Decl::Kind::Record, { "kevent", "flock", "sigvec", "sigaction", "wait" } },
    { clang::Decl::Kind::Var, { "timezone" } },
    { clang::Decl::Kind::ObjCProtocol, { "NSObject", "AVVideoCompositionInstruction", "OS_dispatch_data",
                                         // FIXME: These protocols are part of KontaktIO library (http://kontaktio.github.io/ios-sdk-docs/html//index.html).
                                         // Hardcoding their names here is a dirty temporary solution for name collisions problems. We need a general solution.
                                         "KTKAction", "KTKBeacon", "KTKBeaconProfile", "KTKCloudBeacon", "KTKCompany", "KTKDataTransferObject", "KTKFirmware", "KTKManager", "KTKPaging", "KTKPublicAction", "KTKPublicBeacon", "KTKPublicVenue", "KTKRegion", "KTKSubordinate", "KTKUser", "KTKVenue", "KTKVenueBase" } }
};

map<clang::Decl::Kind, vector<string> >& Meta::IdentifierFactory::getIosSdkNamesToRecalculate()
{
    return IosSdkNamesToRecalculate;
}

static void splitString(const std::string& s, char delim, vector<string>& elems)
{
    stringstream ss(s);
    string item;
    while (getline(ss, item, delim)) {
        elems.push_back(item);
    }
}

Meta::DeclId Meta::IdentifierFactory::getIdentifier(const clang::Decl& decl, bool throwIfEmpty)
{
    // check for cached Identifier
    std::unordered_map<const clang::Decl*, DeclId>::const_iterator cachedId = _declCache.find(&decl);
    if (cachedId != _declCache.end()) {
        return cachedId->second;
    }

    std::string name = calculateName(decl);
    std::string jsName;
    std::string fileName;
    clang::Module* module = nullptr;

    // calculate js name
    std::string originalName = calculateOriginalName(decl);
    std::string recalculationMapName = originalName;
    if (decl.getKind() == clang::Decl::Kind::ObjCProperty || decl.getKind() == clang::Decl::Kind::ObjCMethod) {
        if (const clang::ObjCContainerDecl* containerDecl = clang::dyn_cast<clang::ObjCContainerDecl>(decl.getDeclContext()))
            recalculationMapName = calculateOriginalName(*containerDecl) + "." + originalName;
    }
    jsName = calculateJsName(decl, originalName);
    if (!jsName.empty()) {
        std::vector<std::string> namesToCheck = _namesToRecalculate[decl.getKind()];
        if (std::find(namesToCheck.begin(), namesToCheck.end(), recalculationMapName) != namesToCheck.end()) {
            jsName = recalculateJsName(decl, jsName);
        }
    }

    // calculate file name and module
    clang::SourceLocation location = _sourceManager.getFileLoc(decl.getLocation());
    clang::FileID fileId = _sourceManager.getDecomposedLoc(location).first;
    const clang::FileEntry* entry = _sourceManager.getFileEntryForID(fileId);
    if (entry != nullptr) {
        fileName = entry->getName();
        module = _headerSearch.findModuleForHeader(entry).getModule();
    }

    DeclId id(name, jsName, fileName, module);

    // add to cache
    _declCache.insert(std::pair<const clang::Decl*, DeclId>(&decl, id));

    if (throwIfEmpty) {
        // if name is empty we don't throw exception, it's OK the declaration to be anonymous
        if (id.jsName.empty())
            throw IdentifierCreationException(id, "Unknown js name for declaration.");
        if (id.fileName.empty())
            throw IdentifierCreationException(id, "Unknown file for declaration.");
        if (id.module == nullptr)
            throw IdentifierCreationException(id, "Unknown module for declaration.");
    }

    return id;
}

string Meta::IdentifierFactory::calculateName(const clang::Decl& decl) {
    if (const clang::NamedDecl* namedDecl = clang::dyn_cast<clang::NamedDecl>(&decl)) {
        std::vector<clang::ObjCRuntimeNameAttr*> objCRuntimeNameAttributes = Utils::getAttributes<clang::ObjCRuntimeNameAttr>(*namedDecl);
        if (!objCRuntimeNameAttributes.size()) {
            return namedDecl->getNameAsString();
        }

        return objCRuntimeNameAttributes[0]->getMetadataName().str();
    }

    return "";
}

/*
 * TODO: Check if the next declaration in the context is typedef declaration to the given decl and if true - use the name of the typedef.
 * Example:
 *          typedef struct _ugly_name {
 *              int field;
 *          } NiceName;
 * The algorithm should detect the typedef and use NiceName instead of _ugly_name.
 * Example:
 *          struct _ugly_name {
 *              int field;
 *          }
 *          typedef struct _ugly_name NiceName;
 * Here, the algorithm will also detect the typedef and use NiceName instead of _ugly_name,
 * because the typedef declaration is still the next declaration in context.
 */
static string getTypedefOrOwnName(const clang::TagDecl* tagDecl)
{
    assert(tagDecl);

    if (!tagDecl->hasNameForLinkage()) {
        return ""; // It is absolutely anonymous decl. It has neither name nor typedef name.
    }

    if (tagDecl->getNextDeclInContext() != nullptr) {
        if (const clang::TypedefDecl* nextDecl = clang::dyn_cast<clang::TypedefDecl>(tagDecl->getNextDeclInContext())) {
            if (const clang::ElaboratedType* innerElaboratedType = clang::dyn_cast<clang::ElaboratedType>(nextDecl->getUnderlyingType().getTypePtr())) {
                if (const clang::TagType* tagType = clang::dyn_cast<clang::TagType>(innerElaboratedType->desugar().getTypePtr())) {
                    if (tagType->getDecl() == tagDecl) {
                        return nextDecl->getFirstDecl()->getNameAsString();
                    }
                }
            }
        }
    }

    // The decl has no typedef name, so we return its name.
    return tagDecl->getNameAsString();
}

string Meta::IdentifierFactory::calculateOriginalName(const clang::Decl& decl)
{
    switch (decl.getKind()) {
    case clang::Decl::Kind::Function:
    case clang::Decl::Kind::ObjCInterface:
    case clang::Decl::Kind::ObjCProtocol:
    case clang::Decl::Kind::ObjCCategory:
    case clang::Decl::Kind::ObjCProperty:
    case clang::Decl::Kind::Field:
    case clang::Decl::Kind::EnumConstant:
    case clang::Decl::Kind::Var:
        return clang::dyn_cast<clang::NamedDecl>(&decl)->getNameAsString();
    case clang::Decl::Kind::ObjCMethod: {
        if (const clang::ObjCMethodDecl* method = clang::dyn_cast<clang::ObjCMethodDecl>(&decl)) {
            return method->getSelector().getAsString();
        }
        return "";
    }
    case clang::Decl::Kind::Record: {
        const clang::RecordDecl* recordDecl = clang::dyn_cast<clang::RecordDecl>(&decl);
        return getTypedefOrOwnName(recordDecl);
    }
    case clang::Decl::Kind::Enum: {
        const clang::EnumDecl* enumDecl = clang::dyn_cast<clang::EnumDecl>(&decl);
        return getTypedefOrOwnName(enumDecl);
    }
    default:
        throw logic_error(string("Can't generate original name for ") + decl.getDeclKindName() + " type of declaration.");
    }
}

string Meta::IdentifierFactory::calculateJsName(const clang::Decl& decl, std::string originalName)
{
    switch (decl.getKind()) {
    case clang::Decl::Kind::Record:
    case clang::Decl::Kind::Enum:
    case clang::Decl::Kind::Function:
    case clang::Decl::Kind::ObjCInterface:
    case clang::Decl::Kind::ObjCProtocol:
    case clang::Decl::Kind::ObjCCategory:
    case clang::Decl::Kind::ObjCProperty:
    case clang::Decl::Kind::Field:
    case clang::Decl::Kind::EnumConstant:
    case clang::Decl::Kind::Var:
        return originalName;
    case clang::Decl::Kind::ObjCMethod: {
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

string Meta::IdentifierFactory::recalculateJsName(const clang::Decl& decl, std::string calculatedJsName)
{
    switch (decl.getKind()) {
    case clang::Decl::Kind::Record: {
        const clang::RecordDecl* record = llvm::dyn_cast<clang::RecordDecl>(&decl);
        return calculatedJsName + (record->isStruct() ? "Struct" : "Union");
    }
    case clang::Decl::Kind::Function:
        return calculatedJsName + "Function";
    case clang::Decl::Kind::Enum:
        return calculatedJsName + "Enum";
    case clang::Decl::Kind::ObjCInterface:
        return calculatedJsName + "Interface";
    case clang::Decl::Kind::ObjCProtocol:
        return calculatedJsName + "Protocol";
    case clang::Decl::Kind::ObjCCategory:
        return calculatedJsName + "Category";
    case clang::Decl::Kind::ObjCMethod:
        return calculatedJsName + "Method";
    case clang::Decl::Kind::ObjCProperty:
        return calculatedJsName + "Property";
    case clang::Decl::Kind::Var:
        return calculatedJsName + "Var";
    case clang::Decl::Kind::Field:
        return calculatedJsName + "Field";
    case clang::Decl::Kind::EnumConstant:
        return calculatedJsName + "Field";
    default:
        throw logic_error(string("Can't recalculate js name for ") + decl.getDeclKindName() + " type of declaration.");
    }
}
