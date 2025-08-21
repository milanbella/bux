<!DOCTYPE html>
<html lang="en">

<head>
<?php
	include 'php/include.php'
?>

    <link rel="stylesheet" href="css/guess_game.css">
</head>

<body>
    <header>
<?php
	include 'php/header.php'
?>
    </header>

    <main>
		<h4 id="form-title"> Guess the number </h4>
		<form id="guess-form">
			<input type="number" name="guess-number"/>
			<button id="guess-button" type="button"> Send </button>
		</form>

        <div id="result-container">
            <span id="bux-earned-text"> Congratulation you just earned 1 bux.</span>
        </div>

    </main>

    <footer>
<?php
	include 'php/footer.php'
?>
    </footer>
	<script type="module" src="guess_game.js"></script>
</body>

</html>
