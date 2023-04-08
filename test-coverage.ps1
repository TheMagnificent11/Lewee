if (Test-Path -Path .\coverage) {
  Remove-Item .\coverage -Recurse -Force
}

if (Test-Path -Path .\coverage-logs) {
  Remove-Item .\coverage-logs -Recurse -Force
}

if (Test-Path -Path .\coverage-report) {
  Remove-Item .\coverage-report -Recurse -Force
}

dotnet test .\Lewee-CI.sln --settings coverage.runsettings --results-directory .\coverage --diag .\coverage-logs\log.txt
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"html"
