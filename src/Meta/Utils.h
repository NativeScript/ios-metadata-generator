#pragma once

#include <clang/AST/Decl.h>

namespace Meta {
    class Type;

    class Utils {
    public:
        template<class T>
        static std::vector<T*> getAttributes(const clang::Decl& decl) {
            std::vector<T*> attributes;
            for (clang::Decl::attr_iterator i = decl.attr_begin(); i != decl.attr_end(); ++i) {
                clang::Attr *attribute = *i;
                if(T *typedAttribute = clang::dyn_cast<T>(attribute)) {
                    attributes.push_back(typedAttribute);
                }
            }
            return attributes;
        }

        static bool areTypesEqual(const Type& type1, const Type& type2);

        static bool areTypesEqual(const std::vector<Type>& types1, const std::vector<Type>& types2);
    };
}