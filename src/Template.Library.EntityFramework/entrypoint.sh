#!/bin/bash
set -e

dotnet ef database update ${TARGET_MIGRATION} -v --configuration Release
