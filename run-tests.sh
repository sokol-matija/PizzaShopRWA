#!/bin/bash

# Travel Organization System - Test Runner Script
# This script runs all tests with detailed logging and reporting

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Emojis for better visual feedback
TEST_EMOJI="🧪"
SUCCESS_EMOJI="✅"
FAIL_EMOJI="❌"
INFO_EMOJI="ℹ️"
ROCKET_EMOJI="🚀"
REPORT_EMOJI="📊"

echo -e "${BLUE}${TEST_EMOJI} Travel Organization System - Test Suite${NC}"
echo -e "${BLUE}=================================================${NC}"
echo ""

# Function to print section headers
print_section() {
    echo -e "${PURPLE}${1}${NC}"
    echo -e "${PURPLE}$(printf '=%.0s' {1..50})${NC}"
}

# Function to log test execution
log_test_execution() {
    local project=$1
    local description=$2
    echo -e "${CYAN}${ROCKET_EMOJI} Running ${description}...${NC}"
    echo -e "${YELLOW}Project: ${project}${NC}"
    echo -e "${YELLOW}Timestamp: $(date '+%Y-%m-%d %H:%M:%S')${NC}"
    echo ""
}

# Change to the solution directory
cd "$(dirname "$0")/TravelOrganizationSystem"

# Check if solution exists
if [ ! -f "TravelOrganizationSystem.sln" ]; then
    echo -e "${RED}${FAIL_EMOJI} Solution file not found! Make sure you're in the correct directory.${NC}"
    exit 1
fi

print_section "🏗️  Building Solution"
echo -e "${INFO_EMOJI} Restoring NuGet packages..."
dotnet restore

echo -e "${INFO_EMOJI} Building solution in Release mode..."
dotnet build --configuration Release --no-restore

if [ $? -ne 0 ]; then
    echo -e "${RED}${FAIL_EMOJI} Build failed! Please fix compilation errors.${NC}"
    exit 1
fi

echo -e "${GREEN}${SUCCESS_EMOJI} Build completed successfully!${NC}"
echo ""

# Initialize test results tracking
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0
TEST_RESULTS=()

# Function to run tests for a project
run_project_tests() {
    local project_name=$1
    local project_path=$2
    local description=$3
    
    if [ ! -d "$project_path" ]; then
        echo -e "${YELLOW}⚠️  Skipping ${project_name} - directory not found${NC}"
        return 0
    fi
    
    print_section "${TEST_EMOJI} ${description}"
    log_test_execution "$project_name" "$description"
    
    # Run tests with detailed output
    echo -e "${CYAN}Command: dotnet test ${project_path} --configuration Release --no-build --verbosity detailed${NC}"
    echo ""
    
    if dotnet test "$project_path" \
        --configuration Release \
        --no-build \
        --verbosity detailed \
        --logger "console;verbosity=detailed" \
        --logger "trx;LogFileName=${project_name}-results.trx" \
        --collect:"XPlat Code Coverage"; then
        
        echo -e "${GREEN}${SUCCESS_EMOJI} ${description} - All tests passed!${NC}"
        TEST_RESULTS+=("${GREEN}✅ ${description}: PASSED${NC}")
        
        # Extract test count (this is a simplified approach)
        local test_count=$(find "$project_path" -name "*.cs" -path "*/bin/*" -prune -o -name "*Tests.cs" -print | wc -l)
        PASSED_TESTS=$((PASSED_TESTS + test_count))
        TOTAL_TESTS=$((TOTAL_TESTS + test_count))
    else
        echo -e "${RED}${FAIL_EMOJI} ${description} - Some tests failed!${NC}"
        TEST_RESULTS+=("${RED}❌ ${description}: FAILED${NC}")
        FAILED_TESTS=$((FAILED_TESTS + 1))
        TOTAL_TESTS=$((TOTAL_TESTS + 1))
    fi
    
    echo ""
}

# Run tests for each project
run_project_tests "WebAPI.Tests" "WebAPI.Tests" "WebAPI Unit Tests"
run_project_tests "WebApp.Tests" "WebApp.Tests" "WebApp Unit Tests"
run_project_tests "Integration.Tests" "Integration.Tests" "Integration Tests"

# Generate final report
print_section "${REPORT_EMOJI} Test Execution Summary"

echo -e "${BLUE}📈 Overall Results:${NC}"
echo -e "   Total Test Projects: ${TOTAL_TESTS}"
echo -e "   Passed: ${GREEN}${PASSED_TESTS}${NC}"
echo -e "   Failed: ${RED}${FAILED_TESTS}${NC}"
echo ""

echo -e "${BLUE}📋 Detailed Results:${NC}"
for result in "${TEST_RESULTS[@]}"; do
    echo -e "   $result"
done
echo ""

# Test artifacts information
echo -e "${BLUE}📁 Test Artifacts:${NC}"
echo -e "   📊 Test results (TRX): */TestResults/*.trx"
echo -e "   📈 Coverage reports: */TestResults/*/coverage.cobertura.xml"
echo -e "   📝 Detailed logs: Console output above"
echo ""

# Performance metrics
echo -e "${BLUE}⏱️  Performance:${NC}"
echo -e "   🏁 Test execution completed at: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""

# Success/failure determination
if [ $FAILED_TESTS -eq 0 ]; then
    echo -e "${GREEN}${SUCCESS_EMOJI} All tests completed successfully!${NC}"
    echo -e "${GREEN}🚀 Ready for deployment!${NC}"
    exit 0
else
    echo -e "${RED}${FAIL_EMOJI} Some tests failed!${NC}"
    echo -e "${YELLOW}🔧 Please review the detailed output above and fix failing tests.${NC}"
    exit 1
fi