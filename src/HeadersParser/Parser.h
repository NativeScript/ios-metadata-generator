#pragma once

#include <vector>
#include <string>

std::vector<std::string> parsePaths(std::string& paths);

std::string CreateUmbrellaHeader(const std::vector<std::string>& clangArgs);