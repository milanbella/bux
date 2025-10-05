<!DOCTYPE html>
<html lang="en">

<head>
	<?php
	include 'php/include.php'
	?>

	<link rel="stylesheet" href="css/account.css">
	<script src="https://js.hcaptcha.com/1/api.js" async defer></script>

    <link rel="stylesheet" href="css/php/index_html/testimonials.css">
</head>

<body>
	<header>
		<?php
		include 'php/header.php'
		?>
	</header>

	<main>

		<!--<form id="register-form" class="container-register-form" action="" method="POST">-->
		<form id="register-form" class="container-register-form">
			<h4 class="register-form-header">Pick account where to send R$</h4>
			<div class="register-form-row">
                <label for="username" class="warning-label typing-text">We never ask you for password</label>
                <input type="text" id="username" name="username" placeholder="Roblox username" required class="username-input">
                <div class="rgister-button-container"><button type="submit"> Submit </button></div>
			</div>
			<div class="h-captcha" data-sitekey="9c16973a-002e-43fa-9dd0-fd4899b64e68"></div>
		</form>
		<div id="message"></div>

		<div>
			<a href="avatar.html"> Change your avatar </a>
		</div>

		<div>
            <span id="referrals-count-text"> referrals count: </span> 
        </div>

		<div id="referral-container" class="referral-container">
			<span id="referral-link" class="referral-link">https://example.com/ref?code=ABC123</span>
			<div>
                <button id="referral-copy-btn">Copy</button>
            </div>
		</div>

	</main>

	<footer>
		<?php
		include 'php/footer.php'
		?>
	</footer>
    <?php
    include 'php/index_html/testimonials.php'
    ?>
	<script type="module" src="account.js"></script>
</body>

</html>
