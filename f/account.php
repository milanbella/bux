<!DOCTYPE html>
<html lang="en">

<head>
<?php
	include 'php/include.php'
?>

    <link rel="stylesheet" href="css/account.css">
    <script src="https://js.hcaptcha.com/1/api.js" async defer></script>
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
			<div class="h-captcha" data-sitekey="9c16973a-002e-43fa-9dd0-fd4899b64e68"></div>
        </form>
		<div id="message"></div>

	<form action="" method="POST">
      <input type="text" name="email" placeholder="Email" />
      <input type="password" name="password" placeholder="Password" />
      <div class="h-captcha" data-sitekey="9c16973a-002e-43fa-9dd0-fd4899b64e68"></div>
      <br />
      <input type="submit" value="Submit" />
    </form>

    </main>

    <footer>
<?php
	include 'php/footer.php'
?>
    </footer>
	<script type="module" src="account.js"></script>
</body>

</html>
