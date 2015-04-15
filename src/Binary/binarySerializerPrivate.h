#pragma once

#include "../Meta/MetaEntities.h"

uint8_t convertVersion(Meta::Version version);
bool compareMetas(Meta::Meta& meta1, Meta::Meta& meta2);
bool compareFQN(Meta::FQName& name1, Meta::FQName& name2);
bool isInitMethod(Meta::MethodMeta& meta);