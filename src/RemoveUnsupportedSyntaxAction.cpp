//
// Created by Ivan Buhov on 6/18/15.
//

#include "RemoveUnsupportedSyntaxAction.h"

std::string GenericDeclarationsProcessor::process(clang::Token& token) {
    switch(stateInfo.state) {
        case State::None : {
            if(token.is(clang::tok::TokenKind::at))
                stateInfo.state = State::ExpectingInterface;
            else
                stateInfo.clear();
            return next(token);
        }
        case State::ExpectingInterface : {
            if(token.is(clang::tok::TokenKind::identifier) && preprocessor.getSpelling(token) == "interface")
                stateInfo.state = State::ExpectingIdentifier;
            else
                stateInfo.clear();
            return next(token);
        }
        case State::ExpectingIdentifier : {
            if(token.is(clang::tok::TokenKind::identifier)) {
                std::string spelling = preprocessor.getSpelling(token);
                if(spelling != "NSObject") {
                    stateInfo.genericInterfaceName = preprocessor.getSpelling(token);
                    stateInfo.state = State::ExpectingLeftAngleBracket;
                }
            } else {
                stateInfo.clear();
            }
            return next(token);
        }
        case State::ExpectingLeftAngleBracket : {
            if(token.is(clang::tok::TokenKind::less)) {
                std::vector<std::string>* genericDeclarations = getFromContext<std::vector<std::string>>(GenericsUsagesProcessor::GenericInterfacesKey);
                if(genericDeclarations != nullptr && std::find(genericDeclarations->begin(), genericDeclarations->end(), stateInfo.genericInterfaceName) == genericDeclarations->end()) {
                    genericDeclarations->push_back(stateInfo.genericInterfaceName);
                }
                stateInfo.state = State::InAngleBrackets;
                stateInfo.angleBracketsLevel++;
                return "";
            } else {
                stateInfo.clear();
                return next(token);
            }
        }
        case State::InAngleBrackets : {
            if(token.is(clang::tok::TokenKind::less))
                stateInfo.angleBracketsLevel++;
            else if(token.is(clang::tok::TokenKind::greater))
                stateInfo.angleBracketsLevel--;
            else if(token.is(clang::tok::TokenKind::lessless))
                stateInfo.angleBracketsLevel += 2;
            else if(token.is(clang::tok::TokenKind::greatergreater))
                stateInfo.angleBracketsLevel -= 2;
            else if(token.is(clang::tok::TokenKind::lesslessless))
                stateInfo.angleBracketsLevel += 3;
            else if(token.is(clang::tok::TokenKind::greatergreatergreater))
                stateInfo.angleBracketsLevel -= 3;
            else if(token.is(clang::tok::TokenKind::identifier) && stateInfo.angleBracketsLevel == 1) {
                stateInfo.genericParametersNames.push_back(preprocessor.getSpelling(token));
                stateInfo.state = State::InAngleBracketsAfterIdentifier;
            }
            if(stateInfo.angleBracketsLevel == 0) {
                stateInfo.state = State::InDeclaration;
            }
            return "";
        }
        case State::InAngleBracketsAfterIdentifier : {
            if(!token.is(clang::tok::TokenKind::greater) && !token.is(clang::tok::TokenKind::comma) && !token.is(clang::tok::TokenKind::colon)) {
                stateInfo.genericParametersNames.pop_back();
            }
            stateInfo.state = State::InAngleBrackets;
            return this->process(token);
        }
        case State::InDeclaration : {
            if(token.is(clang::tok::TokenKind::at))
                stateInfo.state = State::ExpectingEnd;
            else if(token.is(clang::tok::TokenKind::identifier) && std::find(stateInfo.genericParametersNames.begin(), stateInfo.genericParametersNames.end(), preprocessor.getSpelling(token)) != stateInfo.genericParametersNames.end()) {
                std::string result = next(token);
                return (result.empty()) ? result : (token.hasLeadingSpace() ? " id" : "id");
            }
            return next(token);
        }
        case State::ExpectingEnd : {
            if(token.is(clang::tok::TokenKind::identifier) && preprocessor.getSpelling(token) == "end")
                stateInfo.clear();
            else
                stateInfo.state = State::InDeclaration;
            return next(token);
        }
    }
}

const std::string GenericsUsagesProcessor::GenericInterfacesKey("GenericsUsagesProcessor.GenericInterfacesKey");

std::string GenericsUsagesProcessor::process(clang::Token& token) {
    switch(stateInfo.state) {
        case State::None : {
            if(token.is(clang::tok::TokenKind::identifier)) {
                std::string tokenSpelling = preprocessor.getSpelling(token);
                std::vector<std::string>* genericInterfaces = getFromContext<std::vector<std::string>>(GenericsUsagesProcessor::GenericInterfacesKey);
                if(genericInterfaces != nullptr && std::find(genericInterfaces->begin(), genericInterfaces->end(), tokenSpelling) != genericInterfaces->end())
                    stateInfo.state = State::ExpectingLeftAngleBracket;
            }
            return next(token);
        }
        case State::ExpectingLeftAngleBracket : {
            if(token.is(clang::tok::TokenKind::less)) {
                stateInfo.state = State::InAngleBrackets;
                stateInfo.angleBracketsLevel++;
                return "";
            } else {
                stateInfo.clear();
                return next(token);
            }
        }
        case State::InAngleBrackets : {
            if(token.is(clang::tok::TokenKind::less))
                stateInfo.angleBracketsLevel++;
            else if(token.is(clang::tok::TokenKind::greater))
                stateInfo.angleBracketsLevel--;
            else if(token.is(clang::tok::TokenKind::lessless))
                stateInfo.angleBracketsLevel += 2;
            else if(token.is(clang::tok::TokenKind::greatergreater))
                stateInfo.angleBracketsLevel -= 2;
            else if(token.is(clang::tok::TokenKind::lesslessless))
                stateInfo.angleBracketsLevel += 3;
            else if(token.is(clang::tok::TokenKind::greatergreatergreater))
                stateInfo.angleBracketsLevel -= 3;
            if(stateInfo.angleBracketsLevel == 0)
                stateInfo.state = State::ExpectingLeftAngleBracket;
            return "";
        }
    }
}

std::string GenericsForwardDeclarationsProcessor::process(clang::Token& token) {
    switch(stateInfo.state) {
        case State::None : {
            if(token.is(clang::tok::TokenKind::at))
                stateInfo.state = State::ExpectingClassDirective;
            else
                stateInfo.clear();
            return next(token);
        }
        case State::ExpectingClassDirective : {
            if(token.is(clang::tok::TokenKind::identifier) && preprocessor.getSpelling(token) == "class")
                stateInfo.state = State::ExpectingIdentifier;
            else
                stateInfo.clear();
            return next(token);
        }
        case State::ExpectingIdentifier : {
            if(token.is(clang::tok::TokenKind::identifier)) {
                stateInfo.state = State::AfterIdentifier;
                stateInfo.genericInterfaceName = preprocessor.getSpelling(token);
            } else {
                stateInfo.clear();
            }
            return next(token);
        }
        case State::AfterIdentifier : {
            if(token.is(clang::tok::TokenKind::less)) {
                stateInfo.state = State::InAngleBrackets;
                stateInfo.angleBracketsLevel++;
                std::vector<std::string> *genericsNames = getFromContext<std::vector<std::string>>(GenericsUsagesProcessor::GenericInterfacesKey);
                if(genericsNames != nullptr && std::find(genericsNames->begin(), genericsNames->end(), stateInfo.genericInterfaceName) == genericsNames->end()) {
                    genericsNames->push_back(stateInfo.genericInterfaceName);
                }
                return "";
            } else if(token.is(clang::tok::TokenKind::comma)) {
                stateInfo.state = State::ExpectingIdentifier;
                return next(token);
            } else if(token.is(clang::tok::TokenKind::semi)) {
                stateInfo.clear();
                return next(token);
            }
            else {
                return next(token);
            }
        }
        case State::InAngleBrackets : {
            if(token.is(clang::tok::TokenKind::less))
                stateInfo.angleBracketsLevel++;
            else if(token.is(clang::tok::TokenKind::greater))
                stateInfo.angleBracketsLevel--;
            else if(token.is(clang::tok::TokenKind::lessless))
                stateInfo.angleBracketsLevel += 2;
            else if(token.is(clang::tok::TokenKind::greatergreater))
                stateInfo.angleBracketsLevel -= 2;
            else if(token.is(clang::tok::TokenKind::lesslessless))
                stateInfo.angleBracketsLevel += 3;
            else if(token.is(clang::tok::TokenKind::greatergreatergreater))
                stateInfo.angleBracketsLevel -= 3;
            if(stateInfo.angleBracketsLevel == 0)
                stateInfo.state = State::AfterIdentifier;
            return "";
        }
    }
}

std::string NullabilityModifiersProcessor::process(clang::Token& token) {
    switch(stateInfo.state) {
        case State::None : {
            if(token.is(clang::tok::TokenKind::identifier)) {
                std::string spelling = preprocessor.getSpelling(token);
                if(spelling == "nonnull" || spelling == "nullable" || spelling == "null_resettable" || spelling == "null_unspecified"  || spelling == "__null_unspecified") {
                    stateInfo.state = State::AfterNullabilityModifier;
                    return "";
                }
            }
            else if(token.is(clang::tok::TokenKind::kw___attribute)) {
                stateInfo.state = State::AfterAttributeToken;
            }
            return next(token);
        }
        case State::AfterNullabilityModifier : {
            stateInfo.clear();
            return token.is(clang::tok::TokenKind::comma) ? "" : next(token);
        }
        case State::AfterAttributeToken : {
            stateInfo.state = token.is(clang::tok::TokenKind::l_paren) ? State::AfterAttributeAndLeftBrace : State::None;
            return next(token);
        }
        case State::AfterAttributeAndLeftBrace : {
            stateInfo.state = token.is(clang::tok::TokenKind::l_paren) ? State::InAttribute : State::None;
            return next(token);
        }
        case State::InAttribute : {
            stateInfo.state = State::None;
            return next(token);
        }
    }
}

std::string KindOfModifierProcessor::process(clang::Token& token) {
    return (token.is(clang::tok::TokenKind::identifier) && preprocessor.getSpelling(token) == "__kindof") ? "" : next(token);
}

std::string DefaultProcessor::process(clang::Token& token) {
    bool isTokenInvalid = false;
    std::string spelling = preprocessor.getSpelling(token, &isTokenInvalid);
    return isTokenInvalid ? "" : (std::string(token.isAtStartOfLine() ? "\n" : "") + (token.hasLeadingSpace() ? " " : "") + spelling);
}

void RemoveUnsupportedSyntaxActionPPCallbacks::FileChanged(clang::SourceLocation Loc, FileChangeReason Reason, clang::SrcMgr::CharacteristicKind FileType, clang::FileID PrevFID) {
    std::string newFileNotNormalizedPath = action->preprocessor->getSourceManager().getFilename(Loc).str();
    char newFileNormalizedPath [PATH_MAX+1];
    realpath(newFileNotNormalizedPath.c_str(), newFileNormalizedPath);
    std::string newFile(newFileNormalizedPath);

    if(Reason == FileChangeReason::EnterFile && action->filesMap.find(newFile) == action->filesMap.end()) {
        if(this->includeDirectives.size() > 0)
            action->filesMap[action->currentFileName] << std::string("\n") << this->includeDirectives[this->includeDirectives.size() - 1];
        action->filesMap.insert(std::pair<std::string, std::stringstream>(newFile, std::stringstream()));
    }
    action->currentFileName = newFile;
}

void RemoveUnsupportedSyntaxActionPPCallbacks::InclusionDirective(clang::SourceLocation HashLoc, const clang::Token &IncludeTok, clang::StringRef FileName,
                                                                  bool IsAngled, clang::CharSourceRange FilenameRange, const clang::FileEntry *File,
                                                                  clang::StringRef SearchPath, clang::StringRef RelativePath, const clang::Module *Imported) {
    std::string openingSymbol = IsAngled ? "<" : "\"";
    std::string closingSymbol = IsAngled ? ">" : "\"";
    std::stringstream includeDirective;
    includeDirective << "#" << action->preprocessor->getSpelling(IncludeTok) << " " << openingSymbol << FileName.str() << closingSymbol;
    this->includeDirectives.push_back(includeDirective.str());
}

void RemoveUnsupportedSyntaxActionPPCallbacks::EndOfMainFile() {
    //printf("End of processing.");
}

bool RemoveUnsupportedSyntaxAction::BeginSourceFileAction(clang::CompilerInstance &CI, clang::StringRef Filename) {
    // setup preprocessor
    CI.getPreprocessor().addPPCallbacks(std::unique_ptr<clang::PPCallbacks>(new RemoveUnsupportedSyntaxActionPPCallbacks(this)));
    this->preprocessor = &CI.getPreprocessor();
    const clang::FileEntry *mainFileEntry = preprocessor->getFileManager().getFile(Filename);
    clang::FileID fileId = preprocessor->getSourceManager().createFileID(mainFileEntry, clang::SourceLocation(), clang::SrcMgr::CharacteristicKind::C_User);
    preprocessor->getSourceManager().setMainFileID(fileId);
    preprocessor->EnterMainSourceFile();

    std::map<std::string, void*> context;

    // setup token processors
    GenericDeclarationsProcessor tokensProcessor(*preprocessor, context,
    std::make_shared<KindOfModifierProcessor>(*preprocessor, context,
    std::make_shared<NullabilityModifiersProcessor>(*preprocessor, context,
    std::make_shared<GenericsForwardDeclarationsProcessor>(*preprocessor, context,
    std::make_shared<GenericsUsagesProcessor>(*preprocessor, context,
    std::make_shared<DefaultProcessor>(*preprocessor, context, nullptr))))));
    //DefaultProcessor tokensProcessor(*preprocessor, context, nullptr);

    // parse tokens
    clang::Token token;
    this->filesMap["all.h"] = std::stringstream();
    do {
        preprocessor->Lex(token);
        if(preprocessor->getDiagnostics().hasErrorOccurred())
            break;
        std::string tokenAsString = tokensProcessor.process(token);
        this->filesMap[currentFileName] << tokenAsString;
        this->filesMap["all.h"] << tokenAsString;
    } while(token.isNot(clang::tok::eof));

    return true;
}