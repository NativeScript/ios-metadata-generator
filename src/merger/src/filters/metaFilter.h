#ifndef METAFILTER_H
#define METAFILTER_H

namespace utils {
    class MetaContainer;
}

namespace filters {
    class MetaFilter {
    public:
        virtual void filter(utils::MetaContainer& container) const { }
    };
}

#endif