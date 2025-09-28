<?php
// defaults
$template = 'home';
$db_connection = 'sqlite:..\private\users.db';
$configuration = array(
    '{FEEDBACK}'          => '',
    '{LOGIN_LOGOUT_TEXT}' => 'Identificar-me',
    '{LOGIN_LOGOUT_URL}'  => '?page=login',   // ← relativo
    '{METHOD}'            => 'POST',          // ← usar POST en formularios
    '{REGISTER_URL}'      => '?page=register',// ← relativo
    '{SITE_NAME}'         => 'La meva pàgina'
);
session_set_cookie_params([
    'lifetime' => 0,         // cookie de sesión (se elimina al cerrar el navegador)
    'path'     => '/',
    'domain'   => '',        // por defecto
    'secure'   => false,     
    'httponly' => true,
    'samesite' => 'Lax'
]);
session_start();


if (isset($_GET['page']) && $_GET['page'] === 'logout') {
    $_SESSION = [];
    if (ini_get('session.use_cookies')) {
        $params = session_get_cookie_params();
        setcookie(session_name(), '', time() - 42000, $params['path'], $params['domain'], $params['secure'], $params['httponly']);
    }
    session_destroy();
    header('Location: ?page=login');
    exit;

}else if (!empty($_SESSION['user'])) {
    $configuration['{FEEDBACK}'] = 'Has iniciat sessió com <b>' . htmlentities($_SESSION['user']) . '</b>';
    $configuration['{LOGIN_LOGOUT_TEXT}'] = 'Tancar "sessió"';
    $configuration['{LOGIN_LOGOUT_URL}']  = '?page=logout';

}else if (isset($_POST['register'])) {
    // --- Validación servidor ---
    $username = trim($_POST['user_name'] ?? '');
    $password = $_POST['user_password'] ?? '';

    // Fuerte: 10+, minúscula, mayúscula, dígito y símbolo
    $strongPwd = '/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{10,}$/';

    // Valida usuario (opcional: ajusta a tu gusto)
    if ($username === '') {
        $configuration['{FEEDBACK}'] = '<mark>ERROR: El nom d\'usuari és obligatori.</mark>';
        $template = 'register';
        $configuration['{REGISTER_USERNAME}'] = '';
    }
    // Valida contraseña
    else if (!preg_match($strongPwd, $password)) {
        $configuration['{FEEDBACK}'] =
            '<mark>ERROR: Contrasenya dèbil.</mark> ' .
            'Ha de tenir com a mínim 10 caràcters, incloent ' .
            'una majúscula, una minúscula, un número i un símbol.';
        $template = 'register';
        $configuration['{REGISTER_USERNAME}'] = htmlentities($username);
    }else {
        try {
            $db = new PDO($db_connection);

            $sql = 'INSERT INTO users (user_name, user_password) VALUES (:user_name, :user_password)';
            $query = $db->prepare($sql);
            $passHash = password_hash($password, PASSWORD_BCRYPT);

            $query->bindValue(':user_name', $username);
            $query->bindValue(':user_password', $passHash);

            if ($query->execute()) {
                $configuration['{FEEDBACK}'] = 'Creat el compte <b>' . htmlentities($username) . '</b>';
                $configuration['{LOGIN_LOGOUT_TEXT}'] = 'Tancar sessió';
                $template = 'login';
                $configuration['{LOGIN_USERNAME}'] = htmlentities($username);

            }
        } catch (Exception $e) {
            $configuration['{FEEDBACK}'] =
                "<mark>ERROR: No s'ha pogut crear el compte <b>" . htmlentities($username) . "</b></mark>";
            $template = 'register';
            $configuration['{REGISTER_USERNAME}'] = htmlentities($username);

        }
        
    }

} else if (isset($_POST['login'])) {

    $db = new PDO($db_connection);
    $sql = 'SELECT * FROM users WHERE user_name = :user_name LIMIT 1';
    $query = $db->prepare($sql);
    $query->bindValue(':user_name', $_POST['user_name']);
    $query->execute();
    $result_row = $query->fetchObject();

    if ($result_row && password_verify($_POST['user_password'], $result_row->user_password)) {
        // Protege contra fijación de sesión
        session_regenerate_id(true);
        $_SESSION['user'] = $result_row->user_name;

        $configuration['{FEEDBACK}'] = '"Sessió" iniciada com <b>' . htmlentities($_SESSION['user']) . '</b>';
        $configuration['{LOGIN_LOGOUT_TEXT}'] = 'Tancar "sessió"';
        $configuration['{LOGIN_LOGOUT_URL}'] = '?page=logout';
        $template = 'home'; 
    } else {
        $configuration['{FEEDBACK}'] = '<mark>ERROR: Usuari desconegut o contrasenya incorrecta</mark>';
        $template = 'login';
        $configuration['{LOGIN_USERNAME}'] = htmlentities($_POST['user_name']);
    }

// --- navegación por GET (mostrar vistas) ---
} else if (isset($_GET['page'])) {

    if ($_GET['page'] == 'register') {
        $template = 'register';
        $configuration['{REGISTER_USERNAME}'] = '';
        $configuration['{LOGIN_LOGOUT_TEXT}'] = 'Ja tinc un compte';

    } else if ($_GET['page'] == 'login') {
        $template = 'login';
        $configuration['{LOGIN_USERNAME}'] = '';

    }
}

// process template and show output
$html = file_get_contents('plantilla_' . $template . '.html', true);
$html = str_replace(array_keys($configuration), array_values($configuration), $html);
echo $html;
