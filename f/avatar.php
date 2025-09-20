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

        <div class="avatar-container">
            <form id="avatar-form">
                <div id="avatar-buttons-container" class="avatar-buttons-container">
                        <input id="avatar-input" type="file" hidden>
                        <label for="avatar-input" class="file-button">Choose Avatar</label>

                        <button id="upload-btn" type="button"> Save </button>
                </div>
            </form>

            <div class="avatar-preview-container">
                <div id="preview"></div>
            </div>


            <div class="avatar-message-container">
                <div id="message"></div>
            </div>
        </div>

    </main>

    <footer>
<?php
	include 'php/footer.php'
?>
    </footer>
	<script type="module" src="avatar.js"></script>
</body>

</html>
