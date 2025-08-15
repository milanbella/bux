set -xe
rm -rf html_wo.tar.gz
tar czf html_wo.tar.gz \
  --exclude='bux/f/.git' \
  --exclude='bux/f/README.md' \
  -C bux/f/ \
  --transform='s|^|html_wo/|' \
  .
sftp -b - root@usrv <<EOF
cd /var/www
put html_wo.tar.gz
bye
EOF
ssh root@usrv  'rm -rf /var/www/html_wo'
ssh root@usrv  'cd /var/www && tar xvf html_wo.tar.gz &&  rm html_wo.tar.gz'
ssh root@usrv  'chown -R www-data:www-data /var/www/html_wo'
