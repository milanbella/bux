<!DOCTYPE html>
<html lang="en">

<head>
    <?php
    include 'php/include.php'
    ?>

    <link rel="stylesheet" href="css/index.css">

    <link rel="stylesheet" href="css/widgets/top_earners/top_earners.css">
	<script type="module" src="widgets/top_earners/top_earners.js"></script>

    <link rel="stylesheet" href="css/php/index_html/top_buttons_left.css">
    <link rel="stylesheet" href="css/php/index_html/top_buttons_right.css">

</head>

<body>
    <header>
        <?php
        include 'php/header.php'
        ?>
    </header>

    <main>

        <div class="main-content">
            <!--<img src="img/banner.png"></img>-->
			<div id="top-earners"></div>

			<div class="top-buttons">
				<?php
				include 'php/index_html/top_buttons_left.php'
				?>
				<?php
				include 'php/index_html/top_buttons_right.php'
				?>
			</div>
        </div>

    </main>

    <footer>
        <?php
        include 'php/footer.php'
        ?>
    </footer>
</body>

</html>
