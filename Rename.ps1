param([string]$name)
$ErrorActionPreference = 'Stop'

If ([String]::IsNullOrEmpty($name)) {
    Throw 'Please input new name!'
}

git version
git clean -dfx

function Update-Content() {
    param([Object]$file)

    (Get-Content $file) -Replace "$placeholder", "$name" | Set-Content $file
}

function Update-FileName() {
    param([Object]$file)

    Rename-Item $file -NewName ($file.Name -Replace "$placeholder", "$name")
}

function Update-DirectoryName() {
    param([Object]$directory)

    Rename-Item $directory.FullName -NewName ($directory.Name -Replace "$placeholder", "$name")
}

$placeholder = 'LibTemplate'

$all_files = Get-ChildItem -Exclude *.ps1, *.gitignore, *.editorconfig -File -Recurse
$all_directorys = Get-ChildItem -Directory -Recurse -Filter "*$placeholder*" | Select-Object FullName, Name, @{n = 'FullNameLength'; e = { $_.Parent.FullName.Length } } | Sort-Object -Property FullNameLength -Descending

$all_files | ForEach-Object {
    Update-Content($_)
    Update-FileName($_)
}

$all_directorys | ForEach-Object {
    Update-DirectoryName($_)
}
