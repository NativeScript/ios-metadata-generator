#ifndef METACONTAINER_H
#define METACONTAINER_H

#include <map>
#include <vector>
#include <memory>
#include <set>
#include "../meta/meta.h"
#include "../filters/metaFilter.h"
#include "../utils/serializer.h"

using namespace std;

namespace utils {
    /*
     * \class MetaContainer
     * \brief Meta object container representing vector that can change in size.
     */
    class MetaContainer {
    private:
        map<string, std::unique_ptr<meta::Meta>> _container;
        vector<std::unique_ptr<meta::CategoryMeta>> _categories;
        set<string> _modules;

    public:
        /*
         * \brief Adds a meta object to this container.
         *
         * The module of this meta object is stored for statistical information. Also \c CategoryMeta
         * objects are stored in a separate place and can be retrieved.
         */
        void add(std::unique_ptr<meta::Meta>&& meta);

        /*
         * \brief Returns the number of meta objects in this container.
         */
        int size() const;
        meta::Meta* operator[](string jsName);

        void clearCategories();

        /// filters
        /*
         * \brief Applies a collection of filters to this container.
         */
        void filter(std::vector<const filters::MetaFilter*> filters);

        /// iterators
        /*
         * \brief Returns an iterator pointing to the first category in this container.
         */
        vector<std::unique_ptr<meta::CategoryMeta>>::iterator beginCategories();
        /*
         * \brief Returns an iterator pointing to the past-the-end category in this container.
         */
        vector<std::unique_ptr<meta::CategoryMeta>>::iterator endCategories();

        /*
         * \brief Returns an iterator pointing to the first module name in this container.
         */
        set<string>::const_iterator beginModules() const;
        /*
         * \brief Returns an iterator pointing to the past-the-end module name in this container.
         */
        set<string>::const_iterator endModules() const;

        /// visitors
        /*
         * \brief Serializes this container contents using the given serializer.
         */
        void serialize(Serializer* serializer);
    };
}

#endif
