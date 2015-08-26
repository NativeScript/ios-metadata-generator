#pragma once

#include "Meta/MetaEntities.h"

uint8_t convertVersion(Meta::Version version);
bool compareMetas(Meta::Meta& meta1, Meta::Meta& meta2);
bool compareIdentifiers(std::shared_ptr<Meta::DeclId> id1, std::shared_ptr<Meta::DeclId> id2);
bool isInitMethod(Meta::MethodMeta& meta);