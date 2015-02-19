#include "metaContainer.h"

void utils::MetaContainer::add(std::unique_ptr<meta::Meta>&& meta) {
    meta::Meta* metaPtr = meta.release();
    meta::CategoryMeta* categoryMeta = dynamic_cast<meta::CategoryMeta*>(metaPtr);
    if (!categoryMeta) {
        this->_modules.emplace(metaPtr->module);
        this->_container.emplace(metaPtr->jsName, std::unique_ptr<meta::Meta>(metaPtr));
    } else {
        this->_categories.push_back(std::unique_ptr<meta::CategoryMeta>(categoryMeta));
    }
}

int utils::MetaContainer::size() const {
    return this->_container.size();
}

meta::Meta* utils::MetaContainer::operator[](std::string jsName) {
    return this->_container[jsName].get();
}

void utils::MetaContainer::clearCategories() {
    this->_categories.clear();
}

void utils::MetaContainer::filter(std::vector<const filters::MetaFilter*> filters) {
    for (const filters::MetaFilter* filter : filters)
    {
        filter->filter(*this);
    }
}

vector<std::unique_ptr<meta::CategoryMeta>>::iterator utils::MetaContainer::beginCategories() {
    return this->_categories.begin();
}

vector<std::unique_ptr<meta::CategoryMeta>>::iterator utils::MetaContainer::endCategories() {
    return this->_categories.end();
}

set<string>::const_iterator utils::MetaContainer::beginModules() const {
    return this->_modules.cbegin();
}

set<string>::const_iterator utils::MetaContainer::endModules() const {
    return this->_modules.cend();
}

void utils::MetaContainer::serialize(utils::Serializer *serializer) {
    serializer->start(this);
    for (auto& metaPair : this->_container) {
        metaPair.second->serialize(serializer);
    }
    serializer->finish(this);
}
