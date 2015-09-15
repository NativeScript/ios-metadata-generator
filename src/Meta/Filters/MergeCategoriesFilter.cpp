//
// Created by Ivan Buhov on 9/5/15.
//

#include "MergeCategoriesFilter.h"

namespace Meta {
static bool isCategory(Meta* meta)
{
    return meta->is(MetaType::Category);
}

void MergeCategoriesFilter::filter(std::list<Meta*>& container)
{
    int mergedCategories = 0;

    for (Meta* meta : container) {
        if (meta->is(MetaType::Category)) {
            CategoryMeta& category = meta->as<CategoryMeta>();
            assert(category.extendedInterface != nullptr);
            InterfaceMeta& interface = *category.extendedInterface;

            for (auto& method : category.instanceMethods) {
                interface.instanceMethods.push_back(method);
            }

            for (auto& method : category.staticMethods) {
                interface.staticMethods.push_back(method);
            }

            for (auto& property : category.properties) {
                interface.properties.push_back(property);
            }

            for (auto& protocol : category.protocols) {
                interface.protocols.push_back(protocol);
            }

            mergedCategories++;
        }
    }

    container.remove_if(isCategory);
    std::cout << "Merged " << mergedCategories << " categories." << std::endl;
}
}
