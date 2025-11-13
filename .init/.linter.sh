#!/bin/bash
cd /home/kavia/workspace/code-generation/ai-trends-report-generator-for-mechanical-engineering-188692-188702/backend
dotnet build --no-restore -v quiet -nologo -consoleloggerparameters:NoSummary /p:TreatWarningsAsErrors=false
LINT_EXIT_CODE=$?
if [ $LINT_EXIT_CODE -ne 0 ]; then
  exit 1
fi

