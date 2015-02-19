#ifndef BINARYSERIALIZERPRIVATE_H
#define BINARYSERIALIZERPRIVATE_H

#include "../meta/meta.h"

uint8_t convertVersion(Version version);
bool compareMetas(meta::Meta& meta1, meta::Meta& meta2);
bool compareFQN(FQName& name1, FQName& name2);
bool isInitMethod(meta::MethodMeta& meta);

#endif
