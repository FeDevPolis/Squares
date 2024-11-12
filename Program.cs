using System;
using Tao.Sdl;
using System.Timers;
using System.Media;
using System.IO;

namespace MyGame
{
    // Se crea el struct para manipular las coordenadas x e y
    struct Coordenadas
    {
        public int PosX { get; set; }
        public int PosY { get; set; }

        public Coordenadas(int x, int y)
        {
            PosX = x;
            PosY = y;
        }
    }

    // Se crea la clase Player con los atributos Name, Position, Score y el métodos Movement()
    class Player
    {
        public string Name { get; set; }        
        public Coordenadas Position { get; set; }   
        //public int posx { get; set; }
        //public int posy { get; set; }
        public int Score { get; set; }

        private bool canMove;


        public Player(string name, int posX, int posY)
        {
            Name = name;            
            Position = new Coordenadas(posX, posY);
            Score = 0;      
            canMove = true;
        }
        
        public void Movement(int windowWidth, int windowHeight, int playerSize)
        {
            if (canMove)
            {
                // Inputs del teclado
                if (Engine.KeyPress(Engine.KEY_LEFT) || Engine.KeyPress(Engine.KEY_A))
                {
                    Position = new Coordenadas(Position.PosX - 32, Position.PosY); // Mover hacia la izquierda
                    canMove = false; // Bloquear movimiento hasta que la tecla sea liberada
                }

                if (Engine.KeyPress(Engine.KEY_RIGHT) || Engine.KeyPress(Engine.KEY_D))
                {
                    Position = new Coordenadas(Position.PosX + 32, Position.PosY); // Mover hacia la derecha
                    canMove = false; // Bloquear movimiento hasta que la tecla sea liberada
                }

                if (Engine.KeyPress(Engine.KEY_UP) || Engine.KeyPress(Engine.KEY_W))
                {
                    Position = new Coordenadas(Position.PosX, Position.PosY - 32); // Mover hacia arriba
                    canMove = false; // Bloquear movimiento hasta que la tecla sea liberada
                }

                if (Engine.KeyPress(Engine.KEY_DOWN) || Engine.KeyPress(Engine.KEY_S))
                {
                    Position = new Coordenadas(Position.PosX, Position.PosY + 32); // Mover hacia abajo
                    canMove = false; // Bloquear movimiento hasta que la tecla sea liberada
                }
            }

            // Detectar si la tecla fue liberada para permitir el próximo movimiento
            if (!Engine.KeyPress(Engine.KEY_LEFT) && !Engine.KeyPress(Engine.KEY_A) &&
                !Engine.KeyPress(Engine.KEY_RIGHT) && !Engine.KeyPress(Engine.KEY_D) &&
                !Engine.KeyPress(Engine.KEY_UP) && !Engine.KeyPress(Engine.KEY_W) &&
                !Engine.KeyPress(Engine.KEY_DOWN) && !Engine.KeyPress(Engine.KEY_S))
            {
                canMove = true; // Permitir mover de nuevo cuando las teclas se liberen
            }
            // Se limita el movimiento del jugador dentro de los límites de la pantalla
            //PosX = Math.Max(0, Math.Min(Position.PosX, windowWidth - playerSize));
            //PosY = Math.Max(32, Math.Min(Position.PosY, windowHeight - playerSize));

            Position = new Coordenadas(
                Math.Max(0, Math.Min(Position.PosX, windowWidth - playerSize)),
                Math.Max(32, Math.Min(Position.PosY, windowHeight - playerSize))
            );
        } 
    }

    class Program
    {
        // Constantes
        // Dimensiones de la ventana
        const int WINDOW_WIDTH = 800;
        const int WINDOW_HEIGHT = 608;
        // Tamaño del jugador y coleccionables
        const int OBJECTS_SIZE = 32;

        // Estáticos
        // instancia de la clase player
        static Player player;
        
        // Imagen jugador
        static Image playerImage;
        static Image playerImageAux;
        // Array de imagen de fondo
        static Image[] backGround = new Image[16];
        // Array de imagen de objetos coleccionables
        static Image[] itemsImages = new Image[10];
        static Image gameOverBG;
        // Fuentes
        static Font titleFont;
        static Font menuFont;
        static Font scoreFont;
        // Audio 
        static SoundPlayer music;
        // Variables utilizadas para el cronometro
        static Timer timer;
        static int countdown = 60;
        // Gestor de escena
        static int stage = 0;            
        static int highScore = 0;
        static readonly string filePath = "assets/highscore.txt";
        //Posición inicial del objeto coleccionable
        static Coordenadas item;
        //static int itemX;
        //static int itemY;
        
        // Indices para recorrer array correspondiente
        static int itemsIndex;
        //static int itemsCounter;

        // Indice para gestionar fondo de pantalla
        static int backGroundIndex;
        // Contador de tiempo de ejecución 
        static int backGroundCounter;

        static Random random = new Random();

        static bool soundOn = true;
        
        static bool isPowerUp = false;


        static void Main(string[] args)
        {
            GameLoop();
        }

        // FUNCIONES

        static void CheckInputs()
        {
            player.Movement(WINDOW_WIDTH, WINDOW_HEIGHT, OBJECTS_SIZE);

            if (Engine.KeyPress(Engine.KEY_B))
            {
                if (stage == 1 || stage == 2 || stage == 3)
                {
                    stage = 0;
                }
            }

            if (Engine.KeyPress(Engine.KEY_P))
            {
                if (stage == 0 || stage == 2)
                {
                    stage = 1;
                    countdown = 60;
                    player.Score = 0;
                    isPowerUp = false;
                }
            }

            if (Engine.KeyPress(Engine.KEY_M))
            {
                soundOn = !soundOn;
            }

            if (Engine.KeyPress((int)Engine.KEY_R))
            {
                if (stage == 3)
                {
                    highScore = 0;
                    SaveHighScore(highScore);
                }
            }

            if (Engine.KeyPress((int)Engine.KEY_H))
            {
                if (stage == 0)
                {
                    stage = 3;
                }
            }

            if (Engine.KeyPress(Engine.KEY_ESC))
            {

                if (stage == 0)
                {
                    Environment.Exit(0); // Salir del juego
                }
            }
        }

        static void Update()
        {
            if (soundOn)
            {
                music.Play();
            }
            BackgroundColor();
            PickUpItemAndScore();
        }

        static void Render()
        {
            // Limpiar la pantalla y dibujar los elementos
            Engine.Clear();

            StagesManager(stage);
            
            Engine.Show();                        
        }

        static void StagesManager(int stage)
        {
            switch (stage)
            {
                case 0:
                                        
                    Engine.Draw(backGround[0], 0, 0);
                    Engine.DrawText($"Square", (WINDOW_WIDTH / 2) - 200, WINDOW_HEIGHT / 4, 0, 0, 0, titleFont);
                    Engine.DrawText($"(P)lay  (H)ighscore  (Esc)ape", (WINDOW_WIDTH / 2) - 150, WINDOW_HEIGHT / 4 * 3, 0, 0, 0, menuFont);
                    Engine.DrawText($"(M)ute/Unmute", (WINDOW_WIDTH / 2) - 100, WINDOW_HEIGHT - 32, 0, 0, 0, menuFont);
                    break;
                
                case 1:

                    Engine.Draw(backGround[backGroundIndex], 0, 0);

                    if (!isPowerUp)
                    {
                        Engine.Draw(playerImage, player.Position.PosX, player.Position.PosY); // Dibujar al jugador
                    }
                    else
                    {
                        Engine.Draw(playerImageAux, player.Position.PosX, player.Position.PosY); // Dibujar al jugador alternativo
                    }
                    
                    Engine.Draw(itemsImages[itemsIndex], item.PosX, item.PosY); // Dibujar el objeto           
                    Engine.DrawText($"Score: {player.Score}", 10, 5, 0, 0, 0, scoreFont);
                    Engine.DrawText($"HighScore: {highScore}", 300, 5, 0, 0, 0, scoreFont);
                    Engine.DrawText($"Time: {countdown}", 700, 5, 0, 0, 0, scoreFont);
                    Engine.DrawText($"(M)ute/Unmute     wasd/arrows to play    (B)ack to Menu", 100, WINDOW_HEIGHT - OBJECTS_SIZE, 0, 0, 0, menuFont);
                    break;   

                case 2:      
                    
                    Engine.Draw(gameOverBG, 0, 0);
                    Engine.DrawText($"Game Over", (WINDOW_WIDTH / 2) - 300, WINDOW_HEIGHT / 4, 255, 255, 255, titleFont);
                    Engine.DrawText($"HighScore: {highScore}", (WINDOW_WIDTH / 2) - 80, WINDOW_HEIGHT / 2, 255, 255, 255, scoreFont);
                    Engine.DrawText($"Your Score: {player.Score}", (WINDOW_WIDTH / 2) - 80, (WINDOW_HEIGHT / 2) + 40, 255, 255, 255, scoreFont);
                    Engine.DrawText($"(P)lay   (B)ack to Menu", (WINDOW_WIDTH / 2) - 110, WINDOW_HEIGHT / 4 * 3, 255, 255, 255, menuFont);
                    Engine.DrawText($"(M)ute/Unmute", (WINDOW_WIDTH / 2) - 80, WINDOW_HEIGHT - 32, 255, 255, 255, menuFont);
                    break;

                case 3:

                    Engine.Draw(gameOverBG, 0, 0);                   
                    Engine.DrawText($"HighScore: {highScore}", (WINDOW_WIDTH / 2) - 80, WINDOW_HEIGHT / 2, 255, 255, 255, scoreFont);
                    Engine.DrawText($"(M)ute/Unmute       (R)eset Highscore       (B)ack to Menu", (WINDOW_WIDTH / 2) - 300, WINDOW_HEIGHT - 32, 255, 255, 255, menuFont);
                    break;
            }

        }
                
        static void GameLoop()
        {
            // Inicializar el motor
            Engine.Initialize(WINDOW_WIDTH, WINDOW_HEIGHT);

            player = new Player("Jugador1", 0, 32);
            highScore = ReadHighScore();

            // Configurar el temporizador
            timer = new Timer(1000); // 1000 ms = 1 segundo
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true; // El temporizador sigue corriendo
            timer.Enabled = true;

            LoadAssets(); // Cargar assets 

            SpawnItem(); // Crear el primer coleccionable

            while (true)
            {
                CheckInputs();
                Update();
                Render();
                Sdl.SDL_Delay(20); // Controlar la velocidad del juego
            }
        }

        static void LoadAssets()
        {
            // Imagenes para animación fondo
            backGround[0] = Engine.LoadImage("assets/bg01.png");
            backGround[1] = Engine.LoadImage("assets/bg02.png");
            backGround[2] = Engine.LoadImage("assets/bg03.png");
            backGround[3] = Engine.LoadImage("assets/bg04.png");
            backGround[4] = Engine.LoadImage("assets/bg05.png");
            backGround[5] = Engine.LoadImage("assets/bg06.png");
            backGround[6] = Engine.LoadImage("assets/bg07.png");
            backGround[7] = Engine.LoadImage("assets/bg08.png");
            backGround[8] = Engine.LoadImage("assets/bg09.png");
            backGround[9] = Engine.LoadImage("assets/bg10.png");
            backGround[10] = Engine.LoadImage("assets/bg11.png");
            backGround[11] = Engine.LoadImage("assets/bg12.png");
            backGround[12] = Engine.LoadImage("assets/bg13.png");
            backGround[13] = Engine.LoadImage("assets/bg14.png");
            backGround[14] = Engine.LoadImage("assets/bg15.png");
            backGround[15] = Engine.LoadImage("assets/bg16.png");

            // Game Over bacground
            gameOverBG = Engine.LoadImage("assets/gameover.png");
            // Imagenes jugador
            playerImage = Engine.LoadImage("assets/jugador01.png");
            playerImageAux = Engine.LoadImage("assets/jugador02.png");

            // Imagenes objetos coleccionables
            // Se hace el for para potenciar la probabilidad de que
            // aparezca uno u otro.
            for (int i = 0; i < itemsImages.Length; i++)
            {
                if (i < 5)
                {
                    itemsImages[i] = Engine.LoadImage("assets/objeto01.png");
                }
                else if (i == 5)
                {
                    itemsImages[i] = Engine.LoadImage("assets/objeto02.png");
                }
                else if (i > 7)
                {

                    itemsImages[i] = Engine.LoadImage("assets/objeto04.png");
                }
                else
                {
                    itemsImages[i] = Engine.LoadImage("assets/objeto03.png");
                }                
            }

            // Fuentes
            titleFont = Engine.LoadFont("assets/font01.ttf", 100);
            scoreFont = Engine.LoadFont("assets/font02.ttf", 25);
            menuFont = Engine.LoadFont("assets/font03.ttf", 25);

            music = new SoundPlayer("assets/sound01.wav");
        }

        static void SpawnItem()
        {
            // Generar una nueva posición para el objeto dentro de las dimensiones de la ventana
            itemsIndex = random.Next(0, 10);
            //itemX = random.Next(0, WINDOW_WIDTH / OBJECTS_SIZE) * OBJECTS_SIZE;
            //itemY = random.Next(1, WINDOW_HEIGHT / OBJECTS_SIZE) * OBJECTS_SIZE;
            item = new Coordenadas(
                random.Next(0, WINDOW_WIDTH / OBJECTS_SIZE) * OBJECTS_SIZE,
                random.Next(1, WINDOW_HEIGHT / OBJECTS_SIZE) * OBJECTS_SIZE
            );
        }

        static void BackgroundColor()
        {
            // Actualizar el color del fondo
            backGroundCounter++;

            if (backGroundCounter == 20)
            {
                backGroundIndex++;

                if (backGroundIndex >= backGround.Length)
                {
                    backGroundIndex = 0;
                }

                backGroundCounter = 0;
            }
        }       

        static void PickUpItemAndScore()
        {
            // Comprueba si el jugador recoge el objeto y aplica un puntaje / efecto / castigo según la recolección
            if (player.Position.PosX < item.PosX + OBJECTS_SIZE && player.Position.PosX + OBJECTS_SIZE > item.PosX && player.Position.PosY < item.PosY + OBJECTS_SIZE && player.Position.PosY + OBJECTS_SIZE > item.PosY)
            {
                if (itemsIndex < 5)
                {
                    player.Score += 50;
                    countdown += 2;
                }
                else if (itemsIndex == 5)
                {
                    player.Score += 500;
                    countdown += 5;
                }
                else if (itemsIndex > 7)
                {
                    isPowerUp = !isPowerUp;
                    if (isPowerUp)
                    {
                        player.Score += 200;                        
                    }                    
                }
                else
                {
                    if (!isPowerUp)
                    {
                        if (random.Next(2) == 0)
                        {
                            countdown -= 10;
                            if (countdown < 0)
                            {
                                countdown = 0;
                            }
                        }
                        else
                        {
                            player.Score -= 200;
                            if (player.Score < 0)
                            {
                                player.Score = 0;
                            }
                        }
                    }
                }
                
                SpawnItem(); // y genera un nuevo objeto
            }
        }

        static int ReadHighScore()
        {
            if (File.Exists(filePath))
            {
                string score = File.ReadAllText(filePath);  // Leer el contenido del archivo
                return int.TryParse(score, out int result) ? result : 0;  // Convertir el texto a entero, o 0 si no es válido
            }
            else
            {
                return 0;  // Si no existe el archivo, el highscore es 0
            }
        }

        static void SaveHighScore(int score)
        {
            File.WriteAllText(filePath, score.ToString());  // Escribir la nueva puntuación en el archivo
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (stage == 1)
            {
                countdown--;
                if (countdown <= 0)
                {
                    if (player.Score > highScore)
                    {
                        highScore = player.Score;
                        SaveHighScore(highScore);
                    }
                    stage = 2;
                }
            }
            
        }
    }
}
