#include "Utils.h"
#include "TypeEntities.h"

bool areIdentifierListsEqual(const std::vector<Meta::DeclId>& vector1, const std::vector<Meta::DeclId>& vector2)
{
    if (vector1.size() != vector2.size()) {
        return false;
    }

    for (std::vector<Meta::DeclId>::size_type i = 0; i < vector1.size(); i++) {
        if (vector1[i] != vector2[i]) {
            return false;
        }
    }
    return true;
}

bool areRecordFieldListsEqual(const std::vector<Meta::RecordField>& vector1, const std::vector<Meta::RecordField>& vector2)
{
    if (vector1.size() != vector2.size()) {
        return false;
    }

    for (std::vector<Meta::RecordField>::size_type i = 0; i < vector1.size(); i++) {
        if ((vector1[i].name != vector2[i].name) || !Meta::Utils::areTypesEqual(vector1[i].encoding, vector2[i].encoding)) {
            return false;
        }
    }
    return true;
}

bool Meta::Utils::areTypesEqual(const Type& type1, const Type& type2)
{
    if (type1.getType() != type2.getType())
        return false;

    switch (type1.getType()) {
    case TypeType::TypeClass: {
        ClassTypeDetails& details1 = type1.getDetailsAs<ClassTypeDetails>();
        ClassTypeDetails& details2 = type2.getDetailsAs<ClassTypeDetails>();
        return areIdentifierListsEqual(details1.protocols, details2.protocols);
    }
    case TypeType::TypeId: {
        IdTypeDetails& details1 = type1.getDetailsAs<IdTypeDetails>();
        IdTypeDetails& details2 = type2.getDetailsAs<IdTypeDetails>();
        return areIdentifierListsEqual(details1.protocols, details2.protocols);
    };
    case TypeType::TypeConstantArray: {
        ConstantArrayTypeDetails& details1 = type1.getDetailsAs<ConstantArrayTypeDetails>();
        ConstantArrayTypeDetails& details2 = type2.getDetailsAs<ConstantArrayTypeDetails>();
        return details1.size == details2.size && areTypesEqual(details1.innerType, details2.innerType);
    };
    case TypeType::TypeIncompleteArray: {
        IncompleteArrayTypeDetails& details1 = type1.getDetailsAs<IncompleteArrayTypeDetails>();
        IncompleteArrayTypeDetails& details2 = type2.getDetailsAs<IncompleteArrayTypeDetails>();
        return areTypesEqual(details1.innerType, details2.innerType);
    };
    case TypeType::TypePointer: {
        PointerTypeDetails& details1 = type1.getDetailsAs<PointerTypeDetails>();
        PointerTypeDetails& details2 = type2.getDetailsAs<PointerTypeDetails>();
        return areTypesEqual(details1.innerType, details2.innerType);
    };
    case TypeType::TypeBlock: {
        BlockTypeDetails& details1 = type1.getDetailsAs<BlockTypeDetails>();
        BlockTypeDetails& details2 = type2.getDetailsAs<BlockTypeDetails>();
        return Utils::areTypesEqual(details1.signature, details2.signature);
    };
    case TypeType::TypeFunctionPointer: {
        FunctionPointerTypeDetails& details1 = type1.getDetailsAs<FunctionPointerTypeDetails>();
        FunctionPointerTypeDetails& details2 = type2.getDetailsAs<FunctionPointerTypeDetails>();
        return Utils::areTypesEqual(details1.signature, details2.signature);
    };
    case TypeType::TypeInterface: {
        InterfaceTypeDetails& details1 = type1.getDetailsAs<InterfaceTypeDetails>();
        InterfaceTypeDetails& details2 = type2.getDetailsAs<InterfaceTypeDetails>();
        return details1.id == details2.id && areIdentifierListsEqual(details1.protocols, details2.protocols);
    };
    case TypeType::TypeStruct: {
        StructTypeDetails& details1 = type1.getDetailsAs<StructTypeDetails>();
        StructTypeDetails& details2 = type2.getDetailsAs<StructTypeDetails>();
        return details1.id == details2.id;
    };
    case TypeType::TypeUnion: {
        UnionTypeDetails& details1 = type1.getDetailsAs<UnionTypeDetails>();
        UnionTypeDetails& details2 = type2.getDetailsAs<UnionTypeDetails>();
        return details1.id == details2.id;
    };
    case TypeType::TypeAnonymousStruct: {
        AnonymousStructTypeDetails& details1 = type1.getDetailsAs<AnonymousStructTypeDetails>();
        AnonymousStructTypeDetails& details2 = type2.getDetailsAs<AnonymousStructTypeDetails>();
        return areRecordFieldListsEqual(details1.fields, details2.fields);
    };
    case TypeType::TypeAnonymousUnion: {
        AnonymousUnionTypeDetails& details1 = type1.getDetailsAs<AnonymousUnionTypeDetails>();
        AnonymousUnionTypeDetails& details2 = type2.getDetailsAs<AnonymousUnionTypeDetails>();
        return areRecordFieldListsEqual(details1.fields, details2.fields);
    };
    default: {
        return true;
    }
    }
}

bool Meta::Utils::areTypesEqual(const std::vector<Meta::Type>& vector1, const std::vector<Meta::Type>& vector2)
{
    if (vector1.size() != vector2.size()) {
        return false;
    }

    for (std::vector<Meta::Type>::size_type i = 0; i < vector1.size(); i++) {
        if (!Meta::Utils::areTypesEqual(vector1[i], vector2[i])) {
            return false;
        }
    }
    return true;
}

bool isAlpha(const std::vector<std::string>& strings, size_t index)
{
    for (auto& str : strings) {
        if (!std::isalpha(str[index])) {
            return false;
        }
    }
    return true;
}

std::string Meta::Utils::getCommonWordPrefix(const std::vector<std::string>& strings)
{
    for (size_t prefixLength = 0; prefixLength < strings[0].size(); prefixLength++) {
        char c = strings[0][prefixLength];
        for (size_t i = 1; i < strings.size(); i++) {
            if (prefixLength >= strings[i].size() || strings[i][prefixLength] != c) {
                while (prefixLength > 0 && (!std::isupper(strings[i][prefixLength]) || !isAlpha(strings, prefixLength))) {
                    prefixLength--;
                }
                return strings[i].substr(0, prefixLength);
            }
        }
    }

    return std::string();
}

void Meta::Utils::getAllLinkLibraries(clang::Module* module, std::vector<clang::Module::LinkLibrary>& result)
{
    for (clang::Module::LinkLibrary lib : module->LinkLibraries)
        result.push_back(lib);
    for (clang::Module::submodule_const_iterator it = module->submodule_begin(); it != module->submodule_end(); ++it)
        getAllLinkLibraries(*it, result);
}
