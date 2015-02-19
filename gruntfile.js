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
    var generatorSrcDir = srcDir + "/src/generator";
    var mergerSrcDir = srcDir + "/src/merger";
    var outGeneratorDir = generatorSrcDir + "/build";
    var libclangExecutablePath = generatorSrcDir + "/Libclang/bin/Release/";
    var mergerBuildProductPath = mergerSrcDir + "/bin";

    grunt.initConfig({
        pkg: grunt.file.readJSON(srcDir + "/package.json"),
        shell: {
            buildGenerator: {
                command: 'xbuild /t:clean src/generator/Libclang.sln && xbuild /p:Configuration=Release src/generator/Libclang.sln'
            },

            buildMerger: {
                command: 'rm -rf build && mkdir build && cd build && cmake -DCMAKE_BUILD_TYPE=MinSizeRel .. && cmake --build . --target MetaMerge --use-stderr',
                options: {
                    execOptions: {
                        cwd: mergerSrcDir,
                    }
                }
            },

            packageGenerator: {
                command: function(outputPath) {
                    return [
                        util.format('mkdir -p %s', outputPath),
                        util.format('mkbundle -o %s Libclang.exe *.dll -z --config Libclang.exe.config', path.join(outputPath, "MetadataGenerator")),
                        util.format('cd %s', outputPath),
                        'LIBMONO_PATH=`otool -L MetadataGenerator | grep \'libmonoboehm\' | awk \'{ print $1 }\'`',
                        'install_name_tool -change "$LIBMONO_PATH" "/usr/local/lib/`basename $LIBMONO_PATH`" MetadataGenerator',
                    ].join(' && ');
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

            packageMerger: {
                command: function(outputPath) {
                    outputPath = path.resolve(outputPath);
                    return util.format("mkdir -p %s && cp MetaMerge %s", outputPath, path.join(outputPath,  "MetaMerge"));
                },
                options: {
                    execOptions: {
                        cwd: mergerBuildProductPath,
                    }
                }
            }
        }
    });

    grunt.loadNpmTasks("grunt-shell");

    grunt.registerTask("default", [
        "packageGenerator",
        "packageMerger"
    ]);

    grunt.registerTask("packageGenerator", function(outputPath){
        outputPath = path.resolve(outputPath || outGeneratorDir);
        grunt.task.run('shell:buildGenerator');
        grunt.task.run(util.format('shell:packageGenerator:%s', path.resolve(outputPath)));
    });

    grunt.registerTask("packageMerger", function(outputPath){
        grunt.task.run('shell:buildMerger');
        if (outputPath) {
            grunt.task.run(util.format('shell:packageMerger:%s', path.resolve(outputPath)));
        }
    });
};
