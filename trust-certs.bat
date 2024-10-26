@echo off
set CERT_PATH=service.crt
echo Adding certificate to trusted root store...

rem Добавляем сертификат в хранилище доверенных корневых сертификатов
certutil -addstore "Root" "%CERT_PATH%"

if %errorlevel% equ 0 (
    echo Certificate added successfully.
) else (
    echo Failed to add certificate.
)

pause