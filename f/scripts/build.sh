#!/bin/bash
cd ..
rm -rf dist
mkdir dist
npm run build
cp -r css dist
cp -r img dist
php index.php > dist/index.html
php account.php > dist/account.html
php leaderboard.php > dist/leaderboard.html
php offers.php > dist/offers.html
php withdraw.php > dist/withdraw.html
php click_game.php > dist/click_game.html

