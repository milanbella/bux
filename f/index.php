<!DOCTYPE html>
<html lang="en">

<head>
    <?php
    include 'php/include.php'
    ?>

    <link rel="stylesheet" href="css/index.css">

    <!--
    <link rel="stylesheet" href="css/widgets/top_earners/top_earners.css">
	<script type="module" src="widgets/top_earners/top_earners.js"></script>
    -->
    <link rel="stylesheet" href="css/widgets/last_redeemers/last_redeemers.css">
	<script type="module" src="widgets/last_redeemers/last_redeemers.js"></script>

    <link rel="stylesheet" href="css/php/index_html/top_buttons_left.css">
    <link rel="stylesheet" href="css/php/index_html/top_buttons_right.css">

    <link rel="stylesheet" href="css/php/index_html/survey_frame.css">

    <link rel="stylesheet" href="css/php/index_html/testimonials.css">

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
			<!--<div id="top-earners"></div>-->
            <div id="last-redeemers"></div>

            <div class="earn-bux-text-container">
                <b> Earn R$ by completing offers </b> 
            </div>
			<div class="top-buttons">
				<?php
				include 'php/index_html/top_buttons_left.php'
				?>
				<?php
				include 'php/index_html/top_buttons_right.php'
				?>
			</div>
			<?php
			include 'php/index_html/survey_frame.php'
			?>
			<?php
			include 'php/index_html/testimonials.php'
			?>
        </div>

    </main>

    <footer>
        <?php
        include 'php/footer.php'
        ?>
    </footer>
	<script type="module" src="index.js"></script>
</body>

</html>
