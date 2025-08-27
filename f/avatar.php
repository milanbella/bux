<!DOCTYPE html>
<html lang="en">

<head>
<?php
	include 'php/include.php'
?>

    <link rel="stylesheet" href="css/avatar.css">
</head>

<body>
    <header>
<?php
	include 'php/header.php'
?>
    </header>

    <main>
		<form>
			<input id="avatar-input" type="file"></input>
			<button id="upload-btn" type="button"> Save </button>
		</form>

		<div id="preview"></div>

    </main>

    <footer>
<?php
	include 'php/footer.php'
?>
    </footer>
	<script type="module" src="avatar.js"></script>
</body>

</html>
