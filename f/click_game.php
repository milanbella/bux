<!DOCTYPE html>
<html lang="en">

<head>
<?php
	include 'php/include.php'
?>

    <link rel="stylesheet" href="css/click_game.css">
</head>

<body>
    <header>
<?php
	include 'php/header.php'
?>
    </header>

    <main>

		<button id="click-button"> Click me 10x for a bux!</button>

        <div id="result-container">
            <span id="bux-earned-text"> Congratulation you just earned 1 bux.</span>
        </div>

    </main>

    <footer>
<?php
	include 'php/footer.php'
?>
    </footer>
	<script type="module" src="click_game.js"></script>
</body>

</html>
