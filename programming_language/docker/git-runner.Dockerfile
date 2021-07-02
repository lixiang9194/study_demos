FROM php:7.1

MAINTAINER lixiang9194


# Install PHP and composer dependencies
RUN apt-get update &&\
    apt-get install -qq git curl libmcrypt-dev libjpeg-dev libpng-dev libfreetype6-dev libbz2-dev &&\
    apt-get clean


# Install needed extensions
RUN docker-php-ext-install pdo pdo_mysql mcrypt zip gd pcntl opcache bcmath &&\
    mkdir -p /usr/src/php/ext/redis &&\
    curl -L https://pecl.php.net/get/redis-4.3.0.tgz | tar xz -C /usr/src/php/ext/redis --strip 1 &&\
    docker-php-ext-install redis


# Installs Composer to easily manage your PHP dependencies.
# remove composer config -g repo.packagist composer https://packagist.laravel-china.org if you don't locate in China.
RUN curl --silent --show-error https://getcomposer.org/installer | php -- --install-dir=/usr/local/bin --filename=composer &&\
    composer config -g repo.packagist composer https://packagist.laravel-china.org


# Installs ansible. The later will allow you deploy project to remote server
# and execute operation on target server remotely from this build enviornment.
RUN apt-get update &&\
    apt-get install -y --no-install-recommends python-setuptools &&\
    apt-get install -y --no-install-recommends python-pip &&\
    pip install ansible


# Installs NodeJS and npm. The later will allow you to easily manage your frontend dependencies.
RUN apt-get update &&\
    apt-get install -y --no-install-recommends gnupg &&\
    curl -sL https://deb.nodesource.com/setup_10.x | bash - &&\
    apt-get update &&\
    apt-get install -y --no-install-recommends nodejs &&\
    npm config set registry https://registry.npm.taobao.org --global
