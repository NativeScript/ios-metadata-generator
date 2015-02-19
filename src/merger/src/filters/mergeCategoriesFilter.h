#ifndef MERGECATEGORIESFILTER_H
#define MERGECATEGORIESFILTER_H

#include "metaFilter.h"

namespace filters {
    class MergeCategoriesFilter : public MetaFilter {
    public:
        virtual void filter(utils::MetaContainer& container) const override;
    };
}

#endif