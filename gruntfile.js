module.exports = function (grunt) {

    var util = require('util');
    var path = require('path');
    var shell = require('shelljs/global');

    var XCODE_PATH = path.dirname(path.dirname(exec('xcode-select -print-path').output.trim()));
    grunt.log.subhead('XCODE_PATH: ' + XCODE_PATH);

    var IPHONEOS_VERSION = exec('xcodebuild -showsdks | grep iphoneos | sort -r | tail -1 | awk \'{print $2}\'').output.trim();
    grunt.log.subhead('IPHONEOS_VERSION: ' + IPHONEOS_VERSION);

    var IPHONEOS_SDK_PATH = path.join(XCODE_PATH, util.format('Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS%s.sdk', IPHONEOS_VERSION));
    grunt.log.subhead('IPHONEOS_SDK_PATH: ' + IPHONEOS_SDK_PATH);

    var srcDir = ".";
    var outFolder = srcDir + "/build";
    var libclangExecutablePath = srcDir + "/src/Libclang/bin/Release/";

    grunt.initConfig({
        pkg: grunt.file.readJSON(srcDir + "/package.json"),
        clean: {
            outFolder: {
                src: [outFolder]
            }
        },
        mkdir: {
            outFolder: {
                options: {
                    create: [outFolder]
                }
            }
        },
        shell: {
            buildVSSolution: {
                command: function (solutionFile) {
                    return util.format('xbuild /t:clean %s && xbuild /p:Configuration=Release %s', solutionFile, solutionFile);
                }
            },

            packageGenerator: {
                command: function() {
                    generatorLocation = path.resolve("build/MetadataGenerator");
                    var mconfigPath = path.join(path.dirname(which('mkbundle')), '..//etc/mono/mconfig/config.xml');
                    return util.format('mkbundle -o %s Libclang.exe Libclang.Core.dll Libclang.DocsetParser.dll NClang.dll Newtonsoft.Json.dll TypeScript.Factory.dll TypeScript.Declarations.dll System.Data.SQLite.dll --deps --static -z --config Libclang.exe.config --machine-config "%s"', generatorLocation, mconfigPath);
                },
                options: {
                    execOptions: {
                        cwd: libclangExecutablePath,
                        env: {
                            'CC': "clang -framework CoreFoundation -liconv -lobjc"
                        }
                    }
                }
            },

            generateMetadata: {
                command: function (umbrellaHeader, outputPath, clangArgs) {
                    if (path.resolve(umbrellaHeader) !== path.normalize(umbrellaHeader)) {
                        umbrellaHeader = path.join('../../../../', umbrellaHeader);
                    }

                    if (path.resolve(outputPath) !== path.normalize(outputPath)) {
                        outputPath = path.join('../../../../', outputPath);
                    }

                    return util.format('mono Libclang.exe -s "%s" -u "%s" -o "%s" -cflags="%s" &&', IPHONEOS_SDK_PATH, umbrellaHeader, outputPath, clangArgs) +
                        util.format('echo "TNS_METADATA_SIZE:" $(du -k %s | awk \'{print $1}\')KB', outputPath);
                },
                options: {
                    execOptions: {
                        cwd: libclangExecutablePath,
                        env: {
                            'DYLD_LIBRARY_PATH': path.join(XCODE_PATH, 'Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib')
                        }
                    }
                }
            }
        }
    });

    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-contrib-copy");
    grunt.loadNpmTasks("grunt-mkdir");
    grunt.loadNpmTasks("grunt-shell");

    grunt.registerTask("default", [
        "package"
    ]);

    grunt.registerTask("build", [
        "shell:buildVSSolution:src/Libclang.sln"
    ]);

    grunt.registerTask("package", [
        "build",
        "clean:outFolder",
        "mkdir:outFolder",
        "shell:packageGenerator"
    ]);

    grunt.registerTask("generate", function (umbrellaHeader, outDirectoryLocation, clangArgs) {
        umbrellaHeader = umbrellaHeader || grunt.option('header');
        outDirectoryLocation = outDirectoryLocation || grunt.option('output');
        clangArgs = clangArgs || grunt.option('cflags') || '';

        grunt.task.run('build');
        grunt.task.run(util.format('shell:generateMetadata:%s:%s:%s', umbrellaHeader, outDirectoryLocation, clangArgs));
    });
};
