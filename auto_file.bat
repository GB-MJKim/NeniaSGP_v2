@echo off
setlocal enabledelayedexpansion

rem ===== 사용자 환경에 맞춘 값 =====
set "SOLUTION_NAME=NeniaSGP_v2"
set "PROJECT_NAME=Nenia.SGP"
rem ================================

set "SOLUTION_ROOT=%CD%"
set "PROJECT_ROOT=%SOLUTION_ROOT%\%PROJECT_NAME%"

if not exist "%SOLUTION_ROOT%\%SOLUTION_NAME%.sln" (
  echo [WARN] "%SOLUTION_NAME%.sln" not found in current folder.
  echo        Current = "%SOLUTION_ROOT%"
)

if not exist "%PROJECT_ROOT%\" (
  echo [ERROR] Project folder not found: "%PROJECT_ROOT%"
  echo         Make sure you run this bat in the solution folder.
  pause
  exit /b 1
)

echo [INFO] SolutionRoot = "%SOLUTION_ROOT%"
echo [INFO] ProjectRoot  = "%PROJECT_ROOT%"

rem 1) 폴더 생성
for %%D in (
  "Models"
  "Services"
  "ViewModels"
  "Views"
  "Utilities"
  "Resources"
  "Resources\Styles"
  "Resources\Icons"
  "Data"
  "Images"
  "Images\Main"
  "Images\Thumbnails"
  "Exports"
  "Logs"
) do (
  if not exist "%PROJECT_ROOT%\%%~D\" mkdir "%PROJECT_ROOT%\%%~D" 2>nul
)

rem 2) 빈 파일(스텁) 생성 - 없을 때만 생성
call :touch "%PROJECT_ROOT%\Models\Product.cs"
call :touch "%PROJECT_ROOT%\Models\Region.cs"
call :touch "%PROJECT_ROOT%\Models\RegionalPrice.cs"

call :touch "%PROJECT_ROOT%\Services\DatabaseService.cs"
call :touch "%PROJECT_ROOT%\Services\ProductService.cs"
call :touch "%PROJECT_ROOT%\Services\RegionService.cs"
call :touch "%PROJECT_ROOT%\Services\CsvImportExportService.cs"
call :touch "%PROJECT_ROOT%\Services\ImageProcessingService.cs"
call :touch "%PROJECT_ROOT%\Services\BackupService.cs"

call :touch "%PROJECT_ROOT%\ViewModels\ProductListViewModel.cs"
call :touch "%PROJECT_ROOT%\ViewModels\ProductEditViewModel.cs"
call :touch "%PROJECT_ROOT%\ViewModels\RegionManagementViewModel.cs"
call :touch "%PROJECT_ROOT%\ViewModels\PageLayoutViewModel.cs"
call :touch "%PROJECT_ROOT%\ViewModels\PriceSettingViewModel.cs"
call :touch "%PROJECT_ROOT%\ViewModels\BackupViewModel.cs"

call :touch "%PROJECT_ROOT%\Utilities\PathHelper.cs"
call :touch "%PROJECT_ROOT%\Utilities\ValidationHelper.cs"

call :touch "%PROJECT_ROOT%\Resources\Styles\_Index.xaml"

echo [DONE] Created under "%PROJECT_ROOT%".
pause
exit /b 0

:touch
set "FILE=%~1"
if exist "%FILE%" exit /b 0
type nul > "%FILE%"
exit /b 0
