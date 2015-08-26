#include "HandleExceptionalMetasFilter.h"

static bool isSpecialCategory(std::shared_ptr<Meta::CategoryMeta> category)
{
    std::shared_ptr<Meta::DeclId> id = category->id;
    std::shared_ptr<Meta::DeclId> intId = category->extendedInterface;
    return id->name == "UIResponderStandardEditActions" && id->jsName == "UIResponderStandardEditActions" && id->module->getFullModuleName() == "UIKit.UIResponder" && intId->name == "NSObject" && intId->jsName == "NSObject" && intId->module->getFullModuleName() == "ObjectiveC.NSObject";
}

void Meta::HandleExceptionalMetasFilter::filter(MetaContainer& container)
{
    // Remove UIResponderStandardEditActions category
    container.removeCategories(isSpecialCategory);

    // Change the return type of [NSNull null] to instancetype
    // TODO: remove the special handling of [NSNull null] from metadata generator and handle it in the runtime
    if (std::shared_ptr<InterfaceMeta> nsNullMeta = container.getMetaAs<InterfaceMeta>("Foundation", "NSNull")) {

        auto method = std::find_if(nsNullMeta->staticMethods.begin(), nsNullMeta->staticMethods.end(),
                                   [&](const std::shared_ptr<MethodMeta>& m) {
                      return m->getSelector() == "null";
        });

        if (method != nsNullMeta->instanceMethods.end()) {
            (*method)->signature[0] = Type::Instancetype();
        }
    }
}