#!/bin/bash
cd ..
rm -rf dist
mkdir dist
npm run build

cp -r css dist
mkdir -p dist/css/widgets/top_earners
cp widgets/top_earners/top_earners.css dist/css/widgets/top_earners

mkdir -p dist/css/php/index_html 
cp php/index_html/top_buttons_left.css dist/css/php/index_html/top_buttons_left.css 
cp php/index_html/top_buttons_right.css dist/css/php/index_html/top_buttons_right.css 

cp -r img dist

php index.php > dist/index.html
php account.php > dist/account.html
php leaderboard.php > dist/leaderboard.html
php offers.php > dist/offers.html
php withdraw.php > dist/withdraw.html
php click_game.php > dist/click_game.html
php guess_game.php > dist/guess_game.html
php avatar.php > dist/avatar.html
php avatars_show.php > dist/avatars_show.html
