#include <iostream>
#include "mergeCategoriesFilter.h"
#include "../utils/metaContainer.h"

void filters::MergeCategoriesFilter::filter(utils::MetaContainer& container) const {
    int categoriesCount = 0;

    for (vector<std::unique_ptr<meta::CategoryMeta>>::iterator categoryIterator = container.beginCategories(); categoryIterator != container.endCategories(); ++categoryIterator) {
        categoriesCount++;
        meta::InterfaceMeta* extendedInterface = dynamic_cast<meta::InterfaceMeta*>(container[(*categoryIterator)->extendedInterface.name]);

        for (auto& method : (*categoryIterator)->instanceMethods) {
            extendedInterface->instanceMethods.push_back(std::move(method));
        }

        for (auto& method : (*categoryIterator)->staticMethods) {
            extendedInterface->staticMethods.push_back(std::move(method));
        }

        for (auto& property : (*categoryIterator)->properties) {
            extendedInterface->properties.push_back(std::move(property));
        }

        for (auto& protocol : (*categoryIterator)->protocols) {
            extendedInterface->protocols.push_back(protocol);
        }
    }

    std::cout << "Merged " << categoriesCount << " categories." << endl;
    container.clearCategories();
}
