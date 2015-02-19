#include "test.h"
#include "utils/metaContainer.h"

using namespace utils;

static std::unique_ptr<meta::Meta> createTestMeta() {
    meta::FunctionMeta* f = new meta::FunctionMeta();
    f->jsName = "Test";
    f->name = "Test";
    f->module = "TestModule";
    return std::unique_ptr<meta::Meta>(f);
}

static std::unique_ptr<meta::Meta> createTestCategoryMeta() {
    meta::CategoryMeta* f = new meta::CategoryMeta();
    f->jsName = "TestCat";
    f->name = "TestCat";
    f->module = "TestModule";
    f->extendedInterface.name = "Test";
    f->extendedInterface.module = "TestModule";
    return std::unique_ptr<meta::Meta>(f);
}

TEST (MetaContainerTests, TestSizeShouldBe0) {
    MetaContainer target;

    EXPECT_EQ(0, target.size());
}

TEST (MetaContainerTests, TestAddMeta_SizeShouldBe1) {
    MetaContainer target;

    target.add(createTestMeta());

    EXPECT_EQ(1, target.size());
}

TEST (MetaContainerTests, TestAddMeta_MetasShouldBe1) {
    MetaContainer target;

    target.add(createTestMeta());

    EXPECT_EQ(1, target.size());
    EXPECT_TRUE(target["Test"]);
}

TEST (MetaContainerTests, TestAddMeta_ModulesShouldBe1) {
    MetaContainer target;

    target.add(createTestMeta());

    EXPECT_EQ("TestModule", *target.beginModules());
}

TEST (MetaContainerTests, TestAdd2Meta_ModulesShouldBe1) {
    MetaContainer target;

    target.add(createTestMeta());
    target.add(createTestMeta());

    EXPECT_EQ("TestModule", *target.beginModules());
    EXPECT_EQ(1, std::distance(target.beginModules(), target.endModules()));
}

TEST (MetaContainerTests, TestAdd3Meta2Modiles_ModulesShouldBe2) {
    MetaContainer target;

    target.add(createTestMeta());
    target.add(createTestMeta());
    std::unique_ptr<meta::Meta> t = createTestMeta();
    t->module = "NewModule";
    target.add(std::move(t));

    EXPECT_EQ(2, std::distance(target.beginModules(), target.endModules()));
}

TEST (MetaContainerTests, TestAddCategoryMeta_MetasShouldBe0) {
    MetaContainer target;

    target.add(createTestCategoryMeta());

    EXPECT_EQ(0, target.size());
    EXPECT_EQ(1, std::distance(target.beginCategories(), target.endCategories()));
}

TEST (MetaContainerTests, TestAddCategoryMeta_ModulesShouldBe0) {
    MetaContainer target;

    target.add(createTestCategoryMeta());

    EXPECT_EQ(0, target.size());
    EXPECT_EQ(0, std::distance(target.beginModules(), target.endModules()));
}
