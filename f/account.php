<!DOCTYPE html>
<html lang="en">

<head>
<?php
	include 'php/include.php'
?>

    <link rel="stylesheet" href="css/account.css">
</head>

<body>
    <header>
<?php
	include 'php/header.php'
?>
    </header>

    <main>

        <form id="register-form" class="container-register-form">
            <h4 class="register-form-header">Provide us your roblox usernname to redeem earned bux!</h4>
            <div class="register-form-row">
                <div>
                    <label for="username">roblox username</label>
                    <input type="text" id="username" name="username" required>
                </div>
                <button type="submit"> Submit </button>
            </div>
        </form>
		<div id="message"></div>

    </main>

    <footer>
<?php
	include 'php/footer.php'
?>
    </footer>
	<script type="module" src="register.js"></script>
</body>

</html>
