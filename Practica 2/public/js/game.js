// Variables globals
var Player1;
var Player2;
var p2_points;
var p1_points;

// Variables para la puntuación
var circle;
var circleInterval;
var p1_points = 0;
var p2_points = 0;



// Connectar al servidor del joc
function unirseAlJoc() {
  fetch("game.php?action=join")
    .then((response) => response.json())
    .then((data) => {
      idJoc = data.game_id;
      idJugador = data.player_id;
      comprovarEstatDelJoc();
    });
}

function startGame() {
  Player1 = new component(30, 30, "red", 10, 120);
  Player2 = new component(30, 30, "blue", 300, 120);
  myGameArea.start();
  createCircle();
  // Bucle: cada 2 segundos intenta crear un círculo si no hay uno visible
  circleInterval = setInterval(function() {
    if (!circle.visible) {
      createCircle();
    }
  }, 2000); // Cambia 2000 por 1000 si quieres 1 segundo
}

var myGameArea = {
  canvas: document.createElement("canvas"),
  start: function () {
    this.canvas.width = 480;
    this.canvas.height = 270;
    this.context = this.canvas.getContext("2d");
    document.body.insertBefore(this.canvas, document.body.childNodes[0]);
    this.interval = setInterval(updateGameArea, 20);
  },
  clear: function () {
    this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
  },
};

function component(width, height, color, x, y) {
  this.width = width;
  this.height = height;
  this.speedX = 0;
  this.speedY = 0;
  this.x = x;
  this.y = y;
  this.update = function () {
    ctx = myGameArea.context;
    ctx.fillStyle = color;
    ctx.fillRect(this.x, this.y, this.width, this.height);
  };
  this.newPos = function () {
    this.x += this.speedX;
    this.y += this.speedY;

    // Limitar dentro del canvas
    if (this.x < 0) {
      this.x = 0;
      this.speedX = 0;
    }
    if (this.x + this.width > myGameArea.canvas.width) {
      this.x = myGameArea.canvas.width - this.width;
      this.speedX = 0;
    }
    if (this.y < 0) {
      this.y = 0;
      this.speedY = 0;
    }
    if (this.y + this.height > myGameArea.canvas.height) {
      this.y = myGameArea.canvas.height - this.height;
      this.speedY = 0;
    }
  };
}

// Poner que el area sea un limite
function updateGameArea() {
  myGameArea.clear();
  Player1.newPos();
  Player1.update();
  Player2.newPos();
  Player2.update();
  drawCircle();
  // Comprovar colisiones
  if (checkCollision(Player1)) {
    circle.visible = false;
    p1_points += 1;
    document.getElementById("p1_score").innerText = p1_points;
  }
  if (checkCollision(Player2)) {
    circle.visible = false;
    p2_points += 1;
    document.getElementById("p2_score").innerText = p2_points;
    
  }
  // Mostrar puntuación (opcional)
  var ctx = myGameArea.context;
  ctx.fillStyle = "black";
  ctx.font = "16px Arial";
  ctx.fillText("P1: " + p1_points, 10, 20);
  ctx.fillText("P2: " + p2_points, 400, 20);

  // Comprobar si alguien ha ganado
  if (p1_points >= 10 || p2_points >= 10) {
    clearInterval(myGameArea.interval);      // Detener el juego
    clearInterval(circleInterval);           // Detener aparición de círculos
    ctx.fillStyle = "green";
    ctx.font = "32px Arial";
    let winner = p1_points >= 10 ? "¡Gana el Jugador 1!" : "¡Gana el Jugador 2!";
    ctx.fillText(winner, 120, 140);
  }
}

function moveup() {
  if (Player1.speedY > -5) {
    Player1.speedY -= 1;
  }
}

function movedown() {
  if (Player1.speedY < 5) {
    Player1.speedY += 1;
  }
}

function moveleft() {
  if (Player1.speedX > -5) {
    Player1.speedX -= 1;
  }
}

function moveright() {
  // Poner limites de movimiento
  if (Player1.speedX < 5) {
    Player1.speedX += 1;
  }
}

// Escuchar teclas WASD
document.addEventListener("keydown", function (event) {
  switch (event.key.toLowerCase()) {
    case "w":
      moveup();
      break;
    case "a":
      moveleft();
      break;
    case "s":
      movedown();
      break;
    case "d":
      moveright();
      break;
  }

  //Gestionamos el movimiento del personaje mandandolo al servidor php
  gestionarMoviment();
});

// ...existing code...

//Función para gestionar el movimiento con PHP, mandando un POST con fetch
function gestionarMoviment() {
  fetch(`game.php?action=click&game_id=${idJoc}`)
    .then((response) => response.json())
    .then((data) => {
      if (data.error) {
        alert(data.error);
      }
    });
}

// Crea el círculo negro para la puntuación
function createCircle() {
  // Radio y posición aleatoria dentro del canvas
  var radius = 15;
  var x = Math.random() * (myGameArea.canvas.width - 2 * radius) + radius;
  var y = Math.random() * (myGameArea.canvas.height - 2 * radius) + radius;
  circle = { x: x, y: y, radius: radius, visible: true };
}


// Dibuja el círculo
function drawCircle() {
  if (circle && circle.visible) {
    var ctx = myGameArea.context;
    ctx.beginPath();
    ctx.arc(circle.x, circle.y, circle.radius, 0, 2 * Math.PI);
    ctx.fillStyle = "black";
    ctx.fill();
  }
}


function checkCollision(player) {
  if (!circle.visible) return false;
  // Centro del jugador
  var playerCenterX = player.x + player.width / 2;
  var playerCenterY = player.y + player.height / 2;
  var dx = playerCenterX - circle.x;
  var dy = playerCenterY - circle.y;
  var distance = Math.sqrt(dx * dx + dy * dy);
  // Colisión si la distancia es menor que la suma de los radios
  return distance < (circle.radius + Math.max(player.width, player.height) / 2);
}