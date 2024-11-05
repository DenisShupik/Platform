#!/bin/sh

until (/usr/bin/mc alias set myminio https://minio:9000 "${MINIO_ACCESS_KEY}" "${MINIO_SECRET_KEY}" --insecure); do
    echo 'Waiting for MinIO...'
    sleep 5
done

# Создаем бакет avatars
/usr/bin/mc mb myminio/avatars --insecure

# Устанавливаем публичный доступ к бакету
/usr/bin/mc anonymous set public myminio/avatars --insecure
