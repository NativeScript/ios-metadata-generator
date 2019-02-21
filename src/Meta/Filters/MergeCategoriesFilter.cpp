//
// Created by Ivan Buhov on 9/5/15.
//

#include "MergeCategoriesFilter.h"

namespace Meta {
static bool isCategory(Meta* meta)
{
    return meta->is(MetaType::Category);
}


class CategoryMetaMerger {
public:
    CategoryMetaMerger(const CategoryMeta& category) :
        category(category)
    {
        assert(category.extendedInterface->introducedIn <= category.introducedIn || category.introducedIn == Version::UnknownVersion);
        assert(category.extendedInterface->deprecatedIn <= category.deprecatedIn || category.deprecatedIn == Version::UnknownVersion);
        assert(category.extendedInterface->obsoletedIn <= category.obsoletedIn || category.obsoletedIn == Version::UnknownVersion);
    }
    
    void mergeMetaFromCategory(Meta* meta) {
        
        // Since we're deleting categories from the metadata we have to
        // transfer the availability attributes to their members
        // (if they haven't explicitly overridden them)
        if (meta->introducedIn == Version::UnknownVersion && this->category.introducedIn != Version::UnknownVersion) {
            meta->introducedIn = this->category.introducedIn;
        }
        if (meta->deprecatedIn == Version::UnknownVersion && this->category.deprecatedIn != Version::UnknownVersion) {
            meta->deprecatedIn = this->category.deprecatedIn;
        }
        if (meta->obsoletedIn == Version::UnknownVersion && this->category.obsoletedIn != Version::UnknownVersion) {
            meta->obsoletedIn = this->category.obsoletedIn;
        }
    }
    
private:
    const CategoryMeta& category;
};
    
    
void MergeCategoriesFilter::filter(std::list<Meta*>& container)
{
    int mergedCategories = 0;

    for (Meta* meta : container) {
        if (meta->is(MetaType::Category)) {
            CategoryMeta& category = meta->as<CategoryMeta>();
            CategoryMetaMerger merger(category);
            
            assert(category.extendedInterface != nullptr);
            InterfaceMeta& interface = *category.extendedInterface;

            for (auto& method : category.instanceMethods) {
                merger.mergeMetaFromCategory(method);
                interface.instanceMethods.push_back(method);
            }

            for (auto& method : category.staticMethods) {
                merger.mergeMetaFromCategory(method);
                interface.staticMethods.push_back(method);
            }

            for (auto& property : category.instanceProperties) {
                merger.mergeMetaFromCategory(property);
                interface.instanceProperties.push_back(property);
            }

            for (auto& property : category.staticProperties) {
                merger.mergeMetaFromCategory(property);
                interface.staticProperties.push_back(property);
            }

            for (auto& protocol : category.protocols) {
                merger.mergeMetaFromCategory(protocol);
                interface.protocols.push_back(protocol);
            }

            mergedCategories++;
        }
    }

    container.remove_if(isCategory);
    std::cout << "Merged " << mergedCategories << " categories." << std::endl;
}
}
