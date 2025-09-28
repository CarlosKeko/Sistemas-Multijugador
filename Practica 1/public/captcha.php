<?php
// Simple self-hosted CAPTCHA image generator (no external services)
declare(strict_types=1);
session_name('SID');
session_start();

// Settings
$length    = 5;
$width     = 160;
$height    = 50;
$font_path = __DIR__ . '/fonts/DejaVuSans.ttf'; // fallback to built-in if missing
$expires_s = 180; // 3 minutos

// Generate code (A-Z 2-9 sin confusiones)
$alphabet = str_split('ABCDEFGHJKLMNPQRSTUVWXYZ23456789');
$code = '';
for ($i = 0; $i < $length; $i++) {
    $code .= $alphabet[random_int(0, count($alphabet)-1)];
}

// Guardar solución en sesión (de un solo uso)
$_SESSION['captcha_answer']  = $code;
$_SESSION['captcha_expires'] = time() + $expires_s;

// Crear imagen
$img = imagecreatetruecolor($width, $height);
$bg  = imagecolorallocate($img, 255, 255, 255);
$fg  = imagecolorallocate($img, 0, 0, 0);
$noise = imagecolorallocate($img, 120, 120, 120);
imagefilledrectangle($img, 0, 0, $width, $height, $bg);

// Ruido
for ($i=0; $i<8; $i++) {
    imageline($img, random_int(0,$width), random_int(0,$height), random_int(0,$width), random_int(0,$height), $noise);
}
for ($i=0; $i<200; $i++) {
    imagesetpixel($img, random_int(0,$width-1), random_int(0,$height-1), $noise);
}

// Texto
if (file_exists($font_path)) {
    $font_size = 20;
    $angle = random_int(-12, 12);
    $bbox = imagettfbbox($font_size, $angle, $font_path, $code);
    $text_width  = $bbox[2] - $bbox[0];
    $text_height = $bbox[1] - $bbox[7];
    $x = (int)(($width - $text_width)/2);
    $y = (int)(($height + $text_height)/2);
    imagettftext($img, $font_size, $angle, $x, $y, $fg, $font_path, $code);
} else {
    // Fallback
    $font = 5;
    $text_width = imagefontwidth($font) * strlen($code);
    $text_height = imagefontheight($font);
    $x = (int)(($width - $text_width)/2);
    $y = (int)(($height - $text_height)/2);
    imagestring($img, $font, $x, $y, $code, $fg);
}

// Output sin caché
header('Content-Type: image/png');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('Pragma: no-cache');
imagepng($img);
imagedestroy($img);
