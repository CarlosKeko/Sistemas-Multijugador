<?php

session_start();

// Connectar a la base de dades SQLite
try {
    $db = new PDO('sqlite:../private/games.db');
    $db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch (PDOException $e) {
    echo json_encode(['error' => 'Connexió amb la base de dades fallida: ' . $e->getMessage()]);
    exit();
}

$accio = isset($_GET['action']) ? $_GET['action'] : '';

$jugador1Circulo = false;
$jugador2Circulo = false;

switch ($accio) {
    case 'join':
        if (!isset($_SESSION['player_id'])) {
            $_SESSION['player_id'] = uniqid();
        } 
        
        $player_id = $_SESSION['player_id'];
        $game_id = null;
        $numJugador = "";
        $circle_x = 0;
        $circle_y = 0;

        // Intentar unir-se a un joc existent on player2 sigui null
        $stmt = $db->prepare('SELECT * FROM games WHERE player2 IS NULL LIMIT 1');
        $stmt->execute();
        $joc_existent = $stmt->fetch(PDO::FETCH_ASSOC);

        if ($joc_existent) {
            $numJugador = 2;
            // Unir-se al joc existent com a player2
            $game_id = $joc_existent['game_id'];
            $circle_x = $joc_existent['circle_x'];
            $circle_y = $joc_existent['circle_y'];
            $stmt = $db->prepare('UPDATE games SET player2 = :player_id WHERE game_id = :game_id');
            $stmt->bindValue(':player_id', $player_id);
            $stmt->bindValue(':game_id', $game_id);
            $stmt->execute();
        } else {
            $circle_x = $_GET['circle_x'];
            $circle_y = $_GET['circle_y'];
            // Crear un nou joc com a player1
            $numJugador = 1;
            $game_id = uniqid();
            $stmt = $db->prepare('INSERT INTO games (game_id, player1, circle_x, circle_y) VALUES (:game_id, :player_id, :circle_x, :circle_y)');
            $stmt->bindValue(':game_id', $game_id);
            $stmt->bindValue(':player_id', $player_id);
            $stmt->bindValue(':circle_x', $circle_x);
            $stmt->bindValue(':circle_y', $circle_y);

            $stmt->execute();
        }

        echo json_encode(['game_id' => $game_id, 'player_id' => $player_id, 'num_jugador' => $numJugador, 'circle_x' => $circle_x, 'circle_y' => $circle_y]);
        break;

    case 'status':        
        $game_id = $_GET['game_id'];
        $numJugador = $_GET['num_jugador'];
        $circle_x = $_GET['circle_x'];
        $circle_y = $_GET['circle_y'];
        $stmt = $db->prepare('SELECT * FROM games WHERE game_id = :game_id');
        $stmt->bindValue(':game_id', $game_id);
        $stmt->execute();
        $joc = $stmt->fetch(PDO::FETCH_ASSOC);

        if (!$joc) {
            echo json_encode(['error' => 'Joc no trobat']);

        }else {
            if ($numJugador == 1) {
                echo json_encode([
                    'ok' => "todo ok",
                    "player2_x" => $joc['player2_x'],
                    "player2_y" => $joc['player2_y'],
                    'circle_x' => $joc['circle_x'],
                    'circle_y' => $joc['circle_y']
                ]);

            }else {
                echo json_encode([
                    'ok' => "todo ok",
                    'player1_x' => $joc['player1_x'],
                    'player1_y' => $joc['player1_y'],
                    'circle_x' => $joc['circle_x'],
                    'circle_y' => $joc['circle_y']
                ]);
            }
        }
    break;

    case "movement":
        $game_id = $_GET['game_id'];
        $player_id = $_SESSION['player_id'];
        $player_x = $_GET['player_x'];
        $player_y = $_GET['player_y'];

        $stmt = $db->prepare('SELECT * FROM games WHERE game_id = :game_id');
        $stmt->bindValue(':game_id', $game_id);
        $stmt->execute();
        $joc = $stmt->fetch(PDO::FETCH_ASSOC);

        if (!$joc || $joc['winner']) {
            echo json_encode(['error' => 'Joc finalitzat o no trobat']);
            break;
        }

        // Determinar quin jugador ha fet el moviment i actualitzar la seva posició
        if ($joc['player1'] === $player_id) {
            $stmt = $db->prepare('UPDATE games SET player1_x = :player1_x, player1_y = :player1_y WHERE game_id = :game_id');
            $stmt->bindValue(':game_id', $game_id);
            $stmt->bindValue(':player1_x', $player_x);
            $stmt->bindValue(':player1_y', $player_y);
            $stmt->execute();
            $joc['points_player1'] += 1; // Actualitzar l'array $joc

        } elseif ($joc['player2'] === $player_id) {
            $stmt = $db->prepare('UPDATE games SET player2_x = :player2_x, player2_y = :player2_y WHERE game_id = :game_id');
            $stmt->bindValue(':game_id', $game_id);
            $stmt->bindValue(':player2_x', $player_x);
            $stmt->bindValue(':player2_y', $player_y);
            $stmt->execute();
            $joc['points_player2'] += 1; // Actualitzar l'array $joc
            
        } else {
            echo json_encode(['error' => 'Jugador invàlid']);
            break;
        }
        break;

    case 'actualizarCirculo':
        $game_id = $_GET['game_id'];
        $circle_x = $_GET['circle_x'];
        $circle_y = $_GET['circle_y'];
        $num_jugador = $_GET['num_jugador'];
        $stmt = $db->prepare('UPDATE games SET circle_x = :circle_x, circle_y = :circle_y WHERE game_id = :game_id');
        $stmt->bindValue(':game_id', $game_id);
        $stmt->bindValue(':circle_x', $circle_x);
        $stmt->bindValue(':circle_y', $circle_y);
        $stmt->execute();
        
        break;
}