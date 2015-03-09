#pragma once

#include <string>
#include <vector>
#include <system_error>

std::error_code
CreateUmbrellaHeaderForAmbientModules(const std::vector<std::string>& args,
                                      std::string* umbrellaHeaderContents,
                                      const std::vector<std::string>& moduleBlacklist = std::vector<std::string>());