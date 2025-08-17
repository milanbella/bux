pids=$(ps -ef | egrep 'nginx$' | awk -c '{print $2}')
echo "INFO: stop nginx"
for pid in $pids
do
  echo "kiling proceess: $pid"
  kill $pid
done
