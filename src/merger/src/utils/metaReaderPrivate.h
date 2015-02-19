#ifndef METAREADERPRIVATE_H
#define METAREADERPRIVATE_H

#include "yaml-cpp/yaml.h"
#include "../meta/meta.h"
#include "../typeEncoding/typeEncoding.h"

FQName parseName(const YAML::Node& node);
Version parseVersion(std::string versionStr);
meta::MetaFlags parseFlags(const YAML::Node& node);
void parseSignature(const YAML::Node& signatureNode, vector<std::unique_ptr<typeEncoding::TypeEncoding>>& signatureVector);
std::unique_ptr<typeEncoding::TypeEncoding> parseTypeEncoding(const YAML::Node& node);
std::unique_ptr<meta::Meta> createMeta(const YAML::Node& node);

#endif
