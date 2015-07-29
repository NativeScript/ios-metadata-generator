#include "RemoveDuplicateMembersFilter.h"
#include "Meta/Utils.h"

bool areMethodsEqual(Meta::MethodMeta& method1, Meta::MethodMeta& method2)
{
    return (method1.getSelector() == method2.getSelector()) && Meta::Utils::areTypesEqual(method1.signature, method2.signature);
}

bool arePropertiesEqual(Meta::PropertyMeta& prop1, Meta::PropertyMeta& prop2)
{
    if (prop1.id.name == prop2.id.name) {
        if ((bool)prop1.getter == (bool)prop2.getter && (bool)prop1.setter == (bool)prop2.setter) {
            if (prop1.getter)
                return areMethodsEqual(*prop1.getter.get(), *prop2.getter.get());
            else
                return areMethodsEqual(*prop1.setter.get(), *prop2.setter.get());
        }
    }
    return false;
}

void removeDuplicateMethods(std::vector<std::shared_ptr<Meta::MethodMeta> >& from, std::vector<std::shared_ptr<Meta::MethodMeta> >& duplicates)
{
    for (std::shared_ptr<Meta::MethodMeta> dupMethod : duplicates) {
        from.erase(std::remove_if(from.begin(),
                                  from.end(),
                                  [&](std::shared_ptr<Meta::MethodMeta>& method) { return areMethodsEqual(*method.get(), *dupMethod.get()); }),
                   from.end());
    }
}

void removeDuplicateProperties(std::vector<std::shared_ptr<Meta::PropertyMeta> >& from, std::vector<std::shared_ptr<Meta::PropertyMeta> >& duplicates)
{
    for (std::shared_ptr<Meta::PropertyMeta> dupProperty : duplicates) {
        from.erase(std::remove_if(from.begin(),
                                  from.end(),
                                  [&](std::shared_ptr<Meta::PropertyMeta>& property) { return arePropertiesEqual(*property.get(), *dupProperty.get()); }),
                   from.end());
    }
}

void removeDuplicateMembersFromChild(std::shared_ptr<Meta::BaseClassMeta> child, std::shared_ptr<Meta::BaseClassMeta> parent)
{
    removeDuplicateMethods(child->staticMethods, parent->staticMethods);
    removeDuplicateMethods(child->instanceMethods, parent->instanceMethods);
    removeDuplicateProperties(child->properties, parent->properties);
}

void processBaseClassAndHierarchyOf(std::shared_ptr<Meta::BaseClassMeta> child, std::shared_ptr<Meta::BaseClassMeta> parent, Meta::MetaContainer& container)
{
    if (child != parent) {
        removeDuplicateMembersFromChild(child, parent);
    }
    for (const Meta::DeclId& protocolId : parent->protocols) {
        std::shared_ptr<Meta::ProtocolMeta> protocol = container.getMetaAs<Meta::ProtocolMeta>(protocolId);
        if (protocol) {
            processBaseClassAndHierarchyOf(child, protocol, container);
        }
    }
    if (parent->is(Meta::MetaType::Interface)) {
        std::shared_ptr<Meta::InterfaceMeta> parentInterface = std::static_pointer_cast<Meta::InterfaceMeta>(parent);
        if (!parentInterface->base.jsName.empty()) {
            std::shared_ptr<Meta::InterfaceMeta> base = container.getMetaAs<Meta::InterfaceMeta>(parentInterface->base);
            if (base) {
                processBaseClassAndHierarchyOf(child, base, container);
            }
        }
    }
}

void Meta::RemoveDuplicateMembersFilter::filter(MetaContainer& container)
{
    for (ModuleMeta& module : container.top_level_modules()) {
        for (const auto& metaPair : module) {
            std::shared_ptr<Meta> meta = metaPair.second;
            if (meta->is(MetaType::Interface) || meta->is(MetaType::Protocol)) {
                std::shared_ptr<BaseClassMeta> baseClass = std::static_pointer_cast<BaseClassMeta>(meta);
                processBaseClassAndHierarchyOf(baseClass, baseClass, container);
            }
        }
    }
}