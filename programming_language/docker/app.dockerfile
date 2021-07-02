FROM php:7.1.22-fpm

# 安装必要的 php 依赖包
RUN apt-get update \
    && apt-get install -qq git curl libmcrypt-dev libjpeg-dev libpng-dev libfreetype6-dev libbz2-dev \
    && apt-get clean

# 安装 php 扩展
RUN docker-php-ext-install pdo pdo_mysql mcrypt zip gd