#!/bin/sh
set -e

# Запускаем сервер MinIO в фоне
minio server /data --console-address ":9001" &
MINIO_PID=$!

# Ждем, пока сервер будет готов (можно использовать более точный механизм ожидания)
sleep 5

# Настраиваем alias для подключения через mc
mc alias set myminio http://localhost:9000 $MINIO_ROOT_USER $MINIO_ROOT_PASSWORD

# Создаем бакет avatars (если он не существует)
mc mb myminio/avatars || true

# Устанавливаем публичную политику для бакета avatars (разрешаем скачивание)
mc anonymous set public myminio/avatars

# Ожидаем завершения работы MinIO
wait $MINIO_PID