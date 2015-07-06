#pragma once
#include "../MetaEntities.h"

bool isSpecialCategory(std::shared_ptr<Meta::CategoryMeta>& category)
{
    Meta::DeclId& id = category->id;
    Meta::DeclId& intId = category->extendedInterface;
    return id.name == "UIResponderStandardEditActions" && id.jsName == "UIResponderStandardEditActions" && id.module->getFullModuleName() == "UIKit.UIResponder" && intId.name == "NSObject" && intId.jsName == "NSObject" && intId.module->getFullModuleName() == "ObjectiveC.NSObject";
}

namespace Meta {
class HandleExceptionalMetasFilter {
public:
    void filter(MetaContainer& container)
    {

        // Remove UIResponderStandardEditActions category
        container.removeCategories(isSpecialCategory);

        // Change the return type of [NSNull null] to instancetype
        // TODO: remove the special handling of [NSNull null] from metadata generator and handle it in the runtime
        if (std::shared_ptr<InterfaceMeta> nsNullMeta = container.getMetaAs<InterfaceMeta>("Foundation", "NSNull")) {

            std::vector<std::shared_ptr<MethodMeta> >::iterator method = std::find_if(nsNullMeta->staticMethods.begin(), nsNullMeta->staticMethods.end(),
                                                                                      [&](const std::shared_ptr<MethodMeta>& method) {
                            return method->getSelector() == "null";
            });

            if (method != nsNullMeta->instanceMethods.end()) {
                (*method)->signature[0] = Type::Instancetype();
            }
        }
    }
};
}