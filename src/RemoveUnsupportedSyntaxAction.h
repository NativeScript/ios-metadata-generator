//
// Created by Ivan Buhov on 6/18/15.
//

#pragma once

#include <sstream>
#include <deque>
#include <clang/Lex/Preprocessor.h>
#include <clang/Frontend/FrontendActions.h>
#include <clang/Frontend/CompilerInstance.h>

class RemoveUnsupportedSyntaxAction;

class TokensProcessor {
protected:
    clang::Preprocessor& preprocessor;
    std::shared_ptr<TokensProcessor> nextProcessor;
    std::map<std::string, void*>& context;

    std::string next(clang::Token& token) {
        return nextProcessor ? nextProcessor->process(token) : "";
    }

public:
    TokensProcessor(clang::Preprocessor& preprocessor, std::map<std::string, void*>& context, std::shared_ptr<TokensProcessor> next = nullptr)
            : preprocessor(preprocessor),
              context(context),
              nextProcessor(next) {}

    virtual std::string process(clang::Token& token) = 0;

    template<class T>
    T* getFromContext(std::string key) {
        auto it = context.find(key);
        return (it == context.end()) ? nullptr : static_cast<T*>(it->second);
    }

    void addToContext(std::string key, void* value) {
        context[key] = value;
    }
};

class GenericDeclarationsProcessor : public TokensProcessor {
    enum State {
        None = 0,
        RecognitionPhase1, // @ (searching for "@interface identifier<")
        RecognitionPhase2, // @interface
        RecognitionPhase3, // @interface identifier
        InAngleBrackets, // @interface identifier<
        InDeclaration,
        EndRecognitionPhase1 // @ (searching for "@end")
    };

    struct StateInfo {
        State state = State::None;
        int angleBracketsLevel = 0;
        std::string genericInterfaceName;
        std::string genericParameterName;

        void clear() {
            state = State::None;
            angleBracketsLevel = 0;
            genericInterfaceName = "";
            genericParameterName = "";
        }
    };

private:
    StateInfo stateInfo;
public:

    GenericDeclarationsProcessor(clang::Preprocessor& preprocessor, std::map<std::string, void*>& context, std::shared_ptr<TokensProcessor> next = nullptr)
            : TokensProcessor(preprocessor, context, next) {}

    virtual std::string process(clang::Token& token) override;
};

class GenericsUsagesProcessor : public TokensProcessor {
    enum State {
        None = 0,
        AfterColon,
        RecognitionPhase1, // @ (searching for "identifier<")
        InAngleBrackets // identifier<
    };

    struct StateInfo {
        State state = State::None;
        int angleBracketsLevel = 0;

        void clear() {
            state = State::None;
            angleBracketsLevel = 0;
        }
    };

private:
    StateInfo stateInfo;
public:
    GenericsUsagesProcessor(clang::Preprocessor& preprocessor, std::map<std::string, void*>& context, std::shared_ptr<TokensProcessor> next = nullptr)
            : TokensProcessor(preprocessor, context, next) {
        addToContext(GenericsUsagesProcessor::GenericInterfacesKey, new std::vector<std::string>);
    }

    static const std::string GenericInterfacesKey;

    virtual std::string process(clang::Token& token) override;
};

class GenericsForwardDeclarationsProcessor : public TokensProcessor {
    enum State {
        None = 0,
        RecognitionPhase1, // @ (searching for "@class identifier<..>")
        RecognitionPhase2, // @class (searching for "@class identifier<..>")
        RecognitionPhase3, // @class identifier (searching for "@class identifier<..>")
        InAngleBrackets, // @class identifier<...> (searching for "@class identifier<..>")
    };

    struct StateInfo {
        State state = State::None;
        std::string genericInterfaceName;
        int angleBracketsLevel = 0;

        void clear() {
            state = State::None;
            genericInterfaceName = "";
            angleBracketsLevel = 0;
        }
    };

private:
    StateInfo stateInfo;
public:
    GenericsForwardDeclarationsProcessor(clang::Preprocessor& preprocessor, std::map<std::string, void*>& context, std::shared_ptr<TokensProcessor> next = nullptr)
            : TokensProcessor(preprocessor, context, next) { }

    virtual std::string process(clang::Token& token) override;
};

class NullabilityModifiersProcessor : public TokensProcessor {
    enum State {
        None = 0,
        AfterNullabilityModifier
    };

    struct StateInfo {
        State state = State::None;

        void clear() {
            state = State::None;
        }
    };

private:
    StateInfo stateInfo;
public:
    NullabilityModifiersProcessor(clang::Preprocessor& preprocessor, std::map<std::string, void*>& context, std::shared_ptr<TokensProcessor> next = nullptr)
            : TokensProcessor(preprocessor, context, next) { }

    virtual std::string process(clang::Token& token) override;
};

class DefaultProcessor : public TokensProcessor {
public:
    DefaultProcessor(clang::Preprocessor& preprocessor, std::map<std::string, void*>& context, std::shared_ptr<TokensProcessor> next = nullptr)
            : TokensProcessor(preprocessor, context, next) {}

    virtual std::string process(clang::Token& token) override;
};

class RemoveUnsupportedSyntaxActionPPCallbacks : public clang::PPCallbacks {
private:
    RemoveUnsupportedSyntaxAction *action;
    std::vector<std::string> includeDirectives;
public:
    RemoveUnsupportedSyntaxActionPPCallbacks(RemoveUnsupportedSyntaxAction *action)
            : action(action),
              includeDirectives() {}

    virtual void FileChanged(clang::SourceLocation Loc, FileChangeReason Reason, clang::SrcMgr::CharacteristicKind FileType, clang::FileID PrevFID) override;

    virtual void InclusionDirective(clang::SourceLocation HashLoc, const clang::Token &IncludeTok, clang::StringRef FileName, bool IsAngled, clang::CharSourceRange FilenameRange,
                                    const clang::FileEntry *File, clang::StringRef SearchPath, clang::StringRef RelativePath, const clang::Module *Imported) override;

    virtual void EndOfMainFile() override;
};

class RemoveUnsupportedSyntaxAction : public clang::PreprocessorFrontendAction {
private:
    friend class RemoveUnsupportedSyntaxActionPPCallbacks;
    clang::Preprocessor *preprocessor;
    std::map<std::string, std::stringstream>& filesMap;
    std::string currentFileName;

public:
    RemoveUnsupportedSyntaxAction(std::map<std::string, std::stringstream>& filesMap)
            : preprocessor(nullptr),
              filesMap(filesMap) {}

    virtual void ExecuteAction() override {}

    virtual bool BeginSourceFileAction(clang::CompilerInstance &CI, clang::StringRef Filename) override;
};

