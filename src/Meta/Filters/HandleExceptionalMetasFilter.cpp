#include <Meta/TypeFactory.h>
#include "HandleExceptionalMetasFilter.h"

namespace Meta {
static bool isSpecialCategory(Meta* meta)
{
    // Remove UIResponderStandardEditActions category
    if (meta->is(MetaType::Category)) {
        InterfaceMeta* extendedInterface = meta->as<CategoryMeta>().extendedInterface;
        return meta->name == "UIResponderStandardEditActions" && meta->module->getFullModuleName() == "UIKit.UIResponder" && extendedInterface->name == "NSObject";
    }
    return false;
}

void HandleExceptionalMetasFilter::filter(std::list<Meta*>& container)
{
    container.remove_if(isSpecialCategory);
    // Change the return type of [NSNull null] to instancetype
    // TODO: remove the special handling of [NSNull null] from metadata generator and handle it in the runtime
    for (Meta* meta : container) {
        if (meta->is(MetaType::Interface) && meta->name == "NSNull" && meta->module->getFullModuleName() == "Foundation.NSNull") {
            InterfaceMeta& nsNullMeta = meta->as<InterfaceMeta>();
            for (MethodMeta* method : nsNullMeta.staticMethods) {
                if (method->getSelector() == "null") {
                    method->signature[0] = TypeFactory::getInstancetype().get();
                    return;
                }
            }
        }
    }
}
}