var Player1;
var Player2;
var p2_points;
var p1_points;

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
  Player2.update();s
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
// ...existing code...

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
