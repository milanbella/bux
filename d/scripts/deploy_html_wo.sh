set -xe
cd ../../..
rm -rf bux_deploy
mkdir bux_deploy
(cd bux/f/scripts && ./build.sh)
cp -r bux/f/dist bux_deploy/html_wo
cd  bux_deploy 
tar czvf html_wo.tar.gz html_wo
sftp -b - root@usrv <<EOF
cd /var/www
put html_wo.tar.gz
bye
EOF
ssh root@usrv  'rm -rf /var/www/html_wo'
ssh root@usrv  'cd /var/www && tar xvf html_wo.tar.gz &&  rm html_wo.tar.gz'
ssh root@usrv  'chown -R www-data:www-data /var/www/html_wo'
echo "deploy finished ok"
