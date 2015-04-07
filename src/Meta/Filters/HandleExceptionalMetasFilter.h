#pragma once
#include "../MetaEntities.h"

namespace Meta {
    class HandleExceptionalMetasFilter {
    public:
        void filter(MetaContainer& container) {
            // Remove "copy:" method (from UIResponderStandardEditActions category) from NSObject
            if(std::shared_ptr<InterfaceMeta> nsObjectMeta = container.getMetaAs<InterfaceMeta>("ObjectiveC", "NSObject")) {
                nsObjectMeta->instanceMethods.erase(std::remove_if(nsObjectMeta->instanceMethods.begin(), nsObjectMeta->instanceMethods.end(),
                        [&](std::shared_ptr<MethodMeta>& m) {
                            MethodMeta *method = m.get();
                            return method->selector == "copy:" && method->module == "UIKit.UIResponder";
                        }), nsObjectMeta->instanceMethods.end());
            }

            // Change the return type of [NSNull null] to instancetype
            // TODO: remove the special handling of [NSNull null] from metadata generator and handle it in the runtime
            if(std::shared_ptr<InterfaceMeta> nsNullMeta = container.getMetaAs<InterfaceMeta>("Foundation", "NSNull")) {

                std::vector<std::shared_ptr<MethodMeta>>::iterator method = std::find_if(nsNullMeta->staticMethods.begin(), nsNullMeta->staticMethods.end(),
                        [&](const std::shared_ptr<MethodMeta>& method) {
                            return method->selector == "null";
                        });

                if(method != nsNullMeta->instanceMethods.end()) {
                    (*method)->signature[0] = Type::Instancetype();
                }
            }
        }
    };
}