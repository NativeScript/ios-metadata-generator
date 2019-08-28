#!/bin/sh
set -e

DEVELOPER_DIR=${DEVELOPER_DIR:-$(/usr/bin/xcodebuild -version -sdk iphoneos PlatformPath)/../../}
MDG=$1
if [ -z "$MDG" ] ; then
    cat 1>&2 <<EOF
Metadata generator executable not specified.
Usage:
    $(basename $0) <metadata generator executable>"
EOF
    exit 1
fi

function GenerateMetadata() {
    MDG=$1
    HEADER=$2
    OUTDIR=$3
    (
        cd $(dirname "$MDG")
        SYSROOT=$DEVELOPER_DIR/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator.sdk

        # delete old output files
        rm -rf $OUTDIR
        # start metadata generator and save verbose log to file, while stripping "verbose: " prefixed messages from command's output
        ./$(basename $MDG) -verbose -output-bin $OUTDIR/metadata-x86_64.bin -output-yaml $OUTDIR/metadata-x86_64.yaml -input-umbrella $HEADER \
        Xclang \
        -isysroot $SYSROOT -arch x86_64 -mios-simulator-version-min=9.0 -std=gnu99 -DDEBUG=1 2>&1 | \
        tee "$(dirname $OUTDIR)/verbose.out" | grep -v "verbose: "

        # Unify paths to SDK
        find $OUTDIR -name \*.yaml -type f -exec sed -i "" -e "s/\/.*\/SDKs/...\/SDKs/g" {} \;
    )
}

TESTSDIR=$(dirname $0)
XCODEVERSION=$(/usr/bin/xcodebuild -version | grep Xcode | cut -f2 -d' ')
EXPECTEDOUTPUTDIR="$TESTSDIR/ExpectedOutput$XCODEVERSION"
if [ ! -d "$EXPECTEDOUTPUTDIR" ]; then
    echo 1>&2 "warning: Directory containing expected test results for Xcode version $XCODEVERSION ($EXPECTEDOUTPUTDIR) doesn't exist. Skipping tests!"
    exit 0
fi

TESTOUTPUTDIR=$TESTSDIR/TestOutput

GenerateMetadata $MDG $TESTSDIR/AllSystemFrameworks.h $TESTOUTPUTDIR

echo "Comparing test outputs..."
(diff -qwr $EXPECTEDOUTPUTDIR $TESTOUTPUTDIR && echo "Test run successful, no differences encountered.") ||
(echo "error: Metadata generator didn't produce the expected output. Fix or accept the new one by replacing $EXPECTEDOUTPUTDIR with $TESTOUTPUTDIR" 1>&2 && false)
