#!/bin/bash
# EscapeSchool Test Runner Script for macOS/Linux
# This script runs Unity tests from the command line

# Default values
TEST_PLATFORM="All"
UNITY_PATH="/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity"
RESULTS_PATH="TestResults"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--platform)
            TEST_PLATFORM="$2"
            shift 2
            ;;
        -u|--unity-path)
            UNITY_PATH="$2"
            shift 2
            ;;
        -r|--results)
            RESULTS_PATH="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: ./run-tests.sh [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  -p, --platform     Test platform: EditMode, PlayMode, or All (default: All)"
            echo "  -u, --unity-path   Path to Unity executable"
            echo "  -r, --results      Results directory (default: TestResults)"
            echo "  -h, --help         Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Find Unity executable
UNITY_EXE=$(ls $UNITY_PATH 2>/dev/null | head -n 1)

if [ -z "$UNITY_EXE" ]; then
    echo "Unity executable not found at: $UNITY_PATH"
    echo "Please specify the correct Unity path using -u parameter"
    echo "Example: ./run-tests.sh -u /Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity"
    exit 1
fi

echo "Using Unity: $UNITY_EXE"

# Create results directory
mkdir -p "$RESULTS_PATH"

PROJECT_PATH="$(cd "$(dirname "$0")" && pwd)/EscapeSchool"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

run_unity_tests() {
    local platform=$1
    local results_file="$RESULTS_PATH/TestResults_${platform}_${TIMESTAMP}.xml"
    
    echo ""
    echo "Running $platform tests..."
    echo "Results will be saved to: $results_file"
    
    "$UNITY_EXE" \
        -runTests \
        -batchmode \
        -projectPath "$PROJECT_PATH" \
        -testResults "$results_file" \
        -testPlatform "$platform" \
        -logFile -
    
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        echo "✓ $platform tests PASSED"
        return 0
    elif [ $exit_code -eq 2 ]; then
        echo "✗ $platform tests FAILED"
        return 1
    else
        echo "✗ $platform tests ERROR (Exit code: $exit_code)"
        return 1
    fi
}

# Run tests based on platform selection
all_passed=true

if [ "$TEST_PLATFORM" = "All" ]; then
    run_unity_tests "EditMode" || all_passed=false
    run_unity_tests "PlayMode" || all_passed=false
else
    run_unity_tests "$TEST_PLATFORM" || all_passed=false
fi

# Summary
echo ""
echo "========================================"
if [ "$all_passed" = true ]; then
    echo "All tests completed successfully!"
    exit 0
else
    echo "Some tests failed. Check results in: $RESULTS_PATH"
    exit 1
fi
