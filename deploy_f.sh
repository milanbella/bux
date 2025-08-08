#!/bin/bash
set -xe

function make_f_dist() {
    rm -rf f_dist
    cp -r f f_dist

    cd f_dist
        rm -rf node_modules
        rm -rf .git
        rm -f .gitignore
        rm -f package-lock.json
        rm -rf tsconfig.json
        rm -f README.md
        find . -type f -name '*.ts' -exec rm -v {} +
        find . -type f -name '*.swp' -exec rm -v {} +
    cd ..
}

function make_archive() {
    rm -rf html_wo.tar.gz
    tar czf html_wo.tar.gz f_dist
    sftp -b - root@usrv <<EOF
cd /var/www
put html_wo.tar.gz
bye
EOF
}

function copy_to_server {
    ssh root@usrv  'rm -rf /var/www/html_wo'
    ssh root@usrv  'cd /var/www && tar xvf html_wo.tar.gz && mv f_dist html_wo &&  rm html_wo.tar.gz'
    ssh root@usrv  'chown -R www-data:www-data /var/www/html_wo'
}

make_f_dist
make_archive
copy_to_server
rm -rf f_dist
rm -rf html_wo.tar.gz

