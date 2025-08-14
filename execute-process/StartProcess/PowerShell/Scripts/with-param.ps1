param (
    [string]$path
)

if (Test-Path $path) {
    Write-Host "File exist: $path"
} else {
    Write-Host "`e[1;31mFile not exitst:`e[0m"
    Write-Host "$path"
}
