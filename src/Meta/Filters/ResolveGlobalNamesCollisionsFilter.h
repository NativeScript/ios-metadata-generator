//
// Created by Ivan Buhov on 9/6/15.
//
#pragma once
#include "Meta/MetaEntities.h"

namespace Meta {
static bool campareMetasByJsName(Meta* meta1, Meta* meta2)
{
    return meta1->jsName < meta2->jsName;
}

class ResolveGlobalNamesCollisionsFilter {
public:
    typedef std::vector<std::pair<clang::Module*, std::vector<Meta*> > > MetasByModules;
    typedef std::unordered_map<std::string, InterfaceMeta*> InterfacesByName;
    typedef std::unordered_map<clang::Module*, std::unordered_map<std::string, std::vector<Meta*> > > ModulesStructure;

    void filter(std::list<Meta*>& container);

    std::unique_ptr<std::pair<MetasByModules, InterfacesByName> > getResult()
    {
        std::unique_ptr<std::pair<MetasByModules, InterfacesByName> > result = llvm::make_unique<std::pair<MetasByModules, InterfacesByName> >(MetasByModules(), InterfacesByName());
        MetasByModules& metasByModules = result->first;
        InterfacesByName& interfacesByName = result->second;
        for (auto& module : _modules) {
            std::pair<clang::Module*, std::vector<Meta*> > modulePair(module.first, std::vector<Meta*>());
            for (const std::pair<std::string, std::vector<Meta*> >& metas : module.second) {
                assert(metas.second.size() == 1);
                for (Meta* meta : metas.second) {
                    modulePair.second.push_back(meta);
                    if (meta->is(MetaType::Interface)) {
                        interfacesByName.insert({ { meta->name, &meta->as<InterfaceMeta>() } });
                    }
                }
            }
            std::sort(modulePair.second.begin(), modulePair.second.end(), campareMetasByJsName);
            metasByModules.push_back(modulePair);
        }

        return result;
    }

private:
    bool addMeta(Meta* meta, bool forceIfNameCollision = false);

    ModulesStructure _modules;
};
}