set -xe
cd ../../..
rm -rf bux_deploy
mkdir bux_deploy
(cd bux/f/scripts && ./build.sh)
cp -r bux/f/dist bux_deploy/html_earnobux
cd  bux_deploy 
tar czvf html_earnobux.tar.gz html_earnobux
sftp -b - root@usrv <<EOF
cd /var/www
put html_earnobux.tar.gz
bye
EOF
ssh root@usrv  'rm -rf /var/www/html_earnobux'
ssh root@usrv  'cd /var/www && tar xvf html_earnobux.tar.gz &&  rm html_earnobux.tar.gz'
ssh root@usrv  'chown -R www-data:www-data /var/www/html_earnobux'
ssh root@usrv  'cd /var/www && touch html_earnobux/ && cd html_earnobux/ && find . -exec touch {} +'
echo "deploy finished ok"
