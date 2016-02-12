#include "HandleMethodsAndPropertiesWithSameNameFilter.h"

namespace Meta {
HandleMethodsAndPropertiesWithSameNameFilter::HandleMethodsAndPropertiesWithSameNameFilter(MetaFactory& metaFactory)
    : m_metaFactory(metaFactory) {
}

void HandleMethodsAndPropertiesWithSameNameFilter::filter(std::list<Meta*>& container) {
    for (Meta* meta : container) {
        if (meta->is(MetaType::Interface)) {
            const clang::ObjCInterfaceDecl* decl = clang::cast<clang::ObjCInterfaceDecl>(meta->declaration);
            for (clang::ObjCPropertyDecl* propertyDecl : decl->properties()) {

                if (clang::ObjCInterfaceDecl* parent = decl->getSuperClass()) {
                    clang::ObjCMethodDecl* duplicate = parent->lookupInstanceMethod(propertyDecl->getGetterName());
                    replaceMethodWithPropertyIfNecessary(duplicate, propertyDecl);
                }

                for (clang::ObjCProtocolDecl* protocol : decl->protocols()) {
                    clang::ObjCMethodDecl* duplicate = protocol->lookupInstanceMethod(propertyDecl->getGetterName());
                    replaceMethodWithPropertyIfNecessary(duplicate, propertyDecl);
                }
            }
        }
    }
}

void HandleMethodsAndPropertiesWithSameNameFilter::replaceMethodWithPropertyIfNecessary(clang::ObjCMethodDecl* duplicateMethod, clang::ObjCPropertyDecl* propertyDecl) {
    if (duplicateMethod && !duplicateMethod->isPropertyAccessor()) {
        clang::Decl* parent_decl = clang::dyn_cast<clang::Decl>(duplicateMethod->getParent());

        Cache& cache = this->m_metaFactory.getCache();
        auto cachedMetaIt = cache.find(parent_decl);
        auto cachedMethodIt = cache.find(duplicateMethod);
        if (cachedMetaIt != cache.end() && cachedMethodIt != cache.end()) {
            BaseClassMeta* parent_meta = static_cast<BaseClassMeta*>(cachedMetaIt->second.first.get());
            MethodMeta* duplicated_method = static_cast<MethodMeta*>(cachedMethodIt->second.first.get());

            std::vector<MethodMeta*>& instanceMethods = parent_meta->instanceMethods;
            auto instanceMethod = std::find(instanceMethods.begin(), instanceMethods.end(), duplicated_method);
            if (instanceMethod != instanceMethods.end()) {
                instanceMethods.erase(instanceMethod);

                clang::SourceRange parentSourceRange = parent_decl->getSourceRange();
                clang::ObjCPropertyDecl* property_decl = clang::ObjCPropertyDecl::Create(parent_decl->getASTContext(), duplicateMethod->getDeclContext(), duplicateMethod->getSourceRange().getBegin(), duplicateMethod->getSelector().getIdentifierInfoForSlot(0), parentSourceRange.getEnd(), parentSourceRange.getBegin(), duplicateMethod->getReturnType(), duplicateMethod->getReturnTypeSourceInfo());
                property_decl->setGetterMethodDecl(duplicateMethod);

                PropertyMeta* property_meta = static_cast<PropertyMeta*>(this->m_metaFactory.create(*property_decl));
                parent_meta->properties.push_back(property_meta);
            }
        }
    }
}
}