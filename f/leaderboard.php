<!DOCTYPE html>
<html lang="en">

<head>
<?php
        include 'php/include.php'
?>

    <link rel="stylesheet" href="css/leaderboard.css">
</head>

<body>
    <header>
<?php
        include 'php/header.php'
?>
    </header>

    <main>
        <h3>Leaderboard</h5>
        <div id="my-place" class="my-place"></div>
        <table id="leaderboard-table" class="leaderboard-table">
            <thead>
                <tr>
                    <th>#</th>
                    <th>User</th>
                    <th></th>
                    <th>R$</th>
                </tr>
            </thead>
            <tbody id="leaderboard-body">
                <tr>
                    <td colspan="3">Loading...</td>
                </tr>
            </tbody>
        </table>
    </main>

    <footer>
<?php
        include 'php/footer.php'
?>
    </footer>
    <script type="module" src="leaderboard.js"></script>
</body>

</html>
