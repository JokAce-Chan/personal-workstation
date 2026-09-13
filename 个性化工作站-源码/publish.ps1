# 编译并发布「个性化工作站」到 ..\个性化工作站
# 用法：右键本文件 → 使用 PowerShell 运行；或在 VS Code 里执行任务「发布 exe」

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $here 'PersonalWorkstation\PersonalWorkstation.csproj'
$target = Join-Path (Split-Path -Parent $here) '个性化工作站'

Write-Host '正在编译（Release）...' -ForegroundColor Cyan
dotnet publish $project -c Release -o $target
if ($LASTEXITCODE -ne 0) {
    Write-Host '编译失败，请把上面的错误发给我。' -ForegroundColor Red
    exit $LASTEXITCODE
}

$dataFile = Join-Path $target 'data\工作站数据.json'
if (Test-Path $dataFile) {
    Write-Host '已存在的数据文件保持不动，不会被覆盖。' -ForegroundColor Yellow
}

Write-Host "完成。程序在：$target" -ForegroundColor Green