#pragma once

#include <clang/AST/Decl.h>

namespace Meta {
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
    };
}