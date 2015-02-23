var util = require('util');
var path = require('path');
var shell = require('shelljs/global');

module.exports = function (grunt) {

    var srcDir = path.resolve(process.cwd());
    var generatorSrcDir =  path.join(srcDir, "src", "generator");
    var mergerSrcDir = path.join(srcDir, "src", "merger");
    var outGeneratorDir = path.join(generatorSrcDir, "build");
    var libclangExecutablePath = path.join(generatorSrcDir, "Libclang", "bin", "Release");
    var mergerBuildProductPath = path.join(mergerSrcDir, "bin");

    grunt.initConfig({
        pkg: grunt.file.readJSON(path.join(srcDir, "/package.json")),
        shell: {
            buildGenerator: {
                command: 'xbuild /p:Configuration=Release src/generator/Libclang.sln'
            },

            buildMerger: {
                command: [
                    //"rm -rf build",
                    "mkdir -p build",
                    "cd build",
                    "cmake -DCMAKE_BUILD_TYPE=MinSizeRel ..",
                    "cmake --build . --target MetaMerge --use-stderr"].join(" && "),
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
