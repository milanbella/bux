find . -path './node_modules' -prune -o -type f -name '*.html' -exec rm -v {} +
find . -path './node_modules' -prune -o -type f -name '*.js' -exec rm -v {} +
find . -path './node_modules' -prune -o -type f -name '*.js.map' -exec rm -v {} +
