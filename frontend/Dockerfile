FROM php:8.2-apache
RUN docker-php-ext-install pdo_mysql
COPY ./ /var/www/html/
COPY wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh
EXPOSE 80
ENTRYPOINT ["/wait-for-it.sh", "db:3306", "--timeout=30", "--", "apache2-foreground"]